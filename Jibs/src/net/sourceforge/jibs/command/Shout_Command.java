package net.sourceforge.jibs.command;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.server.Server;
import net.sourceforge.jibs.util.ClipConstants;

import java.io.PrintWriter;

import java.util.HashMap;

/**
 * The Shout command.
 */
public class Shout_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        String playerName = player.getName();
        Server server = jibsServer.getServer();
        HashMap allClients = server.getAllClients();
        PrintWriter out = player.getOutputStream();

        out.println(ClipConstants.CLIP_YOU_SHOUT + " " + strArgs);

        for (Object obj : allClients.values()) {
            Player clientPlayer = (Player) obj;

            if (!clientPlayer.getName().equals(player.getName())) {
                if (clientPlayer.checkToggle("silent")) {
                    PrintWriter clientOut = clientPlayer.getOutputStream();

                    clientOut.println(ClipConstants.CLIP_SHOUTS + " " +
                                      playerName + strArgs);
                }
            }
        }

        return true;
    }
}
