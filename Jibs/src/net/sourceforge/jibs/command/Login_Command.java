package net.sourceforge.jibs.command;

import net.sourceforge.jibs.gui.JibsTextArea;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.server.Server;
import net.sourceforge.jibs.util.JibsWriter;

import java.io.BufferedReader;
import java.io.IOException;

import java.net.Socket;

import java.sql.Connection;

import java.util.StringTokenizer;

/**
 * The Login_Command will be invoked by jIBS when a user tries to login. Though
 * it adheres to the JibsCommand interface, normal processing is done by jIBS in
 * a different way.
 */
public class Login_Command implements JibsCommand {
    public Player login(JibsServer jibsServer, Server server, Socket client,
                        JibsWriter out, JibsTextArea fta, Connection jdbc,
                        BufferedReader in) {
        Player player = null;
        String strUser = null;

        try {
            strUser = in.readLine();
        } catch (IOException e) {
            jibsServer.logException(e);
        }

        if (strUser != null) {
            StringTokenizer stoken = new StringTokenizer(strUser);
            String[] MyArgs = new String[5];
            int i = 0;

            while (stoken.hasMoreTokens()) {
                String String_Token = stoken.nextToken();

                if (String_Token.equals("login")) {
                    MyArgs[0] = "login";
                    i = 1;
                } else if (i == 0) {
                    MyArgs[0] = String_Token;
                    i = 1;
                } else if (i >= 1) {
                    MyArgs[i++] = String_Token;
                }
            }

            if ((i == 1) && MyArgs[0].equalsIgnoreCase("guest")) {
                NewUser_Command cmd = new NewUser_Command(jibsServer, in, out);

                player = cmd.createNewUser(jibsServer, server, client, "", null);

                if (player != null) {
                    player.canCLIP(false);
                }

                return player;
            }

            if (i == 5) {
                // bypass password confirmation
                try {
                    player = new Player(jibsServer, server, jdbc, out,
                                        MyArgs[1], Integer.parseInt(MyArgs[2]),
                                        MyArgs[3], MyArgs[4]);
                } catch (NumberFormatException e) {
                    jibsServer.logException(e);

                    return null;
                }

                player.canCLIP(true);
            } else {
                // normal password confirmation
                out.print("Password: ");
                out.flush();

                String strPassword = null;

                try {
                    strPassword = in.readLine();
                } catch (IOException e1) {
                    jibsServer.logException(e1);
                }

                player = new Player(jibsServer, server, jdbc, out, "-", -1,
                                    strUser, strPassword);
                player.canCLIP(false);
            }
        }

        return player;
    }

    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        return true;
    }
}
