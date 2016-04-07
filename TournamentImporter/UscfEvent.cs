using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentImporter {
    public class UscfEvent {

        public UscfEvent() {
            this.Players = new List<TournamentPlayer>();
            this.Sections = new List<Section>();
            this.AdditionalHeaders = new Dictionary<string, string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string EventDates { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Affiliate { get; set; }
        public string ChiefTd { get; set; }

        public List<TournamentPlayer> Players { get; set; }
        public List<Section> Sections { get; set; }

        public Dictionary<string, string> AdditionalHeaders { get; set; }
    }
}
