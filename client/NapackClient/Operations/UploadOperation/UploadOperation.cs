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

        [CommandLineArgument(Position = 6, IsRequired = false)]
        [Description("The ID of the user that will create the Napack / is authorized to update the Napack. If not present, the default user will be used.")]
        public string UserId { get; set; }

        [CommandLineArgument(Position = 7, IsRequired = false)]
        [Description("The semicolon-deliminated keys authorizing the user to update this Napack. If not present, the default keys will be used.")]
        public string AuthorizationKeys { get; set; }

        public bool IsValidOperation() => !string.IsNullOrWhiteSpace(this.Operation) && this.Operation.Equals("Upload", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettings));
            string userId = string.IsNullOrWhiteSpace(this.UserId) ? settings.DefaultUserId : this.UserId;
            List<Guid> accessKeys = this.AuthorizationKeys?.Split(';').Select(key => Guid.Parse(key)).ToList() ?? settings.DefaultUserAuthenticationKeys;
            UserSecret userSecret = new UserSecret()
            {
                UserId = userId,
                Secrets = accessKeys
            };

            string packageName = Path.GetFileNameWithoutExtension(this.PackageJsonFile);
            NapackLocalDescriptor napackDescriptor = Serializer.Deserialize<NapackLocalDescriptor>(File.ReadAllText(this.PackageJsonFile));
            napackDescriptor.Validate(settings.DefaultUserId);

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                CreateOrUpdateNapackAsync(packageName, napackDescriptor, userSecret, client).GetAwaiter().GetResult();
            }
        }

        private async Task CreateOrUpdateNapackAsync(string packageName, NapackLocalDescriptor napackDescriptor, UserSecret userSecret, NapackServerClient client)
        {
            bool packageExists = await client.ContainsNapack(packageName).ConfigureAwait(false);
            if (!packageExists && this.UpdateMetadata)
            {
                throw new InvalidOperationException("Cannot create a new package and only perform metadata updates.");
            }
            else if (!packageExists)
            {
                Dictionary<string, NapackFile> files = UploadOperation.ParseNapackFiles(Path.GetDirectoryName(this.PackageJsonFile));
                await this.CreateNapackPackageAsync(packageName, napackDescriptor, files, userSecret, client).ConfigureAwait(false);
            }
            else if (this.UpdateMetadata)
            {
                // TODO determine how to perform a metadata update only.
            }
            else
            {
                // This is a new version creation operation.
                Dictionary<string, NapackFile> files = UploadOperation.ParseNapackFiles(Path.GetDirectoryName(this.PackageJsonFile));
                VersionDescriptor version = await client.UpdatePackageAsync(packageName, this.CreateNapackVersion(napackDescriptor, files), userSecret).ConfigureAwait(false);
                Console.WriteLine($"Updated the {packageName} package to version {version.Major}.{version.Minor}.{version.Patch}");
            }
        }

        private async Task CreateNapackPackageAsync(string packageName, NapackLocalDescriptor napackDescriptor, Dictionary<string, NapackFile> files, UserSecret secret, NapackServerClient client)
        {
            NewNapack newNapack = new NewNapack()
            {
                Description = napackDescriptor.Description,
                MoreInformation = napackDescriptor.MoreInformation,
                AuthorizedUserIds = napackDescriptor.AuthorizedUserIds,
                Tags = napackDescriptor.Tags,
                NewNapackVersion = this.CreateNapackVersion(napackDescriptor, files)
            };

            string response = await client.CreatePackageAsync(packageName, newNapack, secret).ConfigureAwait(false);
            Console.WriteLine($"Package creation result: {response}");
        }

        private NewNapackVersion CreateNapackVersion(NapackLocalDescriptor napackDescriptor, Dictionary<string, NapackFile> files)
        {
            return new NewNapackVersion()
            {
                Files = files,
                Dependencies = napackDescriptor.Dependencies?.Select(dependency => new NapackMajorVersion(dependency.Key, dependency.Value)).ToList() ?? new List<NapackMajorVersion>(),
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