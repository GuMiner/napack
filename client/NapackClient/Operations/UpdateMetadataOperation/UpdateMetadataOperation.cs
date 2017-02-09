using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Napack.Client.Common;
using Napack.Common;
using Ookii.CommandLine;

namespace Napack.Client
{
    [Description("Updates the metadata of an existing Napack.")]
    internal class UpdateMetadataOperation : INapackOperation
    {
        [CommandLineArgument(Position = 0, IsRequired = true)]
        [Description("Set to 'UpdateMetadata' to call this operation.")]
        public string Operation { get; set; }

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The JSON file describing the package being updated")]
        public string PackageFile { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettingsFile { get; set; }

        [CommandLineArgument(Position = 3, IsRequired = false)]
        [Description("The ID of the user that will create the Napack / is authorized to update the Napack. If not present, the default user will be used.")]
        public string UserId { get; set; }

        [CommandLineArgument(Position = 4, IsRequired = false)]
        [Description("The semicolon-deliminated keys authorizing the user to update this Napack. If not present, the default keys will be used.")]
        public string AuthorizationKeys { get; set; }

        public bool IsValidOperation() => !string.IsNullOrWhiteSpace(this.Operation) && this.Operation.Equals("UpdateMetadata", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            NapackClient.Log("Reading in the Napack settings file...");
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettingsFile));

            string userId = this.UserId;
            List<Guid> accessKeys = this.AuthorizationKeys?.Split(';').Select(key => Guid.Parse(key)).ToList();
            if (string.IsNullOrWhiteSpace(this.UserId) || accessKeys == null || accessKeys.Count == 0)
            {
                DefaultCredentials defaultCredentials = Serializer.Deserialize<DefaultCredentials>(File.ReadAllText(NapackClient.GetDefaultCredentialFilePath()));
                userId = defaultCredentials.UserId;
                accessKeys = defaultCredentials.Secrets;
            }

            UserSecret userSecret = new UserSecret()
            {
                UserId = userId,
                Secrets = accessKeys
            };

            string packageName = Path.GetFileNameWithoutExtension(this.PackageFile);
            NapackLocalDescriptor napackDescriptor = Serializer.Deserialize<NapackLocalDescriptor>(File.ReadAllText(this.PackageFile));
            napackDescriptor.Validate(userId, false);

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                NewNapackMetadata metadata = new NewNapackMetadata()
                {
                    AuthorizedUserIds = napackDescriptor.AuthorizedUserIds,
                    Description = napackDescriptor.Description,
                    MoreInformation = napackDescriptor.MoreInformation,
                    Tags = napackDescriptor.Tags
                };

                string response = client.UpdatePackageMetadataAsync(packageName, metadata, userSecret).GetAwaiter().GetResult();
                NapackClient.Log($"Package metadata update result: {response}");
            }
        }
    }
}