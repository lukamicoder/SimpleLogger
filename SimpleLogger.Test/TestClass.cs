using System;
using System.IO;
using NUnit.Framework;

namespace SimpleLogger.Test {
	[TestFixture]
	public class TestClass {
		Logger _logger = Logger.Instance;
		private const string TestMessage = "Test message";
		private const string Expected = "|Fatal|TestClass|" + TestMessage;

		[Test]
		public void TestConsole() {
			_logger.EnableConsoleLog = true;

			using (var sw = new StringWriter()) {
				Console.SetOut(sw);

				_logger.Fatal(TestMessage);

				string output = sw.ToString();

				if (output.Contains(Expected)) {
					Assert.Pass();
				}
			}
			_logger.EnableConsoleLog = false;
		}


		[Test]
		public void TestFile() {
			string logfilename = AppDomain.CurrentDomain.BaseDirectory + "\\TestClass.log";

			if (File.Exists(logfilename)) {
				File.Delete(logfilename);
			}

			_logger.EnableFileLog = true;

			_logger.LogFileName = @"${basedir}\TestClass.log";

			_logger.Fatal(TestMessage);

			if (!File.Exists(logfilename)) {
				Assert.Fail("Log file is not found: " + logfilename);
			}

			var lines = File.ReadAllLines(logfilename);
			if (lines == null || lines.Length == 0) {
				Assert.Fail("Message is not found in :" + logfilename);
			}
			var line = lines[lines.Length - 1];

			if (!line.Contains(Expected)) {
				Assert.Fail("Message is not found in:" + logfilename);
			}

			_logger.EnableFileLog = false;
		}
	}
}