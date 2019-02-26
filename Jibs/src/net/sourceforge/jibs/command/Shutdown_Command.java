package net.sourceforge.jibs.command;

import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;
import net.sourceforge.jibs.util.JibsShutdown;
import net.sourceforge.jibs.util.JibsWriter;

import java.text.SimpleDateFormat;

import java.util.Date;

/**
 * The Shutdown command.
 */
public class Shutdown_Command implements JibsCommand {
    public boolean execute(JibsServer jibsServer, Player player,
                           String strArgs, String[] args) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        JibsWriter out = player.getOutputStream();
        String s;
        int minutes = 0;
        boolean bMinutesValid = false;
        boolean bNow = false;
        boolean bRestart = false;

        if (player.isAdmin()) {
            if (args.length >= 1) {
                try {
                    minutes = Integer.valueOf(args[1]);
                    bMinutesValid = true;
                } catch (NumberFormatException e) {
                    if (args[1].equalsIgnoreCase("now")) {
                        bNow = true;
                    }

                    if (args[1].equalsIgnoreCase("stop")) {
                        ;
                    }
                }
            }

            if (args.length >= 3) {
                if (args[2].equalsIgnoreCase("restart")) {
                    bRestart = true;
                }
            }

            // Stop the shutdown process
            JibsShutdown jibsShutdown = jibsServer.getJibsShutdown();

            if (jibsShutdown != null) {
                jibsShutdown.interrupt();

                try {
                    jibsShutdown.join();
                    out.println("Registered shutdown cancelled.");
                } catch (InterruptedException e) {
                    jibsServer.logException(e);
                }
            }

            jibsServer.setJibsShutdown(null);

            if (bNow) {
                shutdown(jibsServer, player, 0, bRestart);
            }

            if (bMinutesValid) {
                int msecs = minutes * 60 * 1000;
                shutdown(jibsServer, player, msecs, bRestart);
            } else {
                // m_jibs_permissiondenied=Permission denied.
                s = jibsMessages.convert("m_jibs_permissiondenied");
                out.println(s);
            }
        }

        return true;
    }

    private void shutdown(JibsServer jibsServer, Player player, int msecs,
                          boolean bRestart) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        JibsWriter out = player.getOutputStream();
        SimpleDateFormat sdf = new SimpleDateFormat("mm:ss");
        Date stopdate = new Date(Long.valueOf(msecs));
        String s = sdf.format(stopdate);

        if (bRestart) {
            // m_jibs_shutdown_restart=jIBS will shutdown in %0 minutes and
            // restart.
            Object[] obj = new Object[] { s };
            s = jibsMessages.convert("m_jibs_shutdown_restart", obj);
            out.println(s);
        } else {
            // m_jibs_shutdown=jIBS will shutdown in %0 minutes.
            Object[] obj = new Object[] { s };
            s = jibsMessages.convert("m_jibs_shutdown", obj);
            out.println(s);
        }

        // Start the shutdown process to shutdown jIBS in msecs time
        JibsShutdown jibsShutdown = new JibsShutdown(jibsServer, bRestart, msecs);
        jibsServer.setJibsShutdown(jibsShutdown);
        jibsShutdown.start();
    }
}
