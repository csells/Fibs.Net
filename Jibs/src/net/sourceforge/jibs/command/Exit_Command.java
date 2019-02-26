package net.sourceforge.jibs.command;

import net.sourceforge.jibs.backgammon.JibsGame;
import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.gui.JibsTextArea;
import net.sourceforge.jibs.server.*;
import net.sourceforge.jibs.util.ClipConstants;
import net.sourceforge.jibs.util.JibsWriter;

import java.io.*;

import java.util.*;
import java.util.Map.Entry;

/**
 * The Exit command.
 */
public class Exit_Command implements JibsCommand {
    public void displayLogoff(JibsServer jibsServer, JibsWriter out) {
        String sName = jibsServer.getResource("logout");
        File file = new File(sName);
        BufferedReader inp = null;
        ;

        try {
            FileReader fReader = new FileReader(file);
            inp = new BufferedReader(fReader);

            String theLine = inp.readLine();

            while (theLine != null) {
                if (theLine.equals("")) {
                    theLine = " ";
                }

                out.println(theLine);
                theLine = inp.readLine();
            }

            inp.close();
        } catch (FileNotFoundException e) {
            jibsServer.logException(e);
        } catch (IOException e) {
            jibsServer.logException(e);
        } finally {
            try {
                if (inp != null) {
                    inp.close();
                }
            } catch (IOException e) {
                jibsServer.logException(e);
            }
        }
    }

    @SuppressWarnings("unchecked")
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        JibsWriter out = player.getOutputStream();

        try {
            if (out != null) {
                displayLogoff(jibsServer, out);
            }

            JibsGame game = player.getGame();

            if (game != null) {
                if (!game.getBackgammonBoard().isEnded()) {
                    game.save();
                }
            }

            ClientWorker cw = player.getClientWorker();

            if (cw != null) {
                player.getClientWorker().getSocket().close();
                player.getClientWorker().stopWatchThread();
                player.getClientWorker().disConnectPlayer(player);
                cw.getSocket().close();
            }

            // m_you_log_out=%0 logs out.
            Object[] obj = new Object[] { player.getName() };
            String msg = jibsMessages.convert("m_you_log_out", obj);
            JibsTextArea.log(jibsServer, msg, true);

            Map omap = (Map) jibsServer.getServer().getAllClients().clone();
            Map map = Collections.synchronizedMap(omap);
            Set<Entry> set = map.entrySet();

            synchronized (map) {
                for (Entry entry : set) {
                    Player curPlayer = (Player) entry.getValue();
                    msg = ClipConstants.CLIP_LOGOUT + " " + player.getName() +
                          " " + msg;
                    curPlayer.getOutputStream().println(msg);
                }
            }
        } catch (FileNotFoundException e) {
            jibsServer.logException(e);
        } catch (IOException e) {
            jibsServer.logException(e);
        }

        return false;
    }
}
