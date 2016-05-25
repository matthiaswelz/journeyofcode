package args4jlog4j;

import org.apache.logging.log4j.Level;
import org.apache.logging.log4j.core.config.Configurator;
import org.kohsuke.args4j.Option;

public class Parameters {
	enum LogLevel {
		TRACE,
		DEBUG,
		INFO,
		WARN,
		ERROR,
		FATAL,
	}

	@Option(name="-logLevel", usage="The log level")
	public void setLogLevel(LogLevel value) {
		String levelName = value.toString();
		Level level = Level.getLevel(levelName);
		Configurator.setRootLevel(level);
	}
	
	private int count = 6;

	@Option(name="-count", usage="The number of messages to create (must be >= 0)")
	public void setCount(int value) {
		if (value <= 0)
			throw new IllegalArgumentException("value must be >= 0");
		
		this.count = value;
	}
	public int getCount() {
		return this.count;
	}
}
