using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NapackClient
{
    /// <summary>
    /// Performs the napack analysis and (potentially) update operations based on the current settings.
    /// </summary>
    internal class NapackOperator
    {
        private readonly string napackDirectory;
        private readonly List<DefinedNapackVersion> napacks;
        private readonly NapackClientSettings clientSettings;

        public NapackOperator(string napackDirectory, List<DefinedNapackVersion> napacks, NapackClientSettings clientSettings)
        {
            this.napackDirectory = napackDirectory;
            this.napacks = napacks;
            this.clientSettings = clientSettings;
        }

        /// <summary>
        /// Performs the operations. Returns true on success, false otherwise.
        /// </summary>
        internal bool Process()
        {
            // TODO implement a cleanup mechanism to get rid of old napacks.
            List<DefinedNapackVersion> newNapacks = new List<DefinedNapackVersion>(napacks);
            List<DefinedNapackVersion> existingNapacks = new List<DefinedNapackVersion>();
            List<DefinedNapackVersion> unusedNapacks = new List<DefinedNapackVersion>();
            List<string> unknownFolders = new List<string>();
            
            foreach (string directory in Directory.EnumerateDirectories(napackDirectory))
            {
                string napackDirectoryName = Path.GetFileName(directory);
                DefinedNapackVersion napackVersion = null;
                try
                {
                     napackVersion = new DefinedNapackVersion(napackDirectoryName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error parsing napack name: " + napackDirectoryName);
                    Console.Error.WriteLine(ex.Message);
                    unknownFolders.Add(napackDirectoryName);
                }

                if (napackVersion != null)
                {
                    if (napacks.Contains(napackVersion))
                    {
                        existingNapacks.Add(napackVersion);
                        newNapacks.Remove(napackVersion);
                    }
                    else
                    {
                        unusedNapacks.Add(napackVersion);
                    }
                }
            }

            // Right now, we do nothing with old, existing, or erroneous other than log them.
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Found {0} new napacks, {1} existing napacks, {2} unused napacks, and {3} unknown folders in the napack directory.",
                newNapacks.Count, existingNapacks.Count, unusedNapacks.Count, unknownFolders.Count));

            // Variables to declare in a config settings
            int maxParallelism = 20;


            // TODO: Ignore the binaries folder.
            // Download the new napacks into their corresponding folders.
            // Create binaries for the new napacks, replacing existing napacks that may or may not be there.
            return true;
        }
    }
}