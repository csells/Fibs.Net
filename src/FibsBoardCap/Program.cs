using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Fibs;
using System.Linq;

namespace FibsBoardCap {
  class Player {
    public string Name { get; set; }
    public string Opponent { get; set; }
    public int Idle { get; set; }
  }

  public class Program {
    public static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    FibsSession fibs;
    List<Player> players = new List<Player>();
    Player player;
    bool processing = true;

    static void Log(string s) { Console.Error.WriteLine(s); }
    async Task SendAsync(string s) { Log($"send: {s}"); await fibs.SendAsync(s); }

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

        // pick the player provided on the command line or the one least idle
        if (args.Length != 0) {
          player = players.Find(p => string.Compare(p.Name, args[0], true) == 0);
          if (player == null || player.Opponent == null) { Log($"{args[0]} not playing"); return; }
        }
        else {
          player = players.Where(p => p.Opponent != null).OrderBy(p => p.Idle).First();
        }

        Log($"{player.Name}: idle {player.Idle} seconds");
        await SendAsync($"watch {player.Name}");
        while (processing) { await Process(await fibs.ReceiveAsync()); }
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
          await SendAsync($"unwatch {player.Name}");
          player = null;
        }
      };

      foreach (var cm in messages) {
        Debug.WriteLine($"{cm.Cookie}: {cm.Raw}");
        switch (cm.Cookie) {
          case FibsCookie.CLIP_WHO_INFO:
            var player = new Player { Name = cm.Crumbs["name"], Opponent = cm.Crumbs["opponent"] == "-" ? null : cm.Crumbs["opponent"], Idle = int.Parse(cm.Crumbs["idle"]) };
            players.RemoveAll(p => p.Name == player.Name);
            players.Add(player);
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
              Log(cache[0].Crumbs["board"]);
              foreach (var line in cache.Skip(1).Select(l => l.Crumbs["raw"])) { Log(line); }
              cache.Clear();
            }
            break;

          case FibsCookie.FIBS_YouStopWatching:
            player = players.Find(p => string.Compare(p.Name, cm.Crumbs["name"]) == 0);
            if (player == null || player.Opponent == null) { Log($"{cm.Crumbs["name"]} not playing"); processing = false; return; }
            await SendAsync($"watch {player.Name}");
            break;

          case FibsCookie.FIBS_NotDoingAnything:
            Log($"{cm.Crumbs["name"]} is not doing anything interesting.");
            processing = false;
            return;
        }
      }

    }

  }
}
