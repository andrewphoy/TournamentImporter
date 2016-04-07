using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    public class TournamentPlayer {

        public TournamentPlayer() {
            this.Games = new List<Game>();
        }

        public int Index { get; set; }
        public string Name { get; set; }
        public int UscfId { get; set; }
        public string State { get; set; }

        public Rating RegularRating { get; set; }
        public Rating QuickRating { get; set; }
        public Rating BlitzRating { get; set; }

        public float Score { get; set; }

        public List<Game> Games { get; set; }
    }
}
