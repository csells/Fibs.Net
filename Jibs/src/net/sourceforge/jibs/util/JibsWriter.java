package net.sourceforge.jibs.util;

import java.io.OutputStream;
import java.io.PrintWriter;

public class JibsWriter extends PrintWriter {
    public JibsWriter(OutputStream arg0) {
        super(arg0);
    }

    public synchronized void println(String x) {
        super.print(x);
        super.write(13);
        super.write(10);
        super.flush();
    }
}
