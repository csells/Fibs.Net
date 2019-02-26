package net.sourceforge.jibs.gui;

import net.sourceforge.jibs.server.JibsServer;

import java.awt.event.ActionEvent;

import javax.swing.AbstractAction;
import javax.swing.ImageIcon;
import javax.swing.JDialog;
import javax.swing.JFrame;

public class InfoAction extends AbstractAction {
    private static final long serialVersionUID = 4654976179773232277L;
    private JibsServer jibsServer;

    public InfoAction(JibsServer jibsServer, String text, ImageIcon icon,
                      String desc, Object mnemonic) {
        super(text, icon);
        this.jibsServer = jibsServer;
        putValue(SHORT_DESCRIPTION, desc);
        putValue(ACCELERATOR_KEY, mnemonic);

        // putValue(MNEMONIC_KEY, mnemonic);
    }

    public void actionPerformed(ActionEvent e) {
        JDialog dialog = new JDialog(jibsServer.getJibsFrame(), "Parameter", true);

        dialog.getContentPane().add(new VMPanel(dialog, jibsServer, jibsServer.getJibsFrame()));
        dialog.pack();

        VMPanel panel = (VMPanel) dialog.getContentPane().getComponent(0);
        panel.doShow();
        dialog.setResizable(false);
        dialog.setDefaultCloseOperation(JFrame.DO_NOTHING_ON_CLOSE);
        dialog.setLocationRelativeTo(jibsServer.getJibsFrame());
        dialog.setVisible(true);
    }
}
