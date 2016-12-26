using Napack.Analyst.ApiSpec;

namespace Napack.Server
{
    public class ApiModel
    {
        public ApiModel()
        {
        }

        public ApiModel(string packageName, NapackSpec spec)
        {
            this.NapackFullName = packageName;
            this.Spec = spec;
        }

        public string NapackFullName { get; set; }

        public NapackSpec Spec { get; set; }
    }
}