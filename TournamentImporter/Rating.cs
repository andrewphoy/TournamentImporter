using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    public class Rating {
        public bool Unrated { get; set; }
        public float RatingPre { get; set; }
        public float RatingPost { get; set; }
        public int ProvoPre { get; set; }
        public int ProvoPost { get; set; }

        public override string ToString() {
            return string.Format("Rating: {0} -> {1}", RatingPre, RatingPost); 
        }
    }
}
