using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Timers;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files.
    /// </summary>
    /// <remarks>
    /// There really needs to be another abstraction layer here so that the storage layer doesn't hold
    ///  business logic around updating multiple items, etc.
    /// </remarks>
    public class SqliteNapackStorageManager : SqlNapackStorageManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SQLiteConnection database;
        private readonly string databaseFolder;
        
        public SqliteNapackStorageManager(string databaseFileName)
        {
            bool createTablesAndIndices = false;

            logger.Info($"SQLite version: {SQLiteConnection.SQLiteVersion}");
            if (!File.Exists(databaseFileName))
            {
                SQLiteConnection.CreateFile(databaseFileName);
                createTablesAndIndices = true;
            }

            databaseFolder = Path.GetDirectoryName(databaseFileName);
            database = new SQLiteConnection($"Data Source={databaseFileName}");
            database.Open();

            if (createTablesAndIndices)
            {
                logger.Info($"DB doesn't exist, creating tables and indices.");
                CreateTablesAndIndices();
            }
            else
            {
                logger.Info($"DB already exists and has been loaded.");
            }
        }

        public override bool PerformsAutomatedBackups => true;

        protected override IDbConnection DatabaseConnection => this.database;

        protected override string BeginTransactionString => "BEGIN IMMEDIATE TRANSACTION";

        protected override string EndTransactionString => "END TRANSACTION";

        protected override string RollbackTransactionString => "ROLLBACK TRANSACTION";
        
        /// <summary>
        /// Saves the SQLite DB to a local file.
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">Unused</param>
        public override void RunDbBackup(object sender, ElapsedEventArgs e)
        {
            string backupFileName = Path.Combine(databaseFolder, DateTime.UtcNow.ToString("yyy-MM-dd-hh-mm") + ".backup");

            logger.Info("Backing up the DB to " + backupFileName);
            Stopwatch backupTimer = Stopwatch.StartNew();
            try
            {
                SQLiteConnection.CreateFile(backupFileName);
                using (SQLiteConnection backupConnection = new SQLiteConnection($"Data Source={backupFileName}"))
                {
                    backupConnection.Open();
                    database.BackupDatabase(backupConnection, "main", "main", -1, null, 1000);
                }

                logger.Info("Backup completed successfully in " + backupTimer.ElapsedMilliseconds + " ms.");
            }
            catch (Exception ex)
            {
                logger.Error("Error saving the DB backup: " + ex.Message + ". " + ex.StackTrace);
            }

            backupTimer.Stop();
        }
    }
}