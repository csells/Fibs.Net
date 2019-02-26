package net.sourceforge.jibs.gui;

import com.ibatis.sqlmap.client.SqlMapClient;

import net.sourceforge.jibs.server.JibsServer;
import net.sourceforge.jibs.server.Player;

import java.sql.SQLException;

import java.util.List;

import javax.swing.JPanel;

public class JibsUserPanel extends JPanel {
    private static final long serialVersionUID = 2091053677555167005L;
    private JibsServer jibsServer;
    private JibsUserTableModel jibsUserTableModel = null;

    public JibsUserPanel(JibsServer jibsServer) {
        this.jibsServer = jibsServer;
        this.jibsUserTableModel = jibsServer.getUserTableModel();
    }

    @SuppressWarnings("unchecked")
    public void readAllPlayers() {
        try {
            SqlMapClient sqlMap = jibsServer.getSqlMapInstance();
            List<Player> map = sqlMap.queryForList("Player.readAllPlayer", null);
            jibsUserTableModel.getDataVector().removeAllElements();

            for (Player player : map) {
                jibsUserTableModel.getDataVector().add(player);
            }

            jibsUserTableModel.fireTableDataChanged();
        } catch (SQLException e) {
            jibsServer.logException(e);
        }
    }

    public JibsUserTableModel getJibsUserTableModel() {
        return jibsUserTableModel;
    }
}
