using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleLogger {
	public class SyslogClient {
		private int _port = 514;
		private string _serverIPAddress;

		public int Port {
			get { return _port; }
			set { _port = value; }
		}

		public string ServerIPAddress {
			get {
				if (string.IsNullOrEmpty(_serverIPAddress)) {
					_serverIPAddress = Dns.GetHostEntry("localhost").AddressList[0].ToString();
				}

				return _serverIPAddress;
			} set {
				_serverIPAddress = value;
			}
		}

		public bool Send(SysLogMessage msg) {
			if (msg == null || msg.Packet == null) {
				return false;
			}

			bool success;
			byte[] packet = msg.Packet;

			using (var udp = new UdpClient(ServerIPAddress, Port)) {
				try {
					udp.Send(packet, packet.Length);
					udp.Close();

					success = true;
				} catch (Exception) {
					success = false;
				}
			}

			return success;
		}
	}
}