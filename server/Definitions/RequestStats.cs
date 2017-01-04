using System;
using System.Collections.Generic;
using System.Linq;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for requests sent to the Napack Framework Server
    /// </summary>
    public class RequestStats
    {
        // TODO config file
        private static readonly int ThrottleLimitPerHour = 36000; // 10 calls/sec.
        private static TimeSpan LogTime = TimeSpan.FromHours(1);

        public RequestStats()
        {
            this.CallsByIp = new Dictionary<string, List<DateTime>>();
        }

        public Dictionary<string, List<DateTime>> CallsByIp { get; set; }

        /// <summary>
        /// Adds a call for a specified IP, returning true if the IP is currently being throttled.
        /// </summary>
        public bool AddCall(string ip)
        {
            if (!CallsByIp.ContainsKey(ip))
            {
                CallsByIp.Add(ip, new List<DateTime>());
            }

            CallsByIp[ip].Add(DateTime.UtcNow);
            CallsByIp[ip] = CallsByIp[ip].Where(time => time + LogTime > DateTime.UtcNow).ToList();
            return CallsByIp[ip].Count > ThrottleLimitPerHour;
        }
    }
}