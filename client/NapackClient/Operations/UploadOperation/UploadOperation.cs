using System;
using System.ComponentModel;
using System.IO;
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
            NapackLocalDescriptor napackDescriptor = Serializer.Deserialize<NapackLocalDescriptor>(File.ReadAllText(this.PackageJsonFile));
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettings));

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                // TODO:
                // Get the package. If it doesn't exist, this is a create.
                // If the package exists and UpdateMetadata is false, this is a new version update.
                // If the package exists and UpdateMetadata is true, this is a metadata update.
                // If the package doesn't exist and UpdateMetadata is true, this is an invalid operation.
            }
        }
    }
}