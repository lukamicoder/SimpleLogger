using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;

namespace SimpleLogger {
	public sealed class Logger {
		private static Logger _instance;

		private StreamWriter _sw;
		private string _logPath = "";
		private readonly object _locker = new object();
		private static readonly object Locker = new object();
		private string _applicationName;

		private FacilityLevel _syslogFacility = FacilityLevel.Local0;
		private LogLevel _consoleLogTreshold = LogLevel.Debug;
		private LogLevel _eventLogTreshold = LogLevel.Debug;
		private LogLevel _fileLogTreshold = LogLevel.Debug;
		private LogLevel _syslogTreshold = LogLevel.Debug;

		public bool Enabled { get; set; }

		public string ApplicationName {
			get {
				if (String.IsNullOrEmpty(_applicationName)) {
					_applicationName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
					if (!String.IsNullOrEmpty(_applicationName)) {
						_applicationName = _applicationName.Substring(0, _applicationName.IndexOf('.'));
					}
				}
				return _applicationName;
			}

			set { _applicationName = value; }
		}

		public bool EnableConsoleLog { get; set; }

		public LogLevel ConsoleLogTreshold {
			get { return _consoleLogTreshold; }
			set { _consoleLogTreshold = value; }
		}

		public bool EnableEventLog { get; set; }

		public LogLevel EventLogTreshold {
			get { return _eventLogTreshold; }
			set { _eventLogTreshold = value; }
		}

		public bool EnableFileLog { get; set; }

		public LogLevel FileLogTreshold {
			get { return _fileLogTreshold; }
			set { _fileLogTreshold = value; }
		}

		public string LogFileName { get; set; }

		public bool EnableSyslog { get; set; }

		public LogLevel SyslogTreshold {
			get { return _syslogTreshold; }
			set { _syslogTreshold = value; }
		}

		public string SyslogServerIP { get; set; }

		public int SyslogServerPort { get; set; }

		public FacilityLevel SyslogFacility {
			get { return _syslogFacility; }
			set { _syslogFacility = value; }
		}

		public static Logger Instance {
			get {
				lock (Locker) {
					return _instance ?? (_instance = new Logger());
				}
			}
		}

		private Logger() {
			Enabled = true;
			var config = (LoggerConfigurationSection)ConfigurationManager.GetSection("logger");

			if (config == null) {
				return;
			}

			if (config.Enabled) {
				Enabled = true;
			} else {
				return;
			}

			var targets = config.Targets;
			foreach (var target in targets) {
				switch (((TargetElement)target).Type) {
					case LogTarget.Console:
						EnableConsoleLog = true;
						ConsoleLogTreshold = ((TargetElement) target).Threshold;
						break;
					case LogTarget.File:
						EnableFileLog = true;
						FileLogTreshold = ((TargetElement)target).Threshold;
						LogFileName = ((TargetElement) target).FileName;
						break;
					case LogTarget.Syslog:
						EnableSyslog = true;
						SyslogTreshold = ((TargetElement)target).Threshold;
						SyslogServerPort = ((TargetElement) target).Port;
						SyslogServerIP = ((TargetElement) target).Ip;
						SyslogFacility = ((TargetElement)target).Facility;
						ApplicationName = config.ApplicationName;
						break;
					case LogTarget.EventLog:
						EnableEventLog = true;
						EventLogTreshold = ((TargetElement)target).Threshold;
						ApplicationName = config.ApplicationName;
						break;
				}
			}
		}

		public void Debug(string message, Exception exception = null) {
			Write(message, exception, LogLevel.Debug);
		}

		public void Info(string message, Exception exception = null) {
			Write(message, exception, LogLevel.Info);
		}

		public void Warn(string message, Exception exception = null) {
			Write(message, exception, LogLevel.Warning);
		}

		public void Error(string message, Exception exception = null) {
			Write(message, exception, LogLevel.Error);
		}

		public void Fatal(string message, Exception exception = null) {
			Write(message, exception, LogLevel.Fatal);
		}

		private void Write(string message, Exception exception, LogLevel level) {
			if (!Enabled) {
				return;
			}

			if (!EnableSyslog && !EnableFileLog && !EnableEventLog && !EnableConsoleLog) {
				return;
			}

			if (exception != null) {
				var sb = new StringBuilder();

				sb.Append(message).Append(Environment.NewLine);
				while (exception != null) {
					sb.Append("Message: ").Append(exception.Message).Append(Environment.NewLine)
					.Append("Source: ").Append(exception.Source).Append(Environment.NewLine)
					.Append("Target site: ").Append(exception.TargetSite).Append(Environment.NewLine)
					.Append("Stack trace: ").Append(exception.StackTrace).Append(Environment.NewLine);

					exception = exception.InnerException;
				}

				sb.Append(Environment.NewLine).Append("Calling method: ").Append(new StackFrame(2).GetMethod().Name).Append(Environment.NewLine);

				message = sb.ToString();
			}

			string className = "";
			var frame = new StackFrame(2);
			MethodBase method = frame.GetMethod();
			if (method.DeclaringType != null) {
				className = method.DeclaringType.Name;
			}

			var formattedMsg = String.Format("{0}|{1}|{2}|{3}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), level, className, message);

			if (EnableConsoleLog && (int)level <= (int)ConsoleLogTreshold) {
				switch (level) {
					case LogLevel.Fatal:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case LogLevel.Error:
						Console.ForegroundColor = ConsoleColor.DarkRed;
						break;
					case LogLevel.Warning:
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						break;
					case LogLevel.Debug:
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						break;
				}

				Console.WriteLine(formattedMsg);

				Console.ResetColor();
			}

			if (EnableFileLog && (int)level <= (int)FileLogTreshold) {
				SaveMessageToFile(formattedMsg);
			}

			if (EnableSyslog && (int)level <= (int)SyslogTreshold) {
				SendSyslogMessage(message, level);
			}

			if (EnableEventLog && (int)level <= (int)EventLogTreshold) {
				CreateEventLog(formattedMsg, level);
			}
		}

		private void CreateEventLog(string message, LogLevel level) {
			try {
				if (!EventLog.SourceExists(ApplicationName)) {
					EventLog.CreateEventSource(ApplicationName, "Application");
				}
			} catch (SecurityException) {
				return;
			}

			using (var eventLog = new EventLog()) {
				eventLog.Source = ApplicationName;
				switch (level) {
					case LogLevel.Error:
					case LogLevel.Fatal:
						eventLog.WriteEntry(message, EventLogEntryType.Error, 10);
						break;
					case LogLevel.Warning:
						eventLog.WriteEntry(message, EventLogEntryType.Warning, 20);
						break;
					case LogLevel.Info:
					case LogLevel.Debug:
						eventLog.WriteEntry(message, EventLogEntryType.Information, 30);
						break;
				}
			}
		}

		private void SendSyslogMessage(string message, LogLevel level) {
			var msg = new SysLogMessage(ApplicationName, message, (SeverityLevel)(int)level, SyslogFacility);
			var client = new SyslogClient();

			if (!string.IsNullOrEmpty(SyslogServerIP)) {
				client.ServerIPAddress = SyslogServerIP;
			}

			if (SyslogServerPort > 0) {
				client.Port = SyslogServerPort;
			}

			client.Send(msg);
		}

		private void SaveMessageToFile(string message) {
			lock (_locker) {
				if (string.IsNullOrEmpty(_logPath)) {
					if (!String.IsNullOrEmpty(LogFileName)) {
						_logPath = LogFileName;
						if (_logPath.Contains("${basedir}")) {
							_logPath = _logPath.Replace("${basedir}", AppDomain.CurrentDomain.BaseDirectory);
						}
						if (_logPath.Contains("${application}")) {
							_logPath = _logPath.Replace("${application}", GetFileName());
						}
					} else {
						if (Environment.OSVersion.Platform == PlatformID.Unix) {
							_logPath = "/var/log/" + GetFileName();
						} else {
							_logPath = AppDomain.CurrentDomain.BaseDirectory + GetFileName();
						}
					}
				}

				if (!File.Exists(_logPath)) {
					var fs = File.Create(_logPath);
					fs.Close();
				}

				try {
					_sw = File.AppendText(_logPath);
					_sw.WriteLine(message);
					_sw.Flush();
				} finally {
					if (_sw != null) {
						_sw.Close();
					}
				}
			}
		}

		private string GetFileName() {
			if (String.IsNullOrEmpty(ApplicationName)) {
				return Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) + ".log";
			}

			return ApplicationName + ".log";
		}
	}
}