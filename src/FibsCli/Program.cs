﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            Console.WriteLine($"messages.Length= {messages.Length}");
            // TODO: handle this -- probably getting input in the middle of a line...
            //Debug.Assert(!messages.Any(m => m.Cookie == FibsCookie.FIBS_Unknown));
            var json = ToJson(messages);
            Console.WriteLine("json= " + json);
            fibsInputTask = GetFibsInput();
            Prompt();
          }
          else {
            Debug.Assert(false);
          }
        }
      }
    }

    class CookieMessageJsonConverter : JsonConverter {
      public override bool CanConvert(Type objectType) {
        return objectType == typeof(CookieMessage);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
      }

      /* e.g.
      [
        {
          "message": "CLIP_WELCOME",
          "args": {
            "name": "myself",
            "lastLogin": "1041253132",
            "lastHost": "192.168.1.308"
          }
        },
        {
          "message": "CLIP_MOTD_BEGIN"
        },
        {
          "message": "FIBS_Unknown",
          "raw": "adslkfjasdflkjasdflkjasdflkjadsf"
        }
      ]
      */
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        JToken t = JToken.FromObject(value);
        if (t.Type != JTokenType.Object) {
          t.WriteTo(writer);
        }
        else {
          var cm = (CookieMessage)value;
          JObject o = (JObject)t;
          var crumbs = o.GetValue("Crumbs");
          o.RemoveAll();
          o.Add("message", cm.Cookie.ToString());
          if (cm.Crumbs != null) { o.Add("args", crumbs); }
          if (cm.Cookie == FibsCookie.FIBS_Unknown) { o.Add("raw", cm.Raw); }
          o.WriteTo(writer);
        }
      }
    }

    string ToJson(IEnumerable<CookieMessage> messages) =>
      JsonConvert.SerializeObject(messages, Formatting.Indented, new CookieMessageJsonConverter());
  }
}