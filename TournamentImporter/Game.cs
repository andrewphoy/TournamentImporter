using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    public class Game {
        public TournamentPlayer Player { get; set; }
        public TournamentPlayer Opponent { get; set; }

        public string RawData { get; set; }
        public string Result { get; set; }
        public int PlayerIndex { get; set; }
        public int? OpponentIndex { get; set; }
    }
}
