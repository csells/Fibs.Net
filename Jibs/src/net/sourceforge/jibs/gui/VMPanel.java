package net.sourceforge.jibs.gui;

import com.jeta.forms.components.panel.FormPanel;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.util.JibsConvert;

import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.WindowEvent;
import java.awt.event.WindowListener;

import java.io.IOException;
import java.io.InputStream;

import java.util.Properties;

import javax.swing.JButton;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JTextField;

public class VMPanel extends FormPanel {
    private static final long serialVersionUID = -1548906758044612782L;
    private JibsServer jibsServer;
    private JDialog frame;
    private JTextField java_version;
    private JTextField java_vm;
    private JTextField java_vm_version;
    private JTextField os_system;
    private JTextField os_version;
    private JTextField os_arch;
    private JTextField os_processors;
    private JTextField os_language;
    private JTextField totMemory;
    private JTextField freeMemory;
    private JTextField maxMemory;
    private JLabel aboutLabel;
    private JLabel lblBuild;
    private JButton btnOK;

    public VMPanel(JDialog dialog, JibsServer jibsServer, Frame oframe) {
        super("frm/vmPanel.jfrm");
        this.frame = dialog;
        this.jibsServer = jibsServer;
        aboutLabel = getLabel("aboutLabel");
        java_version = getTextField("version");
        java_vm = getTextField("vm");
        java_vm_version = getTextField("vmversion");
        os_system = getTextField("os_system");
        os_version = getTextField("os_version");
        os_arch = getTextField("os_arch");
        os_processors = getTextField("os_processors");
        os_language = getTextField("os_language");
        totMemory = getTextField("totMemory");
        freeMemory = getTextField("freeMemory");
        maxMemory = getTextField("maxMemory");
        btnOK = (JButton) getButton("btnOk");
        lblBuild = getLabel("lblBuild");

        String buildDateString = "";

        try {
            Properties p = new Properties();
            InputStream is = ClassLoader.getSystemResourceAsStream("net/sourceforge/jibs/util/JibsConstants.properties");

            if (is != null) {
                p.load(is);
                buildDateString = p.getProperty("jibsBuild");
                lblBuild.setText("(" + buildDateString + ")");
            }
        } catch (IOException e) {
            jibsServer.logException(e);
        }

        frame.getRootPane().setDefaultButton(btnOK);
        btnOK.addActionListener(new ActionListener() {
                public void actionPerformed(ActionEvent e) {
                    btn_OkActionPerformed(e);
                }
            });
        dialog.addWindowListener(new WindowListener() {
                public void windowActivated(WindowEvent e) {
                }

                public void windowClosed(WindowEvent e) {
                }

                public void windowClosing(WindowEvent e) {
                    btn_OkActionPerformed(null);
                }

                public void windowDeactivated(WindowEvent e) {
                }

                public void windowDeiconified(WindowEvent e) {
                }

                public void windowIconified(WindowEvent e) {
                }

                public void windowOpened(WindowEvent e) {
                }
            });
    }

    public void doShow() {
        StringBuffer buffer = new StringBuffer();

        buffer.append(jibsServer.getResource("aboutVersion"));
        aboutLabel.setText(buffer.toString());
        aboutLabel.setForeground(Color.RED);
        java_version.setText(System.getProperty("java.vm.version"));
        java_vm.setText(System.getProperty("java.vm.name"));
        java_vm_version.setText(System.getProperty("java.vm.info"));
        os_system.setText(System.getProperty("os.name"));
        os_version.setText(System.getProperty("os.version"));
        os_arch.setText(System.getProperty("os.arch"));
        os_processors.setText(Integer.toString(Runtime.getRuntime()
                                                      .availableProcessors()));
        os_language.setText(System.getProperty("user.country"));

        double mem = Runtime.getRuntime().freeMemory() / (1024.0 * 1024.0);
        double x = JibsConvert.convdouble(mem, 3);
        String x1 = Double.toString(x);

        freeMemory.setText(x1 + " MB");
        mem = Runtime.getRuntime().maxMemory() / (1024.0 * 1024.0);
        x = JibsConvert.convdouble(mem, 3);
        x1 = Double.toString(x);
        maxMemory.setText(x1 + " MB");
        mem = Runtime.getRuntime().totalMemory() / (1024.0 * 1024.0);
        x = JibsConvert.convdouble(mem, 3);
        x1 = Double.toString(x);
        totMemory.setText(x1 + " MB");
    }

    public void btn_OkActionPerformed(ActionEvent event) {
        frame.dispose();
    }
}
