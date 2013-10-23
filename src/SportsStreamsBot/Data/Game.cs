using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;

namespace SportsStreamsBot.Data
{
	[DataContract(Name = "game")]
	public class Game
	{
		[DataMember(Name = "id")]
		public string GameID { get; set; }

		[DataMember(Name = "utcStart")]
		public string UtcStartString
		{
			get { return UtcStart.ToString("yyyy-MM-dd HH:mm:ss") + "+0000"; }
			set { throw new NotImplementedException(); }
		}

		// not serialized directly
		public DateTime UtcStart { get; set; }

		[DataMember(Name = "summary")]
		public string Summary { get; set; }
				
		[XmlElement("h")]
		public Team Home { get; set; }

		[XmlElement("a")]
		public Team Away { get; set; }
	}
}
