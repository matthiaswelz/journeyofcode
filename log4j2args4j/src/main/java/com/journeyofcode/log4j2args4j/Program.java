package com.journeyofcode.log4j2args4j;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.kohsuke.args4j.CmdLineParser;

public class Program {
	public static void main(String[] args) {
		Parameters parameters = new Parameters();
		CmdLineParser parser = new CmdLineParser(parameters);
		try {
			parser.parseArgument(args);
		} catch (Exception ex) {
			parser.printUsage(System.out);
			return;
		}		
		
		int count = parameters.getCount();
		new Foo().bar(count);
	}
}

class Foo {
	private static Logger logger = LogManager.getLogger();
	
	public void bar(int count) {
		for (int i = 0; i < count; i++) {
			switch (i % 6) {
			case 0:
				logger.trace("Trace message");
				break;
			case 1:
				logger.debug("Debug message");
				break;
			case 2:
				logger.info("Info message");
				break;
			case 3:
				logger.warn("Warn message");
				break;
			case 4:
				logger.error("Error message");
				break;
			case 5:
				logger.fatal("Fatal message");
				break;
			}
		}
	}
}
