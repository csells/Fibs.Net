using System;
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

    public FibsSession(string host = "fibs.com", int port = 4321) {
      telnet.Client.Connect(host, port);
      if (!telnet.Connected) { throw new Exception($"cannot connect to {host} on {port}"); }
    }

    public bool IsConnected { get { return telnet.Connected; } }

    public async Task Login(string user, string pw) {
      await ExpectAsync(FibsCookie.FIBS_LoginPrompt);
      await WriteLineAsync($"login dotnetcli {FibsVersion} {user} {pw}");
      await ExpectAsync(FibsCookie.CLIP_MOTD_END);
    }

    // Use ToArray instead of yield return to make sure all messages in this string
    // are processed. yield return will stop in the middle if all of the messages
    // aren't pulled.
    CookieMessage[] Process(string s) =>
      s.Split(new string[] { "\r\n" }, StringSplitOptions.None).Select(l => monster.EatCookie(l)).ToArray();

    public async Task WriteLineAsync(string line) {
      await WriteAsync(line + "\n");
    }

    public async Task WriteAsync(string s) {
      if (!telnet.Connected) { throw new Exception("not connected"); }
      byte[] writeBuffer = ASCIIEncoding.ASCII.GetBytes(s);
      await telnet.GetStream().WriteAsync(writeBuffer, 0, writeBuffer.Length);
    }

    public async Task<CookieMessage[]> ReadMessagesAsync(CancellationToken cancel = default(CancellationToken)) =>
      Process(await ReadAsync(cancel == default(CancellationToken) ? CancellationToken.None : cancel));

    async Task<string> ReadAsync(CancellationToken cancel) {
      if (!telnet.Connected) { throw new Exception("not connected"); }
      var count = await telnet.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancel);
      return Encoding.ASCII.GetString(readBuffer, 0, count);
    }

    async Task ExpectAsync(FibsCookie cookie, int timeout = 5000) {
      // keep trying for up to timeout milliseconds, as ReadAsync returns
      // when there is some input before the timeout, so what you're expecting
      // might not be here yet
      var cancel = new CancellationTokenSource(timeout).Token;
      while (!cancel.IsCancellationRequested) {
        var s = await ReadAsync(cancel);
        if (Process(s).Any(cm => cm.Cookie == cookie)) { return; }
      }
      throw new Exception($"{cookie} not found");
    }

    // clean up, clean up, everybody do their share...
    ~FibsSession() { ((IDisposable)this).Dispose(); }
    void IDisposable.Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
  }

}
