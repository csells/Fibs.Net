using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fibs {
  // Known hosts: fibs.com:4321 (default), tigergammon.com:4321
  public class FibsSession : IDisposable {
    static string FibsVersion = "1008";
    CookieMonster monster = new CookieMonster();
    string leftovers = "";
    CommandQueue queue = new CommandQueue();

    public FibsSession(string host = "fibs.com", int port = 4321) { queue.Connect(host, port); }
    public bool IsConnected => queue.IsConnected;

    public async Task<CookieMessage[]> LoginAsync(string user, string password, CancellationToken cancel = default(CancellationToken)) {
      if (cancel == default(CancellationToken)) { cancel = new CancellationTokenSource(5000).Token; }
      var messages = new List<CookieMessage>();
      messages.AddRange(await ExpectAsync(FibsCookie.FIBS_LoginPrompt, cancel));
      await SendAsync($"login dotnetcli {FibsVersion} {user} {password}");
      messages.AddRange(await ExpectAsync(FibsCookie.CLIP_MOTD_END, cancel));
      return messages.ToArray();
    }

    public async Task<CookieMessage[]> ReceiveAsync(CancellationToken cancel = default(CancellationToken)) =>
      Process(await queue.ReadAsync(cancel == default(CancellationToken) ? CancellationToken.None : cancel));

    public async Task SendAsync(string cmd) => await queue.SendAsync(cmd);

    CookieMessage[] Process(string s) {
      // parse leftovers from the last time
      s = leftovers + s;
      leftovers = "";
      var lines = s.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
      var last = lines.Last();

      // if the last string isn't empty, then it wasn't \r\n-terminated
      // (split will return an empty line if it was \r\n-terminated).
      // if it wasn't \r\n-terminated, then it's partial and save it to
      // parse with the next input. EXCEPT we have to special-case login,
      // since it's the only input that won't be \r\n-terminated.
      if (!string.IsNullOrEmpty(last) && !last.StartsWith("login:")) {
        leftovers = last;
        lines.RemoveAt(lines.Count - 1);
      }

      // Make sure to parse all of the lines here
      return lines.Select(l => monster.EatCookie(l)).ToArray();
    }

    async Task<CookieMessage[]> ExpectAsync(FibsCookie cookie, CancellationToken cancel) {
      var allMessages = new List<CookieMessage>();
      while (!cancel.IsCancellationRequested) {
        var s = await queue.ReadAsync(cancel);
        var someMessages = Process(s);
        allMessages.AddRange(someMessages);
        if (someMessages.Any(cm => cm.Cookie == cookie)) { return allMessages.ToArray(); }
      }

      // I had such low expectations...
      throw new Exception($"{cookie} not found");
    }

    // clean up, clean up, everybody do their share...
    ~FibsSession() { this.Dispose(); }
    public void Dispose() { if (queue != null) { queue.Dispose(); queue = null; } }

    // FIBS only takes one command at a time, so we need to implement queueing behavior
    // or commands get silently dropped
    class CommandQueue : IDisposable {
      bool awaitingResponse = false;
      int whoInfoCount = 0;
      Queue<string> cmdQueue = new Queue<string>();
      TcpClient telnet = new TcpClient();
      byte[] readBuffer = new byte[4096];

      public bool IsConnected { get { return telnet.Connected; } }

      internal void Connect(string host, int port) {
        telnet.Client.Connect(host, port);
        if (!telnet.Connected) { throw new Exception($"cannot connect to {host} on {port}"); }
      }

      internal async Task SendAsync(string cmd) {
        if (cmd.Contains('\n')) { throw new FormatException("cmd cannot contain linefeed"); }

        // when we get a command, queue it and check if we can send it
        Debug.WriteLine($"queueing: {cmd}");
        cmdQueue.Enqueue(cmd);
        await CheckSendAsync();
      }

      async Task CheckSendAsync() {
        // only process a queued command if we're not awaiting a response to a previous command
        if (awaitingResponse || cmdQueue.Count == 0) { return; }
        var cmd = cmdQueue.Dequeue();
        awaitingResponse = true;
        Debug.WriteLine($"sending: {cmd}");
        await WriteAsync(cmd + "\n");
      }

      internal async Task<string> ReadAsync(CancellationToken cancel) {
        if (!telnet.Connected) { throw new Exception("not connected"); }
        var count = await telnet.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancel);
        var s = Encoding.ASCII.GetString(readBuffer, 0, count);

        // when we get a response to a previous command, send a queued command
        if (count != 0) {
          awaitingResponse = false;
          await CheckSendAsync();
        }

        return s;
      }

      async Task WriteAsync(string s) {
        if (!telnet.Connected) { throw new Exception("not connected"); }
        byte[] writeBuffer = ASCIIEncoding.ASCII.GetBytes(s);
        await telnet.GetStream().WriteAsync(writeBuffer, 0, writeBuffer.Length);
      }

      // clean up, clean up, everybody do their share...
      ~CommandQueue() { this.Dispose(); }
      public void Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
    }

  }

}
