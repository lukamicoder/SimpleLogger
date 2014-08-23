using System.Configuration;

namespace SimpleLogger {
	class LoggerConfigurationSection : ConfigurationSection {
		[ConfigurationProperty("enabled", IsRequired = true)]
		public bool Enabled {
			get { return (bool)this["enabled"]; }
			set { this["enabled"] = value; }
		}

		[ConfigurationProperty("applicationName", IsRequired = false, IsKey = false)]
		public string ApplicationName {
			get { return this["applicationName"] as string; }
			set { this["applicationName"] = value; }
		}

		[ConfigurationProperty("targets")]
		public TargetElementCollection Targets {
			get { return this["targets"] as TargetElementCollection; }
		}
	}

	class TargetElementCollection : ConfigurationElementCollection {
		protected override ConfigurationElement CreateNewElement() {
			return new TargetElement();
		}

		protected override object GetElementKey(ConfigurationElement element) {
			return ((TargetElement)element).Type;
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "target"; }
		}

		public TargetElement GetService(string key) {
			return (TargetElement)BaseGet(key);
		}
	}

	class TargetElement : ConfigurationElement {
		[ConfigurationProperty("type", IsRequired = true, IsKey = true)]
		public LogTarget Type {
			get { return (LogTarget)this["type"]; }
			set { this["type"] = value; }
		}

		[ConfigurationProperty("threshold", IsRequired = true, IsKey = false)]
		public LogLevel Threshold {
			get { return (LogLevel)this["threshold"]; }
			set { this["type"] = value; }
		}

		[ConfigurationProperty("fileName", IsRequired = false, IsKey = false)]
		public string FileName {
			get { return (string)this["fileName"]; }
			set { this["fileName"] = value; }
		}

		[ConfigurationProperty("ip", IsRequired = false, IsKey = false)]
		public string Ip {
			get { return (string)this["ip"]; }
			set { this["ip"] = value; }
		}

		[ConfigurationProperty("port", IsRequired = false, IsKey = false)]
		public int Port {
			get { return (int)this["port"]; }
			set { this["port"] = value; }
		}

		[ConfigurationProperty("facility", IsRequired = false, IsKey = false)]
		public FacilityLevel Facility {
			get { return (FacilityLevel)this["facility"]; }
			set { this["facility"] = value; }
		}
	}
}
