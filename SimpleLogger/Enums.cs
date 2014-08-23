namespace SimpleLogger {
	public enum LogLevel {
		Fatal = 2,
		Error = 3,
		Warning = 4,
		Info = 6,
		Debug = 7
	}

	public enum LogTarget {
		Console,
		File,
		Syslog,
		EventLog
	}

	public enum SeverityLevel {
		Emergency = 0,
		Alert = 1,
		Critical = 2,
		Error = 3,
		Warning = 4,
		Notice = 5,
		Informational = 6,
		Debug = 7
	}

	public enum FacilityLevel {
		Kernel = 0,
		User = 1,
		Mail = 2,
		Daemon = 3,
		Auth = 4,
		Syslog = 5,
		Printer = 6,
		News = 7,
		Uucp = 8,
		Cron = 9,
		AuthPriv = 10,
		Ftp = 11,
		Ntp = 12,
		Audit = 13,
		Audit2 = 14,
		Cron2 = 15,
		Local0 = 16,
		Local1 = 17,
		Local2 = 18,
		Local3 = 19,
		Local4 = 20,
		Local5 = 21,
		Local6 = 22,
		Local7 = 23
	}
}
