using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StatCat;

namespace TournamentImporter {
    class Program {
        static void Main(string[] args) {
            Options options = OptionAttribute.Parse<Options>(args);

            if (!string.IsNullOrEmpty(options.TournamentId)) {
                HandleTournament(options);
                Console.ReadLine();
                return;
            }

            if (!string.IsNullOrEmpty(options.AffiliateId)) {
                HandleAffiliate(options);
                Console.ReadLine();
                return;
            }

            return;
        }

        static void HandleTournament(Options options) {
            var clients = CreateClients(options);
            var client = clients.Item1;
            var parser = clients.Item2;

            string html = client.DownloadTournament(options.TournamentId);
            if (string.IsNullOrEmpty(html)) {
                // error
                Console.WriteLine("Could not download information for tournament " + options.TournamentId);
                return;
            }

            UscfEvent e = parser.ParseTournament(html, options.TournamentId);

            if (options.Output == OutputType.ratings) {
                var ratings = parser.GetRatings(e, true, "regular");
                RatingStats stats = StatsEngine.GetRatingStats(ratings);
                PrintRatingStats(e, stats);
            } else if (options.Output == OutputType.players) {

            } else {
                // unknown output type

            }
        }

        static void HandleAffiliate(Options options) {
            var clients = CreateClients(options);
            var client = clients.Item1;
            var parser = clients.Item2;

            string html = client.DownloadTournamentsForAffiliate(options.AffiliateId);
            if (string.IsNullOrEmpty(html)) {
                // error
                Console.WriteLine("Could not download tournaments for affiliate " + options.AffiliateId);
                return;
            }

            // get stubs for all of the events by this affiliate
            List<UscfEvent> events = parser.GetTournamentIds(html);
            events = FilterEvents(events, options);

            if (options.Debug) {
                Console.WriteLine("Found {0} tournaments", events.Count);
            }

            List<UscfEvent> fullEvents = new List<UscfEvent>();

            foreach (UscfEvent e in events) {
                string eventHtml = client.DownloadTournament(e.Id);
                UscfEvent fullEvent = parser.ParseTournament(eventHtml, e.Id);
                fullEvents.Add(fullEvent);

                if (options.Output == OutputType.ratings) {
                    var ratings = parser.GetRatings(fullEvent, true, "regular");
                    RatingStats stats = StatsEngine.GetRatingStats(ratings);
                    PrintRatingStats(e, stats);
                }
            }

            if (options.Output == OutputType.ratings) {
                OutputRatingData(fullEvents, parser, options);
            }
        }

        private static List<UscfEvent> FilterEvents(List<UscfEvent> events, Options options) {
            if (!string.IsNullOrEmpty(options.Pattern)) {
                // if a pattern was provided, apply it to the event titles
                Regex titlePattern = new Regex(options.Pattern.ToUpper(), RegexOptions.IgnoreCase);
                events = events.Where(e => titlePattern.IsMatch(e.Title)).ToList();
            }

            if (!string.IsNullOrEmpty(options.StartDate) || !string.IsNullOrEmpty(options.EndDate)) {
                DateTime? dtStart = null, dtEnd = null;
                DateTime dtTryStart, dtTryEnd;

                if (DateTime.TryParse(options.StartDate, out dtTryStart)) {
                    dtStart = dtTryStart;
                }
                if (DateTime.TryParse(options.EndDate, out dtTryEnd)) {
                    dtEnd = dtTryEnd;
                }

                events = events.Where(e =>
                    (!dtStart.HasValue || (dtStart <= e.EndDate))
                    && (!dtEnd.HasValue || (dtEnd >= e.EndDate))
                ).ToList();
            }

            return events;
        }

        private static void PrintRatingStats(UscfEvent e, RatingStats stats) {
            Console.WriteLine("Tournament:\t" + e.Title);
            Console.WriteLine("# Players:\t" + stats.Count);
            Console.WriteLine("Min Rating:\t" + stats.Min);
            Console.WriteLine("Max Rating:\t" + stats.Max);
            Console.WriteLine("Average Rating:\t" + Math.Round(stats.Average, 3));
            Console.WriteLine("Std Dev Rating:\t" + Math.Round(stats.StdDev, 3));
            Console.WriteLine("Lower Quartile:\t" + stats.LowerQuartile);
            Console.WriteLine("Median Rating:\t" + stats.Median);
            Console.WriteLine("Upper Quartile:\t" + stats.UpperQuartile);
            Console.WriteLine("2000+ Players:\t" + stats.CountExperts);
            Console.WriteLine("2200+ Players:\t" + stats.CountMasters);
        }

        private static void OutputRatingData(IEnumerable<UscfEvent> events, UscfParser parser, Options options) {
            string csvPath = null;
            bool writeFile = false;

            if (!string.IsNullOrEmpty(options.OutputFile)) {
                writeFile = true;
                csvPath = options.OutputFile;
            }

            StreamWriter writer = null;
            try {
                if (writeFile) {
                    writer = new StreamWriter(csvPath, false);
                    // write the header row for the file
                    writer.WriteLine(string.Join("\t", new string[] {
                        "ID",
                        "Title",
                        "End Date",
                        "Count",
                        "Min",
                        "Max",
                        "Average",
                        "Std Dev",
                        "Lower Quartile",
                        "Median",
                        "Upper Quartile",
                        "Count Experts",
                        "Count Masters"
                    }));
                }

                foreach (UscfEvent e in events) {
                    var ratings = parser.GetRatings(e, true, "regular");
                    RatingStats stats = StatsEngine.GetRatingStats(ratings);

                    var data = new object[] {
                        e.Id,
                        e.Title,
                        e.EndDate.ToString("yyyy-MM-dd"),
                        stats.Count,
                        stats.Min,
                        stats.Max,
                        Math.Round(stats.Average, 3),
                        Math.Round(stats.StdDev, 3),
                        stats.LowerQuartile,
                        stats.Median,
                        stats.UpperQuartile,
                        stats.CountExperts,
                        stats.CountMasters
                    };

                    if (writeFile) {
                        writer.WriteLine(string.Join("\t", data));
                    }
                }
            } catch (Exception ex) {

            } finally {
                if (writer != null) {
                    writer.Dispose();
                }
            }
        }

        private static Tuple<UscfClient, UscfParser> CreateClients(Options options) {
            UscfClient client = new UscfClient(options.LocalCacheDirectory);
            UscfParser parser = new UscfParser();

            if (options.DisableCaching) {
                client.CacheLocally = false;
            }

            return Tuple.Create(client, parser);
        }
    }
}
