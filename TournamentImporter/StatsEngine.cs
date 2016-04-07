using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatCat;

namespace TournamentImporter {
    static class StatsEngine {

        public static RatingStats GetRatingStats(List<int> ratings) {
            var stats = new RatingStats();
            var sorted = ratings.OrderBy(i => i);

            stats.Min = sorted.Min();
            stats.Max = sorted.Max();
            stats.Count = sorted.Count();
            stats.StdDev = sorted.StdDev();
            stats.Average = sorted.Average();

            var quartiles = sorted.Quartiles();
            stats.LowerQuartile = quartiles[0];
            stats.Median = quartiles[1];
            stats.UpperQuartile = quartiles[2];

            stats.CountExperts = sorted.Count(i => i >= 2000);
            stats.CountMasters = sorted.Count(i => i >= 2200);
            return stats;
        }
    }
}
