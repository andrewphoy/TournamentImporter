using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TournamentImporter {
    internal struct UscfRegex {
        public static Regex ExtractPreTable = new Regex("[-]+\\n\\s*Pair(.*)[-]+", 
            RegexOptions.Singleline | RegexOptions.Compiled);

        public static Regex ExtractPlayer = new Regex(@"^-----[-]+\s*<a\s*href(?:.*?)>(?<index>\d+)<\/a>\s*\|\s*<a\s*href(?:.*?)>(?<name>.*?)<\/a>(?<data>.*?)\|\s*\n(?=-----)", 
            RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Regex GameResult = new Regex(@"\|\s*(?<score>\d+\.\d)\s*\|(?<game>\s*[A-Z]\s*(\d+)?\s*\|)+",
            RegexOptions.Compiled);

        public static Regex ParseGame = new Regex(@"\s*(?<result>[A-Z])\s*(?<opp>\d+)?",
            RegexOptions.Compiled);

        public static Regex RatingChange = new Regex(@"(?<type>[RQB]):\s*(?<pre>[Uu]nrated|\d+)(?<ppre>P(?<pprecnt>\d+))?\s*->\s*(?<post>\d+)(?<ppost>P(?<ppostcnt>\d+))?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Regex StateAndId = new Regex(@"^\s*(?<state>\w+)?\s*\|\s*(?<id>\d{8})\s*/",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public static Regex AffiliateEventsFirstCell = new Regex(@"(?<date>\d{4}-\d{2}-\d{2})\s*<br><small>(?<id>\d+)</small>",
            RegexOptions.Compiled);
    }
}
