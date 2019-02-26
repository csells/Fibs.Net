package net.sourceforge.jibs.gui;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

public class TableListener implements ActionListener {
    private JibsUserPanel jibsUserPanel;

    public TableListener(JibsUserPanel up) {
        this.jibsUserPanel = up;
    }

    public void actionPerformed(ActionEvent e) {
        jibsUserPanel.readAllPlayers();
    }
}
