package net.sourceforge.jibs.command;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.server.Server;
import net.sourceforge.jibs.util.ClipConstants;
import net.sourceforge.jibs.util.JibsWriter;

import java.util.HashMap;

/**
 * The Who command.
 */
public class Who_Command implements JibsCommand {
    // ---------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        Server server = jibsServer.getServer();
        HashMap allClients = server.getAllClients();
        JibsWriter out = player.getOutputStream();

        for (Object obj : allClients.values()) {
            Player aPlayer = (Player) obj;
            JibsWriter aout = aPlayer.getOutputStream();

            if (player.canCLIP()) {
                out.print(ClipConstants.CLIP_WHO_INFO + " ");
            }

            Player.whoinfo(out, player, aPlayer);

            if (!aPlayer.getName().equals(player.getName())) {
                if (aPlayer.canCLIP()) {
                    aout.print(ClipConstants.CLIP_WHO_INFO + " ");
                    Player.whoinfo(aout, aPlayer, player);
                    out.println(ClipConstants.CLIP_WHO_END + " ");
                }
            }
        }

        if (player.canCLIP()) {
            out.println(ClipConstants.CLIP_WHO_END + " ");
        }

        return true;
    }
}
