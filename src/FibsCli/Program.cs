using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fibs {
  class Program {
    static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    FibsSession fibs;
    Dictionary<int, string> TaskTags = new Dictionary<int, string>();

    Task<string> GetConsoleInput() {
      var task = Console.In.ReadLineAsync();
      TaskTags[task.Id] = "console";
      Console.WriteLine($"task: id= {task.Id}, tag= {TaskTags[task.Id]}");
      return task;
    }

    Task<CookieMessage[]> GetFibsInput() {
      var task = fibs.ReadMessagesAsync();
      TaskTags[task.Id] = "fibs";
      Console.WriteLine($"task: id= {task.Id}, tag= {TaskTags[task.Id]}");
      return task;
    }

    async Task RunAsync(string[] args) {
      // FIBS test user
      string user = "dotnetcli";
      string pw = "dotnetcli1";

      using (fibs = new FibsSession()) {
        await fibs.Login(user, pw);

        Console.Write("> ");
        var consoleInputTask = GetConsoleInput();
        var fibsInputTask = GetFibsInput();

        while(true) {
          var task = Task.WhenAny(consoleInputTask, fibsInputTask);
          if( task.Id == consoleInputTask.Id) {
            var line = await consoleInputTask;
            await fibs.WriteLineAsync(line);
            Console.Write("> ");
            consoleInputTask = GetConsoleInput();
          }
          else if( task.Id == fibsInputTask.Id) {
            var cookieMessages = await fibsInputTask;
            // TODO: handle cookie messages
            Console.WriteLine($"cookieMessages.Length= {cookieMessages.Length}");
            fibsInputTask = GetFibsInput();
          }
          else {
            // TODO: why do I always end up here?!
            Console.WriteLine($"unknown task id= {task.Id}");
            Debug.Assert(false);
          }
        }
      }
    }

  }
}
