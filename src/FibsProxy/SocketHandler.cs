using Fibs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FibsProxy {
  // from http://zbrad.github.io/tools/wscore/
  // and from http://dotnetthoughts.net/using-websockets-in-aspnet-core/
  public class SocketHandler {
    readonly ILogger logger;
    readonly FibsProxyOptions options;

    // TODO: IOptionsSnapshot would be better, but can't seem to get it to work with the GetService call below
    public SocketHandler(ILogger<SocketHandler> logger, IOptions<FibsProxyOptions> options) {
      this.logger = logger;
      this.options = options.Value;
    }

    static async Task Acceptor(HttpContext hc, Func<Task> n, IServiceProvider provider) {
      if (!hc.WebSockets.IsWebSocketRequest) { return; }

      var socket = await hc.WebSockets.AcceptWebSocketAsync();
      var logger = (ILogger<SocketHandler>)provider.GetService(typeof(ILogger<SocketHandler>));
      var options = (IOptions<FibsProxyOptions>)provider.GetService(typeof(IOptions<FibsProxyOptions>));

      try {
        await (new SocketHandler(logger, options)).FibsLoop(socket);
      }
      finally {
        logger.LogDebug("Exited FibsLoop().");
      }
    }

    public static void Map(IApplicationBuilder app) {
      app.UseWebSockets();
      app.Use((HttpContext hc, Func<Task> n) => SocketHandler.Acceptor(hc, n, app.ApplicationServices));
    }

    static FibsCookie[] MessagesToSkip = new FibsCookie[] { FibsCookie.FIBS_Empty };

    async Task FibsLoop(WebSocket socket) {
      var noCancel = CancellationToken.None;
      var cancelRead = new CancellationTokenSource();
      var socketSegment = new ArraySegment<byte>(new byte[4096]);

      this.logger.LogDebug("Waiting for login.");
      Task<WebSocketReceiveResult> socketReceiveTask = socket.ReceiveAsync(socketSegment, noCancel);
      var socketInput = await socketReceiveTask;
      if (socketInput.MessageType != WebSocketMessageType.Text) {
        this.logger.LogError("Received non-text message. Closing socket.");
        await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", noCancel);
        return;
      }

      // check for login
      var s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
      var re = new Regex(@"login (?<user>[^ ]+) (?<password>[^ ]+)");
      var match = re.Match(s);
      if (!match.Success) {
        this.logger.LogError("First message was not a login message. Closing socket.");
        await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "must login first", noCancel);
        return;
      }

      // login to FIBS
      // TODO: support creating new users
      // TODO: support other servers besides fibs.com on a per connection basis from the user
      // TODO: support message queueing while there awaiting a response, otherwise FIBS drops requests
      // (and remember to take the hack out of the client...)
      var server = this.options.DefaultServer;
      var port = this.options.DefaultPort;
      var user = match.Groups["user"].Value;
      var password = match.Groups["password"].Value;
      this.logger.LogInformation($"Logging into FIBS on {server}:{port} for {user}");

      using (var fibs = new FibsSession(server, port)) {
        Task<CookieMessage[]> fibsReadTask = fibs.LoginAsync(user, password);

        // get more socket input
        socketReceiveTask = socket.ReceiveAsync(socketSegment, noCancel);

        this.logger.LogDebug("Beginning main loop.");
        while (socket.State == WebSocketState.Open) {
          var task = await Task.WhenAny(socketReceiveTask, fibsReadTask);
          //if (task.IsCanceled) { break; }

          if (task.Equals(socketReceiveTask)) {
            this.logger.LogDebug("Received a message from web socket.");
            socketInput = await socketReceiveTask;

            if (socketInput.MessageType == WebSocketMessageType.Close) {
              await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", noCancel);
              continue;
            }

            if (socketInput.MessageType != WebSocketMessageType.Text) {
              this.logger.LogError("Received non-text message.");
              await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text messages only", noCancel);
              continue;
            }

            if (!socketInput.EndOfMessage) {
              this.logger.LogError("Received message that was too big.");
              await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "message too big", noCancel);
              continue;
            }

            s = Encoding.UTF8.GetString(socketSegment.Array, 0, socketInput.Count);
            if (string.IsNullOrWhiteSpace(s)) {
              this.logger.LogError("Received an empty message.");
              await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "no empty messages", noCancel);
              continue;
            }

            this.logger.LogDebug(s);
            // write socket input to FIBS and wait for more socket input
            await fibs.SendAsync(s);
            socketReceiveTask = socket.ReceiveAsync(socketSegment, cancelRead.Token);
          }
          else if (task.Equals(fibsReadTask)) {
            // read messages from FIBS
            this.logger.LogDebug("Received {0} messages from FIBS.", fibsReadTask.Result.Count());
            var messages = fibsReadTask.Result.Where(cm => !MessagesToSkip.Contains(cm.Cookie)).ToArray();

            if (messages.Length > 0) {
              // write messages to socket
              var jsonText = messages.ToJson();
              var jsonBytes = Encoding.UTF8.GetBytes(jsonText);
              this.logger.LogDebug("Forwarding messages from FIBS to browser.\n{0}", jsonText);
              await socket.SendAsync(new ArraySegment<byte>(jsonBytes, 0, jsonBytes.Length), WebSocketMessageType.Text, true, noCancel);

              if (messages.Any(cm => cm.Cookie == FibsCookie.FIBS_Goodbye)) {
                this.logger.LogDebug("Received Goodbye from FIBS.");
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
        this.logger.LogDebug("FibsLoop: socket closed for {0}", user);
      }
    }
  }
}
