package net.sourceforge.jibs.command;

import net.sourceforge.jibs.backgammon.BackgammonBoard;
import net.sourceforge.jibs.backgammon.JibsGame;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.JibsWriter;

/**
 * The Board command.
 */
public class Board_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        JibsGame game = player.getGame();
        JibsWriter out = player.getOutputStream();

        if (game != null) {
            BackgammonBoard board = game.getBackgammonBoard();
            int turn = board.getTurn();

            if (board.isPlayerX(player)) {
                board.outBoard(out, "You", turn, board.getpDie1().getValue(),
                               board.getpDie2().getValue(),
                               board.getoDie1().getValue(),
                               board.getoDie2().getValue());
            } else {
                BackgammonBoard opBoard = board.switch2O();
                opBoard.outBoard(out, "You", turn,
                                 opBoard.getpDie1().getValue(),
                                 opBoard.getpDie2().getValue(),
                                 opBoard.getoDie1().getValue(),
                                 opBoard.getoDie2().getValue());
            }
        }

        return true;
    }
}
