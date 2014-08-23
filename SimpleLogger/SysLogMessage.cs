using System;
using System.Net;
using System.Text;

namespace SimpleLogger {
	public class SysLogMessage {
		private string _hostname;

		public FacilityLevel Facility { get; set; }
		public SeverityLevel Severity { get; set; }
		public string Tag { get; set; }
		public string Message { get; set; }

		public int Priority {
			get {
				// facility *8 + Severity == Priority Type as a number
				return ((int)Facility * 8) + (int)Severity;
			}
		}

		public string Timestamp {
			get { return DateTime.Now.ToString("MMM dd HH:mm:ss"); }
		}

		public string Hostname {
			get {
				if (String.IsNullOrEmpty(_hostname)) {
					_hostname = Dns.GetHostName();
				}
				return _hostname;
			} set { _hostname = value; }
		}

		public byte[] Packet {
			get {
				var ascii = new ASCIIEncoding();
				byte[] packet = null;

				if (!String.IsNullOrEmpty(Message)) {
					string content = String.Format("<{0}>{1} {2}{3}{4}", Priority,
					                               Timestamp,
					                               string.IsNullOrEmpty(Hostname) ? "" : Hostname + " ",
					                               string.IsNullOrEmpty(Tag) ? "" : Tag + ": ",
					                               Message);

					packet = ascii.GetBytes(content);
				}
				return packet;
			}
		}

		public SysLogMessage(string tag, string message, SeverityLevel severity, FacilityLevel facility) {
			Tag = tag;
			Message = message;
			Severity = severity;
			Facility = facility;
		}
	}
}