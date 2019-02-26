package net.sourceforge.jibs.command;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.PrintWriter;

/**
 * The About command.
 */
public class About_Command implements JibsCommand {
    private void output_about(JibsServer jibsServer, Player player) {
        BufferedReader reader = null;

        try {
            File abaoutFile = new File(jibsServer.getResource("about"));
            reader = new BufferedReader(new FileReader(abaoutFile));

            PrintWriter out = player.getOutputStream();
            String line = "";

            do {
                line = reader.readLine();

                if (line != null) {
                    if (line.equals("")) {
                        line = " ";
                    }

                    out.println(line);
                }
            } while (line != null);

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
        output_about(jibsServer, player);

        return true;
    }
}
