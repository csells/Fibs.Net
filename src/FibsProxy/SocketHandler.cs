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

    static FibsCookie[] MessagesToSkip = new FibsCookie[] { FibsCookie.FIBS_Empty };

    async Task FibsLoop(WebSocket socket) {
      using (var fibs = new FibsSession()) {
        var cancel = CancellationToken.None;
        var socketSegment = new ArraySegment<byte>(new byte[4096]);

        // wait for login
        Task<WebSocketReceiveResult> socketReceiveTask = socket.ReceiveAsync(socketSegment, cancel);
        var socketInput = await socketReceiveTask;
        if (socketInput.MessageType != WebSocketMessageType.Text) {
          await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", cancel);
          return;
        }

        // check for login
        var s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
        var re = new Regex(@"login (?<user>[^ ]+) (?<password>[^ ]+)");
        var match = re.Match(s);
        if (!match.Success) {
          await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "must login first", cancel);
          return;
        }

        // login to FIBS
        // TODO: support creating new users
        // TODO: support other servers besides fibs.com
        // TODO: support message queueing while there awaiting a response, otherwise FIBS drops requests
        // (and remember to take the hack out of the client...)
        var user = match.Groups["user"].Value;
        var password = match.Groups["password"].Value;
        Task<CookieMessage[]> fibsReadTask = fibs.Login(user, password);
        Debug.WriteLine($"FibsLoop: logging into FIBS for {user}");

        // get more socket input
        socketReceiveTask = socket.ReceiveAsync(socketSegment, cancel);

        while (socket.State == WebSocketState.Open) {
          var task = await Task.WhenAny(socketReceiveTask, fibsReadTask);
          //if (task.IsCanceled) { break; }

          if (task.Equals(socketReceiveTask)) {
            socketInput = await socketReceiveTask;

            if(socketInput.MessageType == WebSocketMessageType.Close) {
              await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel);
              continue;
            }

            if (socketInput.MessageType != WebSocketMessageType.Text) {
              await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", cancel);
              continue;
            }

            if (!socketInput.EndOfMessage) {
              await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "message too big", cancel);
              continue;
            }

            s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
            if (string.IsNullOrWhiteSpace(s)) {
              await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "no empty messages", cancel);
              continue;
            }

            // write socket input to FIBS and wait for more socket input
            await fibs.WriteLineAsync(s);
            socketReceiveTask = socket.ReceiveAsync(socketSegment, cancel);
          }
          else if (task.Equals(fibsReadTask)) {
            // read messages from FIBS
            var messages = (await fibsReadTask).Where(cm => !MessagesToSkip.Contains(cm.Cookie)).ToArray();

            if (messages.Length > 0) {
              // write messages to socket
              var json = CookieMessageJsonConverter.ToJson(messages);
              var jsonBytes = Encoding.UTF8.GetBytes(json);
              await socket.SendAsync(new ArraySegment<byte>(jsonBytes, 0, jsonBytes.Length), WebSocketMessageType.Text, true, cancel);

              if (messages.Any(cm => cm.Cookie == FibsCookie.FIBS_Goodbye)) {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel);
                break;
              }
            }

            // wait for more FIBS input
            fibsReadTask = fibs.ReadMessagesAsync();
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
