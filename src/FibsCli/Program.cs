using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fibs {
  class Program {
    static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    FibsSession fibs;
    Task<string> consoleInputTask;
    Task<CookieMessage[]> fibsInputTask;

    Task<string> GetConsoleInput() {
      // from https://smellegantcode.wordpress.com/2012/08/28/a-boring-discovery/
      return Task.Run(() => Console.In.ReadLine());
    }

    Task<CookieMessage[]> GetFibsInput() {
      return fibs.ReceiveAsync();
    }

    void Prompt() {
      Console.Write($"C:{consoleInputTask.Id} F:{fibsInputTask.Id}> ");
    }

    StreamWriter dump = new StreamWriter(File.Open(@"c:\temp\fibs-dump.txt", FileMode.Create, FileAccess.Write, FileShare.Read));

    void DumpMessages(CookieMessage[] messages) {
      var skip = new FibsCookie[] { FibsCookie.FIBS_Empty, FibsCookie.CLIP_MOTD_BEGIN, FibsCookie.CLIP_MOTD_END, FibsCookie.CLIP_WHO_END, };
      messages = messages.Where(cm => !skip.Contains(cm.Cookie)).ToArray();
      if (messages.Length != 0) {
        dump.WriteLine($"{messages.Length} messages:");

        var nocrumbs = new FibsCookie[] { FibsCookie.FIBS_LoginPrompt };
        foreach (var message in messages) {
          dump.Write($"{message.Cookie}: ");
          if (message.Crumbs != null) foreach (var crumb in message.Crumbs) dump.Write($"{crumb.Key}= '{crumb.Value}', ");
          dump.WriteLine();

          if (message.Cookie == FibsCookie.FIBS_Unknown) {
            Debug.Assert(false, $"FIBS_Unknown: '{message.Raw}'");
          }

          if (message.Crumbs == null && !nocrumbs.Contains(message.Cookie)) {
            Debug.Assert(false, $"{message.Cookie}, no crumbs");
          }
        }
      }
      dump.Flush();
    }

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (fibs = new FibsSession()) {
        consoleInputTask = GetConsoleInput();
        fibsInputTask = fibs.LoginAsync(user, pw);
        Prompt();

        while (true) {
          var task = await Task.WhenAny(consoleInputTask, fibsInputTask);
          if (task.Equals(consoleInputTask)) {
            var line = await consoleInputTask;
            if (!string.IsNullOrWhiteSpace(line)) {
              await fibs.SendAsync(line);
            }
            consoleInputTask = GetConsoleInput();
            Prompt();
          }
          else if (task.Equals(fibsInputTask)) {
            var messages = await fibsInputTask;
            DumpMessages(messages);
            if (messages.Any(cm => cm.Cookie == FibsCookie.FIBS_Goodbye)) { break; }
            fibsInputTask = GetFibsInput();
          }
          else {
            Debug.Assert(false);
          }
        }
      }
    }
  }
}