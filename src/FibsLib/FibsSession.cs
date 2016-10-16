using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fibs {
  // Known hosts: fibs.com:4321 (default), tigergammon.com:4321
  public class FibsSession : IDisposable {
    static string FibsVersion = "1008";
    TcpClient telnet = new TcpClient();
    CookieMonster monster = new CookieMonster();
    byte[] readBuffer = new byte[4096];
    string leftovers = "";

    public FibsSession(string host = "fibs.com", int port = 4321) {
      telnet.Client.Connect(host, port);
      if (!telnet.Connected) { throw new Exception($"cannot connect to {host} on {port}"); }
    }

    public bool IsConnected { get { return telnet.Connected; } }

    public async Task<CookieMessage[]> Login(string user, string password) {
      var messages = new List<CookieMessage>();
      messages.AddRange(await ExpectMessageAsync(FibsCookie.FIBS_LoginPrompt));
      await WriteLineAsync($"login dotnetcli {FibsVersion} {user} {password}");
      messages.AddRange(await ExpectMessageAsync(FibsCookie.CLIP_MOTD_END));
      return messages.ToArray();
    }

    public async Task<CookieMessage[]> ReadMessagesAsync(CancellationToken cancel = default(CancellationToken)) =>
      Process(await ReadAsync(cancel == default(CancellationToken) ? CancellationToken.None : cancel));

    public async Task WriteAsync(string s) {
      if (!telnet.Connected) { throw new Exception("not connected"); }
      byte[] writeBuffer = ASCIIEncoding.ASCII.GetBytes(s);
      await telnet.GetStream().WriteAsync(writeBuffer, 0, writeBuffer.Length);
    }

    public async Task WriteLineAsync(string line) {
      await WriteAsync(line + "\n");
    }

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

    async Task<string> ReadAsync(CancellationToken cancel) {
      if (!telnet.Connected) { throw new Exception("not connected"); }
      var count = await telnet.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancel);
      return Encoding.ASCII.GetString(readBuffer, 0, count);
    }

    async Task<CookieMessage[]> ExpectMessageAsync(FibsCookie cookie, int timeout = 5000) {
      var allMessages = new List<CookieMessage>();

      // keep trying for up to timeout milliseconds, as ReadAsync returns
      // when there is some input before the timeout, so what you're expecting
      // might not be here yet
      var cancel = new CancellationTokenSource(timeout).Token;
      while (!cancel.IsCancellationRequested) {
        var s = await ReadAsync(cancel);
        var someMessages = Process(s);
        allMessages.AddRange(someMessages);
        if (someMessages.Any(cm => cm.Cookie == cookie)) { return allMessages.ToArray(); }
      }

      // I had such low expectations...
      throw new Exception($"{cookie} not found");
    }

    // clean up, clean up, everybody do their share...
    ~FibsSession() { ((IDisposable)this).Dispose(); }
    void IDisposable.Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
  }

}
