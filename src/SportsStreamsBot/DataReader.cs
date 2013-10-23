using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SportsStreamsBot.Data;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SportsStreamsBot
{
	class GamesReader
	{
		private int offsetFromEasternTime;
		private string scheduleUrlFormat;
		private string liveStreamsUrl;

		public GamesReader(int offsetFromEasternTime, string scheduleUrlFormat, liveStreamsUrl)
		{
			this.offsetFromEasternTime = offsetFromEasternTime;
			this.scheduleUrlFormat = scheduleUrlFormat;
			this.liveStreamsUrl = liveStreamsUrl;
		}

		public Games GetGames(DateTime scheduleDate)
		{

			var url = string.Format(scheduleUrlFormat, scheduleDate.Year, scheduleDate.Month, scheduleDate.Day);
			


			var games = new Games();

			var contents = GetContents(url);

			if (string.IsNullOrEmpty(contents))
			{
				Debug.WriteLine("No data found for " + url); // not necessarily an error, could be no games
				return games;
			}


			dynamic schedule = JsonConvert.DeserializeObject(contents);

			foreach (dynamic game in schedule.games)
			{
				var gameId = game.gameId;
				var startTimeEastern = DateTime.Parse(game.startTime.ToString());
				// will either have a preview or a recap
				var summary = game.gamePreview != null ? game.gamePreview : game.gameRecap;

				var home = new Team
				{
					Abbreviation = game.h.ab,
					Record = game.h.record,
				};
				
				var away = new Team
				{
					Abbreviation = game.a.ab,
					Record = game.a.record,
				};

				// fix the game time
				var startTimeLocal = startTimeEastern.AddHours(offsetFromEasternTime);

				games.Add(new Game { Away = away, Home = home, GameID = gameId, UtcStart = startTimeLocal.ToUniversalTime(), Summary = summary });
			}

			return games;

		}

		public void AddLiveStreams(List<Game> games)
		{
			var contents = GetContents(liveStreamsUrl);

			if (string.IsNullOrEmpty(contents))
			{
				Debug.WriteLine("No data found for " + url); // not necessarily an error, could be no games
				return games;
			}
		
			// streams aren't grouped by game, they are listed as seperate entries with common data
			// streams are seperated by a semi colon
			// we're createing a list of streams grouped by key (homecity_awaycity)
			// so we can easily match them later
			var streams = new Dictionary<string, List<Stream>>();

			foreach (var streamString in contents.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var data = streamString.Split('|');

				// I don't know what all the parts are, but I know enough...
				/*
					0 seasonid			2012|
					1 gameTime(E)		2013-05-18 20:00:00|
					2 month of season	3|
					3 gameid			41200216|
					4 feedName			IND	41200216 (Home Feed)|
					5 unknown			NYK|
					6 homeStreamName	ind|
					7 awayStreamName	nyk|
					8 homeCity			IND|
					9 awayCity			NYK|
					10 unknown			172.16.72.77|
					11 streamUrl		adaptive://nlds127.neulion.com:443/nlds/nba/ind/as/live/s_ind_live_game_hd|
					12 feedType			home|
					13 feedId?			3_41200216_nyk_ind_2012_h;
				 */
				// this is 
				var gameTimeEastern = DateTime.Parse(data[1]);
				// this is ugly, but the feed time is eastern and contains no timezone info
				// at least a config setting makes this a bit less ugly should someone else run this some day...
				gameTimeEastern = gameTimeEastern.AddHours(offsetFromEasternTime);

				var seasonId = int.Parse(data[0]);
				var monthId = int.Parse(data[2]);
				var gameId = int.Parse(data[3]);
				var homeStreamName = data[6];
				var awayStreamName = data[7];
				var homeCity = data[8];
				var awayCity = data[9];
				var streamUrl = data[11];
				var feedType = (Stream.FeedTypes)Enum.Parse(typeof(Stream.FeedTypes), data[12], true);

				var stream = new Stream(gameTimeEastern, homeStreamName, awayStreamName, homeCity, awayCity, streamUrl, feedType, gameId, monthId);

				if (!streams.ContainsKey(stream.Key))
				{
					streams.Add(stream.Key, new List<Stream>());
				}

				streams[stream.Key].Add(stream);
			}

			// now sort through the streams and combine match them to games
			foreach (var streamList in streams.Values)
			{
				var homeStream = streamList.FirstOrDefault(s => s.FeedType == Stream.FeedTypes.Home);
				var awayStream = streamList.FirstOrDefault(s => s.FeedType == Stream.FeedTypes.Away);

				// one of the streams could be missing
				if (homeStream != null)
				{
					// find matching game
					var game = games.First(g => g.Home.Abbreviation == homeStream.HomeCity);
					game.Home.Live = GetServerNumberFromUrl(homeStream.StreamUrl);
				}

				if (ho



				// one of them has to be active, use that one to setup the data
				Stream gameData = homeStream != null ? homeStream : awayStream;

				var game = new Game();
				game.GameID = gameData.GameID;
				game.MonthID = gameData.MonthID;
				game.UtcStart = gameData.GameTimeEastern.ToUniversalTime();
				game.HomeTeam.City = gameData.HomeCity;
				game.HomeTeam.StreamName = gameData.HomeStreamName;
				game.AwayTeam.City = gameData.AwayCity;
				game.AwayTeam.StreamName = gameData.AwayStreamName;
				game.Summary = summaryDownloader.GetGameSummary(gameData.GameTimeEastern, gameData.MonthID, gameData.GameID);

				if (homeStream != null)
					game.HomeTeam.Server = GetServerNumberFromUrl(homeStream.StreamUrl);
				if (awayStream != null)
					game.AwayTeam.Server = GetServerNumberFromUrl(awayStream.StreamUrl);

				games.Add(game);
			}

			return games;

		}

		private string GetServerNumberFromUrl(string streamUrl)
		{
			// just in case:
			streamUrl = streamUrl.ToLower();
			//adaptive://nlds127.neulion.com:443/nlds/nba/ind/as/live/s_ind_live_game_hd|
			// comes after the first instance of "nlds", goes until first instance of "."
			var startIndex = streamUrl.IndexOf("nlds") + 4; // + 4 for the substring length
			var endIndex = streamUrl.IndexOf(".");

			var serverNumber = streamUrl.Substring(startIndex, endIndex - startIndex);
			return serverNumber;
		}

		private string GetContents(string url)
		{
			try
			{
				using (var client = new WebClient())
				{
					return client.DownloadString(url);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to download from " + url);
				Debug.WriteLine(ex);
				throw;
			}
		}

	}
}
