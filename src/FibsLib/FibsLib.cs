using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fibs {
  // FIBS Client Protocol Detailed Specification: http://www.fibs.com/fibs_interface.html
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

    //public async Task<IEnumerable<CookieMessage>> ReadMessagesAsync(int timeout = 5000) =>
    //  Process(await ReadAsync(timeout));

    public async Task Login(string user, string pw) {
      await ExpectAsync(FibsCookie.FIBS_LoginPrompt);
      await WriteLineAsync($"login dotnetcli {FibsVersion} {user} {pw}");
      await ExpectAsync(FibsCookie.CLIP_MOTD_END);
    }

    // Use ToArray instead of yield return to make sure all messages in this string
    // are processed. yield return will stop in the middle if all of the messages
    // aren't pulled.
    IEnumerable<CookieMessage> Process(string s) =>
      s.Split(new string[] { "\r\n" }, StringSplitOptions.None).Select(l => monster.EatCookie(l)).ToArray();

    public async Task WriteLineAsync(string line) {
      await WriteAsync(line + "\n");
    }

    public async Task WriteAsync(string s) {
      if (!telnet.Connected) { throw new Exception("not connected"); }
      byte[] writeBuffer = ASCIIEncoding.ASCII.GetBytes(s);
      await telnet.GetStream().WriteAsync(writeBuffer, 0, writeBuffer.Length);
    }

    public async Task<string> ReadAsync(int timeout = 5000) {
      if (!telnet.Connected) { throw new Exception("not connected"); }

      var start = DateTime.Now;
      var span = new TimeSpan(0, 0, 0, 0, timeout);
      var sb = new StringBuilder();

      while (DateTime.Now - start < span) {
        while (telnet.Available > 0) {
          var count = telnet.GetStream().Read(readBuffer, 0, readBuffer.Length);
          sb.Append(ASCIIEncoding.ASCII.GetString(readBuffer, 0, count));
        }
        await Task.Delay(1);
      }

      return sb.ToString();
    }

    async Task ExpectAsync(FibsCookie cookie, int timeout = 5000) {
      var start = DateTime.Now;
      var span = new TimeSpan(0, 0, 0, 0, timeout);
      while (DateTime.Now - start < span) {
        var s = await ReadAsync(1);
        if (string.IsNullOrEmpty(s)) { continue; }
        var cookieMessages = Process(s);
        if (cookieMessages.Any(cm => cm.Cookie == cookie)) { return; }
      }

      throw new Exception($"{cookie} not found");
    }

    // clean up, clean up, everybody do their share...
    ~FibsSession() { ((IDisposable)this).Dispose(); }
    void IDisposable.Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
  }

}
