package net.sourceforge.jibs.gui;

import com.jeta.forms.components.panel.FormPanel;
import com.jeta.forms.gui.form.FormAccessor;

import net.sourceforge.jibs.server.JibsServer;

import javax.swing.AbstractButton;
import javax.swing.JButton;
import javax.swing.JLabel;
import javax.swing.JTable;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.table.TableColumn;

public class JibsGui extends FormPanel {
    private static final long serialVersionUID = 3800005237698172527L;
    private static final int LOGINCOLUMN = 6;
    private static final int LOGOUTCOLUMN = 7;
    private JTextArea txtArea;
    private JTextField txtPlayer;
    private JLabel lblTime;
    private JibsUserTableModel jibsUserTableModel;
    private JButton btnInfo;
    private JButton btnExit;
    private JButton btnReload;
    private JButton btnStart;
    private JButton btnStop;
    private JButton btnClear;

    public JibsGui(JibsServer jibsServer) {
        super("frm/jibs.jfrm");

        // switch the Textarea with my implementation JibsTextArea
        FormAccessor console = getFormAccessor("playerconsole");
        txtArea = (JTextArea) console.getTextComponent("txtArea");

        String maxLines = jibsServer.getResource("maxConsoleLines");
        JibsDocument doc = new JibsDocument(Integer.valueOf(maxLines));
        txtArea.setDocument(doc);
        jibsServer.setDoc(doc);

        // get components from the statusbar
        FormAccessor status = getFormAccessor("status");
        txtPlayer = status.getTextField("player");
        lblTime = status.getLabel("time");

        // switch the JTable into proper format
        JTable table = getTable("jibsTable");
        jibsUserTableModel = new JibsUserTableModel();

        TableSorter sorter = new TableSorter(jibsUserTableModel);
        table.setModel(sorter);

        int[] widhts = { 210, 100, 100, 230, 60, 60, 160, 160, 160 };

        for (int i = 0; i < jibsUserTableModel.getColumnCount(); i++) {
            TableColumn column = table.getColumnModel().getColumn(i);

            if (i == JibsGui.LOGINCOLUMN) {
                column.setCellRenderer(new DateCellRenderer());
            }

            if (i == JibsGui.LOGOUTCOLUMN) {
                column.setCellRenderer(new DateCellRenderer());
            }

            column.setPreferredWidth(widhts[i]);
        }

        sorter.setTableHeader(table.getTableHeader());
        table.setModel(sorter);

        // ease adding actions later on
        btnInfo = (JButton) getButton("btnInfo");
        btnExit = (JButton) getButton("btnExit");
        btnReload = (JButton) getButton("btnReload");
        btnStart = (JButton) getButton("btnStart");
        btnStop = (JButton) getButton("btnStop");
    }

    public JTextArea getTextArea() {
        return txtArea;
    }

    public JLabel getStatusDate() {
        return lblTime;
    }

    public JTextField getStatusPlayer() {
        return txtPlayer;
    }

    public JibsUserTableModel getUserTableModel() {
        return jibsUserTableModel;
    }

    public AbstractButton getBtnReload() {
        return btnReload;
    }

    public AbstractButton getBtnInfo() {
        return btnInfo;
    }

    public AbstractButton getBtnExit() {
        return btnExit;
    }

    public AbstractButton getBtnStart() {
        return btnStart;
    }

    public AbstractButton getBtnStop() {
        return btnStop;
    }

    public JButton getBtnClear() {
        return btnClear;
    }

    public void setBtnClear(JButton btnClear) {
        this.btnClear = btnClear;
    }
}
