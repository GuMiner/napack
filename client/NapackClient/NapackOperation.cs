using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Napack.Common;

namespace NapackClient
{
    /// <summary>
    /// Performs the napack analysis and (potentially) update operations based on the current settings.
    /// </summary>
    internal class NapackOperation
    {
        private readonly INapackServerClient nfsClient;
        private readonly List<DefinedNapackVersion> napacks;
        private readonly NapackClientSettings clientSettings;

        public NapackOperation(INapackServerClient nfsClient, List<DefinedNapackVersion> napacks, NapackClientSettings clientSettings)
        {
            this.nfsClient = nfsClient;
            this.napacks = napacks;
            this.clientSettings = clientSettings;
        }

        /// <summary>
        /// Acquires the new napacks.
        /// </summary>
        public void AcquireNapacks(string napackDirectory)
        {
            // TODO implement a cleanup mechanism to get rid of old napacks.
            // TODO detect dependent napacks to avoid listing them in the unused section.
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

            int level = 0;
            List<NapackMajorVersion> dependencies = this.DownloadNewNapacks(napackDirectory, newNapacks);
            TabulateNewDependencies(existingNapacks, newNapacks, dependencies);

            Console.WriteLine("Processed dependency level " + level + " with " + dependencies.Count + " new dependencies found.");

            while (dependencies.Any())
            {
                dependencies = DownloadDependentNapacks(napackDirectory, dependencies);
                TabulateNewDependencies(existingNapacks, newNapacks, dependencies);

                ++level;
                Console.WriteLine("Processed dependency level " + level);
            }
        }

        /// <summary>
        /// Updates the targets files for all listed napacks.
        /// </summary>
        public void UpdateTargets(string napackDirectory)
        {
            // TODO a rewrite should have this use the information from our Napack JSON file, new napacks, to avoid rescanning our directory tree
            List<DefinedNapackVersion> newestNapackVersions = new List<DefinedNapackVersion>();
            foreach (string directory in Directory.GetDirectories(napackDirectory))
            {
                string napackDirectoryName = Path.GetFileName(directory);
                DefinedNapackVersion napackVersion = null;
                try
                {
                    napackVersion = new DefinedNapackVersion(napackDirectoryName);
                }
                catch (Exception ex)
                {
                    // At this point this shound't have any failures, as we would have hit failrures earlier.
                    Console.Error.WriteLine("Error parsing napack name: " + napackDirectoryName);
                    Console.Error.WriteLine(ex.Message);
                }

                DefinedNapackVersion existingVersion = newestNapackVersions.SingleOrDefault(
                    version => version.NapackName.Equals(napackVersion.NapackName, StringComparison.OrdinalIgnoreCase) && version.Major == napackVersion.Major);
                if (existingVersion == null)
                {
                    newestNapackVersions.Add(napackVersion);
                }
                else if ((existingVersion.Minor < napackVersion.Minor) || 
                    (existingVersion.Minor == napackVersion.Minor && existingVersion.Patch < napackVersion.Patch))
                {
                    newestNapackVersions.Remove(existingVersion);
                    newestNapackVersions.Add(napackVersion);
                }
                // else we keep the existing version.
            }

            NapackTargets.SaveNapackTargetsFile(napackDirectory, newestNapackVersions);
        }

        /// <summary>
        /// Tabulates new dependencies by adding the new napacks to the downloaded list, and removing dependencies that have already been retrieved.
        /// </summary>
        private void TabulateNewDependencies(List<DefinedNapackVersion> existingNapacks, List<DefinedNapackVersion> newNapacks, List<NapackMajorVersion> dependencies)
        {
            // TODO this doesn't detect duplicates, which is OK for now.
            existingNapacks.AddRange(newNapacks);
            for (int i = 0; i < dependencies.Count; i++)
            {
                // TODO this is stupidly inefficient as I can do a set operation instead.
                if (existingNapacks.Any(napack => napack.NapackName.Equals(dependencies[i].Name, StringComparison.OrdinalIgnoreCase) && napack.Major == dependencies[i].Major))
                {
                    dependencies.RemoveAt(i);
                    i--;
                }
            }
        }

        private List<NapackMajorVersion> DownloadDependentNapacks(string napackDirectory, List<NapackMajorVersion> newNapacks)
        {
            // TODO parallelize and remove duplication with the below.
            Stopwatch timer = Stopwatch.StartNew();
            List<NapackMajorVersion> dependencies = new List<NapackMajorVersion>();
            foreach (NapackMajorVersion napackVersion in newNapacks)
            {
                NapackVersion version = this.nfsClient.GetMostRecentMajorVersionAsync(napackVersion).GetAwaiter().GetResult();
                SaveNapack(napackDirectory, napackVersion.Name, version);
                if (version.Dependencies.Any())
                {
                    dependencies.AddRange(version.Dependencies);
                }
            }

            Console.WriteLine("Downloaded " + newNapacks.Count + " in " + timer.ElapsedMilliseconds + " ms.");
            return dependencies;
        }

        private List<NapackMajorVersion> DownloadNewNapacks(string napackDirectory, List<DefinedNapackVersion> newNapacks)
        {
            // TODO parallelize
            Stopwatch timer = Stopwatch.StartNew();
            List<NapackMajorVersion> dependencies = new List<NapackMajorVersion>();
            foreach (DefinedNapackVersion napackVersion in newNapacks)
            {
                NapackVersion version = this.nfsClient.GetNapackVersionAsync(napackVersion).GetAwaiter().GetResult();
                SaveNapack(napackDirectory, napackVersion.NapackName, version);
                if (version.Dependencies.Any())
                {
                    dependencies.AddRange(version.Dependencies);
                }
            }

            Console.WriteLine("Downloaded " + newNapacks.Count + " in " + timer.ElapsedMilliseconds + " ms.");
            return dependencies;
        }

        private void SaveNapack(string napackDirectory, string name, NapackVersion version)
        {
            // TODO parallelize asynchronously.
            foreach (KeyValuePair<string, string> file in version.Files)
            {
                try
                {
                    File.WriteAllText(Path.Combine(napackDirectory, file.Key), file.Value);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error saving napack file: " + ex.Message);
                    throw;
                }
            }

            string napackIdentifier = name + "_" + version.Major;
            GenerateTarget(napackDirectory, name, version.Major, version.Minor, version.Patch, version.Files.Keys);
        }

        private void GenerateTarget(string napackDirectory, string napack, int major, int minor, int patch, IEnumerable<string> targetFiles)
        {
            string napackFilename = napack + "_" + major + "_" + minor + "_" + patch;

            StringBuilder targetFileBuilder = new StringBuilder();
            targetFileBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            targetFileBuilder.AppendLine("<Target Name=\"");
            targetFileBuilder.AppendLine(napackFilename + "\" BeforeTargets=\"Build\">");
            targetFileBuilder.AppendLine("  <ItemGroup>");

            foreach (string file in targetFiles)
            {
                targetFileBuilder.AppendLine("    <Content Include=\"" + file + "\" />");
            }

            targetFileBuilder.AppendLine("  </ItemGroup>");
            targetFileBuilder.AppendLine("</Target>");

            string targetsFilename = Path.Combine(napackDirectory, napackFilename, ".targets");
            File.WriteAllText(targetsFilename, targetFileBuilder.ToString());
        }
    }
}