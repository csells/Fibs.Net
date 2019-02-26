package net.sourceforge.jibs.command;

import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.JibsWriter;

/**
 * The Back command.
 */
public class Back_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        JibsWriter out = player.getOutputStream();
        String msg = null;

        if (player.checkToggle("away")) {
            // m_you_back=Welcome back.
            msg = jibsMessages.convert("m_you_back");
            out.println(msg);
            player.changeToggle("away", Boolean.FALSE);
            player.informToggleChange();
        } else {
            // m_you_back_not_away=** You're not away.
            msg = jibsMessages.convert("m_you_back_not_away");
            out.println(msg);
        }

        return true;
    }
}
