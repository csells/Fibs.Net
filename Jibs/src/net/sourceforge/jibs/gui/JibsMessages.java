package net.sourceforge.jibs.gui;

import net.sourceforge.jibs.server.JibsServer;

import org.apache.log4j.Logger;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.Reader;

import java.util.HashMap;
import java.util.Map;
import java.util.StringTokenizer;

public class JibsMessages {
    private static Logger logger = Logger.getLogger(JibsMessages.class);
    private Map<String, String> messageMap = null;
    @SuppressWarnings("unused")
    private JibsServer jibsServer;

    public JibsMessages(JibsServer jibsServer, String filename) {
        BufferedReader buffer = null;

        try {
            this.jibsServer = jibsServer;
            logger.info("Reading message file (" + filename + ")");

            Reader reader = new FileReader(new File(filename));
            buffer = new BufferedReader(reader);

            messageMap = new HashMap<String, String>();

            String line;

            while ((line = buffer.readLine()) != null) {
                if (!line.startsWith("#")) {
                    StringTokenizer stoken = new StringTokenizer(line, "=");

                    while (stoken.hasMoreTokens()) {
                        String par = stoken.nextToken();
                        String value = "";

                        if (stoken.hasMoreTokens()) {
                            value = stoken.nextToken();

                            // already defined ?
                            if (messageMap.containsKey(par)) {
                                logger.warn("" + par + " already defined.");
                            }

                            messageMap.put(par, value);

                            logger.debug(par + "=" + value);
                        }
                    }
                }
            }
        } catch (FileNotFoundException e) {
            jibsServer.logException(e);
        } catch (IOException e) {
            jibsServer.logException(e);
        } finally {
            try {
                if (buffer != null) {
                    buffer.close();
                }
            } catch (IOException e) {
                jibsServer.logException(e);
            }
        }
    }

    public String convert(String string) {
        return load(string);
    }

    public synchronized String convert(String string, Object[] parameter) {
        String s = load(string);

        StringBuffer buffer = new StringBuffer();
        StringBuffer number = null;
        boolean bNumber = false;

        for (int i = 0; i < s.length(); i++) {
            if (s.charAt(i) == '%') {
                number = new StringBuffer();
                bNumber = true;

                continue;
            } else {
                if (bNumber) {
                    if (Character.isDigit(s.charAt(i))) {
                        number.append(s.charAt(i));
                    } else {
                        bNumber = false;

                        if (number != null) {
                            int slot = Integer.parseInt(number.toString());

                            if ((slot >= 0) && (slot < parameter.length)) {
                                buffer.append(parameter[slot]);
                            } else {
                                buffer.append("%");
                                buffer.append(number);
                            }

                            number = null;
                        }

                        buffer.append(s.charAt(i));
                    }
                } else {
                    buffer.append(s.charAt(i));
                    number = null;
                }
            }
        }

        if (number != null) {
            int slot = Integer.parseInt(number.toString());

            if ((slot >= 0) && (slot < parameter.length)) {
                buffer.append(parameter[slot]);
            } else {
                buffer.append("%");
                buffer.append(number);
            }
        }

        return buffer.toString();
    }

    private String load(String ident) {
        String s = (String) messageMap.get(ident);

        if (s == null) {
            logger.warn("Message:\"" + ident + "\" not found.");
            s = "";
        }

        return s;
    }
}
