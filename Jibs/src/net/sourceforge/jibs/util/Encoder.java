package net.sourceforge.jibs.util;

import net.sourceforge.jibs.server.JibsServer;

import java.io.UnsupportedEncodingException;

import java.math.BigInteger;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

public class Encoder {
    public static String encrypt(JibsServer jibsServer, String plaintext,
                                 String algorithm) {
        MessageDigest md = null;

        try {
            md = MessageDigest.getInstance(algorithm);
            md.update(plaintext.getBytes("UTF-8"));
        } catch (UnsupportedEncodingException e) {
            jibsServer.logException(e);

            return null;
        } catch (NoSuchAlgorithmException nsae) {
            jibsServer.logException(nsae);

            return null;
        }

        return (new BigInteger(1, md.digest())).toString(16);
    }
}
