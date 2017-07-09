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
using Microsoft.Extensions.Logging;

namespace FibsProxy {
  // from http://zbrad.github.io/tools/wscore/
  // and from http://dotnetthoughts.net/using-websockets-in-aspnet-core/
  public class SocketHandler {
    private readonly ILogger _logger;

    public SocketHandler(ILogger<SocketHandler> logger)
    {
      _logger = logger;
    }

    static async Task Acceptor(HttpContext hc, Func<Task> n, IServiceProvider provider) {
      if (!hc.WebSockets.IsWebSocketRequest) { return; }
      var socket = await hc.WebSockets.AcceptWebSocketAsync();
      var logger = (ILogger<SocketHandler>)provider
        .GetService(typeof(ILogger<SocketHandler>));
      try
      {
        await (new SocketHandler(logger)).FibsLoop(socket);
      }
      finally
      {
        logger.LogDebug("Exited FibsLoop().");
      }
    }

    public static void Map(IApplicationBuilder app) {
      app.UseWebSockets();
      app.Use((HttpContext hc, Func<Task> n) =>
        SocketHandler.Acceptor(hc, n, app.ApplicationServices));
    }

    static FibsCookie[] MessagesToSkip = new FibsCookie[] { FibsCookie.FIBS_Empty };

    async Task FibsLoop(WebSocket socket) {
      using (var fibs = new FibsSession()) {
        var noCancel = CancellationToken.None;
        var cancelRead = new CancellationTokenSource();
        var socketSegment = new ArraySegment<byte>(new byte[4096]);

        _logger.LogDebug("Waiting for login.");
        Task<WebSocketReceiveResult> socketReceiveTask = socket.ReceiveAsync(socketSegment, noCancel);
        var socketInput = await socketReceiveTask;
        if (socketInput.MessageType != WebSocketMessageType.Text) {
          _logger.LogError("Received non-text message. Closing socket.");
          await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", noCancel);
          return;
        }

        // check for login
        var s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
        var re = new Regex(@"login (?<user>[^ ]+) (?<password>[^ ]+)");
        var match = re.Match(s);
        if (!match.Success) {
          _logger.LogError("First message was not a login message. Closing socket.");
          await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "must login first", noCancel);
          return;
        }

        // login to FIBS
        // TODO: support creating new users
        // TODO: support other servers besides fibs.com
        // TODO: support message queueing while there awaiting a response, otherwise FIBS drops requests
        // (and remember to take the hack out of the client...)
        var user = match.Groups["user"].Value;
        var password = match.Groups["password"].Value;
        _logger.LogInformation("Logging into FIBS for {0}", user);
        Task<CookieMessage[]> fibsReadTask = fibs.LoginAsync(user, password);

        // get more socket input
        socketReceiveTask = socket.ReceiveAsync(socketSegment, noCancel);

        _logger.LogDebug("Beginning main loop.");
        while (socket.State == WebSocketState.Open) {
          var task = await Task.WhenAny(socketReceiveTask, fibsReadTask);
          //if (task.IsCanceled) { break; }

          if (task.Equals(socketReceiveTask)) {
            _logger.LogDebug("Received a message from web socket.");
            socketInput = await socketReceiveTask;

            if(socketInput.MessageType == WebSocketMessageType.Close) {
              await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", noCancel);
              continue;
            }

            if (socketInput.MessageType != WebSocketMessageType.Text) {
              _logger.LogError("Received non-text message.");
              await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", noCancel);
              continue;
            }

            if (!socketInput.EndOfMessage) {
              _logger.LogError("Received message that was too big.");
              await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "message too big", noCancel);
              continue;
            }

            s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
            if (string.IsNullOrWhiteSpace(s)) {
              _logger.LogError("Received an empty message.");
              await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "no empty messages", noCancel);
              continue;
            }

            _logger.LogDebug(s);
            // write socket input to FIBS and wait for more socket input
            await fibs.SendAsync(s);
            socketReceiveTask = socket.ReceiveAsync(socketSegment, cancelRead.Token);
          }
          else if (task.Equals(fibsReadTask)) {
            // read messages from FIBS
            _logger.LogDebug("Received {0} messages from FIBS.", fibsReadTask.Result.Count());
            var messages = fibsReadTask.Result.Where(cm => !MessagesToSkip.Contains(cm.Cookie)).ToArray();

            if (messages.Length > 0) {
              // write messages to socket
              var jsonText = messages.ToJson();
              var jsonBytes = Encoding.UTF8.GetBytes(jsonText);
              _logger.LogDebug("Forwarding messages from FIBS to browser.\n{0}", jsonText);
              await socket.SendAsync(new ArraySegment<byte>(jsonBytes, 0, jsonBytes.Length), WebSocketMessageType.Text, true, noCancel);

              if (messages.Any(cm => cm.Cookie == FibsCookie.FIBS_Goodbye)) {
                _logger.LogDebug("Received Goodbye from FIBS.");
                cancelRead.Cancel();                
                break;
              }
            }

            // wait for more FIBS input
            fibsReadTask = fibs.ReceiveAsync();
          }
          else {
            Debug.Assert(false, "Unknown task");
          }
        }
        _logger.LogDebug("FibsLoop: socket closed for {0}", user);
      }
    }
  }
}
