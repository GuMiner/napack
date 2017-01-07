using System;
using System.Collections.Concurrent;

namespace Napack.Server
{
    public class CountryTracker
    {
        private readonly INapackStorageManager storageManager;

        private static readonly TimeSpan FlushInterval = TimeSpan.FromHours(1);
        private DateTime lastSaveDate;

        public CountryTracker(INapackStorageManager storageManager)
        {
            this.storageManager = storageManager;
            lastSaveDate = DateTime.UtcNow;

            IpToCountry = new ConcurrentDictionary<string, string>();
            CallsPerCountryPerDay = new ConcurrentDictionary<string, ConcurrentDictionary<DateTime, int>>();
            // TODO load the geolocation DB.
            // TODO load the calls per country per day for the current day.
        }
        
        public ConcurrentDictionary<string, string> IpToCountry { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<DateTime, int>> CallsPerCountryPerDay { get; set; }

        public void LogRequest(string ip)
        {
            // TODO
            string country;
            if (!IpToCountry.TryGetValue(ip, out country))
            {
                country = LookupCountry(ip);
                IpToCountry[ip] = country;
            }

            if (!CallsPerCountryPerDay.ContainsKey(country))
            {
               // CallsPerCountryPerDay[country] = 1;
            }
            else
            {
               // ++CallsPerCountryPerDay[country];
            }

            if (DateTime.UtcNow > lastSaveDate + CountryTracker.FlushInterval)
            {
                // TODO save the calls per country per day, removing all days except for today.
            }
        }



        public string LookupCountry(string ip)
        {
            // TODO the DB is in terms of IP blocks, which will need a lookup algorithm.
            return string.Empty;
        }
    }
}