using System;
using System.Linq;
using System.Threading.Tasks;
using MinimalisticTelnet;

namespace Fibs {
  // FIBS Client Protocol Detailed Specification: http://www.fibs.com/fibs_interface.html
  // Known hosts: fibs.com:4321 (default), tigergammon.com:4321
  public class FibsLib : IDisposable {
    TelnetConnection telnet;

    public FibsLib(string host = "fibs.com", int port = 4321) {
      // from https://github.com/9swampy/Telnet/
      telnet = new TelnetConnection(host, port);
      if (!telnet.IsConnected) { throw new Exception($"cannot connect to {host} on {port}"); }
    }

    public async Task Login(string user, string pw) {
      await ExpectAsync("login:");
      await WriteLineAsync($"login dotnetcli 1008 {user} {pw}");
      await ReadAllAsync();
    }

    Task WriteLineAsync(string line) {
      return telnet.WriteLineAsync(line);
    }

    async Task ExpectAsync(string terminator, int timeout = 5000) {
      var start = DateTime.Now;
      var span = new TimeSpan(0, 0, 0, 0, timeout);
      while (DateTime.Now - start < span) {
        var s = await telnet.ReadAsync(100);
        Console.Write(s);
        if (s.TrimEnd().EndsWith(terminator)) { return; }
      }

      throw new Exception($"{terminator} not found");
    }

    async Task ReadAllAsync() {
      while (true) {
        var s = await telnet.ReadAsync(1000);
        if (string.IsNullOrEmpty(s)) { break; }
        // TODO: distinquish between a new login and losing a connection in the middle
        if (s.Trim() == "login:") { throw new Exception("invalid username/password"); }

        var lines = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim());
        foreach (var line in lines) {
          Console.WriteLine("'" + line + "'");
          var cm = ClipBase.Parse(line);
        }
      }
    }

    // clean up, clean up, everybody do their share...
    ~FibsLib() { ((IDisposable)this).Dispose(); }
    void IDisposable.Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
  }

}
