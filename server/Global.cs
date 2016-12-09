using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Owin.Hosting;
using Napack.Analyst;

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

        /// <summary>
        /// Event we wait on to not shutdown the system.
        /// </summary>
        public static ManualResetEvent ShutdownEvent { get; private set; }

        public static void Main(string[] args)
        {
            // Turn off certificate validation, because it doesn't work with self-signed certificates.
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            
            // To get port 80, you'll need to route route port 9876 to port 80 through this: 
            //  "sudo /sbin/iptables -t nat -A PREROUTING -i eth+ -p tcp --dport 80 -j REDIRECT --to-port 9876"
            if (args.Length != 1)
            {
                Log("Expecting the startup URL as the singleton argument!");
            }
            else
            {
                Log("Napack Server Startup.");
                Global.ShutdownEvent = new ManualResetEvent(false);

                // Setup the analyzers from config.
                NapackAnalyst.Initialize();
                NapackNameValidator.Initialize();

                using (WebApp.Start<Startup>(args[0]))
                {
                    Global.ShutdownEvent.WaitOne();
                    Global.Log("Napack Server Shutdown.");
                }
            }
        }
    }
}
