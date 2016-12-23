using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fibs {
  public static class CookieMessageJsonConverterExtension {
    public static string ToJson(this IEnumerable<CookieMessage> messages) =>
      CookieMessageJsonConverter.ToJson(messages);
  }

  class CookieMessageJsonConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
      return objectType == typeof(CookieMessage);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      throw new NotImplementedException();
    }

    /* e.g.
    [ { "cookie": "CLIP_WELCOME",
        "crumbs": {
          "name": "myself",
          "lastLogin": "1041253132",
          "lastHost": "192.168.1.308"
        }
      },
      { "cooke": "CLIP_MOTD_BEGIN" },
      { "cookie": "FIBS_Unknown",
        "crumbs": {
          "raw": "adslkfjasdflkjasdflkjasdflkjadsf"
        }
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
        o.Add("cookie", cm.Cookie.ToString());
        if (cm.Crumbs != null) { o.Add("crumbs", crumbs); }
        o.WriteTo(writer);
      }
    }

    static CookieMessageJsonConverter Default = new CookieMessageJsonConverter();
    public static string ToJson(IEnumerable<CookieMessage> messages) =>
      JsonConvert.SerializeObject(messages, Formatting.None, Default);
  }

}
