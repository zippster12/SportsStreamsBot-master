using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;

namespace SportsStreamsBot
{
	class HockeySummaryDownloader : ISummaryDownloader
	{
		// we might need to factor the key generation outside of this interface, in case NBA and NHL end up having different formats.

		public string GetGameSummary(DateTime startTimeEastern, int monthID, int gameID)
		{
			// sometimes games show up as repeats with a non-broadcast feed
			// they have a long ID, and we can't resolve them
			if (gameID > 9999)
				return string.Empty;
			
			// generate the full game ID
			// format is "yyyymmgggg"
			var fullID = string.Format("{0}{1:00}{2:0000}", startTimeEastern.Year, monthID, gameID);

			var url = string.Format("http://www.nhl.com/gamecenter/en/preview?id={0}", fullID);

			var document = new HtmlWeb().Load(url);

			var bigStoryNode = document.DocumentNode.SelectSingleNode("//div[@id='wideCol']//p[contains(., 'Big story:')]");

			if (bigStoryNode != null)
			{
				// null safety in case they change the format
				if (bigStoryNode.ParentNode.ChildNodes.Count >= 2)
				{
					// format avoids possible NPE if innertext is null.
					var bigStory = string.Format("{0}", bigStoryNode.InnerText);
					// remove the caption
					var colonIndex = bigStory.IndexOf(":");
					bigStory = bigStory.Substring(colonIndex + 1).Trim();
					bigStory = WebUtility.HtmlDecode(bigStory);

					// we're done here
					return bigStory;
				}
			}

			// couldn't find what we were looking for
			return string.Empty;
		}
	}
}
