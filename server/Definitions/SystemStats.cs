using System.Collections.Generic;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for the Napack Framework Server system as a whole.
    /// </summary>
    public class SystemStats
    {
        public SystemStats()
        {
            this.RequestStats = new RequestStats();
        }

        public RequestStats RequestStats { get; set; }

        public Dictionary<string, string> IpToCountry { get; set; }

        public string LookupCountry(string ip)
        {
            return string.Empty;
        }
    }
}