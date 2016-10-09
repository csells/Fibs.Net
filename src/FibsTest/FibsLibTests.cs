using System;
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
    public void ClipWarning() {
      var monster = new CookieMonster();
      var s = "** Warning: You are already logged in.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_WARNINGAlreadyLoggedIn, cm.Cookie);
    }

    [Fact]
    public void ClipUnknownCommandTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "** Unknown command: 'fizzbuzz'";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_UnknownCommand, cm.Cookie);
      Assert.Equal("'fizzbuzz'", cm.Crumbs["command"]);
    }

    [Fact]
    public void ClipWelcomeTest() {
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
    public void ClipOwnInfoTest() {
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
    public void ClipMotdBeginTest() {
      var monster = new CookieMonster();
      var s = "3";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MOTD_BEGIN, cm.Cookie);
    }

    [Fact]
    public void ClipMotdLine1Test() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "+--------------------------------------------------------------------+";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_MOTD, cm.Cookie);
      Assert.Equal(s, cm.Raw);
    }

    [Fact]
    public void ClipMotdLine2Test() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "| It was a dark and stormy night in Oakland.  Outside, the rain      |";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_MOTD, cm.Cookie);
      Assert.Equal(s, cm.Raw);
    }

    [Fact]
    public void ClipMotdEndTest() {
      var monster = new CookieMonster();
      monster.EatCookie("3");
      var s = "4";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MOTD_END, cm.Cookie);
    }

    [Fact]
    public void ClipWhoInfoTest() {
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
    public void ClipWhoInfoEndTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "6";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_WHO_END, cm.Cookie);
    }

    [Fact]
    public void ClipLoginTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "7 someplayer someplayer logs in.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_LOGIN, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("someplayer logs in.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipLogoutTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "8 someplayer someplayer drops connection.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_LOGOUT, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("someplayer drops connection.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipMessageTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "9 someplayer 1041253132 I'll log in at 10pm if you want to finish that game.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["from"]);
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), CookieMonster.ParseTimestamp(cm.Crumbs["time"]));
      Assert.Equal("I'll log in at 10pm if you want to finish that game.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipMessageDeliveredTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "10 someplayer";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE_DELIVERED, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
    }

    [Fact]
    public void ClipMessageSavedTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "11 someplayer";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_MESSAGE_SAVED, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
    }

    [Fact]
    public void ClipSaysTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "12 someplayer Do you want to play a game?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_SAYS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("Do you want to play a game?", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipShoutsTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "13 someplayer Anybody for a 5 point match?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_SHOUTS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("Anybody for a 5 point match?", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipWhispersTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "14 someplayer I think he is using loaded dice :-)";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_WHISPERS, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("I think he is using loaded dice :-)", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipKibitzesTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "15 someplayer G'Day and good luck from Hobart, Australia.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_KIBITZES, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("G'Day and good luck from Hobart, Australia.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipYouSayTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "16 someplayer What's this \"G'Day\" stuff you hick? :-)";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_SAY, cm.Cookie);
      Assert.Equal("someplayer", cm.Crumbs["name"]);
      Assert.Equal("What's this \"G'Day\" stuff you hick? :-)", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipYouShoutTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "17 Watch out for someplayer.  He's a Tasmanian.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_SHOUT, cm.Cookie);
      Assert.Equal("Watch out for someplayer.  He's a Tasmanian.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipYouWhisperTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "18 Hello and hope you enjoy watching this game.";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_WHISPER, cm.Cookie);
      Assert.Equal("Hello and hope you enjoy watching this game.", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipYouKibitzTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "19 Are you sure those dice aren't loaded?";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.CLIP_YOU_KIBITZ, cm.Cookie);
      Assert.Equal("Are you sure those dice aren't loaded?", cm.Crumbs["message"]);
    }

    [Fact]
    public void ClipUnknownTest() {
      var monster = CreateLoggedInCookieMonster();
      var s = "something sump something";
      var cm = monster.EatCookie(s);
      Assert.Equal(FibsCookie.FIBS_Unknown, cm.Cookie);
      Assert.Equal("something sump something", cm.Raw);
    }

  }
}
