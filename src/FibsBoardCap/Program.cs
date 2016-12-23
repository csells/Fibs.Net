using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Fibs;

namespace FibsBoardCap {
  public class Program {
    public static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    FibsSession fibs;
    List<string> players = new List<string>();
    string player;

    async Task SendAsync(string s) { Console.Error.WriteLine($"send: {s}"); await fibs.SendAsync(s); }

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (fibs = new FibsSession()) {
        // catch Ctrl+C
        Console.CancelKeyPress += (sender, e) => {
          SendAsync("bye").GetAwaiter().GetResult();
          Process(fibs.ReceiveAsync().GetAwaiter().GetResult()).GetAwaiter().GetResult();
        };

        // login, set the right properties and watch someone play
        await Process(await fibs.LoginAsync(user, pw));
        await SendAsync("set boardstyle 3");

        player = args.Length != 0 ? args[0] : players[(new Random()).Next(0, players.Count)];
        await SendAsync($"watch {player}");

        while (true) { await Process(await fibs.ReceiveAsync()); }
      }
    }

    List<CookieMessage> cache = new List<CookieMessage>();

    async Task Process(CookieMessage[] messages) {
      // We're expecting one FIBS_Board + 14 FIBS_Unknown messages for the ASCII version of the board
      // If we don't get that, then clear the cache, unwatch whoever we're watching and then start again
      Action protocolError = async () => {
        // something's wrong with the protocol, so unwatch and start again
        Debug.WriteLine("protocol error: clearing cache and unwatching");
        cache.Clear();
        if (player != null) {
          await SendAsync($"unwatch {player}");
          player = null;
        }
      };

      foreach (var cm in messages) {
        Debug.WriteLine($"{cm.Cookie}: {cm.Raw}");
        switch (cm.Cookie) {
          case FibsCookie.CLIP_WHO_INFO:
            var name = cm.Crumbs["name"];
            if (cm.Crumbs["opponent"] == "-") { players.Remove(name); }
            else { players.Add(name); }
            break;

          case FibsCookie.CLIP_OWN_INFO:
            Func<string, bool> ParseBool = s => s == "1";
            var autoboard = ParseBool(cm.Crumbs["autoboard"]);
            var bell = ParseBool(cm.Crumbs["bell"]);
            var moreboards = ParseBool(cm.Crumbs["moreboards"]);
            var notify = ParseBool(cm.Crumbs["notify"]);
            var report = ParseBool(cm.Crumbs["report"]);

            if (!autoboard) { await SendAsync("toggle autoboard"); }
            if (bell) { await SendAsync("toggle bell"); }
            if (!moreboards) { await SendAsync("toggle moreboards"); }
            if (!notify) { await SendAsync("toggle notify"); }
            if (!report) { await SendAsync("toggle report"); }
            break;

          case FibsCookie.FIBS_Board:
            if (cache.Count != 0) { protocolError(); break; }

            // recognize a board in boardstyle 3 and ask for it in boardstyle 2
            cache.Add(cm);
            await SendAsync("set boardstyle 2");
            await SendAsync("board");
            await SendAsync("set boardstyle 3");
            break;

          case FibsCookie.FIBS_Unknown:
            if (cm.Raw.StartsWith("board:")) { throw new Exception($"unparsed board: ${cm.Raw}"); }
            if (cache.Count < 1) { protocolError(); break; }
            if (cache.Count > 14) { protocolError(); break; }
            Debug.Assert(cache[0].Cookie == FibsCookie.FIBS_Board);

            cache.Add(cm);
            if (cache.Count == 15) {
              Console.WriteLine(cache.ToJson());
              cache.Clear();
            }
            break;

          case FibsCookie.FIBS_YouStopWatching:
            player = cm.Crumbs["name"];
            await SendAsync($"watch {player}");
            break;

          case FibsCookie.FIBS_NotDoingAnything:
            throw new Exception($"{cm.Crumbs["name"]} is not doing anything interesting.");
        }
      }

    }

  }
}
