package net.sourceforge.jibs.server;

import com.ibatis.sqlmap.client.SqlMapClient;

import net.sourceforge.jibs.backgammon.BackgammonBoard;
import net.sourceforge.jibs.backgammon.JibsGame;
import net.sourceforge.jibs.backgammon.JibsMatch;
import net.sourceforge.jibs.gui.JibsMessages;
import net.sourceforge.jibs.gui.JibsTextArea;
import net.sourceforge.jibs.util.ClipConstants;
import net.sourceforge.jibs.util.Encoder;
import net.sourceforge.jibs.util.JibsConvert;
import net.sourceforge.jibs.util.JibsNewGameData;
import net.sourceforge.jibs.util.JibsSet;
import net.sourceforge.jibs.util.JibsToggle;
import net.sourceforge.jibs.util.JibsWriter;
import net.sourceforge.jibs.util.TimeSpan;

import org.apache.log4j.Logger;

import java.io.BufferedReader;

import java.net.Socket;

import java.sql.Connection;
import java.sql.SQLException;

import java.text.SimpleDateFormat;

import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Locale;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;
import java.util.TimeZone;

public class Player {
    // ~ Static fields/initializers
    // ---------------------------------------------
    private static Logger logger = Logger.getLogger(Player.class);

    // ~ Instance fields
    // --------------------------------------------------------
    private ClientWorker cw;
    private int clipVersion;
    private JibsServer jibsServer;
    private BufferedReader in;
    private JibsWriter out;
    private boolean valid;
    private TimeSpan idle;
    private JibsGame cur_Game;

    // important fields
    private boolean canCLIP;
    private String clientProgram;
    private Date last_login_date;
    private Date last_logout_date;
    private String last_login_host;
    private String name;
    private boolean admin;
    private JibsQuestion question;
    private Player oppnentPlayer;
    private Player watchingPlayer;
    private String password;
    private double rating;
    private String email;
    private int experience;
    private String strLastLogin;
    private String strLastHost;
    private JibsToggle jibsToggles;
    private JibsSet jibsSet;
    private Server server;
    private int wavings;
    private HashMap<String, Player> watcher;
    private String awayMsg;
    private String toggle;
    private String boardstyle;
    private String linelength;
    private String pagelength;
    private String redoubles;
    private String sortwho;
    private String timezone;

    // ~ Constructors
    // -----------------------------------------------------------
    public Player() {
    }

    public Player(JibsServer jibsServer, Server server, Connection jdbc,
                  JibsWriter out, String clientProgram, int clipVersion,
                  String userName, String passWord, double rating,
                  int experience, String email, boolean admin, Date last_login,
                  String last_host) {
        this.init(jibsServer, server, jdbc, out, clientProgram, clipVersion,
                  userName, passWord, rating, experience, admin, last_login,
                  last_host);
    }

    public Player(JibsServer jibsServer, Server server) {
        this.init(jibsServer, server, null, out, "", -1, "", "", 0.0, 0, false,
                  null, "");
    }

    public Player(JibsServer jibsServer, String name) {
        this.init(jibsServer, server, null, out, "", -1, name, "", 0.0, 0,
                  false, null, "");
    }

    public Player(JibsServer jibsServer, Server server, Connection jdbc,
                  JibsWriter out, String clientProgram, int clipVersion,
                  String strUser, String strPassword) {
        this.init(jibsServer, server, jdbc, out, clientProgram, clipVersion,
                  strUser, strPassword, 1500.0, 0, false, new Date(), null);
    }

    // ~ Methods
    // ----------------------------------------------------------------
    private void init(JibsServer jibsServer, Server server, Connection jdbc,
                      JibsWriter out, String clientProgram, int ClipVersion,
                      String strUser, String strPassword, double rating,
                      int experience, boolean admin, Date last_login,
                      String last_host) {
        this.jibsServer = jibsServer;
        this.server = server;
        this.out = out;
        this.clientProgram = clientProgram;
        this.clipVersion = ClipVersion;
        this.canCLIP = ClipVersion >= 0;

        idle = new TimeSpan(0, 0);
        this.rating = rating;
        this.last_login_date = last_login;
        this.last_login_host = last_host;
        this.name = strUser;
        this.password = strPassword;

        jibsToggles = null;
        jibsSet = null;
        wavings = 0;
        watcher = null;
    }

    public int getExperience() {
        return experience;
    }

    public void setGame(JibsGame game) {
        this.cur_Game = game;
    }

    public boolean getAdmin() {
        return admin;
    }

    public JibsGame getGame() {
        return cur_Game;
    }

    public void startGame(JibsServer jibsServer, JibsNewGameData jngd,
                          JibsGame game, Player player1, Player player2,
                          int length, int turn, JibsMatch matchVersion,
                          int mayDouble1, int mayDouble2) {
        BackgammonBoard board = game.getBackgammonBoard();
        game.startGame(turn, board.getpDie1(), board.getpDie2(),
                       board.getPlayerXPoints(), board.getPlayerOPoints(),
                       mayDouble1, mayDouble2);
    }

    public ClientWorker getClientWorker() {
        return cw;
    }

    public boolean canCLIP() {
        return canCLIP;
    }

    public void setCLIP(boolean canCLIP) {
        this.canCLIP = canCLIP;
    }

    public void setOpponent(Player player) {
        oppnentPlayer = player;
    }

    public void setMatchLength(int nrMatches) {
    }

    public String getName() {
        return ((name != null) && (name.length() > 0)) ? name : "-";
    }

    public String getPassword() {
        return password;
    }

    public double getRating() {
        return rating;
    }

    public String getTimezone() {
        return timezone;
    }

    public long getIdle() {
        long tend = idle.getEnd();
        long tstart = idle.getStart();

        return (tend - tstart) / 1000;
    }

    public Player getOpponent() {
        return oppnentPlayer;
    }

    public Player getWatcher() {
        return watchingPlayer;
    }

    public String getClientProgram() {
        return clientProgram;
    }

    public String getUserName() {
        return name;
    }

    public String getHostName() {
        return strLastHost;
    }

    public Date getLast_login_date() {
        return last_login_date;
    }

    public Date getLast_logout_date() {
        return last_logout_date;
    }

    public void setLast_logout_date(Date last_logout_date) {
        this.last_logout_date = last_logout_date;
    }

    public String getLastLoginHost() {
        return strLastHost;
    }

    public String getEmail() {
        if (email == null) {
            return "-";
        }

        return email;
    }

    public JibsServer getJibsServer() {
        return jibsServer;
    }

    public void setStreams(BufferedReader in, JibsWriter out) {
        this.in = in;
        this.out = out;
    }

    public void setJibsServer(JibsServer server) {
        this.jibsServer = server;
    }

    public void setIdle(TimeSpan idle) {
        this.idle = idle;
    }

    public BufferedReader getInputStream() {
        return in;
    }

    public JibsWriter getOutputStream() {
        return out;
    }

    public void changeRating(double d, int exp) {
        this.rating = d;
        this.experience = exp;

        try {
            SqlMapClient sqlClient = jibsServer.getSqlMapInstance();
            sqlClient.update("Player.updateRating", this);
        } catch (SQLException e) {
            jibsServer.logException(e);
        }
    }

    public boolean changeAddress(String newadr) {
        try {
            this.email = newadr;

            SqlMapClient sqlClient = jibsServer.getSqlMapInstance();
            sqlClient.update("Player.updateMail", this);

            return true;
        } catch (Exception e) {
            jibsServer.logException(e);
        }

        return false;
    }

    public void clip_welcome(JibsServer jibsServer, Player player) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        JibsWriter out = getOutputStream();

        // output connect information
        String strName = getName();
        String myDateStr = "-";

        // m_log_in=%0 logs in.
        Object[] obj = new Object[] { strName };
        String msg = jibsMessages.convert("m_log_in", obj);

        JibsTextArea.log(jibsServer, msg, true);

        // player.informPlayers(msg, null);
        Map map = jibsServer.getServer().getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();
            Player curPlayer = (Player) entry.getValue();

            if (!curPlayer.getName().equalsIgnoreCase(player.getName())) {
                String omsg = ClipConstants.CLIP_LOGIN + " " +
                              player.getName() + " " + msg;
                curPlayer.getOutputStream().println(omsg);
            }
        }

        if (last_login_date != null) {
            strLastLogin = Long.toString(last_login_date.getTime() / 1000);

            SimpleDateFormat formatter = new SimpleDateFormat("E MMMMMMMMMM dd HH:mm:ss yyyy z",
                                                              Locale.US);
            TimeZone tz = TimeZone.getTimeZone(getTimezone());
            formatter.setTimeZone(tz);
            myDateStr = formatter.format(last_login_date);
        } else {
            myDateStr = "'Not known'";
        }

        if (last_login_host != null) {
            strLastHost = last_login_host;
        }

        if (player.canCLIP()) {
            out.println("");

            StringBuffer bf = new StringBuffer();

            bf.append(ClipConstants.CLIP_WELCOME + " " + strName + " " +
                      strLastLogin + " " + strLastHost);
            out.println(bf.toString());
        } else {
            // m_welcome=Welcome %0.
            obj = new Object[] { strName };
            msg = jibsMessages.convert("m_welcome", obj);
            out.println(msg);

            // m_you_lastlogin=Your last login was %0 from %1.
            obj = new Object[] { myDateStr, player.getLastLoginHost() };
            msg = jibsMessages.convert("m_you_lastlogin", obj);
            out.println(msg);
        }
    }

    public void informPlayers(String msg, Player outsider) {
        // send msg to all other player's except outsider
        Map map = jibsServer.getServer().getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();
            Player curPlayer = (Player) entry.getValue();

            if (outsider == null) {
                if (!curPlayer.getName().equals(getName())) {
                    if (curPlayer.checkToggle("notify")) {
                        JibsWriter out = curPlayer.getOutputStream();

                        out.println(msg);
                    }
                }
            } else {
                if (!curPlayer.getName().equals(getName()) &&
                        !curPlayer.getName().equals(outsider.getName())) {
                    if (curPlayer.checkToggle("notify")) {
                        JibsWriter out = curPlayer.getOutputStream();

                        out.println(msg);
                    }
                }
            }
        }
    }

    public static void whoinfo(JibsWriter out, Player clientPlayer,
                               Player loginPlayer) {
        if (clientPlayer != null) {
            String name = loginPlayer.getName();
            String opponent = "-";
            Player oppPlayer = loginPlayer.getOpponent();

            if (oppPlayer != null) {
                opponent = oppPlayer.getName();
            }

            String watching = "-";
            Player watchPlayer = loginPlayer.getWatcher();

            if (watchPlayer != null) {
                watching = watchPlayer.getName();
            }

            boolean ready = loginPlayer.checkToggle("ready");
            boolean away = loginPlayer.checkToggle("away");
            double rating = loginPlayer.getRating();
            int experience = loginPlayer.getExperience();
            long idleSecs = loginPlayer.getIdle();
            Date login = loginPlayer.getLast_login_date();
            String hostname = loginPlayer.getHostName();
            String client = loginPlayer.getClientProgram();
            String email = loginPlayer.getEmail();

            StringBuffer bf = new StringBuffer();

            bf.append(name + " ");
            bf.append(opponent + " ");
            bf.append(watching + " ");
            bf.append(JibsConvert.convBoolean(ready) + " ");
            bf.append(JibsConvert.convBoolean(away) + " ");
            bf.append(JibsConvert.convdouble(rating, 2) + " ");
            bf.append(experience + " ");
            bf.append(idleSecs + " ");

            if (login == null) {
                long t = new java.util.Date().getTime();
                bf.append(t + " ");
            } else {
                bf.append(login.getTime());
                bf.append(" ");
            }

            if (hostname == null) {
                bf.append("- ");
            } else {
                bf.append(hostname + " ");
            }

            bf.append(client + " ");
            bf.append(email + " ");
            // out.println("aleucht - - 1 0 1558.84 27 0 1152787221306 localhost
            // <DEB.eucht_________! - ");
            out.println(bf.toString());
        }
    }

    public void setValid(boolean is_valid) {
        this.valid = is_valid;
    }

    public Player getPlayer(JibsServer jibsServer, String Name) {
        try {
            SqlMapClient sqlMap = jibsServer.getSqlMapInstance();
            Player retPlayer = (Player) sqlMap.queryForObject("Player.getPlayer",
                                                              Name);

            if (retPlayer != null) {
                retPlayer.setJibsServer(jibsServer);
            }

            return retPlayer;
        } catch (Exception e) {
            jibsServer.logException(e);
        }

        return null;
    }

    public void whois(JibsWriter out) {
        /*
         * out.println("Information about bleucht"); out.println(" Last login:
         * Saturday, October 14 12:02 UTC from dslnet.85-22-11.ip187.dokom.de");
         * out.println(" Last logout: Saturday, October 14 12:24 UTC");
         * out.println(" Rating: 1457.75 Experience: 10");
         */
        out.println("Information about " + name + ":");

        SimpleDateFormat formatter = new SimpleDateFormat("E MMMMMMMMMM dd HH:mm:ss yyyy z",
                                                          Locale.US);
        TimeZone tz = TimeZone.getTimeZone(getTimezone());
        formatter.setTimeZone(tz);

        String myDateStr = formatter.format(last_login_date);
        out.println("Last login:  " + myDateStr);

        if (last_logout_date != null) {
            myDateStr = formatter.format(last_logout_date);
            out.println("Last logout: " + myDateStr);
        } else {
            out.println("Last logout: Not known");
        }

        if (jibsServer.getServer().isPlayerOnline(this)) {
            out.println("logged in right now.");
        } else {
            out.println("Not logged in right now.");
        }

        out.println("Rating: " + rating + " Experience: " + experience);

        if (email.equals("-")) {
            out.println("No email address.");
        } else {
            out.println("Email: " + email);
        }
    }

    public boolean is_valid() {
        return valid;
    }

    public boolean is_valid(String strUser, String strPassword) {
        boolean retCode = false;

        try {
            SqlMapClient sqlMap = jibsServer.getSqlMapInstance();
            Player retPlayer = (Player) sqlMap.queryForObject("Player.getPlayer",
                                                              strUser);

            if (retPlayer != null) {
                name = retPlayer.getName();
                password = retPlayer.getPassword();
                rating = retPlayer.getRating();
                experience = retPlayer.getExperience();
                email = retPlayer.getEmail();
                timezone = retPlayer.getTimezone();
                admin = retPlayer.getAdmin();
                jibsToggles = new JibsToggle(retPlayer.getToggle());
                jibsSet = new JibsSet(retPlayer.getBoardStyle(),
                                      retPlayer.getLineLength(),
                                      retPlayer.getPageLength(),
                                      retPlayer.getRedoubles(),
                                      retPlayer.getSortwho(),
                                      retPlayer.getTimezone());

                if (email.equals("")) {
                    email = "-";
                }

                last_login_date = retPlayer.getLast_login_date();
                last_logout_date = retPlayer.getLast_logout_date();
                last_login_host = retPlayer.getLast_login_host();

                if (name.equals(strUser)) {
                    if (password.equals(Encoder.encrypt(jibsServer,
                                                            strPassword, "MD5"))) {
                        retCode = true;
                    } else {
                        String conString = "Connect unknown password" + "(" +
                                           strUser + "," + strPassword + ")";

                        JibsTextArea.log(jibsServer, conString, true);
                    }
                } else {
                    String conString = "Connect unknown user" + "(" + strUser +
                                       "," + strPassword + ")";

                    JibsTextArea.log(jibsServer, conString, true);
                    logger.warn(conString);
                }
            } else {
                String conString = "Connect unknown user" + "(" + strUser +
                                   "," + strPassword + ")";

                JibsTextArea.log(jibsServer, conString, true);
                logger.warn(conString);
            }
        } catch (SQLException sqle) {
            jibsServer.logException(sqle);
            retCode = false;
        }

        return retCode;
    }

    public void setName(String string) {
        this.name = string;
    }

    public void setPassword(String string) {
        this.password = string;
    }

    public void setEmail(String string) {
        this.email = string;
    }

    public void setRating(double double1) {
        this.rating = double1;
    }

    public void setExperience(int int1) {
        this.experience = int1;
    }

    public void setLastHost(String string) {
        this.strLastHost = string;
    }

    public void setLastLogin(Date timestamp) {
        if (timestamp != null) {
            this.last_login_date = timestamp;
        } else {
            this.last_login_date = null;
        }
    }

    public void setAdmin(int boolean1) {
        this.admin = boolean1 > 0;
    }

    public void setQuestion(JibsQuestion question) {
        this.question = question;
    }

    public void ask(JibsQuestion question) {
        this.question = question;
    }

    public JibsQuestion getQuestion() {
        return question;
    }

    public void endGame(Player opponent) {
        if (canCLIP()) {
            getOutputStream().print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        setOpponent(null);
        whoinfo(getOutputStream(), this, this);

        if (canCLIP()) {
            getOutputStream().print(ClipConstants.CLIP_WHO_INFO + " ");
        }

        opponent.setOpponent(null);
        whoinfo(getOutputStream(), this, opponent);
    }

    public void setClientWorker(ClientWorker cw2) {
        this.cw = cw2;
    }

    public void setClientProgram(String clientProgram) {
        this.clientProgram = clientProgram;
    }

    public JibsSet getJibsSet() {
        return jibsSet;
    }

    public Socket getSocket() {
        if (cw != null) {
            return cw.getSocket();
        } else {
            return null;
        }
    }

    public void canCLIP(boolean b) {
        this.canCLIP = b;
    }

    public JibsToggle getJibsToggles() {
        return jibsToggles;
    }

    public void informToggleChange() {
        Map map = jibsServer.getServer().getAllClients();
        Set set = map.entrySet();
        Iterator iter = set.iterator();

        while (iter.hasNext()) {
            Entry entry = (Entry) iter.next();

            Player curPlayer = (Player) entry.getValue();
            JibsWriter out = curPlayer.getOutputStream();

            if (curPlayer.canCLIP()) {
                out.print(ClipConstants.CLIP_WHO_INFO + " ");
            }

            Player.whoinfo(out, curPlayer, this);

            if (curPlayer.canCLIP()) {
                out.println(ClipConstants.CLIP_WHO_END + " ");
            }
        }
    }

    public Server getServer() {
        return server;
    }

    public boolean checkToggle(String string) {
        return jibsToggles.get(string).booleanValue();
    }

    public int getWavings() {
        return wavings;
    }

    public void setWavings(int wavings) {
        this.wavings = wavings;
    }

    public void clip_ownInfo(JibsServer jibsServer) {
        StringBuffer bf = new StringBuffer();

        bf.append(ClipConstants.CLIP_OWN_INFO);
        bf.append(" ");
        bf.append(getName());
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("allowpip")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("autoboard")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("autodouble")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("automove")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("away")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("bell")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("crawford")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("double")));
        bf.append(" ");
        bf.append(experience);
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("greedy")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("moreboards")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("moves")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("notify")));
        bf.append(" ");
        bf.append(JibsConvert.convdouble(rating, 2));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("ratings")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("ready")));
        bf.append(" ");
        bf.append("0"); // redoubles
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("report")));
        bf.append(" ");
        bf.append(JibsConvert.convBoolean(checkToggle("silent")));
        bf.append(" ");
        bf.append("UTC"); // Timezone

        if (canCLIP()) {
            JibsWriter out = getOutputStream();
            out.println(bf.toString());
        }
    }

    public void addWatcher(Player player) {
        JibsWriter out = getOutputStream();
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        String msg = null;

        if (watcher == null) {
            watcher = new HashMap<String, Player>();
        }

        if (!watcher.containsKey(player.getName())) {
            watcher.put(player.getName(), player);

            // m_you_watch=You're now watching %0.
            Object[] obj = new Object[] { getName() };
            msg = jibsMessages.convert("m_you_watch", obj);
            player.getOutputStream().println(msg);
            // m_other_watch=%0 is watching you.
            obj = new Object[] { player.getName() };
            msg = jibsMessages.convert("m_other_watch", obj);
            out.println(msg);
        }
    }

    public void show2WatcherBoard(BackgammonBoard board, String name, int i,
                                  int j, int k, int l) {
        if (watcher != null) {
            Set set = watcher.entrySet();
            Iterator iter = set.iterator();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                Player curPlayer = (Player) entry.getValue();

                if (curPlayer != null) {
                    JibsWriter out = curPlayer.getOutputStream();
                    board.outBoard(out, name, board.getTurn(), i, j, k, l);
                }
            }
        }
    }

    public String getAwayMsg() {
        return awayMsg;
    }

    public void setAwayMsg(String awayMsg) {
        this.awayMsg = awayMsg;
    }

    public void show2WatcherMove(String msg) {
        if (watcher != null) {
            Set set = watcher.entrySet();
            Iterator iter = set.iterator();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                Player curPlayer = (Player) entry.getValue();

                if (curPlayer != null) {
                    JibsWriter out = curPlayer.getOutputStream();
                    out.println(msg);
                }
            }
        }
    }

    public void show2WatcherRoll(String msg) {
        if (watcher != null) {
            Set set = watcher.entrySet();
            Iterator iter = set.iterator();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                Player curPlayer = (Player) entry.getValue();

                if (curPlayer != null) {
                    JibsWriter out = curPlayer.getOutputStream();
                    out.println(msg);
                }
            }
        }
    }

    public Player getWatchingPlayer() {
        return watchingPlayer;
    }

    public void setWatchingPlayer(Player watchingPlayer) {
        this.watchingPlayer = watchingPlayer;
    }

    public void stopWatching(Player player) {
        JibsWriter out = getOutputStream();
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        String msg = null;

        if (watcher != null) {
            Set set = watcher.entrySet();
            Iterator iter = set.iterator();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                String name = (String) entry.getKey();

                if (name.equals(player.getName())) {
                    iter.remove();

                    // m_you_stop_watch=You stop watching %0.
                    Object[] obj = new Object[] { getName() };
                    msg = jibsMessages.convert("m_you_stop_watch", obj);
                    player.getOutputStream().println(msg);

                    // m_other_watch_stop=%0 stops watching you.
                    obj = new Object[] { player.getName() };
                    msg = jibsMessages.convert("m_other_watch_stop", obj);
                    out.println(msg);
                }
            }
        }
    }

    public int informWatcher(String string, Object[] obj, boolean doConvert) {
        JibsMessages jibsMessages = jibsServer.getJibsMessages();
        String msg = null;
        int heard = 0;

        if (watcher != null) {
            Set set = watcher.entrySet();
            Iterator iter = set.iterator();

            while (iter.hasNext()) {
                Entry entry = (Entry) iter.next();
                Player wPlayer = (Player) entry.getValue();
                JibsWriter wOut = wPlayer.getOutputStream();

                if (doConvert) {
                    msg = jibsMessages.convert(string, obj);
                } else {
                    msg = string;
                }

                wOut.println(msg);
                heard++;
            }
        }

        return heard;
    }

    public void changePassword(String newPassword1) {
        this.password = newPassword1;

        SqlMapClient sqlClient = jibsServer.getSqlMapInstance();

        try {
            sqlClient.update("Player.updatePassword", this);
        } catch (SQLException e) {
            jibsServer.logException(e);
        }
    }

    public void changeToggle(String string, Boolean value) {
        jibsToggles.getToggleMap().put(string, value);
    }

    public void setLast_login_date(Date last_login_date) {
        this.last_login_date = last_login_date;
    }

    public String getLast_login_host() {
        return last_login_host;
    }

    public void setLast_login_host(String last_login_host) {
        this.last_login_host = last_login_host;
    }

    public String getBoardStyle() {
        return boardstyle;
    }

    public void setBoardStyle(String boardStyle) {
        this.boardstyle = boardStyle;
    }

    public String getLineLength() {
        return linelength;
    }

    public void setLineLength(String lineLength) {
        this.linelength = lineLength;
    }

    public String getPageLength() {
        return pagelength;
    }

    public void setPageLength(String pageLength) {
        this.pagelength = pageLength;
    }

    public String getRedoubles() {
        return redoubles;
    }

    public void setRedoubles(String redoubles) {
        this.redoubles = redoubles;
    }

    public String getSortwho() {
        return sortwho;
    }

    public void setSortwho(String sortwho) {
        this.sortwho = sortwho;
    }

    public String getToggle() {
        return toggle;
    }

    public void setToggle(String toggle) {
        this.toggle = toggle;
    }

    public void setTimezone(String timezone) {
        this.timezone = timezone;
    }

    public int getClipVersion() {
        return clipVersion;
    }

    public void setClipVersion(int clipVersion) {
        this.clipVersion = clipVersion;
    }

    public boolean isAdmin() {
        return admin;
    }
}
