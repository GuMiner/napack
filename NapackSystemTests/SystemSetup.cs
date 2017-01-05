using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Client.Common;
using Napack.Common;
using Napack.Server;

namespace NapackSystemTests
{
    /// <summary>
    /// TODO I'll likely need to break apart functional tests, functional tests not needing authorization, and unit tests ... but that's for after I have sufficient testing to move onto more deployment.
    /// </summary>
    [TestClass]
    public class SystemSetup
    {
        public const string LocalServer = "http://localhost:9876";
        
        private static Task napackServerTask;

        public static RestClient RestClient;
        public static Napack.Common.UserSecret AuthorizedUser;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContent)
        {
            // Setup server
            napackServerTask = Task.Factory.StartNew(() =>
            {
                Global.SystemConfig = new SystemConfig()
                {
                    AdministratorEmail = "admin@localhost.fake",
                    AdministratorName = "test",
                    EmailHost = "unused",
                    EmailPort = 1234,
                    NameValidationFilePath = @"..\..\..\server\Resources\NameValidation.json",
                    PackageValidationFilePath = @"..\..\..\server\Resources\PackageValidation.json",
                    RequireEmailValidation = false
                };
                Global.Main(new[] { SystemSetup.LocalServer });
            }, TaskCreationOptions.LongRunning);

            // Setup the REST client for that server.
            SystemSetup.RestClient = new RestClient(new Uri(SystemSetup.LocalServer));

            while (!Global.Initialized)
            {
                Thread.Sleep(100);
            }

            // Create a user to perform authenticated requests.
            using (NapackServerClient client = new NapackServerClient(new Uri(SystemSetup.LocalServer)))
            {
                SystemSetup.AuthorizedUser = client.RegisterUserAsync("authorizeduser.id@fake.com").GetAwaiter().GetResult();
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            SystemSetup.RestClient?.Dispose();
            SystemSetup.RestClient = null;

            Global.ShutdownEvent.Set();
            while (!napackServerTask.IsCompleted)
            {
                Thread.Sleep(100);
            }
        }
    }
}
