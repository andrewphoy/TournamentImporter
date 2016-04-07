using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    public class Section {

        public Section() {
            this.DictPlayers = new Dictionary<int, TournamentPlayer>();
            this.Players = new List<TournamentPlayer>();
        }

        public string Title { get; set; }

        public Dictionary<int, TournamentPlayer> DictPlayers { get; set; }
        public List<TournamentPlayer> Players { get; set; }

    }
}
