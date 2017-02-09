using System;
using System.ComponentModel;
using System.IO;
using Napack.Client.Common;
using Napack.Common;
using Ookii.CommandLine;

namespace Napack.Client
{
    /// <summary>
    /// Performs user registration.
    /// </summary>
    [Description("Registers a new user with the Napack Framework Server")]
    internal class RegisterOperation : INapackOperation
    {
        [CommandLineArgument(Position = 0, IsRequired = true)]
        [Description("Set to 'Register' to call this operation.")]
        public string Operation { get; set; }

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The email of the user to be registered.")]
        public string UserEmail { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettingsFile { get; set; }

        [CommandLineArgument(Position = 3, IsRequired = false)]
        [Description("Saves the registered user and credentials as defaults in the current user's default folder.")]
        public bool SaveAsDefault { get; set; }

        public bool IsValidOperation() => !string.IsNullOrWhiteSpace(this.Operation) && this.Operation.Equals("Register", StringComparison.InvariantCultureIgnoreCase);

        public void PerformOperation()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(this.NapackSettingsFile));

            using (NapackServerClient client = new NapackServerClient(settings.NapackFrameworkServer))
            {
                UserSecret secret = client.RegisterUserAsync(this.UserEmail).GetAwaiter().GetResult();
                NapackClient.Log($"User {secret.UserId} successfully registered. Secrets (order&case sensitive):");
                foreach (Guid individualSecret in secret.Secrets)
                {
                    NapackClient.Log($"  {individualSecret}");
                }

                if (this.SaveAsDefault)
                {
                    File.WriteAllText(NapackClient.GetDefaultCredentialFilePath(), Serializer.Serialize(new DefaultCredentials(secret.UserId, secret.Secrets)));
                }
            }
        }
    }
}