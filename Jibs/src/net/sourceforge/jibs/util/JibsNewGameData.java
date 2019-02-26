package net.sourceforge.jibs.util;

import net.sourceforge.jibs.backgammon.Die;
import net.sourceforge.jibs.server.Player;

public class JibsNewGameData {
    private Player player1;
    private Player player2;
    private Player playerX;
    private Player playerO;
    private Die die1;
    private Die die2;
    private int turn;

    public JibsNewGameData(Player player1, Player player2, Player playerX,
                           Player playerO, int startTurn, Die die1, Die die2) {
        this.playerX = playerX;
        this.playerO = playerO;
        this.player1 = player1;
        this.player2 = player2;
        this.die1 = die1;
        this.die2 = die2;
        this.turn = startTurn;
    }

    public Die getDie1() {
        return die1;
    }

    public void setDie1(Die die1) {
        this.die1 = die1;
    }

    public Die getDie2() {
        return die2;
    }

    public void setDie3(Die die2) {
        this.die2 = die2;
    }

    public Player getPlayerO() {
        return playerO;
    }

    public void setPlayerO(Player player) {
        this.playerO = player;
    }

    public Player getPlayerX() {
        return playerX;
    }

    public void setPlayerX(Player player) {
        this.playerX = player;
    }

    public int getTurn() {
        return turn;
    }

    public void setTurn(int turn) {
        this.turn = turn;
    }

    public Player getPlayer1() {
        return player1;
    }

    public Player getPlayer2() {
        return player2;
    }
}
