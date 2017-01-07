using System;

namespace Napack.Server
{
    public class SystemConfig
    {
        public SystemConfig()
        {
        }

        public string AdministratorEmail { get; set; }

        public string AdministratorName { get; set; }

        public string EmailHost { get; set; }

        public int EmailPort { get; set; }

        public string PackageValidationFilePath { get; set; }

        public string NameValidationFilePath { get; set; }

        public string GeolocationDatabaseFolder { get; set; }

        public bool RequireEmailValidation { get; set; }

        public int MaxRequestsPerIpPerInterval { get; set; }

        public TimeSpan RequestThrottlingInterval { get; set; }
    }
}