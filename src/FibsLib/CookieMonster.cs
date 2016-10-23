// From http://www.fibs.com/fcm/
// FIBS Client Protocol Detailed Specification: http://www.fibs.com/fibs_interface.html
/*
 *---  FIBSCookieMonster.c --------------------------------------------------
 *
 *  Created by Paul Ferguson on Tue Dec 24 2002.
 *  Copyright (c) 2003 Paul Ferguson. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 *
 * * Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the distribution.
 *
 * * The name of Paul D. Ferguson may not be used to endorse or promote
 *   products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *---------------------------------------------------------------------------
 * Oct, 2016, csells ported this to C# as part of Fibs.Net, added some useful features
 * http://github.com/csells/fibs.net
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fibs {
  public class CookieMessage {
    public FibsCookie Cookie { get; private set; }
    public string Raw { get; private set; }
    public Dictionary<string, string> Crumbs { get; private set; }
    public CookieMonster.States EatState { get; private set; }

    public CookieMessage(FibsCookie cookie, string raw, Dictionary<string, string> crumbs, CookieMonster.States eatState) {
      // Cannot have zero-length crumb dictionary. Pass null instead.
      Debug.Assert(crumbs == null || crumbs.Count > 0);

      this.Cookie = cookie;
      this.Raw = raw;
      this.Crumbs = crumbs;
      this.EatState = eatState;
    }
  }

  public class CookieMonster {
    // A simple state model
    public enum States {
      FIBS_LOGIN_STATE,
      FIBS_MOTD_STATE,
      FIBS_RUN_STATE,
      FIBS_LOGOUT_STATE
    }

    States MessageState = States.FIBS_LOGIN_STATE;
    States? OldMessageState;

    // Principle data structure. Used internally--clients never see the dough,
    // just the finished cookie.
    class CookieDough {
      public FibsCookie Cookie;
      public Regex Regex;
    };

    static CookieMessage MakeCookie(List<CookieDough> batch, string raw, States eatState) {
      foreach (var dough in batch) {
        var match = dough.Regex.Match(raw);
        if (match.Success) {
          var crumbs = new Dictionary<string, string>();
          var namedGroups = dough.Regex.GetGroupNames().Where(n => !char.IsDigit(n[0]));
          foreach (var name in namedGroups) {
            var value = match.Groups[name].Value;
            crumbs.Add(name, value);

            // only "message" values are allowed to be empty
            Debug.Assert((name == "message") || value != "", $"{dough.Cookie}: missing crumb '{name}'");
          }

          return new CookieMessage(dough.Cookie, raw, crumbs.Count == 0 ? null : crumbs, eatState);
        }
      }

      return null;
    }

    // Returns a cookie message
    // NOTE: The incoming FIBS message should NOT include line terminators.
    public CookieMessage EatCookie(string raw) {
      var eatState = MessageState;
      CookieMessage cm = null;

      switch (MessageState) {
        case States.FIBS_RUN_STATE:
          if (string.IsNullOrEmpty(raw)) {
            cm = new CookieMessage(FibsCookie.FIBS_Empty, raw, null, eatState);
            break;
          }

          char ch = raw[0];
          // CLIP messages and miscellaneous numeric messages
          if (char.IsDigit(ch)) {
            cm = MakeCookie(NumericBatch, raw, eatState);
          }
          // '** ' messages
          else if (ch == '*') {
            cm = MakeCookie(StarsBatch, raw, eatState);
          }
          // all other messages
          else {
            cm = MakeCookie(AlphaBatch, raw, eatState);
          }

          if (cm != null && cm.Cookie == FibsCookie.FIBS_Goodbye) {
            MessageState = States.FIBS_LOGOUT_STATE;
          }
          break;

        case States.FIBS_LOGIN_STATE:
          cm = MakeCookie(LoginBatch, raw, eatState);
          Debug.Assert(cm != null); // there's a catch all
          if (cm.Cookie == FibsCookie.CLIP_MOTD_BEGIN) {
            MessageState = States.FIBS_MOTD_STATE;
          }
          break;

        case States.FIBS_MOTD_STATE:
          cm = MakeCookie(MOTDBatch, raw, eatState);
          Debug.Assert(cm != null); // there's a catch all
          if (cm.Cookie == FibsCookie.CLIP_MOTD_END) {
            MessageState = States.FIBS_RUN_STATE;
          }
          break;

        case States.FIBS_LOGOUT_STATE:
          cm = new CookieMessage(FibsCookie.FIBS_PostGoodbye, raw, new Dictionary<string, string> { { "message", raw } }, eatState);
          break;

        default:
          throw new System.Exception($"Unknown state: {MessageState}");
      }

      if (cm == null) { cm = new CookieMessage(FibsCookie.FIBS_Unknown, raw, null, eatState); }

#if true
      // output the initial state if no state has been shown at all
      if (OldMessageState == null) {
        Debug.WriteLine($"State= {eatState}");
        OldMessageState = eatState;
      }

      Debug.WriteLine($"{cm.Cookie}: '{cm.Raw}'");
      if (cm.Crumbs != null) {
        var crumbs = string.Join(", ", cm.Crumbs.Select(kvp => $"{kvp.Key}= {kvp.Value}"));
        Debug.WriteLine($"\t{crumbs}");
      }

      // output the new state as soon as we transition
      if (OldMessageState != MessageState) {
        Debug.WriteLine($"State= {MessageState}");
        OldMessageState = MessageState;
      }
#endif

      return cm;
    }

    // "-" returned as null
    public static string ParseOptional(string s) {
      return s.Trim() == "-" ? null : s;
    }

    public static DateTime ParseTimestamp(string timestamp) =>
      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(int.Parse(timestamp));

    public static bool ParseBool(string s) => s == "1";

    #region prepare batches
    // Initialize stuff, ready to start pumping out cookies by the thousands.
    // Note that the order of items in this function is important, in some cases
    // messages are very similar and are differentiated by depending on the
    // order the batch is processed.

    static Regex CatchAllIntoMessageRegex = new Regex("(?<message>.*)");

    // for RUN_STATE
    static List<CookieDough> AlphaBatch = new List<CookieDough> {
      new CookieDough { Cookie = FibsCookie.FIBS_Board, Regex = new Regex("^board:(?<player1>[a-zA-Z_<>]+):(?<player2>[a-zA-Z_<>]+):(?<board>[0-9:\\-]+)$"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BAD_Board, Regex = new Regex("^board: (?<board>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouRoll, Regex = new Regex("^You roll (?<die1>[1-6]) and (?<die2>[1-6])"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerRolls, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) rolls (?<die1>[1-6]) and (?<die2>[1-6])"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RollOrDouble, Regex = new Regex("^It's your turn to roll or double\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_RollOrDouble, Regex = new Regex("^It's your turn\\. Please roll or double"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AcceptRejectDouble, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) doubles\\. Type 'accept' or 'reject'\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Doubles, Regex = new Regex("(?<opponent>^[a-zA-Z_<>]+) doubles\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerAcceptsDouble, Regex = new Regex("(?<opponent>^[a-zA-Z_<>]+) accepts the double\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_PleaseMove, Regex = new Regex("^Please move (?<pieces>[1-4]) pieces?\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerMoves, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) moves"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BearingOff, Regex = new Regex("^Bearing off: (?<bearing>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouReject, Regex = new Regex("^You reject\\. The game continues\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouStopWatching, Regex = new Regex("(?<name>[a-zA-Z_<>]+) logs out\\. You're not watching anymore\\."), },    // overloaded	//PLAYER logs out. You're not watching anymore.
      new CookieDough { Cookie = FibsCookie.FIBS_OpponentLogsOut, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) logs out\\. The game was saved"), },         // PLAYER logs out. The game was saved.
      new CookieDough { Cookie = FibsCookie.FIBS_OpponentLogsOut, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) drops connection\\. The game was saved"), },         // PLAYER drops connection. The game was saved.
      new CookieDough { Cookie = FibsCookie.FIBS_OnlyPossibleMove, Regex = new Regex("^The only possible move is (?<move>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_FirstRoll, Regex = new Regex("(?<opponent>[a-zA-Z_<>]+) rolled (?<opponentDie>[1-6]).+rolled (?<yourDie>[1-6])"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MakesFirstMove, Regex = new Regex("(?<opponent>[a-zA-Z_<>]+) makes the first move\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouDouble, Regex = new Regex("^You double\\. Please wait for (?<opponent>[a-zA-Z_<>]+) to accept or reject"), }, // You double. Please wait for PLAYER to accept or reject.
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerWantsToResign, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) wants to resign\\. You will win (?<points>[0-9]+) points?\\. Type 'accept' or 'reject'\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_WatchResign, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) wants to resign\\. (?<player2>[a-zA-Z_<>]+) will win (?<points>[0-9]+) points"), }, // PLAYER wants to resign. PLAYER2 will win 2 points.  (ORDER MATTERS HERE)
      new CookieDough { Cookie = FibsCookie.FIBS_YouResign, Regex = new Regex("^You want to resign. (?<opponent>[a-zA-Z_<>]+) will win (?<points>[0-9]+)"), },  // You want to resign. PLAYER will win 1 .
      new CookieDough { Cookie = FibsCookie.FIBS_ResumeMatchAck5, Regex = new Regex("^You are now playing with (?<opponent>[a-zA-Z_<>]+)\\. Your running match was loaded"), },
      new CookieDough { Cookie = FibsCookie.FIBS_JoinNextGame, Regex = new Regex("^Type 'join' if you want to play the next game, type 'leave' if you don't\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NewMatchRequest, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) wants to play a (?<points>[0-9]+) point match with you\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_WARNINGSavedMatch, Regex = new Regex("^WARNING: Don't accept if you want to continue"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ResignRefused, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) rejects\\. The game continues\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MatchLength, Regex = new Regex("^match length: (?<length>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_TypeJoin, Regex = new Regex("^Type 'join (?<opponent>[a-zA-Z_<>]+)' to accept\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouAreWatching, Regex = new Regex("^You're now watching (?<name>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouStopWatching, Regex = new Regex("^You stop watching (?<name>[a-zA-Z_<>]+)"), },   // overloaded
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerStartsWatching, Regex = new Regex("(?<player1>[a-zA-Z_<>]+) starts watching (?<player2>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerStartsWatching, Regex = new Regex("(?<name>[a-zA-Z_<>]+) is watching you"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerStopsWatching, Regex = new Regex("(?<name>[a-zA-Z_<>]+) stops watching (?<player>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerIsWatching, Regex = new Regex("(?<name>[a-zA-Z_<>]+) is watching "), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerLeftGame, Regex = new Regex("(?<player1>[a-zA-Z_<>]+) has left the game with (?<player2>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ResignWins, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) gives up\\. (?<player2>[a-zA-Z_<>]+) wins (?<points>[0-9]+) points?"), },  // PLAYER1 gives up. PLAYER2 wins 1 point.
      new CookieDough { Cookie = FibsCookie.FIBS_ResignYouWin, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) gives up\\. You win (?<points>[0-9]+) points"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouAcceptAndWin, Regex = new Regex("^You accept and win (?<something>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AcceptWins, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) accepts and wins (?<points>[0-9]+) point"), },      // PLAYER accepts and wins N points.
      new CookieDough { Cookie = FibsCookie.FIBS_PlayersStartingMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) and (?<player2>[a-zA-Z_<>]+) start a (?<points>[0-9]+) point match"), },  // PLAYER and PLAYER start a <n> point match.
      new CookieDough { Cookie = FibsCookie.FIBS_StartingNewGame, Regex = new Regex("^Starting a new game with (?<opponent>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouGiveUp, Regex = new Regex("^You give up"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouWinMatch, Regex = new Regex("^You win the (?<points>[0-9]+) point match (?<winnerScore>[0-9]+)-(?<loserScore>[0-9]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerWinsMatch, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) wins the (?<points>[0-9]+) point match (?<winnerScore>[0-9]+)-(?<loserScore>[0-9]+)"), }, //PLAYER wins the 3 point match 3-0 .
      new CookieDough { Cookie = FibsCookie.FIBS_ResumingUnlimitedMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) and (?<player2>[a-zA-Z_<>]+) are resuming their unlimited match\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_ResumingLimitedMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) and (?<player2>[a-zA-Z_<>]+) are resuming their (?<points>[0-9]+)-point match\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MatchResult, Regex = new Regex("^(?<winner>[a-zA-Z_<>]+) wins a (?<points>[0-9]+) point match against (?<loser>[a-zA-Z_<>]+) +(?<winnerScore>[0-9]+)-(?<loserScore>[0-9]+)"), },  //PLAYER wins a 9 point match against PLAYER  11-6 .
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerWantsToResign, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) wants to resign\\."), },    //  Same as a longline in an actual game  This is just for watching.
      new CookieDough { Cookie = FibsCookie.FIBS_BAD_AcceptDouble, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) accepts? the double\\. The cube shows (?<cube>[0-9]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouAcceptDouble, Regex = new Regex("^You accept the double\\. The cube shows (?<cube>[0-9]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerAcceptsDouble, Regex = new Regex("(?<name>^[a-zA-Z_<>]+) accepts the double\\. The cube shows (?<cube>[0-9]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerAcceptsDouble, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) accepts the double"), },   // while watching
      new CookieDough { Cookie = FibsCookie.FIBS_ResumeMatchRequest, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) wants to resume a saved match with you"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ResumeMatchAck0, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) has joined you\\. Your running match was loaded"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouWinGame, Regex = new Regex("^You win the game and get (?<points>[0-9]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_UnlimitedInvite, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) wants to play an unlimted match with you"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerWinsGame, Regex = new Regex("^(?<opponent>[a-zA-Z_<>]+) wins the game and gets (?<points>[0-9]+) points?. Sorry"), },
      //new CookieDough { Cookie = FibsCookie.FIBS_PlayerWinsGame, Regex = new Regex("^[a-zA-Z_<>]+ wins the game and gets [0-9] points?."), }, // (when watching)
      new CookieDough { Cookie = FibsCookie.FIBS_WatchGameWins, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) wins the game and gets (?<points>[0-9]+) points"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayersStartingUnlimitedMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) and (?<player2>[a-zA-Z_<>]+) start an unlimited match"), }, // PLAYER_A and PLAYER_B start an unlimited match.
      new CookieDough { Cookie = FibsCookie.FIBS_ReportLimitedMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) +- +(?<player2>[a-zA-Z_<>]+) (?<points>[0-9]+) point match (?<score1>[0-9]+)-(?<score2>[0-9]+)"), },  // PLAYER_A        -       PLAYER_B (5 point match 2-2)
      new CookieDough { Cookie = FibsCookie.FIBS_ReportUnlimitedMatch, Regex = new Regex("^(?<player1>[a-zA-Z_<>]+) +- +(?<player2>[a-zA-Z_<>]+) \\(unlimited (?<something>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesStart, Regex = new Regex("^(?<playerX>[a-zA-Z_<>]+) is X - (?<playerO>[a-zA-Z_<>]+) is O"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesRoll, Regex = new Regex("^[XO]: \\([1-6]"), }, // ORDER MATTERS HERE
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesWins, Regex = new Regex("^[XO]: wins"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesDoubles, Regex = new Regex("^[XO]: doubles"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesAccepts, Regex = new Regex("^[XO]: accepts"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesRejects, Regex = new Regex("^[XO]: rejects"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ShowMovesOther, Regex = new Regex("^[XO]:"), },     // AND HERE
      new CookieDough { Cookie = FibsCookie.FIBS_ScoreUpdate, Regex = new Regex("^score in (?<points>[0-9]+) point match:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MatchStart, Regex = new Regex("^Score is (?<score1>[0-9]+)-(?<score2>[0-9]+) in a (?<points>[0-9]+) point match\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Settings, Regex = new Regex("^Settings of variables:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Turn, Regex = new Regex("^turn:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Boardstyle, Regex = new Regex("^boardstyle:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Linelength, Regex = new Regex("^linelength:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Pagelength, Regex = new Regex("^pagelength:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Redoubles, Regex = new Regex("^redoubles:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Sortwho, Regex = new Regex("^sortwho:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Timezone, Regex = new Regex("^timezone:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantMove, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) can't move"), }, // PLAYER can't move || You can't move
      new CookieDough { Cookie = FibsCookie.FIBS_ListOfGames, Regex = new Regex("^List of games:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerInfoStart, Regex = new Regex("^Information about"), },
      new CookieDough { Cookie = FibsCookie.FIBS_EmailAddress, Regex = new Regex("^  Email address:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoEmail, Regex = new Regex("^  No email address"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WavesAgain, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) waves goodbye again"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Waves, Regex = new Regex("^(?<name>[a-zA-Z_<>]+) waves goodbye"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Waves, Regex = new Regex("^You wave goodbye"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WavesAgain, Regex = new Regex("^You wave goodbye again and log out"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoSavedGames, Regex = new Regex("^no saved games"), },
      new CookieDough { Cookie = FibsCookie.FIBS_TypeBack, Regex = new Regex("^You're away\\. Please type 'back'"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SavedMatch, Regex = new Regex("^  (?<player1>[a-zA-Z_<>]+) +(?<score1>[0-9]+) +(?<score2>[0-9]+) +- +(?<something>.*)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SavedMatchPlaying, Regex = new Regex("^ \\*[a-zA-Z_<>]+ +[0-9]+ +[0-9]+ +- +"), },
      // NOTE: for FIBS_SavedMatchReady, see the Stars message, because it will appear to be one of those (has asterisk at index 0).
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerIsWaitingForYou, Regex = new Regex("^[a-zA-Z_<>]+ is waiting for you to log in\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_IsAway, Regex = new Regex("^[a-zA-Z_<>]+ is away: "), },
      new CookieDough { Cookie = FibsCookie.FIBS_AllowpipTrue, Regex = new Regex("^allowpip +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AllowpipFalse, Regex = new Regex("^allowpip +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutoboardTrue, Regex = new Regex("^autoboard +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutoboardFalse, Regex = new Regex("^autoboard +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutodoubleTrue, Regex = new Regex("^autodouble +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutodoubleFalse, Regex = new Regex("^autodouble +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutomoveTrue, Regex = new Regex("^automove +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutomoveFalse, Regex = new Regex("^automove +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BellTrue, Regex = new Regex("^bell +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BellFalse, Regex = new Regex("^bell +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CrawfordTrue, Regex = new Regex("^crawford +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CrawfordFalse, Regex = new Regex("^crawford +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_DoubleTrue, Regex = new Regex("^double +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_DoubleFalse, Regex = new Regex("^double +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MoreboardsTrue, Regex = new Regex("^moreboards +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MoreboardsFalse, Regex = new Regex("^moreboards +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MovesTrue, Regex = new Regex("^moves +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MovesFalse, Regex = new Regex("^moves +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_GreedyTrue, Regex = new Regex("^greedy +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_GreedyFalse, Regex = new Regex("^greedy +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotifyTrue, Regex = new Regex("^notify +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotifyFalse, Regex = new Regex("^notify +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingsTrue, Regex = new Regex("^ratings +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingsFalse, Regex = new Regex("^ratings +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReadyTrue, Regex = new Regex("^ready +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReadyFalse, Regex = new Regex("^ready +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReportTrue, Regex = new Regex("^report +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReportFalse, Regex = new Regex("^report +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SilentTrue, Regex = new Regex("^silent +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SilentFalse, Regex = new Regex("^silent +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_TelnetTrue, Regex = new Regex("^telnet +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_TelnetFalse, Regex = new Regex("^telnet +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WrapTrue, Regex = new Regex("^wrap +YES"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WrapFalse, Regex = new Regex("^wrap +NO"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Junk, Regex = new Regex("^Closed old connection with user"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Done, Regex = new Regex("^Done\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_YourTurnToMove, Regex = new Regex("^It's your turn to move\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_SavedMatchesHeader, Regex = new Regex("^  opponent          matchlength   score \\(your points first\\)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MessagesForYou, Regex = new Regex("^There are messages for you:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RedoublesSetTo, Regex = new Regex("^Value of 'redoubles' set to [0-9]+\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_DoublingCubeNow, Regex = new Regex("^The number on the doubling cube is now [0-9]+"), },
      new CookieDough { Cookie = FibsCookie.FIBS_FailedLogin, Regex = new Regex("^> [0-9]+"), },             // bogus CLIP messages sent after a failed login
      new CookieDough { Cookie = FibsCookie.FIBS_Average, Regex = new Regex("^Time (UTC)  average min max"), },
      new CookieDough { Cookie = FibsCookie.FIBS_DiceTest, Regex = new Regex("^[nST]: "), },
      new CookieDough { Cookie = FibsCookie.FIBS_LastLogout, Regex = new Regex("^  Last logout:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcStart, Regex = new Regex("^rating calculation:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^Probability that underdog wins:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("is 1-Pu if underdog wins"), }, // P=0.505861 is 1-Pu if underdog wins and Pu if favorite wins
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^Experience: "), },          // Experience: fergy 500 - jfk 5832
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^K=max\\(1"), },           // K=max(1 ,		-Experience/100+5) for fergy: 1.000000
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^rating difference"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^change for"), },          // change for fergy: 4*K*sqrt(N)*P=2.023443
      new CookieDough { Cookie = FibsCookie.FIBS_RatingCalcInfo, Regex = new Regex("^match length  "), },
      new CookieDough { Cookie = FibsCookie.FIBS_WatchingHeader, Regex = new Regex("^Watching players:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SettingsHeader, Regex = new Regex("^The current settings are:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AwayListHeader, Regex = new Regex("^The following users are away:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingExperience, Regex = new Regex("^  Rating: +[0-9]+\\."), },        // Rating: 1693.11 Experience: 5781
      new CookieDough { Cookie = FibsCookie.FIBS_NotLoggedIn, Regex = new Regex("^  Not logged in right now\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_IsPlayingWith, Regex = new Regex("is playing with"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SavedScoreHeader, Regex = new Regex("^opponent +matchlength"), },   //	opponent          matchlength   score (your points first)
      new CookieDough { Cookie = FibsCookie.FIBS_StillLoggedIn, Regex = new Regex("^  Still logged in\\."), },     //  Still logged in. 2:12 minutes idle.
      new CookieDough { Cookie = FibsCookie.FIBS_NoOneIsAway, Regex = new Regex("^None of the users is away\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerListHeader, Regex = new Regex("^No  S  username        rating  exp login    idle  from"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingsHeader, Regex = new Regex("^ rank name            rating    Experience"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ClearScreen, Regex = new Regex("^.\\[, },H.\\[2J"), },       // ANSI clear screen sequence
      new CookieDough { Cookie = FibsCookie.FIBS_Timeout, Regex = new Regex("^Connection timed out\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Goodbye, Regex = new Regex("(?<message>           Goodbye\\.)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_LastLogin, Regex = new Regex("^  Last login:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoInfo, Regex = new Regex("^No information found on user"), },
    };

    //--- Numeric messages ---------------------------------------------------
    static List<CookieDough> NumericBatch = new List<CookieDough> {
      new CookieDough { Cookie = FibsCookie.CLIP_WHO_INFO, Regex = new Regex("^5 (?<name>[^ ]+) (?<opponent>[^ ]+) (?<watching>[^ ]+) (?<ready>[01]) (?<away>[01]) (?<rating>[0-9]+\\.[0-9]+) (?<experience>[0-9]+) (?<idle>[0-9]+) (?<login>[0-9]+) (?<hostName>[^ ]+) (?<client>[^ ]+) (?<email>[^ ]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Average, Regex = new Regex("^[0-9][0-9]:[0-9][0-9]-"), },     // output of average command
      new CookieDough { Cookie = FibsCookie.FIBS_DiceTest, Regex = new Regex("^[1-6]-1 [0-9]"), },         // output of dicetest command
      new CookieDough { Cookie = FibsCookie.FIBS_DiceTest, Regex = new Regex("^[1-6]: [0-9]"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Stat, Regex = new Regex("^[0-9]+ bytes"), },          // output from stat command
      new CookieDough { Cookie = FibsCookie.FIBS_Stat, Regex = new Regex("^[0-9]+ accounts"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Stat, Regex = new Regex("^[0-9]+ ratings saved. reset log"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Stat, Regex = new Regex("^[0-9]+ registered users."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Stat, Regex = new Regex("^[0-9]+\\([0-9]+\\) saved games check by cron"), },
      new CookieDough { Cookie = FibsCookie.CLIP_WHO_END, Regex = new Regex("^6$"), },
      new CookieDough { Cookie = FibsCookie.CLIP_SHOUTS, Regex = new Regex("^13 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_SAYS, Regex = new Regex("^12 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_WHISPERS, Regex = new Regex("^14 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_KIBITZES, Regex = new Regex("^15 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_YOU_SAY, Regex = new Regex("^16 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_YOU_SHOUT, Regex = new Regex("^17 (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_YOU_WHISPER, Regex = new Regex("^18 (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_YOU_KIBITZ, Regex = new Regex("^19 (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_LOGIN, Regex = new Regex("^7 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_LOGOUT, Regex = new Regex("^8 (?<name>[a-zA-Z_<>]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_MESSAGE, Regex = new Regex("^9 (?<from>[a-zA-Z_<>]+) (?<time>[0-9]+) (?<message>.*)"), },
      new CookieDough { Cookie = FibsCookie.CLIP_MESSAGE_DELIVERED, Regex = new Regex("^10 (?<name>[a-zA-Z_<>]+)$"), },
      new CookieDough { Cookie = FibsCookie.CLIP_MESSAGE_SAVED, Regex = new Regex("^11 (?<name>[a-zA-Z_<>]+)$"), },
    };

    //--- '**' messages ------------------------------------------------------
    static List<CookieDough> StarsBatch = new List<CookieDough> {
      new CookieDough { Cookie = FibsCookie.FIBS_Username, Regex = new Regex("^\\*\\* User"), },
      new CookieDough { Cookie = FibsCookie.FIBS_Junk, Regex = new Regex("^\\*\\* You tell "), },        // "** You tell PLAYER: xxxxx"
      new CookieDough { Cookie = FibsCookie.FIBS_YouGag, Regex = new Regex("^\\*\\* You gag"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouUngag, Regex = new Regex("^\\*\\* You ungag"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouBlind, Regex = new Regex("^\\*\\* You blind"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouUnblind, Regex = new Regex("^\\*\\* You unblind"), },
      new CookieDough { Cookie = FibsCookie.FIBS_UseToggleReady, Regex = new Regex("^\\*\\* Use 'toggle ready' first"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NewMatchAck9, Regex = new Regex("^\\*\\* You are now playing an unlimited match with "), },
      new CookieDough { Cookie = FibsCookie.FIBS_NewMatchAck10, Regex = new Regex("^\\*\\* You are now playing a [0-9]+ point match with "), },  // ** You are now playing a 5 point match with PLAYER
      new CookieDough { Cookie = FibsCookie.FIBS_NewMatchAck2, Regex = new Regex("^\\*\\* Player [a-zA-Z_<>]+ has joined you for a"), }, // ** Player PLAYER has joined you for a 2 point match.
      new CookieDough { Cookie = FibsCookie.FIBS_YouTerminated, Regex = new Regex("^\\*\\* You terminated the game"), },
      new CookieDough { Cookie = FibsCookie.FIBS_OpponentLeftGame, Regex = new Regex("^\\*\\* Player [a-zA-Z_<>]+ has left the game. The game was saved\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerLeftGame, Regex = new Regex("has left the game\\."), },   // overloaded
      new CookieDough { Cookie = FibsCookie.FIBS_YouInvited, Regex = new Regex("^\\*\\* You invited"), },
      new CookieDough { Cookie = FibsCookie.FIBS_YourLastLogin, Regex = new Regex("^\\*\\* Last login:"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoOne, Regex = new Regex("^\\*\\* There is no one called (?<name>[a-zA-Z_<>]+)"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AllowpipFalse, Regex = new Regex("^\\*\\* You don't allow the use of the server's 'pip' command\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_AllowpipTrue, Regex = new Regex("^\\*\\* You allow the use the server's 'pip' command\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutoboardFalse, Regex = new Regex("^\\*\\* The board won't be refreshed"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutoboardTrue, Regex = new Regex("^\\*\\* The board will be refreshed"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutodoubleTrue, Regex = new Regex("^\\*\\* You agree that doublets"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutodoubleFalse, Regex = new Regex("^\\*\\* You don't agree that doublets"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutomoveFalse, Regex = new Regex("^\\*\\* Forced moves won't"), },
      new CookieDough { Cookie = FibsCookie.FIBS_AutomoveTrue, Regex = new Regex("^\\*\\* Forced moves will"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BellFalse, Regex = new Regex("^\\*\\* Your terminal won't ring"), },
      new CookieDough { Cookie = FibsCookie.FIBS_BellTrue, Regex = new Regex("^\\*\\* Your terminal will ring"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CrawfordFalse, Regex = new Regex("^\\*\\* You would like to play without using the Crawford rule\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_CrawfordTrue, Regex = new Regex("^\\*\\* You insist on playing with the Crawford rule\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_DoubleFalse, Regex = new Regex("^\\*\\* You won't be asked if you want to double\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_DoubleTrue, Regex = new Regex("^\\*\\* You will be asked if you want to double\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_GreedyTrue, Regex = new Regex("^\\*\\* Will use automatic greedy bearoffs\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_GreedyFalse, Regex = new Regex("^\\*\\* Won't use automatic greedy bearoffs\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MoreboardsTrue, Regex = new Regex("^\\*\\* Will send rawboards after rolling\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MoreboardsFalse, Regex = new Regex("^\\*\\* Won't send rawboards after rolling\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MovesTrue, Regex = new Regex("^\\*\\* You want a list of moves after this game\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MovesFalse, Regex = new Regex("^\\*\\* You won't see a list of moves after this game\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotifyFalse, Regex = new Regex("^\\*\\* You won't be notified"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotifyTrue, Regex = new Regex("^\\*\\* You'll be notified"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingsTrue, Regex = new Regex("^\\*\\* You'll see how the rating changes are calculated\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_RatingsFalse, Regex = new Regex("^\\*\\* You won't see how the rating changes are calculated\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReadyTrue, Regex = new Regex("^\\*\\* You're now ready to invite or join someone\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReadyFalse, Regex = new Regex("^\\*\\* You're now refusing to play with someone\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReportFalse, Regex = new Regex("^\\*\\* You won't be informed"), },
      new CookieDough { Cookie = FibsCookie.FIBS_ReportTrue, Regex = new Regex("^\\*\\* You will be informed"), },
      new CookieDough { Cookie = FibsCookie.FIBS_SilentTrue, Regex = new Regex("^\\*\\* You won't hear what other players shout\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_SilentFalse, Regex = new Regex("^\\*\\* You will hear what other players shout\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_TelnetFalse, Regex = new Regex("^\\*\\* You use a client program"), },
      new CookieDough { Cookie = FibsCookie.FIBS_TelnetTrue, Regex = new Regex("^\\*\\* You use telnet"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WrapFalse, Regex = new Regex("^\\*\\* The server will wrap"), },
      new CookieDough { Cookie = FibsCookie.FIBS_WrapTrue, Regex = new Regex("^\\*\\* Your terminal knows how to wrap"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerRefusingGames, Regex = new Regex("^\\*\\* [a-zA-Z_<>]+ is refusing games\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotWatching, Regex = new Regex("^\\*\\* You're not watching\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotWatchingPlaying, Regex = new Regex("^\\*\\* You're not watching or playing\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotPlaying, Regex = new Regex("^\\*\\* You're not playing\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoUser, Regex = new Regex("^\\*\\* There is no one called "), },
      new CookieDough { Cookie = FibsCookie.FIBS_AlreadyPlaying, Regex = new Regex("is already playing with"), },
      new CookieDough { Cookie = FibsCookie.FIBS_DidntInvite, Regex = new Regex("^\\*\\* [a-zA-Z_<>]+ didn't invite you."), },
      new CookieDough { Cookie = FibsCookie.FIBS_BadMove, Regex = new Regex("^\\*\\* You can't remove this piece"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantMoveFirstMove, Regex = new Regex("^\\*\\* You can't move "), },     // ** You can't move 3 points in your first move
      new CookieDough { Cookie = FibsCookie.FIBS_CantShout, Regex = new Regex("^\\*\\* Please type 'toggle silent' again before you shout\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_MustMove, Regex = new Regex("^\\*\\* You must give [1-4] moves"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MustComeIn, Regex = new Regex("^\\*\\* You have to remove pieces from the bar in your first move\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_UsersHeardYou, Regex = new Regex("^\\*\\* [0-9]+ users? heard you\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Junk, Regex = new Regex("^\\*\\* Please wait for [a-zA-Z_<>]+ to join too\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_SavedMatchReady, Regex = new Regex("^\\*\\*[a-zA-Z_<>]+ +[0-9]+ +[0-9]+ +- +[0-9]+"), },    // double star before a name indicates you have a saved game with this player
      new CookieDough { Cookie = FibsCookie.FIBS_NotYourTurnToRoll, Regex = new Regex("^\\*\\* It's not your turn to roll the dice\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_NotYourTurnToMove, Regex = new Regex("^\\*\\* It's not your turn to move\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_YouStopWatching, Regex = new Regex("^\\*\\* You stop watching"), },
      new CookieDough { Cookie = FibsCookie.FIBS_UnknownCommand, Regex = new Regex("^\\*\\* Unknown command: (?<command>.*)$"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantWatch, Regex = new Regex("^\\*\\* You can't watch another game while you're playing\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantInviteSelf, Regex = new Regex("^\\*\\* You can't invite yourself\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_DontKnowUser, Regex = new Regex("^\\*\\* Don't know user"), },
      new CookieDough { Cookie = FibsCookie.FIBS_MessageUsage, Regex = new Regex("^\\*\\* usage: message <user> <text>"), },
      new CookieDough { Cookie = FibsCookie.FIBS_PlayerNotPlaying, Regex = new Regex("^\\*\\* [a-zA-Z_<>]+ is not playing\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantTalk, Regex = new Regex("^\\*\\* You can't talk if you won't listen\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_WontListen, Regex = new Regex("^\\*\\* [a-zA-Z_<>]+ won't listen to you\\."), },
      new CookieDough { Cookie = FibsCookie.FIBS_Why, Regex = new Regex("Why would you want to do that"), },   // (not sure about ** vs *** at front of line.)
      new CookieDough { Cookie = FibsCookie.FIBS_Ratings, Regex = new Regex("^\\* *[0-9]+ +[a-zA-Z_<>]+ +[0-9]+\\.[0-9]+ +[0-9]+"), },
      new CookieDough { Cookie = FibsCookie.FIBS_NoSavedMatch, Regex = new Regex("^\\*\\* There's no saved match with "), },
      new CookieDough { Cookie = FibsCookie.FIBS_WARNINGSavedMatch, Regex = new Regex("^\\*\\* WARNING: Don't accept if you want to continue"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantGagYourself, Regex = new Regex("^\\*\\* You talk too much, don't you\\?"), },
      new CookieDough { Cookie = FibsCookie.FIBS_CantBlindYourself, Regex = new Regex("^\\*\\* You can't read this message now, can you\\?"), },
      new CookieDough { Cookie = FibsCookie.FIBS_RunningDiceTest, Regex = new Regex("^\\*\\*\\* running dice test:"), },
    };

    // for LOGIN_STATE
    static List<CookieDough> LoginBatch = new List<CookieDough> {
      new CookieDough { Cookie = FibsCookie.FIBS_LoginPrompt, Regex = new Regex("^login:") },
      new CookieDough { Cookie = FibsCookie.FIBS_WARNINGAlreadyLoggedIn, Regex = new Regex("^\\*\\* Warning: You are already logged in\\.") },
      new CookieDough { Cookie = FibsCookie.CLIP_WELCOME, Regex = new Regex("^1 (?<name>[a-zA-Z_<>]+) (?<lastLogin>[0-9]+) (?<lastHost>.*)") },
      new CookieDough { Cookie = FibsCookie.CLIP_OWN_INFO, Regex = new Regex("^2 (?<name>[a-zA-Z_<>]+) (?<allowPip>[01]) (?<autoBoard>[01]) (?<autoDouble>[01]) (?<autoMove>[01]) (?<away>[01]) (?<bell>[01]) (?<crawford>[01]) (?<double>[01]) (?<experience>[0-9]+) (?<greedy>[01]) (?<moreBoards>[01]) (?<moves>[01]) (?<notify>[01]) (?<rating>[0-9]+\\.[0-9]+) (?<ratings>[01]) (?<ready>[01]) (?<redoubles>[0-9a-zA-Z]+) (?<report>[01]) (?<silent>[01]) (?<timezone>.*)") },
      new CookieDough { Cookie = FibsCookie.CLIP_MOTD_BEGIN, Regex = new Regex("^3$") },
      new CookieDough { Cookie = FibsCookie.FIBS_FailedLogin, Regex = new Regex("^> [0-9]+") },  // bogus CLIP messages sent after a failed login
      new CookieDough { Cookie = FibsCookie.FIBS_PreLogin, Regex = CatchAllIntoMessageRegex },  // catch all
    };

    // Only interested in one message here, but we still use a message list for simplicity and consistency.
    // for MOTD_STATE
    static List<CookieDough> MOTDBatch = new List<CookieDough> {
       new CookieDough { Cookie = FibsCookie.CLIP_MOTD_END, Regex = new Regex("^4$") },
       new CookieDough { Cookie = FibsCookie.FIBS_MOTD, Regex = CatchAllIntoMessageRegex }, // catch all
    };
    #endregion
  }

  #region cookie types
  public enum FibsCookie {
    CLIP_WELCOME = 1,
    CLIP_OWN_INFO = 2,
    CLIP_MOTD_BEGIN = 3,
    CLIP_MOTD_END = 4,
    CLIP_WHO_INFO = 5,
    CLIP_WHO_END = 6,
    CLIP_LOGIN = 7,
    CLIP_LOGOUT = 8,
    CLIP_MESSAGE = 9,
    CLIP_MESSAGE_DELIVERED = 10,
    CLIP_MESSAGE_SAVED = 11,
    CLIP_SAYS = 12,
    CLIP_SHOUTS = 13,
    CLIP_WHISPERS = 14,
    CLIP_KIBITZES = 15,
    CLIP_YOU_SAY = 16,
    CLIP_YOU_SHOUT = 17,
    CLIP_YOU_WHISPER = 18,
    CLIP_YOU_KIBITZ = 19,
    FIBS_PreLogin, // the ASCII "FIBS" art, etc.
    FIBS_LoginPrompt,
    FIBS_WARNINGAlreadyLoggedIn, // csells: already logged in warning
    FIBS_FailedLogin,       // use this to detect a failed login (e.g. wrong password)
    FIBS_MOTD,
    FIBS_Goodbye,
    FIBS_PostGoodbye,       // "send cookies", etc.
    FIBS_Unknown,         // don't know the type, probably can ignore
    FIBS_Empty,           // empty string
    FIBS_Junk,            // a message we don't care about, but is not unknown
    FIBS_ClearScreen,
    FIBS_BAD_AcceptDouble,      // DANGER, WILL ROBINSON!!! See notes in .c file about these two cookies!
    FIBS_BAD_Board,
    FIBS_Average,
    FIBS_DiceTest,
    FIBS_Stat,
    FIBS_Why,
    FIBS_NoInfo,
    FIBS_LastLogout,
    FIBS_RatingCalcStart,
    FIBS_RatingCalcInfo,
    FIBS_SettingsHeader,
    FIBS_PlayerListHeader,
    FIBS_AwayListHeader,
    FIBS_RatingExperience,
    FIBS_NotLoggedIn,
    FIBS_StillLoggedIn,
    FIBS_NoOneIsAway,
    FIBS_RatingsHeader,
    FIBS_IsPlayingWith,
    FIBS_Timeout,
    FIBS_UnknownCommand,
    FIBS_Username,
    FIBS_LastLogin,
    FIBS_YourLastLogin,
    FIBS_Registered,
    FIBS_ONEUSERNAME,
    FIBS_EnterUsername,
    FIBS_EnterPassword,
    FIBS_TypeInNo,
    FIBS_SavedScoreHeader,
    FIBS_NoSavedGames,
    FIBS_UsersHeardYou,
    FIBS_MessagesForYou,
    FIBS_IsAway,
    FIBS_OpponentLogsOut,
    FIBS_Waves,
    FIBS_WavesAgain,
    FIBS_YouGag,
    FIBS_YouUngag,
    FIBS_YouBlind,
    FIBS_YouUnblind,
    FIBS_WatchResign,
    FIBS_UseToggleReady,
    FIBS_WARNINGSavedMatch,
    FIBS_NoSavedMatch,
    FIBS_AlreadyPlaying,
    FIBS_DidntInvite,
    FIBS_WatchingHeader,
    FIBS_NotWatching,
    FIBS_NotWatchingPlaying,
    FIBS_NotPlaying,
    FIBS_PlayerNotPlaying,
    FIBS_NoUser,
    FIBS_CantInviteSelf,
    FIBS_CantWatch,
    FIBS_CantTalk,
    FIBS_CantBlindYourself,
    FIBS_CantGagYourself,
    FIBS_WontListen,
    FIBS_TypeBack,
    FIBS_NoOne,
    FIBS_BadMove,
    FIBS_MustMove,
    FIBS_MustComeIn,
    FIBS_CantShout,
    FIBS_DontKnowUser,
    FIBS_MessageUsage,
    FIBS_Done,
    FIBS_SavedMatchesHeader,
    FIBS_NotYourTurnToRoll,
    FIBS_NotYourTurnToMove,
    FIBS_YourTurnToMove,
    FIBS_Ratings,
    FIBS_PlayerInfoStart,
    FIBS_EmailAddress,
    FIBS_NoEmail,
    FIBS_ListOfGames,
    FIBS_SavedMatch,
    FIBS_SavedMatchPlaying,
    FIBS_SavedMatchReady,
    FIBS_YouAreWatching,
    FIBS_YouStopWatching,
    FIBS_PlayerStartsWatching,
    FIBS_PlayerStopsWatching,
    FIBS_PlayerIsWatching,
    FIBS_ReportUnlimitedMatch,
    FIBS_ReportLimitedMatch,
    FIBS_RollOrDouble,
    FIBS_YouWinMatch,
    FIBS_PlayerWinsMatch,
    FIBS_YouReject,
    FIBS_YouResign,
    FIBS_ResumeMatchRequest,
    FIBS_ResumeMatchAck0,
    FIBS_ResumeMatchAck5,
    FIBS_NewMatchRequest,
    FIBS_UnlimitedInvite,
    FIBS_YouInvited,
    FIBS_NewMatchAck9,
    FIBS_NewMatchAck10,
    FIBS_NewMatchAck2,
    FIBS_YouTerminated,
    FIBS_OpponentLeftGame,
    FIBS_PlayerLeftGame,
    FIBS_PlayerRefusingGames,
    FIBS_TypeJoin,
    FIBS_ShowMovesStart,
    FIBS_ShowMovesWins,
    FIBS_ShowMovesRoll,
    FIBS_ShowMovesDoubles,
    FIBS_ShowMovesAccepts,
    FIBS_ShowMovesRejects,
    FIBS_ShowMovesOther,
    FIBS_Board,
    FIBS_YouRoll,
    FIBS_PlayerRolls,
    FIBS_PlayerMoves,
    FIBS_Doubles,
    FIBS_AcceptRejectDouble,
    FIBS_StartingNewGame,
    FIBS_PlayerAcceptsDouble,
    FIBS_YouAcceptDouble,
    FIBS_Settings,
    FIBS_Turn,
    FIBS_FirstRoll,
    FIBS_DoublingCubeNow,
    FIBS_CantMove,
    FIBS_CantMoveFirstMove,
    FIBS_ResignRefused,
    FIBS_YouWinGame,
    FIBS_OnlyPossibleMove,
    FIBS_AcceptWins,
    FIBS_ResignWins,
    FIBS_ResignYouWin,
    FIBS_WatchGameWins,
    FIBS_ScoreUpdate,
    FIBS_MatchStart,
    FIBS_YouAcceptAndWin,
    FIBS_OnlyMove,
    FIBS_BearingOff,
    FIBS_PleaseMove,
    FIBS_MakesFirstMove,
    FIBS_YouDouble,
    FIBS_MatchLength,
    FIBS_PlayerWantsToResign,
    FIBS_PlayerWinsGame,
    FIBS_JoinNextGame,
    FIBS_ResumingUnlimitedMatch,
    FIBS_ResumingLimitedMatch,
    FIBS_PlayersStartingMatch,
    FIBS_PlayersStartingUnlimitedMatch,
    FIBS_MatchResult,
    FIBS_YouGiveUp,
    FIBS_PlayerIsWaitingForYou,
    FIBS_Boardstyle,
    FIBS_Linelength,
    FIBS_Pagelength,
    FIBS_Redoubles,
    FIBS_Sortwho,
    FIBS_Timezone,
    FIBS_RedoublesSetTo,
    FIBS_AllowpipTrue,
    FIBS_AllowpipFalse,
    FIBS_AutoboardTrue,
    FIBS_AutoboardFalse,
    FIBS_AutodoubleTrue,
    FIBS_AutodoubleFalse,
    FIBS_AutomoveTrue,
    FIBS_AutomoveFalse,
    FIBS_BellTrue,
    FIBS_BellFalse,
    FIBS_CrawfordTrue,
    FIBS_CrawfordFalse,
    FIBS_DoubleTrue,
    FIBS_DoubleFalse,
    FIBS_MoreboardsTrue,
    FIBS_MoreboardsFalse,
    FIBS_MovesTrue,
    FIBS_MovesFalse,
    FIBS_GreedyTrue,
    FIBS_GreedyFalse,
    FIBS_NotifyTrue,
    FIBS_NotifyFalse,
    FIBS_RatingsTrue,
    FIBS_RatingsFalse,
    FIBS_ReadyTrue,
    FIBS_ReadyFalse,
    FIBS_ReportTrue,
    FIBS_ReportFalse,
    FIBS_SilentTrue,
    FIBS_SilentFalse,
    FIBS_TelnetTrue,
    FIBS_TelnetFalse,
    FIBS_WrapTrue,
    FIBS_WrapFalse,
    FIBS_RunningDiceTest, // csells
  }
  #endregion
}
