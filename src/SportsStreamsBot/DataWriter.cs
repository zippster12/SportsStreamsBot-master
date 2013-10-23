﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SportsStreamsBot.Data;
//using System.Xml.Serialization;
//using System.IO;
//using System.Xml;

//    public class GamesWriter
//    {
//        // using an interface here so that we're not too tightly coupled with reddit
//        // there's a good chance I'll find a new place to host.
//        IDataSubmitter submitter;

//        public GamesWriter(IDataSubmitter submitter)
//        {
//            this.submitter = submitter;
//        }

//        public void WriteGames(string sport, Games games)
//        {
//            // don't write if there are no games
//            if (games.Count == 0)
//                return;

//            var xml = GetXml(games);
//            var title = GetTitle(sport);

//            submitter.SubmitGames(title, xml);
//        }

//        private string GetXml(Games games)
//        {
//            var serializer = new XmlSerializer(typeof(Games));
//            var settings = new XmlWriterSettings
//            {
//                Indent = false,
//                OmitXmlDeclaration = false,
//                Encoding = Encoding.UTF8
//            };

//            // quick and dirty, we're likely to miss something here, but the way python decodes our self post, it will break if there are ampersands
//            // we may run into other characters we have to manually look for...
//            foreach (var game in games)
//            {
//                game.Summary = game.Summary.Replace("&", "and");
//                // god dammit python.
//                // python doesn't like empty tags like "<summary />"
//                if (string.IsNullOrEmpty(game.Summary))
//                {
//                    game.Summary = "No information available";
//                }
//            }

//            using (var textWriter = new StringWriter())
//            {
//                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
//                {
//                    serializer.Serialize(xmlWriter, games);
//                }
//                var xml = textWriter.ToString();

//                // quick and dirty workaround - python complains of utf-8 characters, so lets pretend it's utf-8
//                xml = xml.Replace("utf-16", "utf-8");
//                return xml;
//            }
//        }

//        private string GetTitle(string sport)
//        {
//            var date = DateTime.Now.ToString("yyyy/MM/dd");

//            return string.Format("{0} - {1}", sport, date);
//        }

//    }
//}
