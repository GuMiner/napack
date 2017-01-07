using System;
using System.Collections.Generic;
using System.Linq;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for requests for a single IP sent to the Napack Framework Server
    /// </summary>
    public class RequestStats
    {
        public RequestStats()
        {
            this.Calls = new List<DateTime>();
        }

        public List<DateTime> Calls { get; set; }

        /// <summary>
        /// Adds a call for this IP, returning true if this IP is now throttled.
        /// </summary>
        public bool AddCall()
        {
            Calls.Add(DateTime.UtcNow);
            Calls = Calls.Where(time => time + Global.SystemConfig.RequestThrottlingInterval > DateTime.UtcNow).ToList();
            return Calls.Count > Global.SystemConfig.MaxRequestsPerIpPerInterval;
        }
    }
}