using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Owin.Hosting;
using Napack.Analyst;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Starts OWIN (which starts Nancy) from the command line.
    /// </summary>
    public class Global
    {
        /// <summary>
        /// TODO: Convert to Nlog
        /// </summary>
        public static Action<string> Log => (input) => Console.WriteLine(input);

        public static string AdministratorEmail { get; private set; }

        public static string AdministratorName { get; private set; }

        /// <summary>
        /// Returns true once initialization has completed, false otherwise.
        /// </summary>
        public static bool Initialized { get; private set; } = false;

        /// <summary>
        /// Event we wait on to not shutdown the system.
        /// </summary>
        public static ManualResetEvent ShutdownEvent { get; private set; }

        public static void Main(string[] args)
        {
            // Read in common values from our App.config file.
            Global.AdministratorEmail = ConfigurationManager.AppSettings["AdministratorEmail"];
            Global.AdministratorName = ConfigurationManager.AppSettings["AdministratorName"];
            EmailManager.Initialize(ConfigurationManager.AppSettings["NFSEmailHost"], int.Parse(ConfigurationManager.AppSettings["NFSEmailPort"]));

            // Turn off certificate validation, because it doesn't work with self-signed certificates.
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            
            if (args.Length != 1)
            {
                Log("Expecting the startup URL as the singleton argument!");
            }
            else
            {
                Log("Napack Server Startup...");
                Global.ShutdownEvent = new ManualResetEvent(false);

                Log("Serializer Setup...");
                Serializer.Setup();

                Log("Analyst Setup...");
                NapackAnalyst.Initialize();

                Log("Starting web server...");
                using (WebApp.Start<Startup>(args[0]))
                {
                    Global.Initialized = true;
                    Global.ShutdownEvent.WaitOne();
                    Global.Log("Napack Server Shutdown.");
                }
            }
        }
    }
}
