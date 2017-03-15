using System;
using System.IO;
using System.Linq;
using System.Text;
using Fibs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FibsTest {
  class BoardCrumbs {
    public string player1 { get; set; }
    public string player2 { get; set; }
    public int matchLength { get; set; }
    public int player1Score { get; set; }
    public int player2Score { get; set; }
    public string board { get; set; }
    public int turnColor { get; set; }
    public string player1Dice { get; set; }
    public string player2Dice { get; set; }
    public int doublingCube { get; set; }
    public int player1MayDouble { get; set; }
    public int player2MayDouble { get; set; }
    public int wasDoubled { get; set; }
    public int player1Color { get; set; }
    public int direction { get; set; }
    public int player1Home { get; set; }
    public int player2Home { get; set; }
    public int player1Bar { get; set; }
    public int player2Bar { get; set; }
    public int canMove { get; set; }
    public int redoubles { get; set; }
  }

  class Player {
    public string Color { get; private set; }
    public string Name { get; private set; }
    public int Score { get; private set; }
    public int Bar { get; private set; }
    public int Home { get; private set; }
    public int[] Dice { get; private set; }
    public bool IsTurn { get; private set; }
    public bool MayDouble { get; private set; }

    public Player(string color, BoardCrumbs b) {
      Assert.True(color == "O" || color == "X");

      this.Color = color;
      if (CookieMonster.ParseBoardColor(b.player1Color) == this.Color) {
        Name = b.player1;
        Score = b.player1Score;
        Bar = b.player1Bar;
        Home = b.player1Home;
        Dice = b.player1Dice == "0:0" ? null : b.player1Dice.Split(':').Select(s => int.Parse(s)).ToArray();
        MayDouble = b.player1MayDouble == 1;
      }
      else {
        Name = b.player2;
        Score = b.player2Score;
        Bar = b.player2Bar;
        Home = b.player2Home;
        Dice = b.player2Dice == "0:0" ? null : b.player2Dice.Split(':').Select(s => int.Parse(s)).ToArray();
        MayDouble = b.player2MayDouble == 1;
      }

      IsTurn = CookieMonster.ParseTurnColor(b.turnColor) == Color;
    }

  }

  public class FibsBoardTests {
    static string RenderBoard(BoardCrumbs b) {
      var lines = new StringBuilder[14];
      var playerO = new Player("O", b);
      var playerX = new Player("X", b);

      var playerTurn = playerO.IsTurn ? playerO : playerX.IsTurn ? playerX : null;
      var turn = playerTurn.Dice == null ? $"turn: {playerTurn.Name}" : $"{playerTurn.Name} rolled {playerTurn.Dice[0]} {playerTurn.Dice[1]}.";
      var playerCube = b.doublingCube == 1 ? null : playerO.MayDouble ? playerO : playerX.MayDouble ? playerX : null;
      var cube = playerCube == null ? $"{b.doublingCube}" : $"{b.doublingCube} (owned by {playerCube.Name})";

      Assert.True(playerO.IsTurn == true || playerX.IsTurn == true || (playerO.IsTurn == false && playerX.IsTurn == false));
      Assert.True(playerO.Dice == null || playerX.Dice == null || (playerO.Dice == null && playerX.Dice == null));

      switch (b.direction) {
        case 1: RenderBoardForwards(b, lines, playerO, playerX, turn, cube); break;
        case -1: RenderBoardBackwards(b, lines, playerO, playerX, turn, cube); break;
        default: Assert.True(false, $"unknown direction= {b.direction}"); break;
      }

      return lines.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s.ToString())).ToString();
    }

    static void RenderBoardBackwards(BoardCrumbs b, StringBuilder[] lines, Player playerO, Player playerX, string turn, string cube) {
      Assert.Equal(b.direction, -1);

      lines[00] = new StringBuilder($"   +13-14-15-16-17-18-------19-20-21-22-23-24-+ X: {playerX.Name} - score: {playerX.Score}");
      lines[01] = new StringBuilder($"   |                  |   |                   |");
      lines[02] = new StringBuilder($"   |                  |   |                   |");
      lines[03] = new StringBuilder($"   |                  |   |                   |");
      lines[04] = new StringBuilder($"   |                  |   |                   |");
      lines[05] = new StringBuilder($"   |                  |   |                   |");
      lines[06] = new StringBuilder($"  v|                  |BAR|                   |    {b.matchLength}-point match");
      lines[07] = new StringBuilder($"   |                  |   |                   |");
      lines[08] = new StringBuilder($"   |                  |   |                   |");
      lines[09] = new StringBuilder($"   |                  |   |                   |");
      lines[10] = new StringBuilder($"   |                  |   |                   |");
      lines[11] = new StringBuilder($"   |                  |   |                   |");
      lines[12] = new StringBuilder($"   +12-11-10--9--8--7--------6--5--4--3--2--1-+ O: {playerO.Name} - score: {playerO.Score}");
      lines[13] = new StringBuilder($"   BAR: O-{playerO.Bar} X-{playerX.Bar}   OFF: O-{playerO.Home} X-{playerX.Home}   Cube: {cube}  {turn}");

      // place the pieces
      int[] pipPieces = b.board.Split(':').Select(s => int.Parse(s)).ToArray();
      Assert.Equal(26, pipPieces.Length);
      var piecesO = 0;
      var piecesX = 0;
      for (var pip = 1; pip != 25; ++pip) {
        int pieces = pipPieces[pip];
        char color = ' ';

        if (pieces > 0) { color = 'O'; piecesO += pieces; }
        else if (pieces < 0) { color = 'X'; pieces = -pieces; piecesX += pieces; }
        else { Assert.Equal(0, pieces); continue; }

        if (pip >= 13 && pip <= 18) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[i + 1][5 + (pip - 13) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[4 + 1][5 + (pip - 13) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 19 && pip <= 24) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[i + 1][29 + (pip - 19) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[4 + 1][29 + (pip - 19) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 1 && pip <= 6) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[11 - i][44 - (pip - 1) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[8 - 1][44 - (pip - 1) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 7 && pip <= 12) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[11 - i][20 - (pip - 7) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[8 - 1][20 - (pip - 7) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else {
          Assert.True(false, $"pip out of range: {pip}");
        }
      }

      // place the pieces on the bar (direction == 1)
      Assert.True(playerO.Bar >= 0);
      for (var o = 0; o != Math.Min(playerO.Bar, 5); ++o) {
        lines[11 - o][24] = 'O';
      }

      if (playerO.Bar > 5) {
        string pileup = playerO.Bar.ToString();
        lines[7 - 1][24] = pileup[0];
        Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
      }

      Assert.True(playerX.Bar >= 0);
      for (var x = 0; x != Math.Min(playerX.Bar, 5); ++x) {
        lines[x + 1][24] = 'X';
      }

      if (playerX.Bar > 5) {
        string pileup = playerX.Bar.ToString();
        lines[4 + 1][24] = pileup[0];
        Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
      }

      Assert.Equal(15, piecesO + playerO.Bar + playerO.Home);
      Assert.Equal(15, piecesX + playerX.Bar + playerX.Home);
    }

    static void RenderBoardForwards(BoardCrumbs b, StringBuilder[] lines, Player playerO, Player playerX, string turn, string cube) {
      Assert.Equal(b.direction, 1);

      lines[00] = new StringBuilder($"   +-1--2--3--4--5--6--------7--8--9-10-11-12-+ O: {playerO.Name} - score: {playerO.Score}");
      lines[01] = new StringBuilder($"   |                  |   |                   |");
      lines[02] = new StringBuilder($"   |                  |   |                   |");
      lines[03] = new StringBuilder($"   |                  |   |                   |");
      lines[04] = new StringBuilder($"   |                  |   |                   |");
      lines[05] = new StringBuilder($"   |                  |   |                   |");
      lines[06] = new StringBuilder($"   |                  |BAR|                   |v    {b.matchLength}-point match");
      lines[07] = new StringBuilder($"   |                  |   |                   |");
      lines[08] = new StringBuilder($"   |                  |   |                   |");
      lines[09] = new StringBuilder($"   |                  |   |                   |");
      lines[10] = new StringBuilder($"   |                  |   |                   |");
      lines[11] = new StringBuilder($"   |                  |   |                   |");
      lines[12] = new StringBuilder($"   +24-23-22-21-20-19-------18-17-16-15-14-13-+ X: {playerX.Name} - score: {playerX.Score}");
      lines[13] = new StringBuilder($"   BAR: O-{playerO.Bar} X-{playerX.Bar}   OFF: O-{playerO.Home} X-{playerX.Home}   Cube: {cube}  {turn}");

      // place the pieces for direction == 1
      int[] pipPieces = b.board.Split(':').Select(s => int.Parse(s)).ToArray();
      Assert.Equal(26, pipPieces.Length);
      var piecesO = 0;
      var piecesX = 0;
      for (var pip = 1; pip != 25; ++pip) {
        int pieces = pipPieces[pip];
        char color = ' ';

        if (pieces > 0) { color = 'O'; piecesO += pieces; }
        else if (pieces < 0) { color = 'X'; pieces = -pieces; piecesX += pieces; }
        else { Assert.Equal(0, pieces); continue; }

        if (pip >= 1 && pip <= 6) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[i + 1][5 + (pip - 1) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[4 + 1][5 + (pip - 1) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 7 && pip <= 12) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[i + 1][29 + (pip - 7) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[4 + 1][29 + (pip - 7) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 13 && pip <= 18) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[11 - i][44 - (pip - 13) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[8 - 1][44 - (pip - 13) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 19 && pip <= 24) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[11 - i][20 - (pip - 19) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[8 - 1][20 - (pip - 19) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else {
          Assert.True(false, $"pip out of range: {pip}");
        }
      }

      // place the pieces on the bar (direction == 1)
      Assert.True(playerO.Bar >= 0);
      for (var o = 0; o != Math.Min(playerO.Bar, 5); ++o) {
        lines[o + 1][24] = 'O';
      }

      if (playerO.Bar > 5) {
        string pileup = playerO.Bar.ToString();
        lines[4 + 1][24] = pileup[0];
        Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
      }

      Assert.True(playerX.Bar >= 0);
      for (var x = 0; x != Math.Min(playerX.Bar, 5); ++x) {
        lines[11 - x][24] = 'X';
      }

      if (playerX.Bar > 5) {
        string pileup = playerX.Bar.ToString();
        lines[7 - 1][24] = pileup[0];
        Assert.True(pileup.Length == 1, "Nxt handling twx-digit pileups");
      }

      Assert.Equal(15, piecesO + playerO.Bar + playerO.Home);
      Assert.Equal(15, piecesX + playerX.Bar + playerX.Home);
    }

    [Fact]
    public static void BoardCapsRender() {
      string boardLinesPath = Path.Combine(Path.GetTempPath(), "boardLines.txt");
      string renderedBoardPath = Path.Combine(Path.GetTempPath(), "renderedBoard.txt");
      // Clean out files left over from earlier test runs.
      File.Delete(boardLinesPath);
      File.Delete(renderedBoardPath);

      foreach (var filename in Directory.EnumerateFiles("boardcaps", "*.json")) {
        var lines = File.ReadLines(filename).ToArray();
        for (var i = 0; i != lines.Length; ++i) {
          if (lines[i][0] == '#') { continue; } // skip commented out lines
          var json = JArray.Parse(lines[i]);
          var boardCrumbs = JsonConvert.DeserializeObject<BoardCrumbs>(json[0]["crumbs"].ToString());
          var boardLines = json
            .Skip(1)
            .Select(c => c["crumbs"]["raw"].ToString())
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s))
            .ToString();
          var renderedBoard = RenderBoard(boardCrumbs);
          if (boardLines != renderedBoard) {
            File.WriteAllText(boardLinesPath, boardLines);
            File.WriteAllText(renderedBoardPath, renderedBoard);
            Assert.True(false, $"file= {filename}, line= {i + 1}");
          }
        }
      }
    }
  }

}
