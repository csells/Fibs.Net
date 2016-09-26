// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be
// from http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library
// csells: updated for .NET Core

using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MinimalisticTelnet {
  enum Verbs {
    WILL = 251,
    WONT = 252,
    DO = 253,
    DONT = 254,
    IAC = 255
  }

  enum Options {
    SGA = 3
  }

  class TelnetConnection : IDisposable {
    TcpClient tcpSocket;

    public TelnetConnection(string host, int port) {
      tcpSocket = new TcpClient();
      tcpSocket.ConnectAsync(host, port).GetAwaiter().GetResult();
    }

    public async Task<string> Login(string user, string pw, int timeout) {
      string s = await ReadAsync(timeout);
      if (!s.TrimEnd().EndsWith(":")) {
        throw new Exception("Failed to connect: no login prompt");
      }

      await WriteLineAsync(user);
      s += await ReadAsync(timeout);
      if (!s.TrimEnd().EndsWith(":")) {
        throw new Exception("Failed to connect: no password prompt");
      }

      await WriteLineAsync(pw);
      s += await ReadAsync(timeout);
      return s;
    }

    public Task WriteLineAsync(string cmd) {
      return WriteAsync(cmd + "\n");
    }

    public Task WriteAsync(string cmd) {
      if (!tcpSocket.Connected) { throw new Exception("not connected"); }
      byte[] buf = ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
      return tcpSocket.GetStream().WriteAsync(buf, 0, buf.Length);
    }

    public async Task<string> ReadAsync(int timeout) {
      if (!tcpSocket.Connected) { throw new Exception("not connected"); }
      StringBuilder sb = new StringBuilder();
      do {
        ParseTelnet(sb);
        await Task.Delay(timeout);
      } while (tcpSocket.Available > 0);
      return sb.ToString();
    }

    public bool IsConnected => tcpSocket.Connected;

    void ParseTelnet(StringBuilder sb) {
      while (tcpSocket.Available > 0) {
        int input = tcpSocket.GetStream().ReadByte();
        switch (input) {
          case -1:
            break;
          case (int)Verbs.IAC:
            // interpret as command
            int inputverb = tcpSocket.GetStream().ReadByte();
            if (inputverb == -1) break;
            switch (inputverb) {
              case (int)Verbs.IAC:
                //literal IAC = 255 escaped, so append char 255 to string
                sb.Append(inputverb);
                break;
              case (int)Verbs.DO:
              case (int)Verbs.DONT:
              case (int)Verbs.WILL:
              case (int)Verbs.WONT:
                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                int inputoption = tcpSocket.GetStream().ReadByte();
                if (inputoption == -1) break;
                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                if (inputoption == (int)Options.SGA)
                  tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                else
                  tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                tcpSocket.GetStream().WriteByte((byte)inputoption);
                break;
              default:
                break;
            }
            break;
          default:
            sb.Append((char)input);
            break;
        }
      }
    }

    ~TelnetConnection() { Dispose(); }
    public void Dispose() { if (tcpSocket != null) { tcpSocket.Dispose(); tcpSocket = null; } }
  }
}
