using System;
using System.Collections.Generic;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for an individual Napack within the Napack Framework Server
    /// </summary>
    public class NapackStats
    {
        public long Downloads { get; set; }

        public long Views { get; set; }

        public int Versions { get; set; }

        public double AverageVersionSizeInMiB { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public TimeSpan AverageUpdateFrequency { get; set; }

        public List<string> AllAuthors { get; set; }
    }
}