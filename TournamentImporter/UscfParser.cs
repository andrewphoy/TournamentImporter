using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TournamentImporter {
    class UscfParser {
        public UscfParser() {

        }

        public UscfEvent ParseTournament(string html, string id = null) {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            UscfEvent e = new UscfEvent();
            e.Id = id;

            var tableNodes = doc.DocumentNode.SelectNodes("//table[@cellpadding='3']");
            var preFormatNodes = doc.DocumentNode.SelectNodes("//pre");

            var tableCount = (tableNodes != null) ? tableNodes.Count : 0;
            var preCount = (preFormatNodes != null) ? preFormatNodes.Count : 0;

            if (tableCount < 2) {
                // error
                return null;
            }

            if (preCount >= tableCount) {
                // missing data for one or more sections
                return null;
            }

            // the first matched node is the summary
            var summary = tableNodes[0];
            ProcessSummaryTable(e, summary);

            // the second matched node is the list of sections

            // each following node is a single section in the tournament
            int offset = tableCount - preCount;
            for (int i = 0; i < preCount; i++) {
                ProcessSection(e, tableNodes[i + offset], preFormatNodes[i]);
            }

            return e;
        }

        /// <summary>
        /// Extracts a list of post tournament ratings
        /// If a player competes in more than one section only the first section will be counted
        /// </summary>
        /// <param name="e">UscfEvent</param>
        /// <returns></returns>
        public List<int> GetRatings(UscfEvent e, bool postEventRatings = true, string type = "regular") {

            Dictionary<int, int> playerRatings = new Dictionary<int, int>();

            foreach (TournamentPlayer player in e.Players) {
                int id = player.UscfId;
                float? rating = null;
                switch (type.ToLower()) {
                    case "regular":
                    case "reg":
                    case "r":
                        if (player.RegularRating != null) {
                            if (postEventRatings) {
                                rating = player.RegularRating.RatingPost;
                            } else {
                                rating = player.RegularRating.RatingPre;
                            }
                        }
                        break;
                    case "quick":
                    case "q":
                        if (player.QuickRating != null) {
                            if (postEventRatings) {
                                rating = player.QuickRating.RatingPost;
                            } else {
                                rating = player.QuickRating.RatingPre;
                            }
                        }
                        break;
                    case "blitz":
                    case "b":
                        if (player.BlitzRating != null) {
                            if (postEventRatings) {
                                rating = player.BlitzRating.RatingPost;
                            } else {
                                rating = player.BlitzRating.RatingPre;
                            }
                        }
                        break;
                    default:
                        throw new Exception("Invalid rating type");
                }

                if (!playerRatings.ContainsKey(id) && rating.HasValue) {
                    playerRatings.Add(id, (int)Math.Round(rating.Value));
                }

            }

            return playerRatings.Values.ToList();
        }

        private void ProcessSummaryTable(UscfEvent e, HtmlNode summary) {
            // the html on the uscf site is missing a </tr>, so skip the first row
            // the first row says "Event Summary", so we're not missing anything important
            var rows = summary.Descendants("tr").Skip(1);

            List<HtmlNode> tds;
            int count;
            bool success;
            string key = "";
            string value = "";

            foreach (HtmlNode row in rows) {
                tds = row.Descendants("td").ToList();
                count = (tds != null) ? tds.Count() : 0;
                success = false;
                switch (count) {
                    case 2:
                        key = tds[0].InnerText.Trim();
                        value = tds[1].InnerText.Trim();
                        success = true;
                        break;
                    case 3:
                        key = tds[0].InnerText.Trim();
                        value = tds[2].InnerText.Trim();
                        success = true;
                        break;
                    default:
                        break;
                }
                if (success) {
                    e.AddSummaryItem(key, value);
                }
            }
        }

        private void ProcessSection(UscfEvent e, HtmlNode nodeSection, HtmlNode pre) {
            var html = nodeSection.OuterHtml;
            Match matchPre = UscfRegex.ExtractPreTable.Match(pre.InnerHtml);
            if (!matchPre.Success) {
                throw new Exception("Could not extract section data from html");
            }

            Section section = new Section();

            string raw = matchPre.Value;
            MatchCollection players = UscfRegex.ExtractPlayer.Matches(raw);
            foreach (Match matchPlayer in players) {
                TournamentPlayer p = new TournamentPlayer();
                string data = matchPlayer.Groups["data"].Value;

                p.Name = matchPlayer.Groups["name"].Value;
                p.Index = int.Parse(matchPlayer.Groups["index"].Value);

                Match matchState = UscfRegex.StateAndId.Match(data);
                p.State = matchState.Groups["state"].Value;
                p.UscfId = int.Parse(matchState.Groups["id"].Value);

                Match m = UscfRegex.GameResult.Match(data);
                p.Score = float.Parse(m.Groups["score"].Value);
                var games = m.Groups["game"].Captures;

                // extract the raw game data
                foreach (Capture game in games) {
                    p.AddGame(game.Value);
                }

                // get the rating info for the player
                MatchCollection ratingMatches = UscfRegex.RatingChange.Matches(data);
                p.AddRatings(ratingMatches);
                section.Players.Add(p);
                section.DictPlayers.Add(p.Index, p);
                e.Players.Add(p);
            }

            // process the players in the section to actually assign opponents
            foreach (TournamentPlayer player in section.Players) {
                foreach (Game game in player.Games) {
                    if (game.OpponentIndex.HasValue && game.OpponentIndex.Value > 0) {
                        game.Opponent = section.DictPlayers[game.OpponentIndex.Value];
                    }
                }
            }

            e.Sections.Add(section);
        }

        public List<UscfEvent> GetTournamentIds(string html) {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<UscfEvent> events = new List<UscfEvent>();

            var tableNodes = doc.DocumentNode.SelectNodes("//table[@cellpadding='4']");
            foreach (HtmlNode node in tableNodes) {
                var rows = node.Descendants("tr");
                foreach (HtmlNode row in rows) {
                    var tds = row.Descendants("td").ToList();
                    if (tds.Count == 2) {
                        var matchFirst = UscfRegex.AffiliateEventsFirstCell.Match(tds[0].InnerHtml);
                        if (matchFirst.Success) {
                            string title = "";
                            string otherDetails = "";

                            string dateString = matchFirst.Groups["date"].Value;
                            string id = matchFirst.Groups["id"].Value;

                            var linkNode = tds[1].Descendants("a").FirstOrDefault();
                            if (linkNode != null) {
                                title = linkNode.InnerText;
                            }

                            var smallNode = tds[1].Descendants("small").FirstOrDefault();
                            if (smallNode != null) {
                                otherDetails = smallNode.InnerText;
                            }

                            // build the event and append to events
                            UscfEvent e = new UscfEvent();
                            e.Id = id;
                            e.ParseDate(dateString);
                            e.Title = title;

                            events.Add(e);
                        }
                    }
                }

            }

            return events;
        }
    }
}