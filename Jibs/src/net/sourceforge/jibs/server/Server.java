package net.sourceforge.jibs.server;

import net.sourceforge.jibs.command.*;
import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.util.JibsShutdown;
import net.sourceforge.jibs.util.JibsWriter;

import org.apache.log4j.Logger;

import java.io.*;

import java.net.ServerSocket;
import java.net.Socket;

import java.text.DateFormat;
import java.text.SimpleDateFormat;

import java.util.*;
import java.util.Map.Entry;

public class Server implements Runnable {
    private static final Logger logger = Logger.getLogger(Server.class);
    private ServerSocket listener;
    private JibsMessages jibsMessages;
    private HashMap<String, Player> allClients;
    private JibsServer jibsServer;
    private boolean runs;

    public Server(JibsMessages jibsMessages, JibsServer server,
                  ServerSocket listener, int portno) {
        this.listener = listener;
        this.jibsServer = server;
        this.jibsMessages = jibsMessages;
        allClients = new HashMap<String, Player>();
        runs = true;
    }

    public JibsServer getJibsServer() {
        return jibsServer;
    }

    public HashMap<String, Player> getAllClients() {
        return allClients;
    }

    public boolean runs() {
        return runs;
    }

    public void logo(JibsWriter out, int nrTries) {
        BufferedReader inp = null;

        try {
            if (nrTries == 1) {
                String sName = jibsServer.getResource("login");
                File file = new File(sName);
                FileReader fReader = new FileReader(file);
                inp = new BufferedReader(fReader);

                String theLine = inp.readLine();

                while (theLine != null) {
                    out.println(theLine);
                    theLine = inp.readLine();
                }

                inp.close();

                SimpleDateFormat formatter = new SimpleDateFormat("MMMMMMMMMM dd HH:mm:ss yyyy z",
                                                                  Locale.US);
                Date now = new Date();
                String myLoginStr1 = formatter.format(now);

                DateFormat fmt = new SimpleDateFormat("MMMMMMMMMM dd HH:mm:ss yyyy z",
                                                      Locale.US);

                fmt.setTimeZone(TimeZone.getTimeZone("UTC"));

                String myLoginStr2 = fmt.format(now);

                out.println(myLoginStr1 + "        [" + myLoginStr2 + "]");
            }

            out.print("login: ");
            out.flush();
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

    public void run() {
        while (runs) {
            Socket client;
            BufferedReader in;
            JibsWriter out;
            boolean bLogin = true;

            try {
                client = listener.accept();

                in = new BufferedReader(new InputStreamReader(client.getInputStream()));
                out = new JibsWriter(client.getOutputStream());

                JibsShutdown jibsShutdown = jibsServer.getJibsShutdown();

                if (jibsShutdown != null) {
                    Date dt = jibsShutdown.getShutdownDate();
                    Date now = new Date();
                    long diff = Math.abs(now.getTime() - dt.getTime());

                    if (diff < (10 * 60 * 1000)) {
                        out.println("jIBS will shutdown. No login allowed.");
                        client.close();
                        bLogin = false;
                    }
                }

                if (bLogin) {
                    ClientWorker w = new ClientWorker(jibsMessages, this,
                                                      client, in, out);
                    Thread t = new Thread(w);
                    t.start();
                }
            } catch (IOException e) {
                boolean doLog = e.getMessage()
                                 .equalsIgnoreCase("Socket is closed");
                doLog |= e.getMessage().equalsIgnoreCase("socket closed");

                if (!doLog) {
                    jibsServer.logException(e);
                }
            }
        }
    }

    public Player getPlayer(String string) {
        HashMap map = getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();
            Player curPlayer = (Player) entry.getValue();

            if (curPlayer.getName().equals(string)) {
                return curPlayer;
            }
        }

        return null;
    }

    public Collection<Player> getAwayPlayer() {
        HashMap map = getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();
        Collection<Player> list = new ArrayList<Player>();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();
            Player curPlayer = (Player) entry.getValue();

            if (curPlayer.checkToggle("away")) {
                list.add(curPlayer);
            }
        }

        if (list.size() > 0) {
            return list;
        } else {
            return null;
        }
    }

    public static Logger getLogger() {
        return logger;
    }

    public JibsMessages getJibsMessages() {
        return jibsMessages;
    }

    public void setJibsMessages(JibsMessages jibsMessages) {
        this.jibsMessages = jibsMessages;
    }

    public ServerSocket getListener() {
        return listener;
    }

    public void setListener(ServerSocket listener) {
        this.listener = listener;
    }

    public boolean isRuns() {
        return runs;
    }

    public void setRuns(boolean runs) {
        this.runs = runs;
    }

    public void setAllClients(HashMap<String, Player> allClients) {
        this.allClients = allClients;
    }

    public void setJibsServer(JibsServer jibsServer) {
        this.jibsServer = jibsServer;
    }

    @SuppressWarnings("unchecked")
    public void closeAllClients() {
        Map<String, Player> omap = (Map) getAllClients().clone();
        Map map = Collections.synchronizedMap(omap);

        synchronized (map) {
            Set set = map.entrySet();
            Iterator iter = set.iterator();
            Exit_Command exitCommand = new Exit_Command();
            JibsCommand leaveCommand = new Leave_Command();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                Player curPlayer = (Player) entry.getValue();
                leaveCommand.execute(jibsServer, curPlayer, "", null);
                exitCommand.displayLogoff(getJibsServer(),
                                          curPlayer.getOutputStream());

                ClientWorker cw = curPlayer.getClientWorker();

                if (cw != null) {
                    cw.disConnectPlayer(curPlayer);
                    cw.stopWatchThread();

                    try {
                        cw.getSocket().close();
                    } catch (IOException e) {
                        jibsServer.logException(e);
                    }
                }
            }
        }
    }

    public boolean isPlayerOnline(Player player) {
        boolean retCode = false;
        HashMap map = getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();
            Player curPlayer = (Player) entry.getValue();

            if (curPlayer.getName().equals(player.getName())) {
                retCode = true;
                ;
            }
        }

        return retCode;
    }
}
