using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TournamentImporter {
    /// <summary>
    /// Downloads html files from the uschess.org website
    /// Optionally caches the files locally as appropriate
    /// </summary>
    class UscfClient {
        private WebClient _client;

        public UscfClient() : this(null)  {
        }

        public UscfClient(string localPath) {
            _client = new WebClient();
            _client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36");

            // setup local caching
            if (string.IsNullOrEmpty(localPath )) {
                string tempDir = Path.Combine(Path.GetTempPath(), "USCF Tournament Data");
                try {
                    Directory.CreateDirectory(tempDir);
                    this.CacheLocally = true;
                    this.SuppressCaching = false;
                    this.LocalDirectory = tempDir;
                } catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    this.SuppressCaching = true;
                    this.CacheLocally = false;
                    this.LocalDirectory = null;
                }
            } else {
                if (Directory.Exists(localPath)) {
                    this.CacheLocally = true;
                    this.SuppressCaching = false;
                    this.LocalDirectory = localPath;
                } else {
                    this.SuppressCaching = true;
                    this.CacheLocally = false;
                    this.LocalDirectory = null;
                }
            }
        }

        private bool SuppressCaching { get; set; }
        public bool CacheLocally { get; set; }
        public string LocalDirectory { get; set; }

        private bool UseCaching() {
            return (!SuppressCaching) && CacheLocally && !string.IsNullOrEmpty(LocalDirectory);
        }

        /// <summary>
        /// Downloads a specific tournament represented by USCF string ID
        /// </summary>
        /// <param name="id">USCF tournament identifier</param>
        /// <example>DownloadTournament("201602259682")</example>
        /// <returns>HTML string</returns>
        public string DownloadTournament(string id) {
            string url = "http://www.uschess.org/msa/XtblMain.php?{0}.0";
            string local = Path.Combine(LocalDirectory, id + ".html");

            if (UseCaching()) {
                if (File.Exists(local)) {
                    return File.ReadAllText(local);
                }
            }

            try {
                if (UseCaching()) {
                    _client.DownloadFile(string.Format(url, id), local);
                    if (File.Exists(local)) {
                        return File.ReadAllText(local);
                    }
                }

                return _client.DownloadString(string.Format(url, id));

            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public string DownloadTournamentsForAffiliate(string affiliateId) {
            //http://www.uschess.org/msa/AffDtlTnmtHst.php?A5000408
            string url = "http://www.uschess.org/msa/AffDtlTnmtHst.php?{0}.0";

            // we cache this data, but only for a day because new tournaments could invalidate a page
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            string local = Path.Combine(LocalDirectory, affiliateId + "-" + dateString + ".html");

            if (UseCaching()) {
                if (File.Exists(local)) {
                    return File.ReadAllText(local);
                }
            }

            try {
                if (UseCaching()) {
                    _client.DownloadFile(string.Format(url, affiliateId), local);
                    if (File.Exists(local)) {
                        return File.ReadAllText(local);
                    }
                }

                return _client.DownloadString(string.Format(url, affiliateId));
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
