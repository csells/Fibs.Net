using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fibs {
  public abstract class ClipBase {
    static Dictionary<string, ClipBase> prefixParser = new Dictionary<string, ClipBase>() {
      { "** Warning: ", new ClipWarning() },
      { "** Unknown command: ", new ClipUnknownCommand() },
      { "** Please ", new ClipPlease() },
      { "1 ", new ClipWelcome() },
      { "2 ", new ClipOwnInfo() },
      { "3", new ClipMotdBegin() },
      { "+", new ClipMotdLine() },
      { "|", new ClipMotdLine() },
      { "4", new ClipMotdEnd() },
      { "5 ", new ClipWhoInfo() },
      { "6", new ClipWhoInfoEnd() },
      { "7 ", new ClipLogin() },
      { "8 ", new ClipLogout() },
      { "9 ", new ClipMessage() },
      { "10 ", new ClipMessageDelivered() },
      { "11 ", new ClipMessageSaved() },
      { "12 ", new ClipSays() },
      { "13 ", new ClipShouts() },
      { "14 ", new ClipWhispers() },
      { "15 ", new ClipKibitzes() },
      { "16 ", new ClipYouSay() },
      { "17 ", new ClipYouShout() },
      { "18 ", new ClipYouWhisper() },
      { "19 ", new ClipYouKibitz() },
      { "", new ClipUnknown() }
    };

    public static ClipBase Parse(string s) {
      var parser = prefixParser.First(pp => pp.Key == s.Substring(0, Math.Min(s.Length, pp.Key.Length))).Value;
      return parser.CreateFromParts(s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public abstract ClipBase CreateFromParts(string[] parts);

    protected void Log(string s) {
#if DEBUG
      Console.WriteLine(s);
#endif
    }
  }

  public class ClipPlease : ClipBase {
    public string Message { get; private set; }

    // e.g. ** Please type 'toggle silent' again before you shout.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(string.Join(" ", parts, 0, 2) == "** Please");
      return new ClipPlease { Message = string.Join(" ", parts, 2, parts.Length - 2) };
    }
  }

  public class ClipUnknownCommand : ClipBase {
    public string Command { get; private set; }

    // e.g. "** Unknown command: 'fizzbuzz'"
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(string.Join(" ", parts, 0, 3) == "** Unknown command:");
      return new ClipUnknownCommand { Command = string.Join(" ", parts, 3, parts.Length - 3) };
    }
  }

  public class ClipUnknown : ClipBase {
    public string Message { get; private set; }
    public override ClipBase CreateFromParts(string[] parts) {
      var clip = new ClipUnknown { Message = string.Join(" ", parts) };
      Log($"ClipUnknown: Message= '{clip.Message}");
      return clip;
    }
  }

  public class ClipYouKibitz : ClipBase {
    public string Message { get; private set; }

    // e.g. 19 Are you sure those dice aren't loaded?
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "19");
      var clip = new ClipYouKibitz {
        Message = string.Join(" ", parts, 1, parts.Length - 1),
      };
      Log($"ClipYouKibitz: Message= {clip.Message}");
      return clip;
    }
  }

  public class ClipYouWhisper : ClipBase {
    public string Message { get; private set; }

    // e.g. 18 Hello and hope you enjoy watching this game.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "18");
      var clip = new ClipYouWhisper {
        Message = string.Join(" ", parts, 1, parts.Length - 1),
      };
      Log($"ClipYouWhisper: Message= {clip.Message}");
      return clip;
    }
  }

  public class ClipYouShout : ClipBase {
    public string Message { get; private set; }

    // e.g. 17 Watch out for someplayer.He's a Tasmanian.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "17");
      var clip = new ClipYouShout {
        Message = string.Join(" ", parts, 1, parts.Length - 1),
      };
      Log($"ClipYouShout: Message= {clip.Message}");
      return clip;
    }
  }

  public class ClipYouSay : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 16 someplayer What's this "G'Day" stuff you hick?  :-)
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "16");
      var clip = new ClipYouSay {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipYouSay: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipKibitzes : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 15 someplayer G'Day and good luck from Hobart, Australia.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "15");
      var clip = new ClipKibitzes {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipKibitzes: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipWhispers : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 14 someplayer I think he is using loaded dice  :-)
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "14");
      var clip = new ClipWhispers {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipWhispers: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipShouts : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 13 someplayer Anybody for a 5 point match?
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "13");
      var clip = new ClipShouts {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipShouts: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipSays : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 12 someplayer Do you want to play a game?
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "12");
      var clip = new ClipSays {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };
      Log($"ClipSays: Name= {clip.Name}, Message= '{clip.Message}'");
      return clip;
    }
  }

  public class ClipMessageSaved : ClipBase {
    public string Name { get; private set; }

    // e.g. 11 someplayer
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "11");
      var clip = new ClipMessageSaved {
        Name = parts[1],
      };
      Log($"ClipMessageSaved: Message= {clip.Name}");
      return clip;
    }
  }

  public class ClipMessageDelivered : ClipBase {
    public string Name { get; private set; }

    // e.g. 10 someplayer
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "10");
      var clip = new ClipMessageDelivered {
        Name = parts[1],
      };
      Log($"ClipMessageDelivered: Message= {clip.Name}");
      return clip;
    }
  }

  public class ClipMessage : ClipBase {
    public string From { get; private set; }
    public DateTime Time { get; private set; }
    public string Message { get; private set; }

    // e.g. 9 someplayer 1041253132 I'll log in at 10pm if you want to finish that game.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "9");

      var clip = new ClipMessage {
        From = parts[1],
        Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(int.Parse(parts[2])),
        Message = string.Join(" ", parts, 3, parts.Length - 3),
      };

      Log($"ClipMessage: From= {clip.From}, Time= {clip.Time.ToLocalTime()}, Message= '{clip.Message}'");
      return clip;
    }
  }

  public class ClipLogout : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 8 rbud rbud logs out.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "8");
      var clip = new ClipLogout {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipLogout: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipLogin : ClipBase {
    public string Name { get; private set; }
    public string Message { get; private set; }

    // e.g. 7 someplayer someplayer logs in.
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "7");
      var clip = new ClipLogin {
        Name = parts[1],
        Message = string.Join(" ", parts, 2, parts.Length - 2),
      };

      Log($"ClipLogin: {clip.Name} '{clip.Message}'");
      return clip;
    }
  }

  public class ClipWhoInfoEnd : ClipBase {
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "6");
      return this;
    }
  }

  // e.g. 5 mgnu_advanced someplayer - 1 0 1912.15 827 8 1040515752 192.168.143.5 3DFiBs -
  // e.g. 5 someplayer mgnu_advanced - 0 0 1418.61 23 1914 1041272421 192.168./40.3 MacFIBS someplayer@somewhere.com
  // e.g. 5 anotherplayer - - 0 0 1439.79 1262 410 1041251697 somehost.com - -
  public class ClipWhoInfo : ClipBase {
    public string Name { get; private set; }
    public string Opponent { get; private set; }
    public string Watching { get; private set; }
    public bool Ready { get; private set; }
    public bool Away { get; private set; }
    public double Rating { get; private set; }
    public int Experience { get; private set; }
    public DateTime IdleSince { get; private set; }
    public DateTime Login { get; private set; }
    public string Hostname { get; private set; }
    public string Client { get; private set; }
    public string Email { get; private set; }

    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "5");
      var clip = new ClipWhoInfo {
        Name = parts[1],
        Opponent = parts[2] == "-" ? null : parts[2],
        Watching = parts[3] == "-" ? null : parts[3],
        Ready = parts[4] == "1",
        Away = parts[5] == "1",
        Rating = float.Parse(parts[6]),
        Experience = int.Parse(parts[7]),
        IdleSince = DateTime.Now.ToUniversalTime() - TimeSpan.FromSeconds(int.Parse(parts[8])),
        Login = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(int.Parse(parts[9])),
        Hostname = parts[10],
        Client = parts[11] == "-" ? null : parts[11],
        Email = parts[12] == "-" ? null : parts[12],
      };

      Log("ClipWhoInfo:");
      Log($"\tName= {clip.Name}");
      Log($"\tOpponent= {clip.Opponent}");
      Log($"\tWatching= {clip.Watching}");
      Log($"\tReady= {clip.Ready}");
      Log($"\tAway= {clip.Away}");
      Log($"\tRating= {clip.Rating}");
      Log($"\tExperience= {clip.Experience}");
      Log($"\tIdleSince= {clip.IdleSince}");
      Log($"\tLogin= {clip.Login}");
      Log($"\tHostname= {clip.Hostname}");
      Log($"\tClient= {clip.Client}");
      Log($"\tEmail= {clip.Email}");

      return clip;
    }
  }

  public class ClipMotdLine : ClipBase {
    public string Line { get; private set; }

    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0][0] == '+' || parts[0][0] == '|');
      var clip = new ClipMotdLine {
        Line = parts[0][0] == '+'
        ? null
        : string.Join(" ", parts, 1, parts.Length - 2)
      };
      Log($"ClipMotdLine: Line= '{clip.Line}'");
      return clip;
    }
  }

  public class ClipMotdEnd : ClipBase {
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "4");
      return this;
    }
  }

  public class ClipMotdBegin : ClipBase {
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "3");
      return this;
    }
  }

  public class ClipWarning : ClipBase {
    public string Message { get; private set; }

    // e.g. "** Warning: You are already logged in."
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(string.Join(" ", parts, 0, 2) == "** Warning:");
      return new ClipWarning { Message = string.Join(" ", parts, 2, parts.Length - 2) };
    }
  }

  public class ClipWelcome : ClipBase {
    public DateTime LastLogin { get; private set; }
    public string LastHost { get; private set; }

    // e.g. "1 myself 1041253132 192.168.1.308"
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "1");

      var clip = new ClipWelcome {
        LastLogin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(int.Parse(parts[2])),
        LastHost = parts[3],
      };

      Log($"ClipWelcome: last-login= {clip.LastLogin.ToLocalTime():M/d/yyyy h:mm tt} last-host={clip.LastHost}");
      return clip;
    }
  }

  public class ClipOwnInfo : ClipBase {
    public bool AllowPip { get; private set; }
    public bool AutoBoard { get; private set; }
    public bool AutoDouble { get; private set; }
    public bool AutoMove { get; private set; }
    public bool Away { get; private set; }
    public bool Bell { get; private set; }
    public bool Crawford { get; private set; }
    public bool Double { get; private set; }
    public int Experience { get; private set; }
    public bool Greedy { get; private set; }
    public bool MoreBoards { get; private set; }
    public bool Moves { get; private set; }
    public bool Notify { get; private set; }
    public double Rating { get; private set; }
    public bool Ratings { get; private set; }
    public bool Ready { get; private set; }
    public string Redoubles { get; private set; }
    public bool Report { get; private set; }
    public bool Silent { get; private set; }
    public string Timezone { get; private set; }

    // e.g. 2 myself 1 1 0 0 0 0 1 1 2396 0 1 0 1 3457.85 0 0 0 0 0 Australia/Melbourne
    public override ClipBase CreateFromParts(string[] parts) {
      Debug.Assert(parts[0] == "2");

      var clip = new ClipOwnInfo {
        AllowPip = parts[2] == "1",
        AutoBoard = parts[3] == "1",
        AutoDouble = parts[4] == "1",
        AutoMove = parts[5] == "1",
        Away = parts[6] == "1",
        Bell = parts[7] == "1",
        Crawford = parts[8] == "1",
        Double = parts[9] == "1",
        Experience = int.Parse(parts[10]),
        Greedy = parts[11] == "1",
        MoreBoards = parts[12] == "1",
        Moves = parts[13] == "1",
        Notify = parts[14] == "1",
        Rating = double.Parse(parts[15]),
        Ratings = parts[16] == "1",
        Ready = parts[17] == "1",
        Redoubles = parts[18],
        Report = parts[19] == "1",
        Silent = parts[20] == "1",
        Timezone = parts[21],
      };

      Log($"ClipOwnInfo:");
      Log($"\tAllowPip= {clip.AllowPip}");
      Log($"\tAutoBoard= {clip.AutoBoard}");
      Log($"\tAutoDouble= {clip.AutoDouble}");
      Log($"\tAutoMove= {clip.AutoMove}");
      Log($"\tAway= {clip.Away}");
      Log($"\tBell= {clip.Bell}");
      Log($"\tCrawford= {clip.Crawford}");
      Log($"\tDouble= {clip.Double}");
      Log($"\tExperience= {clip.Experience}");
      Log($"\tGreedy= {clip.Greedy}");
      Log($"\tMoreBoards= {clip.MoreBoards}");
      Log($"\tMoves= {clip.Moves}");
      Log($"\tNotify= {clip.Notify}");
      Log($"\tRating= {clip.Rating}");
      Log($"\tRatings= {clip.Ratings}");
      Log($"\tReady= {clip.Ready}");
      Log($"\tRedoubles= {clip.Redoubles}");
      Log($"\tReport= {clip.Report}");
      Log($"\tSilent= {clip.Silent}");
      Log($"\tTimezone= {clip.Timezone}");

      return clip;
    }
  }

}
