using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for the Napack Framework Server system as a whole.
    /// </summary>
    /// <remarks>
    /// This class and its contents is stored in memory.
    /// </remarks>
    public class SystemStats
    {
        private CountryTracker countryTracker;

        public SystemStats(INapackStorageManager storageManager)
        {
            this.countryTracker = new CountryTracker(storageManager);
            this.RequestStats = new Dictionary<string, RequestStats>();
        }

        public Dictionary<string, RequestStats> RequestStats { get; set; }

        public bool AddCall(string ip)
        {
            countryTracker.LogRequest(ip);

            if (!RequestStats.ContainsKey(ip))
            {
                RequestStats[ip] = new RequestStats();
            }

            return RequestStats[ip].AddCall();
        }
    }
}