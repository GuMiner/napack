﻿using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Owin.Hosting;
using Napack.Analyst;
using Napack.Common;
using Napack.Server.Utils;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Starts OWIN (which starts Nancy) from the command line.
    /// </summary>
    public class Global
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static System.Timers.Timer dbBackupTimer;

        public static string RootDirectory { get; private set; }

        public static SystemConfig SystemConfig { get; set; } = null;

        public static INapackStorageManager NapackStorageManager { get; set; } = null;

        public static EmailManager EmailManager { get; set; } = null;

        public static SystemStats SystemStats { get; private set; }

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
            Global.RootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            logger.Info("Serializer Setup...");
            Serializer.Setup();

            logger.Info("System Config loading...");
            Global.SystemConfig = Global.SystemConfig ?? Serializer.Deserialize<SystemConfig>(File.ReadAllText(ConfigurationManager.AppSettings["SystemConfigFilename"]));

            logger.Info("Email management loading...");
            Global.EmailManager = Global.EmailManager ?? new EmailManager(new SmtpEmailSender(Global.SystemConfig.EmailHost, Global.SystemConfig.EmailPort));

            logger.Info("Database loading...");
            bool useInMemoryVariant = false;
            Global.NapackStorageManager = Global.NapackStorageManager ?? 
                ((args.Length > 1 && bool.TryParse(args[1], out useInMemoryVariant) && useInMemoryVariant) ? 
                    (INapackStorageManager)new InMemoryNapackStorageManager() :
                    new PostgresqlNapackStorageManager());

            if (Global.NapackStorageManager.PerformsAutomatedBackups)
            {
                logger.Info("Database backup thread loading...");
                dbBackupTimer = new System.Timers.Timer() 
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = TimeSpan.FromDays(1).TotalMilliseconds, // Run every day.
                };

                dbBackupTimer.Elapsed += Global.NapackStorageManager.RunDbBackup;
                dbBackupTimer.Start();
            }

            logger.Info("System stats management loading...");
            Global.SystemStats = new SystemStats();

            // Turn off certificate validation, because it doesn't work with self-signed certificates.
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            
            if (args.Length < 1)
            {
                logger.Fatal("Expecting at a minimum the startup URL as the first argument!");
            }
            else
            {
                logger.Info("Napack Server Startup...");
                Global.ShutdownEvent = new ManualResetEvent(false);

                logger.Info("Analyst Setup...");
                NapackAnalyst.Initialize(Global.SystemConfig.PackageValidationFilePath, Global.SystemConfig.NameValidationFilePath);

                logger.Info("Starting web server...");
                using (WebApp.Start<Startup>(args[0]))
                {
                    Global.Initialized = true;
                    Global.ShutdownEvent.WaitOne();
                    logger.Info("Napack Server Shutting down...");

                    logger.Info("Databsae backup timer stopped.");
                    dbBackupTimer?.Stop();

                    logger.Info("Napack Server Shutdown.");
                }
            }
        }
    }
}
