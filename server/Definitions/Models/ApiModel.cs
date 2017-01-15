using System.Collections.Generic;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    public class ApiModel
    {
        public ApiModel()
        {
        }

        public ApiModel(string packageName, NapackSpec spec, List<NapackMajorVersion> dependencies)
        {
            this.NapackFullName = packageName;
            this.Spec = spec;
            this.Dependencies = dependencies;
        }

        public string NapackFullName { get; set; }

        public NapackSpec Spec { get; set; }

        public List<NapackMajorVersion> Dependencies { get; set; }
    }
}