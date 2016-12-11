using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Napack.Client.Common;
using Napack.Common;
using Ookii.CommandLine;

namespace Napack.Client
{
    /// <summary>
    /// Performs the napack analysis and (potentially) update operations based on the current settings.
    /// </summary>
    [Description("Updates Napacks consumed within the current project.")]
    internal class UpdateOperation : INapackOperation
    {
        private const string LockFileSuffix = ".lock";

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The operation being performed.")]
        public string Operation { get; set; }

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The JSON file listing the Napacks used by the current project.")]
        public string NapackJson { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettings { get; set; }

        [CommandLineArgument(Position = 3, IsRequired = true)]
        [Description("The directory storing downloaded Napacks")]
        public string NapackDirectory { get; set; }

        public bool IsValidOperation() => this.Operation.Equals("Update", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            // The specified napacks.
            Dictionary<string, string> rawNapacks = Serializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(this.NapackJson));
            List<NapackVersionIdentifier> listedNapacks = rawNapacks.Select(item => new NapackVersionIdentifier(item.Key + "." + item.Value)).ToList();

            // The specified napacks, plus those dependencies. This doesn't have to exist -- we'll detect everything as unused if it doesn't and clear the napack folder.
            Dictionary<string, List<NapackVersion>> knownNapacks = new Dictionary<string, List<NapackVersion>>();
            try
            {
                knownNapacks = Serializer.Deserialize<Dictionary<string, List<NapackVersion>>>(File.ReadAllText(this.NapackJson + UpdateOperation.LockFileSuffix));
            }
            catch (Exception)
            {
                Console.WriteLine("Didn't find the napack lock file; packages may be redownloaded.");
            }
            
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettings));

            // Now that we've read in everything, start processing.
            knownNapacks = this.RemoveUnusedNapacks(knownNapacks);

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                this.AcquireNapacks(client, listedNapacks, knownNapacks);
            }
            
            // Save the lock file now that we're in an updated state.
            File.WriteAllText(this.NapackJson + UpdateOperation.LockFileSuffix, Serializer.Serialize(knownNapacks));

            // Extract out our version identifiers
            List<NapackVersionIdentifier> napackVersionIdentifiers = new List<NapackVersionIdentifier>();
            Dictionary<string, NapackVersion> allNapackVersions = new Dictionary<string, NapackVersion>();
            foreach (KeyValuePair<string, List<NapackVersion>> napackVersions in knownNapacks)
            {
                foreach (NapackVersion napackVersion in napackVersions.Value)
                {
                    napackVersionIdentifiers.Add(new NapackVersionIdentifier(napackVersions.Key, napackVersion.Major, napackVersion.Minor, napackVersion.Patch));
                    allNapackVersions.Add(napackVersions.Key, napackVersion);
                }
            }

            NapackTargets.SaveNapackTargetsFile(this.NapackDirectory, napackVersionIdentifiers);
            NapackAttributions.SaveNapackAttributionsFile(this.NapackDirectory, allNapackVersions);
        }

        /// <summary>
        /// Remove folders that aren't napacks or napack major versions that aren't referenced.
        /// </summary>
        /// <returns>The known napacks, with napacks that don't exist removed.</returns>
        private Dictionary<string, List<NapackVersion>> RemoveUnusedNapacks(IDictionary<string, List<NapackVersion>> knownNapacks)
        {
            int unknownFolders = 0;
            int unusedNapacks = 0;
            List<NapackVersionIdentifier> versions = new List<NapackVersionIdentifier>();
            foreach (string directory in Directory.EnumerateDirectories(this.NapackDirectory))
            {
                string napackDirectoryName = Path.GetFileName(directory);
                NapackVersionIdentifier napackVersion = null;
                try
                {
                    napackVersion = new NapackVersionIdentifier(napackDirectoryName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Directory is not a Napack; removing " + napackDirectoryName);
                    Console.WriteLine(ex.Message);
                    Directory.Delete(directory, true);
                    ++unknownFolders;
                }

                if (!knownNapacks.ContainsKey(napackVersion.NapackName) ||
                    !knownNapacks[napackVersion.NapackName].Any(version => version.Major == napackVersion.Major))
                {
                    // This is an unknown napack or major version. Delete.
                    Console.WriteLine("Napack is not used, deleting: " + napackDirectoryName);
                    Directory.Delete(directory, true);
                    ++unusedNapacks;
                }

                versions.Add(napackVersion);
            }

            Dictionary<string, List<NapackVersion>> redactedKnownNapacks = new Dictionary<string, List<NapackVersion>>();
            foreach (KeyValuePair<string, List<NapackVersion>> napack in knownNapacks)
            {
                foreach (NapackVersion version in napack.Value)
                {
                    if (versions.Any(ver => ver.NapackName.Equals(napack.Key) && ver.Major == version.Major))
                    {
                        if (!redactedKnownNapacks.ContainsKey(napack.Key))
                        {
                            redactedKnownNapacks.Add(napack.Key, new List<NapackVersion>());
                        }

                        redactedKnownNapacks[napack.Key].Add(version);
                    }
                }
            }

            Console.WriteLine($"Deleted {unknownFolders} unknown folders and {unusedNapacks} unused napacks.");
            return redactedKnownNapacks;
        }

        private void AcquireNapacks(NapackServerClient client, List<NapackVersionIdentifier> listedNapacks, Dictionary<string, List<NapackVersion>> knownNapacks)
        {
            List<NapackVersionIdentifier> newNapacks = new List<NapackVersionIdentifier>();
            foreach (NapackVersionIdentifier listedNapack in listedNapacks)
            {
                if (!knownNapacks.Any(napack => 
                        napack.Key.Equals(listedNapack.NapackName, StringComparison.InvariantCulture) && 
                        napack.Value.Any(individualPack => individualPack.Major == listedNapack.Major)))
                {
                    // No napack was found with the specified name and major version.
                    newNapacks.Add(listedNapack);
                }
            }

            Console.WriteLine($"Found {newNapacks.Count} new napacks to download.");

            
            // Ah, for the want of a multiple return...
            List<Tuple<NapackVersionIdentifier, NapackVersion>> newDownloadedNapacks = this.DownloadNewNapacks(client, newNapacks);
            List<NapackMajorVersion> dependencies = TabulateNewDependencies(knownNapacks, newDownloadedNapacks);

            int level = 0;
            Console.WriteLine("Processed dependency level " + level + " with " + dependencies.Count + " new dependencies found.");

            while (dependencies.Any())
            {
                newDownloadedNapacks = DownloadDependentNapacks(client, dependencies);
                dependencies = TabulateNewDependencies(knownNapacks, newDownloadedNapacks);

                ++level;
                Console.WriteLine("Processed dependency level " + level);
            }
        }

        /// <summary>
        /// Tabulates new dependencies by adding the new napacks to the downloaded list, 
        /// and removing dependencies that have already been retrieved.
        /// </summary>
        private List<NapackMajorVersion> TabulateNewDependencies(Dictionary<string, List<NapackVersion>> knownNapacks,
            List<Tuple<NapackVersionIdentifier, NapackVersion>> downloadedNapacks)
        {
            // Add to the known napacks everything downloaded.
            foreach (Tuple<NapackVersionIdentifier, NapackVersion> newNapack in downloadedNapacks)
            {
                if (!knownNapacks.ContainsKey(newNapack.Item1.NapackName))
                {
                    knownNapacks.Add(newNapack.Item1.NapackName, new List<NapackVersion>());
                }

                knownNapacks[newNapack.Item1.NapackName].Add(newNapack.Item2);
            }

            List<NapackMajorVersion> dependencies = downloadedNapacks.SelectMany(tuple => tuple.Item2.Dependencies).ToList();
            for (int i = 0; i < dependencies.Count; i++)
            {
                // Remove any dependencies that already exist.
                if (knownNapacks.Any(napack => napack.Key.Equals(dependencies[i].Name, StringComparison.InvariantCulture) && 
                        napack.Value.Any(individualNapack => individualNapack.Major == dependencies[i].Major)))
                {
                    dependencies.RemoveAt(i);
                    i--;
                }
            }

            return dependencies;
        }

        private List<Tuple<NapackVersionIdentifier, NapackVersion>> DownloadDependentNapacks(NapackServerClient client, List<NapackMajorVersion> newNapacks)
        {
            return DownloadNapacks<NapackMajorVersion>(newNapacks,
                (napackMajorVersion) => client.GetMostRecentMajorVersionAsync(napackMajorVersion),
                (napackMajorVersion, napackVersion) =>
                    new NapackVersionIdentifier(napackMajorVersion.Name, napackVersion.Major, napackVersion.Minor, napackVersion.Patch));
        }

        /// <summary>
        /// Downloads new napacks, returning the dependencies found.
        /// </summary>
        private List<Tuple<NapackVersionIdentifier, NapackVersion>> DownloadNewNapacks(NapackServerClient client, List<NapackVersionIdentifier> newNapacks)
        {
            return DownloadNapacks<NapackVersionIdentifier>(newNapacks,
                (napackIdentifier) => client.GetNapackVersionAsync(napackIdentifier),
                (napackIdentifier, unused) => napackIdentifier);
        }

        private List<Tuple<NapackVersionIdentifier, NapackVersion>> DownloadNapacks<T>(List<T> newNapacks, 
            Func<T, Task<NapackVersion>> downloadOperation, Func<T, NapackVersion, NapackVersionIdentifier> identifierOperation)
        {
            Stopwatch timer = Stopwatch.StartNew();
            List<Task<Tuple<NapackVersionIdentifier, NapackVersion>>> napackDownloadTasks =
                new List<Task<Tuple<NapackVersionIdentifier, NapackVersion>>>();
            foreach (T napackItem in newNapacks)
            {
                napackDownloadTasks.Add(Task.Run(async () =>
                {
                    NapackVersion version = await downloadOperation(napackItem).ConfigureAwait(false);
                    NapackVersionIdentifier identifier = identifierOperation(napackItem, version);
                    await this.SaveNapackAsync(identifier, version).ConfigureAwait(false);
                    return Tuple.Create<NapackVersionIdentifier, NapackVersion>(identifier, version);
                }));
            }

            Task.WhenAll(napackDownloadTasks).GetAwaiter().GetResult();

            Console.WriteLine("Downloaded " + newNapacks.Count + " in " + timer.ElapsedMilliseconds + " ms.");
            return napackDownloadTasks.Select(task => task.Result).ToList();
        }

        private async Task SaveNapackAsync(NapackVersionIdentifier versionId, NapackVersion version)
        {
            List<Task> fileWriteTasks = new List<Task>();
            foreach (KeyValuePair<string, NapackFile> file in version.Files)
            {
                fileWriteTasks.Add(this.SaveFileAsync(file.Key, file.Value.Contents));
            }

            fileWriteTasks.Add(GenerateTargetAsync(versionId, version.Files.ToDictionary(item => item.Key, item => item.Value.MsbuildType)));

            try
            {
                await Task.WhenAll(fileWriteTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error saving napack file: " + ex.Message);
                throw;
            }
        }

        private Task GenerateTargetAsync(NapackVersionIdentifier versionId, IDictionary<string, string> targetFiles)
        {
            string napackFilename = versionId.GenerateTargetName();

            StringBuilder targetFileBuilder = new StringBuilder();
            targetFileBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            targetFileBuilder.AppendLine("<Target Name=\"");
            targetFileBuilder.AppendLine(napackFilename + "\" BeforeTargets=\"Build\">");
            targetFileBuilder.AppendLine("  <ItemGroup>");

            foreach (KeyValuePair<string, string> file in targetFiles)
            {
                targetFileBuilder.AppendLine("    <" + file.Value + " Include=\"" + file.Key + "\" />");
            }

            targetFileBuilder.AppendLine("  </ItemGroup>");
            targetFileBuilder.AppendLine("</Target>");

            string targetsFilename = Path.Combine(this.NapackDirectory, napackFilename, ".targets");
            return SaveFileAsync(targetsFilename, targetFileBuilder.ToString());
        }

        private async Task SaveFileAsync(string file, string contents)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            using (FileStream stream = File.OpenWrite(file))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }
    }
}