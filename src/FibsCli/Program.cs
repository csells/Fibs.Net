using System;
using System.Threading.Tasks;

namespace Fibs {
  public static class AsyncHacks {
    public static void Sync(this Task task) { task.GetAwaiter().GetResult(); }
    public static T Sync<T>(this Task<T> task) => task.GetAwaiter().GetResult();
  }

  class Program {
    static void Main(string[] args) {
      // FIBS test user
      var user = "dotnetcli";
      var pw = "dotnetcli1";

      using (var fibs = new FibsLib()) {
        fibs.Login(user, pw).Sync();
        Console.Write("[Enter]");
        Console.ReadLine();
      }
    }
  }
}
