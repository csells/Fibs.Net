﻿using System;
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
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["allowPip"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["autoBoard"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["autoDouble"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["autoMove"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["away"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["bell"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["crawford"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["double"]));
      Assert.Equal(2396, int.Parse(cm.Crumbs["experience"]));
      Assert.False(CookieMonster.ParseBool(cm.Crumbs["greedy"]));
      Assert.True(CookieMonster.ParseBool(cm.Crumbs["moreBoards"]));
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

  }
}
