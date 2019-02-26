package net.sourceforge.jibs.server;

import com.ibatis.common.resources.Resources;

import com.ibatis.sqlmap.client.SqlMapClient;
import com.ibatis.sqlmap.client.SqlMapClientBuilder;

import net.sourceforge.jibs.backgammon.JibsRandom;
import net.sourceforge.jibs.command.*;
import net.sourceforge.jibs.gui.*;
import net.sourceforge.jibs.util.*;

import org.apache.log4j.Logger;

import java.awt.Dimension;
import java.awt.event.KeyEvent;

import java.io.*;

import java.net.ServerSocket;
import java.net.URL;

import java.util.*;

import javax.swing.*;

public class JibsServer {
    // -------------------------------------------------------------------------------------------------
    private static final Logger logger = Logger.getLogger(JibsServer.class);
    private static final long serialVersionUID = -4324503684036776752L;
    private JFrame jibsFrame;
    private String confFileName = null;
    private SqlMapClient sqlMap = null;
    private Map<String, JibsCommand> allCmds;
    private JibsDocument doc = null;
    private JibsConfiguration fConf = null;
    private JibsConsole jibsConsole = null;
    private JibsMessages jibsMessages = null;
    private JibsRandom jibsRandom;
    private JibsUserPanel jibsUserPanel = null;
    private javax.swing.JInternalFrame jInternalFrame1;
    private ServerSocket listener = null;
    private JibsWriter logWriter;
    private int portno;
    private JMenuItem startMenu;
    private JMenuItem stopMenu;
    private Thread serverThread;
    private boolean bServerRuns = false;
    private JibsGui jibsGUI = null;
    private JibsStatusBar jibsStatusbar = null;
    private InfoAction infoAction;
    private RunAction runAction;
    private StopAction stopAction;
    private ExitAction exitAction;
    private ReloadAction reloadAction;
    private JibsShutdown jibsShutdown;
    private boolean bUseSwing;

    // ------------------------------------------------------------------------------------------------------------
    private Server theServer = null;

    // ---------------------------------------------------------------------------------------------------------------
    public JibsServer(String fileName) {
        confFileName = fileName;
        fConf = new JibsConfiguration(this, fileName);
        bUseSwing = Boolean.parseBoolean(fConf.getResource("useSwing"));

        if (useSwing()) {
            initComponents();

            Dimension dim = getJibsFrame().getToolkit().getScreenSize();
            int width = (int) (dim.getWidth());

            getJibsFrame().setSize(width, 490);
        }
    }

    public static void main(String[] args) {
        logger.info("Trying to start sever");

        JibsServer jibsServer = new JibsServer(args[0]);

        if (jibsServer.useSwing()) {
            jibsServer.getJibsFrame().setLocation(0, 0);
            jibsServer.getJibsFrame()
                      .setTitle(jibsServer.getResource("aboutVersion"));

            URL imgURL = ClassLoader.getSystemResource("images/jibs_thumb.gif");
            ImageIcon image = new ImageIcon(imgURL);
            jibsServer.getJibsFrame().setIconImage(image.getImage());
        }

        jibsServer.startServerMenuItemActionPerformed(null);
    }

    public JibsCommand getCmd(String strCmd) {
        // get best match for strCmd to be executed, unless not ambigous
        // re -> unknown command re (conflict (ambigous) between 'resign' and
        // 'reject')
        String sCmd = strCmd.toLowerCase();
        JibsCommand cmd = null;

        Iterator cmdIterator = allCmds.keySet().iterator();

        int matches = 0;

        while (cmdIterator.hasNext()) {
            String cmdString = (String) cmdIterator.next();

            if (cmdString.startsWith(sCmd)) {
                cmd = (JibsCommand) allCmds.get(cmdString);
                matches++;
            }

            if (cmdString.equalsIgnoreCase(sCmd)) {
                cmd = (JibsCommand) allCmds.get(cmdString);
                matches = 0; // exact match, use it immediately

                break;
            }
        }

        if (matches > 1) {
            return null; // cmd is ambigous
        }

        return cmd;
    }

    // --------------------------------------------------------------------------------------------------------------------
    public JibsMessages getJibsMessages() {
        return jibsMessages;
    }

    public JibsRandom getJibsRandom() {
        return jibsRandom;
    }

    public JibsUserPanel getJibsUserPanel() {
        return jibsUserPanel;
    }

    public Writer getLog() {
        return logWriter;
    }

    public String getResource(String res) {
        return fConf.getResource(res);
    }

    public Server getServer() {
        return theServer;
    }

    public JTextArea getTextArea() {
        return jibsConsole.getTxtPane();
    }

    private void initComponents() {
        try {
            UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        } catch (ClassNotFoundException e) {
            logException(e);
        } catch (InstantiationException e) {
            logException(e);
        } catch (IllegalAccessException e) {
            logException(e);
        } catch (UnsupportedLookAndFeelException e) {
            logException(e);
        }

        jibsFrame = new JFrame();
        infoAction = new InfoAction(this, "Info",
                                    createImageIcon("images/info.gif", ""), "",
                                    KeyStroke.getKeyStroke(KeyEvent.VK_I,
                                                           KeyEvent.ALT_MASK));
        runAction = new RunAction(this, "Start",
                                  createImageIcon("images/run.gif", ""), "",
                                  KeyStroke.getKeyStroke(KeyEvent.VK_S,
                                                         KeyEvent.ALT_MASK));
        stopAction = new StopAction(this, "Stop",
                                    createImageIcon("images/stop.gif", ""), "",
                                    KeyStroke.getKeyStroke(KeyEvent.VK_H,
                                                           KeyEvent.ALT_MASK));
        exitAction = new ExitAction(this, "Exit",
                                    createImageIcon("images/exit.gif", ""), "",
                                    KeyStroke.getKeyStroke(KeyEvent.VK_X,
                                                           KeyEvent.ALT_MASK));
        reloadAction = new ReloadAction(this, "Reload",
                                        createImageIcon("images/refresh.gif", ""),
                                        "", null);

        JMenuBar jibsMenuBar = new JMenuBar();
        JMenu fileMenu = new JMenu("File");

        JMenuItem exitMenu = new JMenuItem("Exit");
        exitMenu.setAction(exitAction);
        fileMenu.add(exitMenu);

        JMenu serverMenu = new JMenu("Server");
        startMenu = new JMenuItem("Start server");
        startMenu.setAction(runAction);
        stopMenu = new JMenuItem("Stop");
        stopMenu.setAction(stopAction);
        serverMenu.add(startMenu);
        serverMenu.add(stopMenu);

        JMenu aboutMenu = new JMenu("About");
        JMenuItem vmMenu = new JMenuItem("Java VM...");
        vmMenu.setAction(infoAction);
        aboutMenu.add(vmMenu);
        jibsMenuBar.add(fileMenu);
        jibsMenuBar.add(serverMenu);
        jibsMenuBar.add(aboutMenu);
        jibsGUI = new JibsGui(this);

        jInternalFrame1 = new javax.swing.JInternalFrame();

        jibsConsole = new JibsConsole(this);
        jibsStatusbar = new JibsStatusBar(this);
        jibsUserPanel = new JibsUserPanel(this);
        jibsGUI.getBtnInfo().setAction(infoAction);
        jibsGUI.getBtnExit().setAction(exitAction);
        jibsGUI.getBtnStart().setAction(runAction);
        jibsGUI.getBtnStop().setAction(stopAction);
        jibsGUI.getBtnReload().setAction(reloadAction);

        jInternalFrame1.setVisible(true);

        getJibsFrame().addWindowListener(new JibsWindow(this));

        getJibsFrame().getContentPane()
            .add(jibsMenuBar, java.awt.BorderLayout.NORTH);
        getJibsFrame().getContentPane()
            .add(jibsGUI, java.awt.BorderLayout.CENTER);
    }

    public boolean isExitCmd(String cmd) {
        String[] allExitCmds = {
                                   "bye", "adios", "ciao", "end", "exit",
                                   "logout", "quit", "tschoe"
                               };
        boolean b = false;

        for (int i = 0; i < allExitCmds.length; i++)
            if (allExitCmds[i].equalsIgnoreCase(cmd)) {
                b = true;
            }

        return b;
    }

    public void startServerMenuItemActionPerformed(java.awt.event.ActionEvent evt) {
        SplashWindowJibs splashWindow = null;

        try {
            if (evt == null) {
                if (useSwing()) {
                    String splashScreen = fConf.getResource("splashScreen");
                    splashWindow = new SplashWindowJibs(this, splashScreen, null);
                }
            }

            jibsMessages = new JibsMessages(this,
                                            fConf.getResource("MessageFile"));

            portno = Integer.parseInt(fConf.getResource("Port"));
            listener = new ServerSocket(portno);
            allCmds = new HashMap<String, JibsCommand>();

            // register all commands
            allCmds.put("about", new About_Command());
            allCmds.put("accept", new Accept_Command());
            allCmds.put("address", new Address_Command());
            allCmds.put("average", new NImplemented_Command());
            allCmds.put("away", new Away_Command());
            allCmds.put("back", new Back_Command());
            allCmds.put("beaver", new NImplemented_Command());
            allCmds.put("blind", new NImplemented_Command());
            allCmds.put("board", new Board_Command());
            allCmds.put("bye", new Exit_Command());
            allCmds.put("adios", new Exit_Command());
            allCmds.put("ciao", new Exit_Command());
            allCmds.put("end", new Exit_Command());
            allCmds.put("exit", new Exit_Command());
            allCmds.put("logout", new Exit_Command());
            allCmds.put("quit", new Exit_Command());
            allCmds.put("tschoe", new Exit_Command());
            allCmds.put("clear", new NImplemented_Command());
            allCmds.put("cls", new NImplemented_Command());
            allCmds.put("date", new NImplemented_Command());
            allCmds.put("dicetest", new NImplemented_Command());
            allCmds.put("double", new Double_Command());
            allCmds.put("erase", new NImplemented_Command());
            allCmds.put("gag", new NImplemented_Command());
            allCmds.put("help", new NImplemented_Command());
            allCmds.put("invite", new Invite_Command());
            allCmds.put("join", new Join_Command());
            allCmds.put("kibitz", new Kibitz_Command());
            allCmds.put("last", new NImplemented_Command());
            allCmds.put("leave", new Leave_Command());
            allCmds.put("login", new Login_Command());
            allCmds.put("look", new NImplemented_Command());
            allCmds.put("matrix", new NImplemented_Command());
            allCmds.put("man", new NImplemented_Command());
            allCmds.put("message", new NImplemented_Command());
            allCmds.put("motd", new Motd_Command());
            allCmds.put("move", new Move_Command());
            allCmds.put("m", new Move_Command());
            allCmds.put("off", new NImplemented_Command());
            allCmds.put("oldboard", new NImplemented_Command());
            allCmds.put("oldmoves", new NImplemented_Command());
            allCmds.put("otter", new NImplemented_Command());
            allCmds.put("panic", new NImplemented_Command());
            allCmds.put("password", new Password_Command());
            allCmds.put("pip", new NImplemented_Command());
            allCmds.put("raccoon", new NImplemented_Command());
            allCmds.put("ratings", new Ratings_Command());
            allCmds.put("rawwho", new NImplemented_Command());
            allCmds.put("redouble", new NImplemented_Command());
            allCmds.put("reject", new Reject_Command());
            allCmds.put("resign", new Resign_Command());
            allCmds.put("roll", new Roll_Command());
            allCmds.put("save", new NImplemented_Command());
            allCmds.put("say", new NImplemented_Command());
            allCmds.put("set", new Set_Command());
            allCmds.put("shout", new Shout_Command());
            allCmds.put("show", new NImplemented_Command());
            allCmds.put("shutdown", new Shutdown_Command());
            allCmds.put("sortwho", new NImplemented_Command());
            allCmds.put("stat", new NImplemented_Command());
            allCmds.put("tell", new Tell_Command());
            allCmds.put("time", new NImplemented_Command());
            allCmds.put("toggle", new Toggle_Command());
            allCmds.put("unwatch", new Unwatch_Command());
            allCmds.put("version", new NImplemented_Command());
            allCmds.put("watch", new Watch_Command());
            allCmds.put("waitfor", new NImplemented_Command());
            allCmds.put("wave", new Wave_Command());
            allCmds.put("where", new NImplemented_Command());
            allCmds.put("whisper", new Whisper_Command());
            allCmds.put("who", new Who_Command());
            allCmds.put("whois", new Whois_Command());

            if (useSwing()) {
                jibsUserPanel.readAllPlayers();
            }

            jibsRandom = new JibsRandom();
            theServer = new Server(jibsMessages, this, listener, portno);

            serverThread = new Thread(theServer);

            serverThread.start();

            if (useSwing()) {
                if (splashWindow != null) {
                    splashWindow.dispose();
                }

                runAction.setEnabled(false);
                stopAction.setEnabled(true);
                bServerRuns = true;
                getJibsFrame().setVisible(true); // now display the main window
            }

            JibsTextArea.log(this, "Server started on port " + portno, true);
        } catch (Exception e) {
            String sMsg = "jIBS could not start. Check the configuration file '" +
                          confFileName + "'";
            logger.fatal(sMsg, e);

            if (splashWindow != null) {
                splashWindow.dispose();
            }

            JOptionPane.showMessageDialog(null, sMsg, "Warning",
                                          JOptionPane.WARNING_MESSAGE);
            System.exit(1);
        }
    }

    public void stopServerMenuItemActionPerformed(java.awt.event.ActionEvent evt) {
        if (runs()) {
            boolean bStop = false;

            try {
                Server server = getServer();
                int onlinePlayer = server.getAllClients().size();

                if (evt != null) {
                    if (onlinePlayer > 0) {
                        String warnExit = getResource("warnExit");
                        Boolean bWarn = Boolean.valueOf(warnExit);

                        if (bWarn) {
                            String msg;

                            if (onlinePlayer > 1) {
                                msg = "There are still " + onlinePlayer +
                                      " players online.\n" +
                                      "When you continue these players will be disconnected without warning.\n" +
                                      "They might consider this to be rude behaviour.\n" +
                                      "\n" + "Do you want to continue?";
                            } else {
                                msg = "There is still one player online.\n" +
                                      "When you continue this player will be disconnected without warning.\n" +
                                      "He/She might consider this to be rude behaviour.\n" +
                                      "\n" + "Do you want to continue?";
                            }

                            if (useSwing()) {
                                int option = JOptionPane.showConfirmDialog(getJibsFrame(),
                                                                           msg,
                                                                           "Exit",
                                                                           JOptionPane.YES_NO_OPTION);

                                if (option == JOptionPane.YES_OPTION) {
                                    bStop = true;
                                }
                            } else {
                                bStop = true;
                            }
                        } else {
                            bStop = true;
                        }
                    } else {
                        bStop = true;
                    }
                } else {
                    bStop = true;
                }

                if (bStop) {
                    server.getListener().close();
                    server.setRuns(false);
                    getServerThread().join();
                    server.closeAllClients();
                    bServerRuns = false;
                }
            } catch (IOException e) {
                logException(e);
            } catch (InterruptedException e) {
                logException(e);
            }

            runAction.setEnabled(true);
            stopAction.setEnabled(false);
            JibsTextArea.log(this, "Server stopped.", true);
        }
    }

    public void logException(Exception e) {
        String e1 = JibsStackTrace.getCustomStackTrace(e);
        JibsTextArea.log(this, e1, true);
        logger.warn(e);
    }

    public SqlMapClient getSqlMapInstance() {
        if (sqlMap == null) {
            try {
                String sqlMapConfig = fConf.getResource("dbConfiguration");
                Reader reader = Resources.getResourceAsReader(sqlMapConfig);
                sqlMap = SqlMapClientBuilder.buildSqlMapClient(reader);
            } catch (IOException e) {
                logException(e);
            }
        }

        return sqlMap;
    }

    public Thread getServerThread() {
        return serverThread;
    }

    public void setServerThread(Thread serverThread) {
        this.serverThread = serverThread;
    }

    public boolean runs() {
        return bServerRuns;
    }

    public JibsStatusBar getStatusBar() {
        return jibsStatusbar;
    }

    public JibsGui getJibsGUI() {
        return jibsGUI;
    }

    public JibsUserTableModel getUserTableModel() {
        return jibsGUI.getUserTableModel();
    }

    public JibsShutdown getJibsShutdown() {
        return jibsShutdown;
    }

    public void setJibsShutdown(JibsShutdown jibsShutdown) {
        this.jibsShutdown = jibsShutdown;
    }

    public JibsDocument getDoc() {
        return doc;
    }

    public void setDoc(JibsDocument doc) {
        this.doc = doc;
    }

    protected static ImageIcon createImageIcon(String path, String description) {
        URL imgURL = ClassLoader.getSystemResource(path);

        if (imgURL != null) {
            return new ImageIcon(imgURL, description);
        } else {
            logger.warn("Couldn't find file: " + path);

            return null;
        }
    }

    public JFrame getJibsFrame() {
        return jibsFrame;
    }

    public boolean useSwing() {
        return bUseSwing;
    }
}
