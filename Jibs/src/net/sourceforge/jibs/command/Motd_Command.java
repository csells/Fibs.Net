package net.sourceforge.jibs.command;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.ClipConstants;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.PrintWriter;

/**
 * The Motd command.
 */
public class Motd_Command implements JibsCommand {
    private void output_motd(JibsServer jibsServer, Player player) {
        BufferedReader reader = null;

        try {
            File motdFile = new File(jibsServer.getResource("motd"));
            reader = new BufferedReader(new FileReader(motdFile));

            PrintWriter out = player.getOutputStream();
            String line = "";

            if (player.canCLIP()) {
                out.println(ClipConstants.CLIP_MOTD_BEGIN);
            }

            do {
                line = reader.readLine();

                if (line != null) {
                    if (line.equals("")) {
                        line = " ";
                    }

                    out.println(line);
                }
            } while (line != null);

            if (player.canCLIP()) {
                out.println(ClipConstants.CLIP_MOTD_END);
            }

            reader.close();
            out.flush();
        } catch (FileNotFoundException e) {
            jibsServer.logException(e);
        } catch (IOException e) {
            jibsServer.logException(e);
        } finally {
            try {
                if (reader != null) {
                    reader.close();
                }
            } catch (IOException e) {
                jibsServer.logException(e);
            }
        }
    }

    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        output_motd(jibsServer, player);

        return true;
    }
}
