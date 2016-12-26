using System;
using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Holds statistics for an individual Napack within the Napack Framework Server
    /// </summary>
    public class NapackStats
    {
        public NapackStats()
        {
            this.AllAuthors = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            this.Downloads = 0;
            this.Views = 0;
            this.Versions = 0;
            this.AverageUpdateFrequency = TimeSpan.Zero;
            this.AverageVersionSizeInkiB = 0;
        }

        public void AddVersion(NewNapackVersion newVersion)
        {
            foreach (string author in newVersion.Authors)
            {
                this.AllAuthors.Add(author);
            }

            const double charactersPerKiB = 124; // Assuming UTF8 and no special characters
            double versionSizeInKiB = (double)newVersion.Files.Sum(file => file.Value.Contents.Length) / charactersPerKiB;
            this.AverageVersionSizeInkiB = (this.AverageVersionSizeInkiB * this.Versions + versionSizeInKiB) / (this.Versions + 1);

            TimeSpan updateFrequency = DateTime.UtcNow - this.LastUpdateTime;
            this.LastUpdateTime = DateTime.UtcNow;
            this.AverageUpdateFrequency = TimeSpan.FromSeconds(
                ((this.AverageUpdateFrequency.TotalSeconds * this.Versions + updateFrequency.TotalSeconds) / (this.Versions + 1)));

            // This must go last to ensure proper average calculations.
            this.Versions++;
        }

        public HashSet<string> AllAuthors { get; set; }

        public long Downloads { get; set; }

        public long Views { get; set; }

        public double AverageVersionSizeInkiB { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public TimeSpan AverageUpdateFrequency { get; set; }

        public int Versions { get; set; }
    }
}