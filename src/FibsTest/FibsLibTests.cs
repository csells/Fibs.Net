using System;
using System.Collections.Generic;
using Fibs;
using Xunit;

namespace FibsTest {
  public class FibsLibTests {
    CookieMonster CreateLoggedInCookieMonster() {
      var monster = new CookieMonster();
      monster.EatCookie("3"); // simulate MOTD
      monster.EatCookie("4"); // simulate MOTD end
      return monster;
    }

    [Fact]
    public void FIBS_WARNINGAlreadyLoggedIn() {
      var monster = new CookieMonster();
      var s = "** Warning: You are already logged in.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_WARNINGAlreadyLoggedIn, cm.Cookie);
    }

    [Fact]
    public void FIBS_UnknownCommand() {
      var monster = CreateLoggedInCookieMonster();
      var s = "** Unknown command: 'fizzbuzz'";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_UnknownCommand, cm.Cookie);
      Assert.Equal("'fizzbuzz'", cm.Crumbs["command"]);
    }

    [Fact]
    public void CLIP_WELCOME() {
      var monster = new CookieMonster();
      var s = "1 myself 1041253132 192.168.1.308";
      var cm = monster.EatCookie(s);

      Assert.Equal(FibsCookie.CLIP_WELCOME, cm.Cookie);
      Assert.Equal("myself", cm.Crumbs["name"]);
      var lastLogin = CookieMonster.ParseTimestamp(cm.Crumbs["lastLogin"]);
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), lastLogin);
      Assert.Equal("192.168.1.308", cm.Crumbs["lastHost"]);
    }

    [Fact]
    public void CLIP_OWN_INFO() {
      var monster = new CookieMonster();
      var s = "2 myself 1 1 0 0 0 0 1 1 2396 0 1 0 1 3457.85 0 0 0 0 0 Australia/Melbourne";
      var cm = monster.EatCookie(s);

      Assert.Equal(FibsCookie.CLIP_OWN_INFO, cm.Cookie);
      Assert.Equal("myself", cm.Crumbs["name"]);
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["allowpip"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["autoboard"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["autodouble"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["automove"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["away"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["bell"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["crawford"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["double"]));
      Assert.Equal(2396, int.Parse(cm.Crumbs["experience"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["greedy"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["moreboards"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["moves"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["notify"]));
      Assert.Equal(3457.85, double.Parse(cm.Crumbs["rating"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["ratings"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["ready"]));
      Assert.Equal("0", cm.Crumbs["redoubles"]);
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["report"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["silent"]));
      Assert.Equal("Australia/Melbourne", cm.Crumbs["timezone"]);
    }

    [Fact]
    public void CLIP_MOTD_BEGIN() {
      var monster = new CookieMonster();
      var s = "3";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MOTD_BEGIN, cm.Cookie);
    }

    [Fact]
    public void FIBS_MOTD1() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "+--------------------------------------------------------------------+";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_MOTD, cm.Cookie);
      Assert.Equal(s, cm.Raw);
    }

    [Fact]
    public void FIBS_MOTD2() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "| It was a dark and stormy night in Oakland.  Outside, the rain      |";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_MOTD, cm.Cookie);
      Assert.Equal(s, cm.Raw);
    }

    [Fact]
    public void CLIP_MOTD_END() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "4";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MOTD_END, cm.Cookie);
    }

    [Fact]
    public void CLIP_WHO_INFO() {
      var monster = CreateLoggedInCookieMonster();
      var s = "5 someplayer mgnu_advanced - 0 0 1418.61 23 1914 1041253132 192.168.40.3 MacFIBS someplayer@somewhere.com";
      var cm = monster.EatCookie(s);

      Assert.Equal(FibsCookie.CLIP_WHO_INFO, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("mgnu_advanced", cm.Crumbs["opponent"]);
      Assert.Null(CookieMonster.ParseOptional(cm.Crumbs["watching"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["ready"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["away"]));
      Assert.InRange(double.Parse(cm.Crumbs["rating"]), 1418.60, 1418.62);
      Assert.Equal(23, int.Parse(cm.Crumbs["experience"]));
      Assert.Equal(1914, int.Parse(cm.Crumbs["idle"]));
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), CookieMonster.ParseTimestamp(cm.Crumbs["login"]));
      Assert.Equal("192.168.40.3", cm.Crumbs["hostName"]);
      Assert.Equal("MacFIBS", CookieMonster.ParseOptional(cm.Crumbs["client"]));
      Assert.Equal("someplayer@somewhere.com", CookieMonster.ParseOptional(cm.Crumbs["email"]));
    }

    [Fact]
    public void CLIP_WHO_END() {
      var monster = CreateLoggedInCookieMonster();
      var s = "6";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_WHO_END, cm.Cookie);
    }

    [Fact]
    public void CLIP_LOGIN() {
      var monster = CreateLoggedInCookieMonster();
      var s = "7 someplayer someplayer logs in.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_LOGIN, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("someplayer logs in.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_LOGOUT() {
      var monster = CreateLoggedInCookieMonster();
      var s = "8 someplayer someplayer drops connection.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_LOGOUT, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("someplayer drops connection.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_MESSAGE() {
      var monster = CreateLoggedInCookieMonster();
      var s = "9 someplayer 1041253132 I'll log in at 10pm if you want to finish that game.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["from"]);
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), CookieMonster.ParseTimestamp(cm.Crumbs["time"]));
      Assert.Equal("I'll log in at 10pm if you want to finish that game.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_MESSAGE_DELIVERED() {
      var monster = CreateLoggedInCookieMonster();
      var s = "10 someplayer";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE_DELIVERED, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
    }

    [Fact]
    public void CLIP_MESSAGE_SAVED() {
      var monster = CreateLoggedInCookieMonster();
      var s = "11 someplayer";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE_SAVED, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
    }

    [Fact]
    public void CLIP_SAYS() {
      var monster = CreateLoggedInCookieMonster();
      var s = "12 someplayer Do you want to play a game?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_SAYS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("Do you want to play a game?", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_SHOUTS() {
      var monster = CreateLoggedInCookieMonster();
      var s = "13 someplayer Anybody for a 5 point match?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_SHOUTS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("Anybody for a 5 point match?", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_WHISPERS() {
      var monster = CreateLoggedInCookieMonster();
      var s = "14 someplayer I think he is using loaded dice :-)";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_WHISPERS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("I think he is using loaded dice :-)", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_KIBITZES() {
      var monster = CreateLoggedInCookieMonster();
      var s = "15 someplayer G'Day and good luck from Hobart, Australia.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_KIBITZES, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("G'Day and good luck from Hobart, Australia.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_YOU_SAY() {
      var monster = CreateLoggedInCookieMonster();
      var s = "16 someplayer What's this \"G'Day\" stuff you hick? :-)";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_SAY, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("What's this \"G'Day\" stuff you hick? :-)", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_YOU_SHOUT() {
      var monster = CreateLoggedInCookieMonster();
      var s = "17 Watch out for someplayer.  He's a Tasmanian.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_SHOUT, cm.Cookie);
      Assert.Equal("Watch out for someplayer.  He's a Tasmanian.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_YOU_WHISPER() {
      var monster = CreateLoggedInCookieMonster();
      var s = "18 Hello and hope you enjoy watching this game.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_WHISPER, cm.Cookie);
      Assert.Equal("Hello and hope you enjoy watching this game.", cm.Crumbs["message"]);
    }

    [Fact]
    public void CLIP_YOU_KIBITZ() {
      var monster = CreateLoggedInCookieMonster();
      var s = "19 Are you sure those dice aren't loaded?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_KIBITZ, cm.Cookie);
      Assert.Equal("Are you sure those dice aren't loaded?", cm.Crumbs["message"]);
    }

    [Fact]
    public void IBS_Unknown() {
      var monster = CreateLoggedInCookieMonster();
      var s = "something sump something";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_Unknown, cm.Cookie);
      Assert.Equal("something sump something", cm.Raw);
    }

    [Fact]
    public void FIBS_PlayerLeftGame() {
      var monster = CreateLoggedInCookieMonster();
      var s = "bob has left the game with alice.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_PlayerLeftGame, cm.Cookie);
      Assert.Equal("bob", cm.Crumbs["player1"]);
      Assert.Equal("alice", cm.Crumbs["player2"]);
    }

    [Fact]
    public void FIBS_PreLogin() {
      var monster = new CookieMonster();
      var s = "Saturday, October 15 17:01:02 MEST   ( Sat Oct 15 15:01:02 2016 UTC )";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_PreLogin, cm.Cookie);
      Assert.Equal(s, cm.Crumbs["message"]);
    }

    [Fact]
    public void FIBS_Goodbye() {
      var monster = CreateLoggedInCookieMonster();
      var s = "           Goodbye.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_Goodbye, cm.Cookie);
      Assert.Equal(s, cm.Crumbs["message"]);
    }

    [Fact]
    public void FIBS_PostGoodbye() {
      var monster = CreateLoggedInCookieMonster();
      monster.EatCookie("           Goodbye.");
      var s = "If you enjoyed using this server please send picture postcards,";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_PostGoodbye, cm.Cookie);
      Assert.Equal(s, cm.Crumbs["message"]);
    }

    [Fact]
    public void FIBS_MatchResult() {
      var monster = CreateLoggedInCookieMonster();
      var s = "BlunderBot wins a 1 point match against LunaRossa  1-0 .";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_MatchResult, cm.Cookie);
      Assert.Equal("BlunderBot", cm.Crumbs["winner"]);
      Assert.Equal("LunaRossa", cm.Crumbs["loser"]);
      Assert.Equal(1, int.Parse(cm.Crumbs["points"]));
      Assert.Equal(1, int.Parse(cm.Crumbs["winnerScore"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["loserScore"]));
    }

    [Fact]
    public void FIBS_PlayersStartingMatch() {
      var monster = CreateLoggedInCookieMonster();
      var s = "BlunderBot_IV and eggieegg start a 1 point match.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_PlayersStartingMatch, cm.Cookie);
      Assert.Equal("BlunderBot_IV", cm.Crumbs["player1"]);
      Assert.Equal("eggieegg", cm.Crumbs["player2"]);
      Assert.Equal(1, int.Parse(cm.Crumbs["points"]));
    }

    [Fact]
    public void FIBS_ResumingLimitedMatch() {
      var monster = CreateLoggedInCookieMonster();
      var s = "inim and utah are resuming their 2-point match.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_ResumingLimitedMatch, cm.Cookie);
      Assert.Equal("inim", cm.Crumbs["player1"]);
      Assert.Equal("utah", cm.Crumbs["player2"]);
      Assert.Equal(2, int.Parse(cm.Crumbs["points"]));
    }

    [Fact]
    public void FIBS_NoOne() {
      var monster = CreateLoggedInCookieMonster();
      var s = "** There is no one called playerOne.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_NoOne, cm.Cookie);
      Assert.Equal("playerOne", cm.Crumbs["name"]);
    }

    [Fact]
    public void FIBS_SettingsValueYes() {
      var monster = CreateLoggedInCookieMonster();
      var s = "allowpip        YES";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsValue, cm.Cookie);
      Assert.Equal("allowpip", cm.Crumbs["name"]);
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["value"]));
    }

    [Fact]
    public void FIBS_SettingsValueNo() {
      var monster = CreateLoggedInCookieMonster();
      var s = "autodouble      NO";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsValue, cm.Cookie);
      Assert.Equal("autodouble", cm.Crumbs["name"]);
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["value"]));
    }

    [Fact]
    public void FIBS_SettingsYoureNotAway() {
      var monster = CreateLoggedInCookieMonster();
      var s = "** You're not away.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsValue, cm.Cookie);
      Assert.Equal("away", cm.Crumbs["name"]);
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["value"]));
    }

    [Fact]
    public void FIBS_SettingsValueChangeToYes() {
      var monster = CreateLoggedInCookieMonster();
      var settingPhrases = new Dictionary<string, string> {
        ["allowpip"] = "** You allow the use the server's 'pip' command.",
        ["autoboard"] = "** The board will be refreshed after every move.",
        ["autodouble"] = "** You agree that doublets during opening double the cube.",
        ["automove"] = "** Forced moves will be done automatically.",
        ["away"] = "You're away. Please type 'back'",
        ["bell"] = "** Your terminal will ring the bell if someone talks to you or invites you",
        ["crawford"] = "** You insist on playing with the Crawford rule.",
        ["double"] = "** You will be asked if you want to double.",
        ["greedy"] = "** Will use automatic greedy bearoffs.",
        ["moreboards"] = "** Will send rawboards after rolling.",
        ["moves"] = "** You want a list of moves after this game.",
        ["notify"] = "** You'll be notified when new users log in.",
        ["ratings"] = "** You'll see how the rating changes are calculated.",
        ["ready"] = "** You're now ready to invite or join someone.",
        ["report"] = "** You will be informed about starting and ending matches.",
        ["silent"] = "** You will hear what other players shout.",
        ["telnet"] = "** You use telnet and don't need extra 'newlines'.",
        ["wrap"] = "** The server will wrap long lines.",
      };

      foreach (var pair in settingPhrases) {
        var cm = monster.EatCookie(pair.Value);
        Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
        Assert.Equal(pair.Key, cm.Crumbs["name"]);
        Assert.True(CookieMonster.ParseBool(cm.Crumbs["value"]), $"{cm.Crumbs["name"]}= {cm.Crumbs["value"]}");
      }
    }

    [Fact]
    public void FIBS_SettingsValueChangeToNo() {
      var monster = CreateLoggedInCookieMonster();
      var settingPhrases = new Dictionary<string, string> {
        ["allowpip"] = "** You don't allow the use of the server's 'pip' command.",
        ["autoboard"] = "** The board won't be refreshed after every move.",
        ["autodouble"] = "** You don't agree that doublets during opening double the cube.",
        ["automove"] = "** Forced moves won't be done automatically.",
        ["away"] = "Welcome back.",
        ["bell"] = "** Your terminal won't ring the bell if someone talks to you or invites you",
        ["crawford"] = "** You would like to play without using the Crawford rule.",
        ["double"] = "** You won't be asked if you want to double.",
        ["greedy"] = "** Won't use automatic greedy bearoffs.",
        ["moreboards"] = "** Won't send rawboards after rolling.",
        ["moves"] = "** You won't see a list of moves after this game.",
        ["notify"] = "** You won't be notified when new users log in.",
        ["ratings"] = "** You won't see how the rating changes are calculated.",
        ["ready"] = "** You're now refusing to play with someone.",
        ["report"] = "** You won't be informed about starting and ending matches.",
        ["silent"] = "** You won't hear what other players shout.",
        ["telnet"] = "** You use a client program and will receive extra 'newlines'.",
        ["wrap"] = "** Your terminal knows how to wrap long lines.",
      };

      foreach (var pair in settingPhrases) {
        var cm = monster.EatCookie(pair.Value);
        Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
        Assert.Equal(pair.Key, cm.Crumbs["name"]);
        Assert.False(CookieMonster.ParseBool(cm.Crumbs["value"]), $"{cm.Crumbs["name"]}= {cm.Crumbs["value"]}");
      }
    }

    [Fact]
    public void FIBS_RedoublesChangeToNone() {
      var monster = CreateLoggedInCookieMonster();
      var s = "Value of 'redoubles' set to 'none'.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
      Assert.Equal("redoubles", cm.Crumbs["name"]);
      Assert.Equal(0, int.Parse(cm.Crumbs["value"]));
    }

    public void FIBS_RedoublesChangeToNumber() {
      var monster = CreateLoggedInCookieMonster();
      var s = "Value of 'redoubles' set to 42.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
      Assert.Equal("redoubles", cm.Crumbs["name"]);
      Assert.Equal(42, int.Parse(cm.Crumbs["value"]));
    }

    [Fact]
    public void FIBS_RedoublesChangeToUnlimited() {
      var monster = CreateLoggedInCookieMonster();
      var s = "Value of 'redoubles' set to 'unlimited'.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
      Assert.Equal("redoubles", cm.Crumbs["name"]);
      Assert.Equal("unlimited", cm.Crumbs["value"]);
    }

    public void FIBS_TimezoneChange() {
      var monster = CreateLoggedInCookieMonster();
      var s = "Value of 'timezone' set to America/Los_Angeles.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_SettingsChange, cm.Cookie);
      Assert.Equal("timezone", cm.Crumbs["name"]);
      Assert.Equal("America/Los_Angeles", cm.Crumbs["value"]);
    }

    [Fact]
    public void FIBS_Board() {
      // from http://www.fibs.com/fibs_interface.html#board_state
      var monster = CreateLoggedInCookieMonster();
      var s = "board:You:someplayer:3:0:1:0:-2:0:0:0:0:5:0:3:0:0:0:-5:5:0:0:0:-3:0:-5:0:0:0:0:2:0:1:6:2:0:0:1:1:1:0:1:-1:0:25:0:0:0:0:2:0:0:0";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_Board, cm.Cookie);
      Assert.Equal("You", cm.Crumbs["player1"]);
      Assert.Equal("someplayer", cm.Crumbs["player2"]);
      Assert.Equal(3, int.Parse(cm.Crumbs["matchLength"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["player1Score"]));
      Assert.Equal(1, int.Parse(cm.Crumbs["player2Score"]));
      Assert.Equal("0:-2:0:0:0:0:5:0:3:0:0:0:-5:5:0:0:0:-3:0:-5:0:0:0:0:2:0", cm.Crumbs["board"]);
      Assert.Equal("O", CookieMonster.ParseBoardTurn(cm.Crumbs["turnColor"]));
      Assert.Equal("6:2", cm.Crumbs["player1Dice"]);
      Assert.Equal("0:0", cm.Crumbs["player2Dice"]);
      Assert.Equal(1, int.Parse(cm.Crumbs["doublingCube"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["player1MayDouble"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["player2MayDouble"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["wasDoubled"]));
      Assert.Equal("O", CookieMonster.ParseBoardColor(cm.Crumbs["player1Color"]));
      Assert.Equal(-1, int.Parse(cm.Crumbs["direction"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["player1Home"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["player2Home"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["player1Bar"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["player2Bar"]));
      Assert.Equal(2, int.Parse(cm.Crumbs["canMove"]));
      Assert.Equal(0, int.Parse(cm.Crumbs["redoubles"]));
    }

  }
}
