using System;
using System.ComponentModel;
using System.IO;
using Napack.Client.Common;
using Napack.Common;
using Ookii.CommandLine;

namespace Napack.Client
{
    /// <summary>
    /// Finishes user registration.
    /// </summary>
    [Description("Verifies a user's email with the Napack Framework Server to complete registration.")]
    internal class VerifyEmailOperation : INapackOperation
    {
        [CommandLineArgument(Position = 0, IsRequired = true)]
        [Description("Set to 'VerifyEmail' to call this operation.")]
        public string Operation { get; set; }

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The email of the user being verified.")]
        public string UserEmail { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The email verification code.")]
        public Guid VerificationCode { get; set; }

        [CommandLineArgument(Position = 3, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettingsFile { get; set; }

        public bool IsValidOperation() => !string.IsNullOrWhiteSpace(this.Operation) && this.Operation.Equals("VerifyEmail", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettingsFile));

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                string response = client.VerifyUserAsync(this.UserEmail, this.VerificationCode).GetAwaiter().GetResult();
                NapackClient.Log("Verification status:");
                NapackClient.Log(response);
            }
        }
    }
}