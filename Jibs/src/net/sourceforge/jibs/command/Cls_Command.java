package net.sourceforge.jibs.command;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;

public class Cls_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        return true;
    }
}
