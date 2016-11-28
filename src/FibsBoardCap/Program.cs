using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibs;

namespace FibsBoardCap {
  public class Program {
    public static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    FibsSession fibs;
    List<string> players = new List<string>();

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (fibs = new FibsSession()) {
        // catch Ctrl+C
        Console.CancelKeyPress += (sender, e) => {
          fibs.SendAsync("bye").GetAwaiter().GetResult();
          Process(fibs.ReceiveAsync().GetAwaiter().GetResult()).GetAwaiter().GetResult();
        };

        // login, set the right properties and watch someone play
        await Process(await fibs.LoginAsync(user, pw));
        await fibs.SendAsync("set boardstyle 3");

        var player = args.Length != 0 ? args[0] : players[(new Random()).Next(0, players.Count)];
        Console.WriteLine($"watching {player}");
        await fibs.SendAsync($"watch {player}");

        while (true) { await Process(await fibs.ReceiveAsync()); }
      }
    }

    async Task Process(CookieMessage[] messages) {
      foreach (var cm in messages) {
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

            if (!autoboard) { await fibs.SendAsync("toggle autoboard"); }
            if (bell) { await fibs.SendAsync("toggle bell"); }
            if (!moreboards) { await fibs.SendAsync("toggle moreboards"); }
            if (!notify) { await fibs.SendAsync("toggle notify"); }
            if (!report) { await fibs.SendAsync("toggle report"); }
            break;

          case FibsCookie.FIBS_Board:
            // recognize a board in boardstyle 3 and ask for it in boardstyle 2
            Console.WriteLine($"{cm.Cookie}: {cm.Raw}");
            await fibs.SendAsync("set boardstyle 2");
            await fibs.SendAsync($"board");
            await fibs.SendAsync("set boardstyle 3");
            break;

          case FibsCookie.FIBS_Unknown:
            if (cm.Raw.StartsWith("board:")) { throw new Exception($"unparsed board: ${cm.Raw}"); }
            Console.WriteLine($"{cm.Cookie}: {cm.Raw}");
            break;
        }
      }

    }

  }
}
