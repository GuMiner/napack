using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Napack.Common
{
    /// <summary>
    /// Lists the necessary fields for adding a new napack version to an existing Napack.
    /// </summary>
    /// <remarks>
    /// The URI defines the napack being updated.
    /// The type of update (major/minor/patch) is automatically computed from the change in dependencies and public API surface on the files.
    /// If the files have no publically-facing changes and dependencies are not added => patch change.
    /// If the files have no publically-breaking changes (but do have publically-facing changes) or dependencies are added => minor change.
    /// If the files have publically-breaking API changes or dependencies are updated => major change.
    /// </remarks>
    public class NewNapackVersion
    {
        public bool ForceMajorUpversioning { get; set; }

        public bool ForceMinorUpversioning { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Authors { get; set; }

        /// <summary>
        /// The files associated with the package.
        /// </summary>
        /// <remarks>
        /// The keys are the path of the file within the napack, the values are the files themselves
        /// </remarks>
        [JsonProperty(Required = Required.Always)]
        public Dictionary<string, NapackFile> Files { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public License License { get; set; }

        /// <summary>
        /// The dependent napacks for this package.
        /// </summary>
        public List<NapackMajorVersion> Dependencies { get; set; }

        /// <summary>
        /// Validates that this new version has the minimal set of required fields.
        /// </summary>
        public void Validate()
        {
            if (this.Authors.Count == 0)
            {
                throw new InvalidNapackException("At least one author must be specified per version.");
            }

            if (this.Files.Count == 0 || !this.Files.Any(file => file.Value.MsbuildType == NapackFile.ContentType))
            {
                throw new InvalidNapackException("At least one file must be present of type Content.");
            }
        }
            
        public void UpdateNamespaceOfFiles(string napackName, int majorVersion)
        {
            string regex = $"namespace\\s+{napackName}";
            string replacement = $"namespace {napackName}.{majorVersion}";
            foreach (NapackFile file in this.Files.Values)
            {
                file.Contents = Regex.Replace(file.Contents, regex, replacement, 
                    RegexOptions.Singleline | RegexOptions.Compiled);
            }
        }
    }
}