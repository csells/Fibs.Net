using System;
using Fibs;
using Xunit;

namespace FibsTest {
  public class FibsLibTests {
    [Fact]
    public void ClipWarning() {
      var s = "** Warning: You are already logged in.";
      var clip = (ClipWarning)ClipBase.Parse(s);
      Assert.Equal("You are already logged in.", clip.Message);
    }

    [Fact]
    public void ClipUnknownCommandTest() {
      var s = "** Unknown command: 'fizzbuzz'";
      var clip = (ClipUnknownCommand)ClipBase.Parse(s);
      Assert.Equal("'fizzbuzz'", clip.Command);
    }

    [Fact]
    public void ClipPleaseTest() {
      var s = "** Please type 'toggle silent' again before you shout.";
      var clip = (ClipPlease)ClipBase.Parse(s);
      Assert.Equal("type 'toggle silent' again before you shout.", clip.Message);
    }

    [Fact]
    public void ClipWelcomeTest() {
      var s = "1 myself 1041253132 192.168.1.308";
      var clip = (ClipWelcome)ClipBase.Parse(s);
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), clip.LastLogin);
      Assert.Equal("192.168.1.308", clip.LastHost);
    }

    [Fact]
    public void ClipOwnInfoTest() {
      var s = "2 myself 1 1 0 0 0 0 1 1 2396 0 1 0 1 3457.85 0 0 0 0 0 Australia/Melbourne";
      var clip = (ClipOwnInfo)ClipBase.Parse(s);
      Assert.True(clip.AllowPip);
      Assert.True(clip.AutoBoard);
      Assert.False(clip.AutoDouble);
      Assert.False(clip.AutoMove);
      Assert.False(clip.Away);
      Assert.False(clip.Bell);
      Assert.True(clip.Crawford);
      Assert.True(clip.Double);
      Assert.Equal(2396, clip.Experience);
      Assert.False(clip.Greedy);
      Assert.True(clip.MoreBoards);
      Assert.False(clip.Moves);
      Assert.True(clip.Notify);
      Assert.Equal(3457.85, clip.Rating);
      Assert.False(clip.Ratings);
      Assert.False(clip.Ready);
      Assert.Equal("0", clip.Redoubles);
      Assert.False(clip.Report);
      Assert.False(clip.Silent);
      Assert.Equal("Australia/Melbourne", clip.Timezone);
    }

    [Fact]
    public void ClipMotdBeginTest() {
      var s = "3";
      var clip = (ClipMotdBegin)ClipBase.Parse(s);
    }

    [Fact]
    public void ClipMotdLine1Test() {
      var s = "+--------------------------------------------------------------------+";
      var clip = (ClipMotdLine)ClipBase.Parse(s);
      Assert.Null(clip.Line);
    }

    [Fact]
    public void ClipMotdLine2Test() {
      var s = "| It was a dark and stormy night in Oakland.  Outside, the rain      |";
      var clip = (ClipMotdLine)ClipBase.Parse(s);
      Assert.Equal("It was a dark and stormy night in Oakland. Outside, the rain", clip.Line);
    }

    [Fact]
    public void ClipMotdEndTest() {
      var s = "4";
      var clip = (ClipMotdEnd)ClipBase.Parse(s);
    }

    [Fact]
    public void ClipWhoInfoTest() {
      var s = "5 someplayer mgnu_advanced - 0 0 1418.61 23 1914 1041253132 192.168.40.3 MacFIBS someplayer@somewhere.com";
      var clip = (ClipWhoInfo)ClipBase.Parse(s);
      var now = DateTime.Now.ToUniversalTime();

      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("mgnu_advanced", clip.Opponent);
      Assert.Null(clip.Watching);
      Assert.False(clip.Ready);
      Assert.False(clip.Away);
      Assert.InRange(clip.Rating, 1418.60, 1418.62);
      Assert.Equal(23, clip.Experience);
      Assert.Equal((now - TimeSpan.FromSeconds(1914)).ToString(), clip.IdleSince.ToString());
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), clip.Login);
      Assert.Equal("192.168.40.3", clip.Hostname);
      Assert.Equal("MacFIBS", clip.Client);
      Assert.Equal("someplayer@somewhere.com", clip.Email);
    }

    [Fact]
    public void ClipWhoInfoEndTest() {
      var s = "6";
      var clip = (ClipWhoInfoEnd)ClipBase.Parse(s);
    }

    [Fact]
    public void ClipLoginTest() {
      var s = "7 someplayer someplayer logs in.";
      var clip = (ClipLogin)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("someplayer logs in.", clip.Message);
    }

    [Fact]
    public void ClipLogoutTest() {
      var s = "8 someplayer someplayer drops connection.";
      var clip = (ClipLogout)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("someplayer drops connection.", clip.Message);
    }

    [Fact]
    public void ClipMessageTest() {
      var s = "9 someplayer 1041253132 I'll log in at 10pm if you want to finish that game.";
      var clip = (ClipMessage)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.From);
      Assert.Equal(DateTime.Parse("12/30/2002 12:58:52 PM"), clip.Time);
      Assert.Equal("I'll log in at 10pm if you want to finish that game.", clip.Message);
    }

    [Fact]
    public void ClipMessageDeliveredTest() {
      var s = "10 someplayer";
      var clip = (ClipMessageDelivered)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
    }

    [Fact]
    public void ClipMessageSavedTest() {
      var s = "11 someplayer";
      var clip = (ClipMessageSaved)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
    }

    [Fact]
    public void ClipSaysTest() {
      var s = "12 someplayer Do you want to play a game?";
      var clip = (ClipSays)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("Do you want to play a game?", clip.Message);
    }

    [Fact]
    public void ClipShoutsTest() {
      var s = "13 someplayer Anybody for a 5 point match?";
      var clip = (ClipShouts)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("Anybody for a 5 point match?", clip.Message);
    }

    [Fact]
    public void ClipWhispersTest() {
      var s = "14 someplayer I think he is using loaded dice :-)";
      var clip = (ClipWhispers)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("I think he is using loaded dice :-)", clip.Message);
    }

    [Fact]
    public void ClipKibitzesTest() {
      var s = "15 someplayer G'Day and good luck from Hobart, Australia.";
      var clip = (ClipKibitzes)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("G'Day and good luck from Hobart, Australia.", clip.Message);
    }

    [Fact]
    public void ClipYouSayTest() {
      var s = "16 someplayer What's this \"G'Day\" stuff you hick? :-)";
      var clip = (ClipYouSay)ClipBase.Parse(s);
      Assert.Equal("someplayer", clip.Name);
      Assert.Equal("What's this \"G'Day\" stuff you hick? :-)", clip.Message);
    }

    [Fact]
    public void ClipYouShoutTest() {
      var s = "17 Watch out for someplayer. He's a Tasmanian.";
      var clip = (ClipYouShout)ClipBase.Parse(s);
      Assert.Equal("Watch out for someplayer. He's a Tasmanian.", clip.Message);
    }

    [Fact]
    public void ClipYouWhisperTest() {
      var s = "18 Hello and hope you enjoy watching this game.";
      var clip = (ClipYouWhisper)ClipBase.Parse(s);
      Assert.Equal("Hello and hope you enjoy watching this game.", clip.Message);
    }

    [Fact]
    public void ClipYouKibitzTest() {
      var s = "19 Are you sure those dice aren't loaded?";
      var clip = (ClipYouKibitz)ClipBase.Parse(s);
      Assert.Equal("Are you sure those dice aren't loaded?", clip.Message);
    }

    [Fact]
    public void ClipUnknownTest() {
      var s = "something sump something";
      var clip = (ClipUnknown)ClipBase.Parse(s);
      Assert.Equal("something sump something", clip.Message);
    }

  }
}
