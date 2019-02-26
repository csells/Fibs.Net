package net.sourceforge.jibs.command;

import net.sourceforge.jibs.backgammon.BackgammonBoard;
import net.sourceforge.jibs.backgammon.Die;
import net.sourceforge.jibs.backgammon.JibsGame;
import net.sourceforge.jibs.backgammon.JibsMatch;
import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.gui.JibsTextArea;
import net.sourceforge.jibs.server.ClientWorker;
import net.sourceforge.jibs.server.JibsQuestion;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.ClipConstants;
import net.sourceforge.jibs.util.InviteQuestion;
import net.sourceforge.jibs.util.JibsNewGameData;
import net.sourceforge.jibs.util.JibsWriter;
import net.sourceforge.jibs.util.ResumeQuestion;
import net.sourceforge.jibs.util.SavedGameParam;

/**
 * The Join command.
 */
public class Join_Command implements JibsCommand {
    private JibsServer jibsServer;

    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        this.jibsServer = jibsServer;

        JibsGame game = player.getGame();
        Player opponent = null;
        int matchlength = 0;
        JibsMatch matchVersion = null;

        if (game != null) {
            BackgammonBoard board = game.getBackgammonBoard();

            if (board.isEnded()) {
                return doJoinForNewGame(player, args);
            } else {
                if (!board.isEnded()) {
                    opponent = board.getOpponent(player);
                    matchlength = board.getMatchlength();
                    matchVersion = board.getMatchVersion();
                    board.setJoinPlayer(player);

                    if ((board.getJoinPlayer1() != null) &&
                            (board.getJoinPlayer2() != null)) {
                        // start next game
                        BackgammonBoard nextBoard = null;
                        nextBoard = new BackgammonBoard(jibsServer,
                                                        board.getPlayerX()
                                                             .getName(),
                                                        board.getPlayerO()
                                                             .getName(),
                                                        matchlength,
                                                        matchVersion);
                        nextBoard.setPlayerX(board.getPlayerX());
                        nextBoard.setPlayerO(board.getPlayerO());
                        nextBoard.setPlayer1Got(board.getPlayer1Got());
                        nextBoard.setPlayer2Got(board.getPlayer2Got());

                        boolean useCrawford = player.checkToggle("crawford") &&
                                              opponent.checkToggle("crawford");

                        if (useCrawford) {
                            int diff1 = 0;
                            int diff2 = 0;

                            diff1 = board.getMatchlength() -
                                    board.getPlayerXPoints();

                            if (diff1 == 1) {
                                nextBoard.setCrawFordGame(true);
                                nextBoard.setMayDouble1(1);
                                nextBoard.setMayDouble2(0);
                            }

                            diff2 = board.getMatchlength() -
                                    board.getPlayerOPoints();

                            if (diff2 == 1) {
                                nextBoard.setCrawFordGame(true);
                                nextBoard.setMayDouble2(1);
                                nextBoard.setMayDouble1(0);
                            }
                        }

                        board = nextBoard;
                        game.setBackgammonBoard(board);

                        greet(board.getPlayerX(), board.getPlayerO(),
                              board.getMatchVersion(), board.getMatchlength());

                        JibsNewGameData jibsNewGameData = jibsNewGameData(game.getPlayerA(),
                                                                          game.getPlayerB());
                        player.getGame().getBackgammonBoard()
                              .setpDie1(jibsNewGameData.getDie1());
                        player.getGame().getBackgammonBoard()
                              .setpDie2(jibsNewGameData.getDie2());
                        player.getGame().getBackgammonBoard()
                              .setJoinPlayer1(null);
                        player.getGame().getBackgammonBoard()
                              .setJoinPlayer2(null);
                        player.startGame(jibsServer, jibsNewGameData, game,
                                         player.getGame().getBackgammonBoard()
                                               .getPlayerX(),
                                         player.getGame().getBackgammonBoard()
                                               .getPlayerO(), matchlength,
                                         jibsNewGameData.getTurn(),
                                         matchVersion, board.getMayDouble1(),
                                         board.getMayDouble2());
                    }
                }
            }
        } else {
            return doJoinForNewGame(player, args);
        }

        return true;
    }

    private boolean doJoinForNewGame(Player player, String[] args) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        Player opponent = null;
        int matchlength = 0;
        JibsMatch matchVersion = null;
        JibsWriter out = player.getOutputStream();
        JibsGame game = player.getGame();

        if (args.length <= 1) {
            // m_join_no_user=** Error: Join who?
            String msg = jibsMessages.convert("m_join_no_user");
            out.println(msg);
        } else {
            opponent = ClientWorker.getPlayer(jibsServer, args[1]);

            if (opponent == null) {
                // m_join_no_invite=** %0 didn't invite you.
                Object[] obj = { args[1] };
                String msg = jibsMessages.convert("m_join_no_invite", obj);
                out.println(msg);
            } else {
                player.setOpponent(opponent);
                opponent.setOpponent(player);

                JibsQuestion question = opponent.getQuestion();

                if (question instanceof ResumeQuestion) {
                    informNewGame(null, player, opponent);

                    ResumeQuestion resumeQuestion = (ResumeQuestion) question;
                    SavedGameParam resumeData = resumeQuestion.getResumeData();
                    JibsGame resumeGame = JibsGame.constructGame(jibsServer,
                                                                 resumeData);
                    BackgammonBoard board = resumeGame.getBackgammonBoard();
                    resumeGame.setJibsServer(jibsServer);

                    int startturn = board.getTurn();
                    Die dice1 = board.getpDie1();
                    Die dice2 = board.getpDie2();
                    int player1Points = board.getPlayerXPoints();
                    int player2Points = board.getPlayerOPoints();

                    player.setGame(resumeGame);
                    opponent.setGame(resumeGame);
                    resumeGame.resumeGame(resumeData, startturn, dice1, dice2,
                                          player1Points, player2Points);
                }

                if (question instanceof InviteQuestion) {
                    InviteQuestion inviteQuestion = (InviteQuestion) question;
                    matchlength = inviteQuestion.getMatchLength();
                    matchVersion = inviteQuestion.getMatchVersion();

                    informNewGame(game, player, opponent);

                    // m_player_joined=** Player %0 has joined you for a %1
                    // point match.
                    Object[] obj = new Object[] { player.getName(), matchlength };
                    String msg = jibsMessages.convert("m_player_joined", obj);
                    opponent.getOutputStream().println(msg);
                    greet(player, opponent, matchVersion, matchlength);

                    JibsNewGameData jibsNewGameData = jibsNewGameData(player,
                                                                      opponent);

                    game = new JibsGame(jibsServer,
                                        jibsNewGameData.getPlayerX(),
                                        jibsNewGameData.getPlayerO(),
                                        jibsNewGameData.getPlayerX(),
                                        jibsNewGameData.getPlayerO(),
                                        matchlength, 0, 0, matchVersion);
                    player.setGame(game);
                    opponent.setGame(game);

                    BackgammonBoard board = game.getBackgammonBoard();
                    board.setpDie1(jibsNewGameData.getDie1());
                    board.setpDie2(jibsNewGameData.getDie2());
                    game.setBackgammonBoard(board);
                    JibsGame.deleteGames(jibsServer,
                                         jibsNewGameData.getPlayerX(),
                                         jibsNewGameData.getPlayerO());
                    // m_start_match=%0 and %1 start a new %2-point match.
                    obj = new Object[] {
                              jibsNewGameData.getPlayerX().getName(),
                              jibsNewGameData.getPlayerO().getName(),
                              matchlength
                          };
                    msg = jibsMessages.convert("m_start_match", obj);
                    JibsTextArea.log(jibsServer, msg, true);

                    jibsNewGameData.getPlayerX()
                                   .startGame(jibsServer, jibsNewGameData,
                                              game,
                                              jibsNewGameData.getPlayerX(),
                                              jibsNewGameData.getPlayerO(),
                                              matchlength,
                                              jibsNewGameData.getTurn(),
                                              matchVersion, 1, 1);
                }
            }
        }

        return true;
    }

    public void greet(Player player1, Player player2, JibsMatch matchVersion,
                      int matchlength) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        String playerName = player1.getName();
        String oppplayerName = player2.getName();
        JibsWriter out1 = player1.getOutputStream();

        JibsWriter out2 = player2.getOutputStream();

        // m_start_match_with=Starting a new game with %0.
        Object[] obj = { oppplayerName };
        String msg = jibsMessages.convert("m_start_match_with", obj);
        out1.println(msg);

        if (matchVersion.getVersion() == JibsMatch.nPointMatch) {
            // m_start_match_with=Starting a new game with %0.
            obj = new Object[] { playerName };
            msg = jibsMessages.convert("m_start_match_with", obj);
            out2.println(msg);

            // m_start_match=%0 and %1 start a new %2-point match.
            oppplayerName = player2.getName();
            obj = new Object[] {
                      playerName, oppplayerName, Integer.valueOf(matchlength)
                  };
            msg = jibsMessages.convert("m_start_match", obj);
            player1.informPlayers(msg, player2);
            // message needs to be sent also to both watcher lists
            player1.informWatcher("m_start_match", obj, true);
            player2.informWatcher("m_start_match", obj, true);
        } else {
            // m_you_play_unlimited=** You are now playing an unlimited match
            // with %1
            obj = new Object[] { playerName };
            msg = jibsMessages.convert("m_you_play_unlimited", obj);
            out2.println(msg);
            // m_start_match_unlimited=%0 and %1 start an unlimited match.
            oppplayerName = player2.getName();
            obj = new Object[] { playerName, oppplayerName };
            msg = jibsMessages.convert("m_start_match_unlimited", obj);
            player1.informPlayers(msg, player2);
        }
    }

    private JibsNewGameData jibsNewGameData(Player player, Player opponent) {
        JibsNewGameData jngd = null;
        Die die1 = new Die(jibsServer);
        Die die2 = new Die(jibsServer);
        int startTurn = determineStartPlayer(player, opponent, die1, die2);

        switch (startTurn) {
        case 1:
            jngd = new JibsNewGameData(player, opponent, opponent, player, -1,
                                       die2, die1);

            break;

        case -1:
            jngd = new JibsNewGameData(player, opponent, player, opponent, -1,
                                       die1, die2);

            break;
        }

        return jngd;
    }

    private void informNewGame(JibsGame game, Player player, Player opponent) {
        JibsWriter out = player.getOutputStream();

        if (player.canCLIP()) {
            player.getOutputStream().print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        Player.whoinfo(player.getOutputStream(), player, player);

        if (player.canCLIP()) {
            out.print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        Player.whoinfo(player.getOutputStream(), player, opponent);

        if (player.canCLIP()) {
            out.println(ClipConstants.CLIP_WHO_END + " ");
        }

        if (opponent.canCLIP()) {
            opponent.getOutputStream().print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        Player.whoinfo(opponent.getOutputStream(), opponent, player);

        if (opponent.canCLIP()) {
            opponent.getOutputStream().print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        Player.whoinfo(opponent.getOutputStream(), opponent, opponent);

        if (opponent.canCLIP()) {
            opponent.getOutputStream().println(ClipConstants.CLIP_WHO_END +
                                               " ");
        }
    }

    private int determineStartPlayer(Player player1, Player player2,
                                     Die rollDice1, Die rollDice2) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();

        JibsWriter out1 = player1.getOutputStream();
        JibsWriter out2 = player2.getOutputStream();
        String playerName = player1.getName();
        String oppplayerName = player2.getName();
        String msg = null;
        Object[] obj = null;

        do {
            rollDice1.roll();
            rollDice2.roll();
            rollDice1.setValue(3); // joining Player
            rollDice2.setValue(1); // inviting Player

            // m_both_roll=%0 rolled %1, %2 rolled %3
            obj = new Object[] {
                      "You", Integer.valueOf(rollDice1.getValue()),
                      oppplayerName, Integer.valueOf(rollDice2.getValue())
                  };
            msg = jibsMessages.convert("m_both_roll", obj);
            out1.println(msg);
            // m_both_roll=%0 rolled %1, %2 rolled %3
            obj = new Object[] {
                      player1.getName(), Integer.valueOf(rollDice1.getValue()),
                      oppplayerName, Integer.valueOf(rollDice2.getValue())
                  };
            player1.informWatcher("m_both_roll", obj, true);
            player2.informWatcher("m_both_roll", obj, true);
            // m_both_roll=%0 rolled %1, %2 rolled %3
            obj = new Object[] {
                      "You", Integer.valueOf(rollDice2.getValue()), playerName,
                      Integer.valueOf(rollDice1.getValue())
                  };
            msg = jibsMessages.convert("m_both_roll", obj);
            out2.println(msg);
        } while (rollDice1.getValue() == rollDice2.getValue());

        if (rollDice1.getValue() > rollDice2.getValue()) {
            // m_your_turn=It's your turn to move.
            msg = jibsMessages.convert("m_your_turn");
            out1.println(msg);
            // m_makes_first_move=%0 makes the first move.
            obj = new Object[] { playerName };
            msg = jibsMessages.convert("m_makes_first_move", obj);
            out2.println(msg);
            player2.informWatcher("m_makes_first_move", obj, true);
            player1.informWatcher("m_makes_first_move", obj, true);

            return -1;
        } else {
            // m_your_turn=It's your turn to move.
            msg = jibsMessages.convert("m_your_turn");
            out2.println(msg);
            // m_makes_first_move=%0 makes the first move.
            obj = new Object[] { oppplayerName, };
            msg = jibsMessages.convert("m_makes_first_move", obj);
            out1.println(msg);
            player1.informWatcher("m_makes_first_move", obj, true);
            player2.informWatcher("m_makes_first_move", obj, true);

            return 1;
        }
    }
}
