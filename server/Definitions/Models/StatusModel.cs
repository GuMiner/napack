using System;

namespace Napack.Server
{
    public class StatusModel
    {
        public StatusModel()
        {
        }

        public StatusModel(long totalCalls, long uniqueIps, TimeSpan uptime)
        {
            this.TotalCalls = totalCalls;
            this.UniqueIps = uniqueIps;
            this.Uptime = uptime;
        }

        public long TotalCalls { get; set; }

        public long UniqueIps { get; set; }

        public TimeSpan Uptime { get; set; }
    }
}