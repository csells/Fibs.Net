package net.sourceforge.jibs.backgammon;

import net.sourceforge.jibs.server.JibsServer;

public class Die {
    private int dice_value;
    private JibsServer jibsServer;

    public Die(JibsServer jibsServer) {
        this.jibsServer = jibsServer;
        dice_value = 0;
    }

    public int getValue() {
        return dice_value;
    }

    public void roll() {
        JibsRandom random = jibsServer.getJibsRandom();
        dice_value = random.nextInt(6) + 1;
    }

    public void setValue(int value) {
        dice_value = value;
    }

    public JibsServer getJibsServer() {
        return jibsServer;
    }

    public void setJibsServer(JibsServer jibsServer) {
        this.jibsServer = jibsServer;
    }
}
