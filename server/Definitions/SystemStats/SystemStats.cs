using System;
using System.Collections.Concurrent;
using System.Threading;

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
        private object lockObject;
        private DateTime startTime;

        public SystemStats()
        {
            this.RequestStats = new ConcurrentDictionary<string, RequestStats>();
            this.TotalCallsSinceUptime = 0;
            this.UniqueIpsSinceUptime = 0;
            this.lockObject = new object();
            this.startTime = DateTime.UtcNow;
        }

        public ConcurrentDictionary<string, RequestStats> RequestStats { get; private set; }

        public long TotalCallsSinceUptime;

        public long UniqueIpsSinceUptime;

        public TimeSpan Uptime => DateTime.UtcNow - startTime;

        public bool AddCall(string ip)
        {
            Interlocked.Increment(ref this.TotalCallsSinceUptime);
            ++this.TotalCallsSinceUptime;

            if (!RequestStats.ContainsKey(ip))
            {
                lock (this.lockObject)
                {
                    if (!RequestStats.ContainsKey(ip))
                    {
                        Interlocked.Increment(ref this.UniqueIpsSinceUptime);
                        RequestStats.AddOrUpdate(ip, new RequestStats(), (key, existing) => existing);
                    }
                }
            }

            return RequestStats[ip].AddCall();
        }
    }
}