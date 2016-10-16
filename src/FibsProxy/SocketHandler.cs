using System;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Fibs;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace FibsProxy {
  // from http://zbrad.github.io/tools/wscore/
  // and from http://dotnetthoughts.net/using-websockets-in-aspnet-core/
  public class SocketHandler {
    static async Task Acceptor(HttpContext hc, Func<Task> n) {
      if (!hc.WebSockets.IsWebSocketRequest) { return; }
      var socket = await hc.WebSockets.AcceptWebSocketAsync();
      await (new SocketHandler()).FibsLoop(socket);
    }

    public static void Map(IApplicationBuilder app) {
      app.UseWebSockets();
      app.Use(SocketHandler.Acceptor);
    }

    static FibsCookie[] MessagesToSkip = new FibsCookie[] { FibsCookie.FIBS_Empty, FibsCookie.CLIP_MOTD_BEGIN, };

    async Task FibsLoop(WebSocket socket) {
      using (var fibs = new FibsSession()) {
        var socketSegment = new ArraySegment<byte>(new byte[4096]);

        // wait for login
        Task<WebSocketReceiveResult> socketReceiveTask = socket.ReceiveAsync(socketSegment, CancellationToken.None);
        var socketInput = await socketReceiveTask;
        if (socketInput.MessageType != WebSocketMessageType.Text) {
          await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", CancellationToken.None);
          return;
        }

        // check for login
        var s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
        var re = new Regex(@"login (?<user>[^ ]+) (?<password>[^ ]+)");
        var match = re.Match(s);
        if (!match.Success) {
          await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "must login first", CancellationToken.None);
          return;
        }

        // prepare to cancel ongoing tasks
        var fibsCancel = new CancellationTokenSource();
        var socketCancel = new CancellationTokenSource();

        // login to FIBS
        // TODO: support creating new users
        // TODO: support other servers besides fibs.com
        var user = match.Groups["user"].Value;
        var password = match.Groups["password"].Value;
        Task<CookieMessage[]> fibsReadTask = fibs.Login(user, password, fibsCancel.Token);
        Debug.WriteLine($"FibsLoop: logging into FIBS for {user}");

        // get more socket input
        socketReceiveTask = socket.ReceiveAsync(socketSegment, socketCancel.Token);

        while (socket.State == WebSocketState.Open) {
          var task = await Task.WhenAny(socketReceiveTask, fibsReadTask);
          //if (task.IsCanceled) { break; }

          if (task.Equals(socketReceiveTask)) {
            socketInput = await socketReceiveTask;

            if(socketInput.MessageType == WebSocketMessageType.Close) {
              await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
              continue;
            }

            if (socketInput.MessageType != WebSocketMessageType.Text) {
              await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", CancellationToken.None);
              continue;
            }

            if (!socketInput.EndOfMessage) {
              await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "message too big", CancellationToken.None);
              continue;
            }

            s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
            if (string.IsNullOrWhiteSpace(s)) {
              await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "no empty messages", CancellationToken.None);
              continue;
            }

            // write socket input to FIBS and wait for more socket input
            await fibs.WriteLineAsync(s);
            socketReceiveTask = socket.ReceiveAsync(socketSegment, socketCancel.Token);
          }
          else if (task.Equals(fibsReadTask)) {
            // read messages from FIBS
            var messages = (await fibsReadTask).Where(cm => !MessagesToSkip.Contains(cm.Cookie)).ToArray();

            if (messages.Length > 0) {
              // write messages to socket
              var json = CookieMessageJsonConverter.ToJson(messages);
              var jsonBytes = Encoding.UTF8.GetBytes(json);
              await socket.SendAsync(new ArraySegment<byte>(jsonBytes, 0, jsonBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);

              if (messages.Any(cm => cm.Cookie == FibsCookie.FIBS_Goodbye)) {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                break;
              }
            }

            // wait for more FIBS input
            fibsReadTask = fibs.ReadMessagesAsync(fibsCancel.Token);
          }
          else {
            Debug.Assert(false, "Unknown task");
          }
        }
        Debug.WriteLine($"FibsLoop: socket closed for {user}");
      }
    }

  }
}
