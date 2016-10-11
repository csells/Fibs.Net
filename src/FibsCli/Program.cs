using System;
using System.Diagnostics;
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
      return fibs.ReadMessagesAsync();
    }

    void Prompt() {
      Console.Write($"C:{consoleInputTask.Id} F:{fibsInputTask.Id}> ");
    }

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (fibs = new FibsSession()) {
        await fibs.Login(user, pw);

        consoleInputTask = GetConsoleInput();
        fibsInputTask = GetFibsInput();
        Prompt();

        while (true) {
          var task = await Task.WhenAny(consoleInputTask, fibsInputTask);
          if (task.Equals(consoleInputTask)) {
            var line = await consoleInputTask;
            if (!string.IsNullOrWhiteSpace(line)) {
              await fibs.WriteLineAsync(line);
            }
            consoleInputTask = GetConsoleInput();
            Prompt();
          }
          else if (task.Equals(fibsInputTask)) {
            var messages = await fibsInputTask;
            // TODO: handle messages
            Console.WriteLine($"messages.Length= {messages.Length}");
            fibsInputTask = GetFibsInput();
            Prompt();
          }
          else {
            Debug.Assert(false);
          }
        }
      }
    }

  }
}
