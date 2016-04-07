using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    class Options {

        [Option("debug", false, Name = "Debug", Description = "Display debugging information")]
        public bool Debug { get; set; }

        [Option("nocache", false, Name = "NoCache", Description = "Disable local caching")]
        public bool DisableCaching { get; set; }

        [Option("c|cache", null, Name = "Cache", Description = "The local cache directory")]
        public string LocalCacheDirectory { get; set; }

        [Option("t|tourney|tournament", null, Name = "[T]ournament", Description = "The USCF tournament identifier")]
        public string TournamentId { get; set; }

        [Option("a|affiliate", null, Name = "[A]ffiliate", Description = "The USCF affiliate identifier (starts with the letter A)")]
        public string AffiliateId { get; set; }

        [Option("o|output", "Ratings", Name = "[O]utput", Description = "The data that will be output, options include ratings, players")]
        public OutputType Output { get; set; }

        [Option("file", null, Name = "File", Description = "The output file destination; providing this will record data in a csv file with the given path")]
        public string OutputFile { get; set; }

        [Option("pattern", null, Name = "Pattern", Description = "A regular expression pattern applied to tournament titles")]
        public string Pattern { get; set; }

        [Option("start", null, Name = "Start", Description = "The start date used for filtering tournaments by end date")]
        public string StartDate { get; set; }

        [Option("end", null, Name = "End", Description = "The end date used for filtering tournaments by end date")]
        public string EndDate { get; set; }


    }
}
