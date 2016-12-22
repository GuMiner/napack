using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Napack.Client.Common;
using Napack.Common;
using Ookii.CommandLine;

namespace Napack.Client
{
    /// <summary>
    /// Performs the upload of a new or updated napack.
    /// </summary>
    [Description("Uploads a Napacks, creating a new napack or an update to an existing one.")]
    internal class UploadOperation : INapackOperation
    {
        [CommandLineArgument(Position = 0, IsRequired = true)]
        [Description("The operation being performed.")]
        public string Operation { get; set; }

        // Todo version arguments, and arguments to determine if this is a create vs update.
        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The JSON file describing the package being uploaded")]
        public string PackageJsonFile { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettings { get; set; }

        [CommandLineArgument(Position = 3, IsRequired = false)]
        [Description("Updates the package metadata (description, more information, tags, and authorized users) instead of the package.")]
        public bool UpdateMetadata { get; set; }

        [CommandLineArgument(Position = 4, IsRequired = false)]
        [Description("Forces any upversioning of this package to at a minimum increment the major version.")]
        public bool ForceMajorUpversioning { get; set; }

        [CommandLineArgument(Position = 5, IsRequired = false)]
        [Description("Forces any upversioning of this package to at a minimum increment the minor version.")]
        public bool ForceMinorUpversioning { get; set; }

        public bool IsValidOperation() => !string.IsNullOrWhiteSpace(this.Operation) && this.Operation.Equals("Upload", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettings));

            string packageName = Path.GetFileNameWithoutExtension(this.PackageJsonFile);
            NapackLocalDescriptor napackDescriptor = Serializer.Deserialize<NapackLocalDescriptor>(File.ReadAllText(this.PackageJsonFile));
            napackDescriptor.Validate(settings.DefaultUserId);

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                bool packageExists = false;
                if (!packageExists && this.UpdateMetadata)
                {
                    throw new InvalidOperationException("Cannot create a new package and only perform metadata updates.");
                }
                else if (packageExists)
                {
                    Dictionary<string, NapackFile> files = UploadOperation.ParseNapackFiles(Path.GetDirectoryName(this.PackageJsonFile));
                    this.CreateNapackPackage(packageName, napackDescriptor, files, client);
                }
                else if (this.UpdateMetadata)
                {
                    // TODO determine how to perform a metadata update only.
                }
                else
                {
                    // This is a new version creation operation.
                    Dictionary<string, NapackFile> files = UploadOperation.ParseNapackFiles(Path.GetDirectoryName(this.PackageJsonFile));
                    VersionDescriptor version = client.UpdatePackageAsync(packageName, this.CreateNapackVersion(napackDescriptor, files)).GetAwaiter().GetResult();
                    Console.WriteLine($"Updated the {packageName} package to version {version.Major}.{version.Minor}.{version.Patch}");
                }
            }
        }

        private void CreateNapackPackage(string packageName, NapackLocalDescriptor napackDescriptor, Dictionary<string, NapackFile> files, NapackServerClient client)
        {
            NewNapack newNapack = new NewNapack()
            {
                Description = napackDescriptor.Description,
                MoreInformation = napackDescriptor.MoreInformation,
                AuthorizedUserIds = napackDescriptor.AuthorizedUserIds,
                Tags = napackDescriptor.Tags,
                NewNapackVersion = this.CreateNapackVersion(napackDescriptor, files)
            };

            string response = client.CreatePackageAsync(packageName, newNapack).GetAwaiter().GetResult();
            Console.WriteLine($"Package creation result: {response}");
        }

        private NewNapackVersion CreateNapackVersion(NapackLocalDescriptor napackDescriptor, Dictionary<string, NapackFile> files)
        {
            return new NewNapackVersion()
            {
                Files = files,
                Dependencies = napackDescriptor.Dependencies.Select(dependency => new NapackMajorVersion(dependency.Key, dependency.Value)).ToList(),
                Authors = napackDescriptor.Authors,
                ForceMajorUpversioning = this.ForceMajorUpversioning,
                ForceMinorUpversioning = this.ForceMinorUpversioning,
                License = new Napack.Common.License()
                {
                    LicenseText = napackDescriptor.License.LicenseText,
                    LicenseType = napackDescriptor.License.Type
                }
            };
        }

        private static Dictionary<string, NapackFile> ParseNapackFiles(string directory)
        {
            Dictionary<string, NapackFile> filesFound = new Dictionary<string, NapackFile>();
            foreach (string subdirectory in Directory.GetDirectories(directory))
            {
                foreach(KeyValuePair<string, NapackFile> file in UploadOperation.ParseNapackFiles(subdirectory))
                {
                    filesFound.Add(file.Key, file.Value);
                }
            }

            // Read all the files in a directory level asynchronously.
            Dictionary<string, Task<string>> fileReadTasks = new Dictionary<string, Task<string>>();
            foreach (string file in Directory.GetFiles(directory))
            {
                fileReadTasks.Add(file, Task.Run(() => File.ReadAllText(file)));
            }

            Task.WhenAll(fileReadTasks.Select(kvp => kvp.Value)).GetAwaiter().GetResult();

            foreach (KeyValuePair<string, string> pathAndContents in fileReadTasks.ToDictionary(task => task.Key, task => task.Value.Result))
            {
                // If the file starts with '// MSBuildType = TYPE', use TYPE instead. TODO this is kinda hacky and should be improved.
                string contentType = NapackFile.ContentType;
                if (pathAndContents.Value.StartsWith(NapackFile.BuildTypeHeader))
                {
                    int firstNewline = pathAndContents.Value.IndexOf("\n");
                    contentType = pathAndContents.Value.Substring(NapackFile.BuildTypeHeader.Length, firstNewline - NapackFile.BuildTypeHeader.Length);
                }

                filesFound.Add(pathAndContents.Key, new NapackFile()
                {
                    MsbuildType = contentType,
                    Contents = pathAndContents.Value
                });
            }

            return filesFound;
        }
    }
}