using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace SportsStreamsBot
{
	class Program
	{
		static void Main(string[] args)
		{
			var sports = ConfigurationManager.AppSettings["Sports"];
			var offsetFromEasternTime = int.Parse(ConfigurationManager.AppSettings["OffsetFromEasternTime"]);
			var scheduleUrlFormat = ConfigurationManager.AppSettings["ScheduleUrlFormat"];
			var outputPath = ConfigurationManager.AppSettings["OutputPath"];

			//var submitter = RedditDataSubmitter.Create();

			var reader = new GamesReader(offsetFromEasternTime, scheduleUrlFormat);
			//var writer = new GamesWriter(submitter);

			// 7 days worth of processing - last 2, today, and 4 after
			var now = DateTime.Now;
			var dates = new DateTime[] 
			{
				DateTime.Now.AddDays(-2),
				DateTime.Now.AddDays(-1),
				now,
				DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(4),
			};

			var serializer = new JsonSerializer();

			foreach (var date in dates)
			{
				var games = reader.GetGames(date);
				var filename = string.Format(@"{0}\{1:yyyy-mm-dd}.json", outputPath, date); //.ToString("yyyy-mm-dd");

				// for today's date, get live streams
				if (date == now)
				{

				}


				using (var writer = new JsonTextWriter(new StreamWriter(filename, false)))
				{
					serializer.Serialize(writer, games);
				}
			}


		}
	}
}
