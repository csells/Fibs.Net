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

  class PlayerInfo {
    public string Color { get; private set; }
    public string Name { get; private set; }
    public int Score { get; private set; }
    public int Bar { get; private set; }
    public int Home { get; private set; }
    public bool IsTurn { get; private set; }

    public PlayerInfo(string color, BoardCrumbs b) {
      Assert.True(color == "O" || color == "X");

      this.Color = color;
      if (CookieMonster.ParseBoardColor(b.player1Color) == this.Color) {
        Name = b.player1;
        Score = b.player1Score;
        Bar = b.player1Bar;
        Home = b.player1Home;
      }
      else {
        Name = b.player2;
        Score = b.player2Score;
        Bar = b.player2Bar;
        Home = b.player2Home;
      }

      IsTurn = CookieMonster.ParseTurnColor(b.turnColor) == Color;
    }

  }

  /* {"cookie":"FIBS_Board","crumbs":{
   *  "player1":"imlooney",
   *  "player2":"Brahms",
   *  "matchLength":"5",
   *  "player1Score":"1",
   *  "player2Score":"2",
   *  "board":"0:-1:0:0:-1:0:5:3:2:1:0:0:-4:3:0:0:0:-1:0:-4:-2:0:-2:0:1:0",
   *  "turnColor":"1",
   *  "player1Dice":"0:0",
   *  "player2Dice":"0:0",
   *  "doublingCube":"1",
   *  "player1MayDouble":"1",
   *  "player2MayDouble":"1",
   *  "wasDoubled":"0",
   *  "player1Color":"-1",
   *  "direction":"1",
   *  "player1Home":"0",
   *  "player2Home":"0",
   *  "player1Bar":"0",
   *  "player2Bar":"0",
   *  "canMove":"2",
   *  "redoubles":"0"
   * }}
   * 
   * =>
   * 
   *   +-1--2--3--4--5--6--------7--8--9-10-11-12-+ O: Brahms - score: 2
   *   | X        X     O |   |  O  O  O        X |
   *   |                O |   |  O  O           X |
   *   |                O |   |  O              X |
   *   |                O |   |                 X |
   *   |                O |   |                   |
   *   |                  |BAR|                   |v    5-point match
   *   |                  |   |                   |
   *   |                X |   |                   |
   *   |                X |   |                 O |
   *   |       X     X  X |   |                 O |
   *   | O     X     X  X |   |     X           O |
   *   +24-23-22-21-20-19-------18-17-16-15-14-13-+ X: imlooney - score: 1
   *   BAR: O-0 X-0   OFF: O-0 X-0   Cube: 1  turn: Brahms
  */
  public class FibsBoardTests {

    static string[] RenderBoard(BoardCrumbs b) {
      var lines = new StringBuilder[14];
      var playerO = new PlayerInfo("O", b);
      var playerX = new PlayerInfo("X", b);

      Assert.True(playerO.IsTurn == true || playerX.IsTurn == true || (playerO.IsTurn == false && playerX.IsTurn == false));
      var playerTurn = playerO.IsTurn ? playerO : playerX.IsTurn ? playerX : null;

      if (b.direction != 1) { return new string[] { "TODO" }; }

      // empty board for direction == 1
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
      lines[13] = new StringBuilder($"   BAR: O-{playerO.Bar} X-{playerX.Bar}   OFF: O-{playerO.Home} X-{playerX.Home}   Cube: {b.doublingCube}  turn: {playerTurn.Name}");

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

        if (pip >= 1 && pip <= 6) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[i + 1][5 + (pip - 1) * 3] = color;
          }

          if( pieces > 5 ) {
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
            lines[7 - 1][44 - (pip - 13) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else if (pip >= 19 && pip <= 24) {
          for (var i = 0; i != Math.Min(pieces, 5); ++i) {
            lines[11 - i][20 - (pip - 19) * 3] = color;
          }

          if (pieces > 5) {
            string pileup = pieces.ToString();
            lines[7 - 1][20 - (pip - 19) * 3] = pileup[0];
            Assert.True(pileup.Length == 1, "Not handling two-digit pileups");
          }
        }
        else {
          Assert.True(false, $"pip out of range: {pip}");
        }
      }

      Assert.Equal(15, piecesO + playerO.Bar + playerO.Home);
      Assert.Equal(15, piecesX + playerX.Bar + playerX.Home);

      return lines.Select(sb=>sb.ToString()).ToArray();
    }

    [Fact]
    public static void BoardCaps1Render() {
      foreach (var filename in Directory.EnumerateFiles("boardcaps", "*.json").Take(1)) {
        foreach (var line in File.ReadLines(filename).Take(1)) {
          var json = JArray.Parse(line);
          var boardCrumbs = JsonConvert.DeserializeObject<BoardCrumbs>(json[0]["crumbs"].ToString());
          var boardLines = json
            .Skip(1)
            .Select(c => c["crumbs"]["raw"].ToString())
            .ToArray();
          var renderedBoard = RenderBoard(boardCrumbs);
          for (var i = 0; i != boardLines.Length; ++i) {
            Assert.Equal(boardLines[i], renderedBoard[i]);
          }
        }
      }
    }

  }
}