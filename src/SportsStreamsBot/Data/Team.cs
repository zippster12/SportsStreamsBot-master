using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SportsStreamsBot.Data
{
	[DataContract(Name = "team")]
	public class Team
	{
		public string Abbreviation { get; set; }
		public string Record { get; set; }
		public string Live { get; set; }
		public string ReplayShort { get; set; }
		public string ReplayFull { get; set; }

	}
}
