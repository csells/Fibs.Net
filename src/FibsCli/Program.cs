using System.Threading.Tasks;

namespace Fibs {
  class Program {
    static void Main(string[] args) {
      (new Program()).RunAsync(args).GetAwaiter().GetResult();
    }

    // FIBS test user
    string user = "dotnetcli";
    string pw = "dotnetcli1";
    FibsSession fibs;

    async Task RunAsync(string[] args) {
      using (fibs = new FibsSession()) {
        await fibs.Login(user, pw);
      }
    }
  }
}
