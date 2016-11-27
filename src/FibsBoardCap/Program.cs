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

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (var fibs = new FibsSession()) {
        // catch Ctrl+C
        Console.CancelKeyPress += (sender, e) => {
          fibs.SendAsync("bye").GetAwaiter().GetResult();
          Process(fibs.ReceiveAsync().GetAwaiter().GetResult());
        };

        // login, set the right properties and watch someone play
        Process(await fibs.LoginAsync(user, pw));

        if (!autoboard) { await fibs.SendAsync("toggle autoboard"); }
        if (bell) { await fibs.SendAsync("toggle bell"); }
        if (!moreboards) { await fibs.SendAsync("toggle moreboards"); }
        if (!notify) { await fibs.SendAsync("toggle notify"); }
        if (!report) { await fibs.SendAsync("toggle report"); }
        await fibs.SendAsync("set boardstyle 3");

        while (true) {
          var messages = await fibs.ReceiveAsync();
          Process(messages);
          // TODO
        }
      }
    }

    HashSet<string> players = new HashSet<string>();
    bool autoboard;
    bool bell;
    bool moreboards;
    bool notify;
    bool report;

    void Process(CookieMessage[] messages) {
      foreach (var cm in messages) {
        switch (cm.Cookie) {
          case FibsCookie.CLIP_WHO_INFO:
            var name = cm.Crumbs["name"];
            if (cm.Crumbs["opponent"] == "-") { players.Remove(name); }
            else { players.Add(name); }
            break;

          case FibsCookie.CLIP_OWN_INFO:
            Func<string, bool> ParseBool = s => s == "1";
            autoboard = ParseBool(cm.Crumbs["autoboard"]);
            bell = ParseBool(cm.Crumbs["bell"]);
            moreboards = ParseBool(cm.Crumbs["moreboards"]);
            notify = ParseBool(cm.Crumbs["notify"]);
            report = ParseBool(cm.Crumbs["report"]);

            Console.WriteLine($"value: autoboard= {cm.Crumbs["autoboard"]}");
            Console.WriteLine($"value: bell= {cm.Crumbs["bell"]}");
            Console.WriteLine($"value: moreboards= {cm.Crumbs["moreboards"]}");
            Console.WriteLine($"value: notify= {cm.Crumbs["notify"]}");
            Console.WriteLine($"value: report= {cm.Crumbs["report"]}");
            break;

          case FibsCookie.FIBS_SettingsValue:
          case FibsCookie.FIBS_SettingsChange:
            Console.WriteLine($"value: {cm.Crumbs["name"]}= {cm.Crumbs["value"]}");
            break;

          case FibsCookie.FIBS_Goodbye:
            Console.WriteLine("received: goodbye");
            break;
        }
      }

    }

  }
}