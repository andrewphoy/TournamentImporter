using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TournamentImporter {
    public static class ExtensionMethods {
        public static Regex DoubleSpaceReplace = new Regex("\\s+", RegexOptions.Compiled);

        public static string HtmlTrim(this string str) {
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&nbsp", " ");
            str = DoubleSpaceReplace.Replace(str, " ");
            str = str.Trim();
            return str;
        }

        public static bool AddSummaryItem(this UscfEvent e, string name, string value) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentNullException(nameof(value));
            }
            name = name.HtmlTrim();
            value = value.HtmlTrim();

            bool resolvedHeader = true;

            switch (name.ToLower()) {
                case "event":
                    e.Title = value;
                    break;
                case "location":
                    e.Location = value;
                    break;
                case "event date(s)":
                    e.EventDates = value;
                    // try to parse start and end dates
                    e.ParseDate(value);
                    break;
                case "sponsoring affiliate":
                    e.Affiliate = value;
                    break;
                case "chief td":
                    e.ChiefTd = value;
                    break;
                default:
                    resolvedHeader = false;
                    break;
            }

            if (!resolvedHeader) {
                e.AdditionalHeaders[name] = value;
            }

            return true;
        }

        public static void ParseDate(this UscfEvent e, string dateString) {
            // 2016-02-02 thru 2016-02-23
            // 2016-03-03
            try {
                if (dateString.Contains("thru")) {
                    var parts = dateString.Split(new string[] { "thru" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2) {
                        DateTime dtStart, dtEnd;
                        if (DateTime.TryParse(parts[0], out dtStart)) {
                            e.StartDate = dtStart;
                        }
                        if (DateTime.TryParse(parts[1], out dtEnd)) {
                            e.EndDate = dtEnd;
                        }
                    }
                } else {
                    DateTime dt;
                    if (DateTime.TryParse(dateString, out dt)) {
                        e.EndDate = dt;
                    }
                }
            } catch (Exception ex) {
                // as a fallback, try to get the end date from the event id
                //201603054572
                try {
                    int year, month, day;
                    if (e.Id.Length > 8) {
                        year = int.Parse(e.Id.Substring(0, 4));
                        month = int.Parse(e.Id.Substring(4, 2));
                        day = int.Parse(e.Id.Substring(6, 2));

                        e.EndDate = new DateTime(year, month, day);
                    }
                } catch (Exception innerex) {
                    // do nothing
                }
            }
        }

        public static void AddRatings(this TournamentPlayer p, MatchCollection mc) {
            foreach (Match m in mc) {
                p.AddRating(m);
            }
        }

        public static void AddRating(this TournamentPlayer p, Match m) {
            Rating rating = new Rating();
            string type = m.Groups["type"].Value;
            string strRatingPre = m.Groups["pre"].Value;

            float ratingPre = 0;
            bool notUnrated = float.TryParse(strRatingPre, out ratingPre);
            rating.RatingPre = ratingPre;

            rating.Unrated = !notUnrated;
            rating.RatingPost = float.Parse(m.Groups["post"].Value);

            bool provoPre = m.Groups["ppre"].Success;
            bool provoPost = m.Groups["ppost"].Success;
            rating.ProvoPre = provoPre ? int.Parse(m.Groups["pprecnt"].Value) : 0;
            rating.ProvoPost = provoPost ? int.Parse(m.Groups["ppostcnt"].Value) : 0;


            rating.RatingPre = ratingPre;
            switch (type) {
                case "R":
                    p.RegularRating = rating;
                    break;
                case "Q":
                    p.QuickRating = rating;
                    break;
                case "B":
                    p.BlitzRating = rating;
                    break;
            }

        }

        public static void AddGame(this TournamentPlayer p, string gameData) {
            var g = new Game();
            g.RawData = gameData.Trim(new char[] { ' ', '|' });
            g.Player = p;
            g.PlayerIndex = p.Index;

            Match m = UscfRegex.ParseGame.Match(gameData);
            g.Result = m.Groups["result"].Value;
            if (m.Groups["opp"].Success) {
                g.OpponentIndex = int.Parse(m.Groups["opp"].Value);
            }

            p.Games.Add(g);
        }
    }
}
