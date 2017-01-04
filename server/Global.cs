using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Owin.Hosting;
using Napack.Analyst;
using Napack.Common;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Starts OWIN (which starts Nancy) from the command line.
    /// </summary>
    public class Global
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
                logger.Fatal("Expecting the startup URL as the singleton argument!");
            }
            else
            {
                logger.Info("Napack Server Startup...");
                Global.ShutdownEvent = new ManualResetEvent(false);

                logger.Info("Serializer Setup...");
                Serializer.Setup();

                logger.Info("Analyst Setup...");
                NapackAnalyst.Initialize();

                logger.Info("Starting web server...");
                using (WebApp.Start<Startup>(args[0]))
                {
                    Global.Initialized = true;
                    Global.ShutdownEvent.WaitOne();
                    logger.Info("Napack Server Shutdown.");
                }
            }
        }
    }
}
