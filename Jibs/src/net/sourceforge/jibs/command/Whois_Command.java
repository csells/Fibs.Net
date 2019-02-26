package net.sourceforge.jibs.command;

import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.JibsWriter;

/**
 * The Whois command.
 */
public class Whois_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        String msg = null;
        JibsWriter out = player.getOutputStream();
        JibsMessages jibsMessages = jibsServer.getJibsMessages();

        if (args.length == 2) {
            String Name = args[1];
            Player whoisPlayer = player.getPlayer(player.getJibsServer(), Name);

            if (whoisPlayer != null) {
                whoisPlayer.whois(out);
            } else {
                // m_noone=** There is no one called %0.
                Object[] obj = new Object[] { Name };
                msg = jibsMessages.convert("m_noone", obj);
                out.println(msg);

                return true;
            }
        } else {
            // m_argument_mssing=** please give a name as an argument.
            msg = jibsMessages.convert("m_argument_mssing");
            out.println(msg);
        }

        return true;
    }
}
