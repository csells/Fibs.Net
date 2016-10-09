﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalisticTelnet;

namespace Fibs {
  // FIBS Client Protocol Detailed Specification: http://www.fibs.com/fibs_interface.html
  // Known hosts: fibs.com:4321 (default), tigergammon.com:4321
  public class FibsLib : IDisposable {
    static string FibsVersion = "1008";
    TelnetConnection telnet;
    CookieMonster monster = new CookieMonster();

    public FibsLib(string host = "fibs.com", int port = 4321) {
      // from https://github.com/9swampy/Telnet/
      telnet = new TelnetConnection(host, port);
      if (!telnet.IsConnected) { throw new Exception($"cannot connect to {host} on {port}"); }
    }

    public async Task Login(string user, string pw) {
      await ExpectAsync(FibsCookie.FIBS_LoginPrompt);
      await WriteLineAsync($"login dotnetcli {FibsVersion} {user} {pw}");
      await ReadAllAsync();
    }

    // Use ToArray instead of yield return to make sure all messages in this string
    // are processed. yield return will stop in the middle if all of the messages
    // aren't pulled.
    IEnumerable<CookieMessage> Process(string s) =>
      s.Split(new string[] { "\r\n" }, StringSplitOptions.None).Select(l => monster.EatCookie(l)).ToArray();

    Task WriteLineAsync(string line) => telnet.WriteLineAsync(line);

    async Task ExpectAsync(FibsCookie cookie, int timeout = 5000) {
      var start = DateTime.Now;
      var span = new TimeSpan(0, 0, 0, 0, timeout);
      while (DateTime.Now - start < span) {
        var s = await telnet.ReadAsync(1);
        var cookieMessages = Process(s);
        if (cookieMessages.Any(cm => cm.Cookie == cookie)) { return; }
      }

      throw new Exception($"{cookie} not found");
    }

    async Task ReadAllAsync() {
      while (true) {
        var s = await telnet.ReadAsync(1000);
        Process(s);
        if (string.IsNullOrEmpty(s)) { break; }
      }
    }

    // clean up, clean up, everybody do their share...
    ~FibsLib() { ((IDisposable)this).Dispose(); }
    void IDisposable.Dispose() { if (telnet != null) { telnet.Dispose(); telnet = null; } }
  }

}
