using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    class RatingStats {
        public int Count { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double StdDev { get; set; }
        public double Median { get; set; }
        public double Average { get; set; }
        public double LowerQuartile { get; set; }
        public double UpperQuartile { get; set; }
        public int CountExperts { get; set; }
        public int CountMasters { get; set; }
    }
}
