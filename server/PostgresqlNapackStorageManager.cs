using System;
using System.Collections.Generic;
using System.Data;
using System.Timers;
using Microsoft.Extensions.Logging;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;
using NLog;
using Npgsql;
using Npgsql.Logging;

namespace Napack.Server
{
    class PostgresqlNapackStorageManager : SqlNapackStorageManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private NpgsqlConnection database;

        public PostgresqlNapackStorageManager()
        {
            // Redirect logs to NLog.
            NpgsqlLogManager.LoggerFactory = new LoggerFactory().AddNLog(logger.Factory);

            bool createTablesAndIndices = false;
            
            database = new NpgsqlConnection($"Data Source=localhost;Port=5432;Database=napack;UserName={AdminModule.GetAdminUserName()};Password={AdminModule.GetAdminPassword()};SSL Mode=Prefer");
            database.Open();
            if (this.RequiresFirstTimeSetup())

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

        public bool RequiresFirstTimeSetup()
        {
            return ExecuteCommand(
                $"SELECT tablename FROM pg_catalog.pg_tables where tablename = '{SqlNapackStorageManager.PackageStoreTable}'",
                (command) =>
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }

                    return false;
                });
        }

        protected override string BeginTransactionString => "BEGIN";

        protected override IDbConnection DatabaseConnection => this.database;

        protected override string EndTransactionString => "COMMIT";

        protected override string RollbackTransactionString => "ROLLBACK";

        public override bool PerformsAutomatedBackups => false;

        public override void RunDbBackup(object sender, ElapsedEventArgs e)
        {
            // We use pg_dump instead to run the backup.
        }
    }
}
