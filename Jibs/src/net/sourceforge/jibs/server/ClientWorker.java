package net.sourceforge.jibs.server;

import com.ibatis.sqlmap.client.SqlMapClient;

import net.sourceforge.jibs.command.JibsCommand;
import net.sourceforge.jibs.command.Login_Command;
import net.sourceforge.jibs.command.Unknown_Command;
import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.gui.JibsTextArea;
import net.sourceforge.jibs.util.JibsTimer;
import net.sourceforge.jibs.util.JibsWriter;
import net.sourceforge.jibs.util.TimeSpan;

import org.apache.log4j.Logger;

import java.io.BufferedReader;

import java.net.InetAddress;
import java.net.Socket;
import java.net.SocketException;

import java.sql.SQLException;

import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.StringTokenizer;

import javax.swing.SwingUtilities;

public class ClientWorker implements Runnable {
    private static final Logger logger = Logger.getLogger(ClientWorker.class);
    private BufferedReader in;
    private long inActiveMillis = 0;
    private JibsTimer inactiveTimer;
    private JibsServer jibsServer;
    private Thread jibsThread;
    private String lastCmd;
    private Date now_Cmd;
    private int once;
    private JibsWriter out;
    private Player player;
    private boolean runs;
    private Server server;
    private Socket socket;
    private Date timeStampOfLastCmd;

    // Constructor
    ClientWorker(JibsMessages jibsMessages, Server Server, Socket client,
                 BufferedReader in, JibsWriter out) {
        this.server = Server;
        this.socket = client;
        this.in = in;
        this.out = out;
        jibsServer = Server.getJibsServer();
        runs = true;
        once = 0;
        inActiveMillis = Integer.parseInt(jibsServer.getResource("ActivityTimeout"));
    }

    public static Player getPlayer(JibsServer jibsServer, Player player,
                                   String name) {
        Server server = jibsServer.getServer();
        HashMap allClients = server.getAllClients();
        Iterator iter = allClients.values().iterator();

        while (iter.hasNext()) {
            Player possibleInvitee = (Player) iter.next();

            if (possibleInvitee.getName().equalsIgnoreCase(name)) {
                return possibleInvitee;
            }
        }

        return null;
    }

    public static Player getPlayer(JibsServer jibsServer, String name) {
        HashMap allClients = jibsServer.getServer().getAllClients();
        Iterator iter = allClients.values().iterator();

        while (iter.hasNext()) {
            Player obj = (Player) iter.next();

            if (obj.getName().equalsIgnoreCase(name)) {
                return obj;
            }
        }

        return null;
    }

    public void connectPlayer(Player player) {
        this.player = player;

        HashMap<String, Player> allClients = getServer().getAllClients();

        allClients.put(player.getName(), player);

        if (jibsServer.useSwing()) {
            SwingUtilities.invokeLater(new Runnable() {
                    public void run() {
                        jibsServer.getStatusBar().getPlayerLabel()
                                  .setText("" +
                                           getServer().getAllClients().size());
                    }
                });
        }
    }

    public void disConnectPlayer(Player player) {
        try {
            this.player = player;
            player.setLast_logout_date(new Date());

            HashMap<String, Player> allClients = getServer().getAllClients();
            allClients.remove(player.getName());

            SqlMapClient sqlClient = jibsServer.getSqlMapInstance();
            player.setLast_logout_date(new Date());
            sqlClient.update("Player.updateLogout", player);

            if (jibsServer.useSwing()) {
                SwingUtilities.invokeLater(new Runnable() {
                        public void run() {
                            jibsServer.getStatusBar().getPlayerLabel()
                                      .setText("" +
                                               getServer().getAllClients().size());
                        }
                    });
            }
        } catch (SQLException e) {
            jibsServer.logException(e);
        }
    }

    public boolean executeCmd(String strCmd) {
        if (player != null) {
            logger.info(player.getName() + ":" + strCmd);
        } else {
            logger.info("<Unknown player>:" + strCmd);
        }

        if (inactiveTimer == null) {
            inactiveTimer = new JibsTimer(jibsServer, this, player,
                                          inActiveMillis);
            jibsThread = new Thread(inactiveTimer);
            jibsThread.start();
        }

        if (inactiveTimer != null) {
            jibsThread.interrupt();

            try {
                jibsThread.join();
            } catch (InterruptedException e) {
                // jibsServer.logException(e);
                jibsThread = null;
            }

            if (inActiveMillis >= 0) {
                jibsThread = new Thread(inactiveTimer);
                jibsThread.start();
            }
        }

        String strArgs = "";
        boolean retCode = true;

        if (strCmd != null) {
            now_Cmd = new Date(new java.util.Date().getTime());

            if (timeStampOfLastCmd == null) {
                timeStampOfLastCmd = now_Cmd;
            }

            TimeSpan duration = new TimeSpan(timeStampOfLastCmd.getTime(),
                                             now_Cmd.getTime());
            Player player = getPlayer();

            if (player != null) {
                player.setIdle(duration);
            }

            timeStampOfLastCmd = now_Cmd;

            StringTokenizer stoken = new StringTokenizer(strCmd);
            int nrOfArgs = stoken.countTokens();
            String[] totalArgs = null;

            if (nrOfArgs > 0) {
                totalArgs = new String[nrOfArgs];
            }

            stoken = new StringTokenizer(strCmd);

            int i = 0;

            while (stoken.hasMoreTokens()) {
                totalArgs[i++] = stoken.nextToken();
            }

            // split the command and the args
            int index = -1;

            if (totalArgs != null) {
                index = strCmd.indexOf(totalArgs[0]);

                if (index >= 0) {
                    strArgs = strCmd.substring(index + totalArgs[0].length());
                }

                // is there a cmd to execute the request?
                JibsCommand fc = jibsServer.getCmd(totalArgs[0]);

                if (fc != null) {
                    if ((lastCmd == null) || !jibsServer.isExitCmd(lastCmd)) {
                        retCode = fc.execute(jibsServer, player, strArgs,
                                             totalArgs);
                    }

                    lastCmd = strCmd;
                } else {
                    JibsCommand jibscmd = new Unknown_Command();

                    jibscmd.execute(jibsServer, player, strArgs, totalArgs);
                }
            }
        }

        return retCode;
    }

    public BufferedReader getInputReder() {
        return in;
    }

    public Player getPlayer() {
        return player;
    }

    public JibsWriter getPrintWriter() {
        return out;
    }

    public Server getServer() {
        return server;
    }

    public Socket getSocket() {
        return socket;
    }

    public void run() {
        try {
            boolean bStopLogin = false;
            JibsServer jibsServer = getServer().getJibsServer();
            String retries = jibsServer.getResource("Retries");
            int MaxRetries = Integer.parseInt(retries);

            while (runs) {
                if (!bStopLogin) {
                    if (!bStopLogin && (once < MaxRetries)) {
                        once++;
                        server.logo(out, once);

                        Login_Command Login = new Login_Command();
                        Player realplayer = Login.login(jibsServer, server,
                                                        socket, out, null,
                                                        null, in);

                        if (realplayer != null) {
                            realplayer.setClientWorker(this);

                            if (realplayer.getName().equalsIgnoreCase("guest")) {
                                bStopLogin = true;
                                runs = false;
                                executeCmd("bye");
                            }
                        }

                        if ((realplayer != null) &&
                                (realplayer.is_valid(realplayer.getUserName(),
                                                         realplayer.getPassword()))) {
                            // Update the DB-record
                            SqlMapClient sqlClient = jibsServer.getSqlMapInstance();
                            connectPlayer(realplayer);

                            bStopLogin = true;

                            if (realplayer.canCLIP()) {
                                realplayer.getJibsSet().getSetMap()
                                          .put("boardstyle", "3");
                            }

                            realplayer.setStreams(in, out);
                            realplayer.clip_welcome(jibsServer, realplayer);
                            realplayer.clip_ownInfo(jibsServer);
                            realplayer.setValid(true);

                            realplayer.setLast_login_date(new Date());

                            Socket client = getSocket();
                            InetAddress iadr = client.getInetAddress();
                            realplayer.setLast_login_host(iadr.getHostName());
                            sqlClient.update("Player.updateLogin", realplayer);

                            executeCmd("motd");
                            executeCmd("who");
                        } else {
                            out.println("Login incorrect");

                            if ((realplayer != null) && (realplayer.canCLIP())) {
                                runs = false;
                                bStopLogin = true;
                                realplayer.getSocket().close();
                            }
                        }
                    } else {
                        // m_too_many_errors=Too many errors for "%0" trying to
                        // log in:Connection closed
                        Object[] obj = { "Unknown User" };

                        String msg = jibsServer.getJibsMessages()
                                               .convert("m_too_many_errors", obj);

                        JibsTextArea.log(jibsServer, msg, true);

                        if (player == null) {
                            getSocket().close();
                        } else {
                            executeCmd("bye");
                        }

                        runs = false;
                    }
                } else {
                    Player clientPlayer = getPlayer();

                    if (!clientPlayer.canCLIP()) {
                        String Prompt = "> ";

                        out.print(Prompt);
                        out.flush();
                    }

                    String strCmd = null;

                    try {
                        strCmd = in.readLine();
                    } catch (SocketException e1) {
                        boolean doLog = e1.getMessage()
                                          .equalsIgnoreCase("Software caused connection abort: recv failed");
                        doLog |= e1.getMessage()
                                   .equalsIgnoreCase("socket closed");

                        if (!doLog) {
                            jibsServer.logException(e1);
                        }
                    }

                    if (strCmd == null) {
                        // Client has disconnected
                        executeCmd("bye");
                        runs = false;
                    } else {
                        // the command the user has entered
                        try {
                            if (strCmd.equalsIgnoreCase("!!")) {
                                strCmd = lastCmd; // repeat the lastCmd
                            }

                            executeCmd(strCmd);
                        } catch (Exception e) {
                            jibsServer.logException(e);
                        }
                    }
                }
            }
        } catch (Exception e) {
            jibsServer.logException(e);
        }
    }

    public void stopWatchThread() {
        jibsThread.interrupt();

        try {
            jibsThread.join();
        } catch (InterruptedException e) {
            jibsThread = null;
        }
    }
}
