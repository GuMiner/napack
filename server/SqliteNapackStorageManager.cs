using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;
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
    public class SqliteNapackStorageManager : INapackStorageManager, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // If you're using more than this, you're searching for the wrong thing.
        private const int maxSearchTerms = 20;

        private SQLiteConnection database;
        private readonly string databaseFolder;
        
        private const string UsersTable = "users";
        private const string AuthorPackageTable = "authorPackage";
        private const string UserPackageTable = "userPackage";
        private const string PackageConsumersTable = "packageConsumers";
        private const string PackageStatsTable = "packageStats";
        private const string PackageSpecsTable = "packageSpecs";
        private const string PackageMetadataTable = "packageMetadata";
        private const string PackageStoreTable = "packageStore";

        private const string PackageMetadataDescriptionAndTagsIndex = "packageMetadataDATIndex";

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
        
        private void CreateTablesAndIndices()
        {
            string usersTable = $"{UsersTable} (" +
                "email TEXT PRIMARY KEY NOT NULL, " +
                "userData TEXT NOT NULL)";
            CreateTable(UsersTable, usersTable);

            string authorPackageTable = $"{AuthorPackageTable} (" +
                "authorName TEXT PRIMARY KEY NOT NULL, " +
                "packageVersionList TEXT NOT NULL)";
            CreateTable(AuthorPackageTable, authorPackageTable);

            string userPackageTable = $"{UserPackageTable} (" +
                "userId TEXT PRIMARY KEY NOT NULL, " +
                "packageNameList TEXT NOT NULL)";
            CreateTable(UserPackageTable, userPackageTable);

            string packageConsumersTable = $"{PackageConsumersTable} (" +
                "packageMajorVersionId TEXT PRIMARY KEY NOT NULL, " +
                "consumingPackages TEXT NOT NULL)";
            CreateTable(PackageConsumersTable, packageConsumersTable);

            string statsTable = $"{PackageStatsTable} (" +
                "packageName TEXT PRIMARY KEY NOT NULL, " +
                "packageStat TEXT NOT NULL)";
            CreateTable(PackageStatsTable, statsTable);

            string specsTable = $"{PackageSpecsTable} (" +
                "packageVersion TEXT PRIMARY KEY NOT NULL, " +
                "packageSpec TEXT NOT NULL)";
            CreateTable(PackageSpecsTable, specsTable);

            string packageMetadataTable = $"{PackageMetadataTable} (" +
                "packageName TEXT PRIMARY KEY NOT NULL, " +
                "descriptionAndTags TEXT COLLATE NOCASE NOT NULL, " +
                "metadata TEXT NOT NULL)";
            CreateTable(PackageMetadataTable, packageMetadataTable);
            CreateIndex(PackageMetadataDescriptionAndTagsIndex, PackageMetadataTable, "descriptionAndTags");

            string packageStoreTable = $"{PackageStoreTable} (" +
                "packageVersion TEXT PRIMARY KEY NOT NULL, " +
                "package TEXT NOT NULL)";
            CreateTable(PackageStoreTable, packageStoreTable);
        }

        private void CreateTable(string tableName, string tableCreationLogic)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {tableCreationLogic}";
                command.ExecuteNonQuery();
                logger.Info($"Created table {tableName}.");
            }
        }

        private void CreateIndex(string indexName, string tableName, string columnName)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                command.CommandText = $"CREATE INDEX {indexName} ON {tableName} ({columnName} COLLATE NOCASE)";
                command.ExecuteNonQuery();
                logger.Info($"Created index {indexName} on {tableName}");
            }
        }

        private void ExecuteCommand(string commandText, Action<IDbCommand> commandAction)
        {
            ExecuteCommand(commandText, (command) =>
            {
                logger.Info(command.ToString());
                commandAction(command);
                return 0;
            });
        }

        private T ExecuteCommand<T>(string commandText, Func<IDbCommand, T> commandAction)
        {
            return ExecuteCommand((command) =>
            {
                command.CommandText = commandText;
                return commandAction(command);
            });
        }

        private T ExecuteCommand<T>(Func<IDbCommand, T> commandAction)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                try
                {
                    return commandAction(command);
                }
                catch (SQLiteException se)
                {
                    logger.Warn(se.Message);
                    throw;
                }
            }
        }

        private void ExecuteTransactionCommand(Action<IDbCommand> commandAction)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                try
                {
                    command.CommandText = "BEGIN IMMEDIATE TRANSACTION";
                    command.ExecuteNonQuery();

                    commandAction(command);

                    command.CommandText = "END TRANSACTION";
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException se)
                {
                    try
                    {
                        command.CommandText = "ROLLBACK TRANSACTION";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Transaction rollback failure!");
                        logger.Warn(ex.Message);
                    }

                    logger.Warn(se.Message);
                    throw;
                }
            }
        }

        public T GetItem<T>(IDbCommand currentCommand, string table, string keyName, string keyEncoded, string valueName)
            where T: class
        {
            currentCommand.CommandText = $"SELECT {valueName} FROM {table} WHERE {keyName} = '{keyEncoded}'";
            using (IDataReader reader = currentCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    return Serializer.DeserializeFromBase64<T>(reader.GetString(0));
                }

                return null;
            }
        }

        public T GetItem<T>(IDbCommand currentCommand, string table, string keyName, string keyEncoded, string valueName, Func<T> missingAction)
            where T : class
        {
            currentCommand.CommandText = $"SELECT {valueName} FROM {table} WHERE {keyName} = '{keyEncoded}'";
            using (IDataReader reader = currentCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    return Serializer.DeserializeFromBase64<T>(reader.GetString(0));
                }

                return missingAction();
            }
        }

        public void AddItem<T>(string key, T item, string table, string keyName, string valueName, bool distinct)
        {
            string keyEncoded = Serializer.SerializeToBase64<string>(key);
            ExecuteTransactionCommand((command) =>
            {
                List<T> items = GetItem<List<T>>(command, table, keyName, keyEncoded, valueName);
                if (items == null)
                {
                    string itemsEncoded = Serializer.SerializeToBase64(new List<T>() { item });
                    command.Parameters.Add(keyEncoded);
                    command.Parameters.Add(itemsEncoded);
                    command.CommandText = $"INSERT INTO {table} VALUES (?, ?)";
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                else
                {
                    items.Add(item);
                    items = distinct ? items.Distinct().ToList() : items;
                    string itemsEncoded = Serializer.SerializeToBase64(items);

                    command.CommandText = $"UPDATE {table} SET {valueName} = '{itemsEncoded}' WHERE {keyName} = '{keyEncoded}'";
                    command.ExecuteNonQuery();
                }
            });
        }

        public void AddUser(UserIdentifier user)
        {
            string userEmail = Serializer.SerializeToBase64<string>(user.Email.ToUpperInvariant());
            string userJson = Serializer.SerializeToBase64(user);
            
            ExecuteCommand($"INSERT INTO {UsersTable} VALUES ('{userEmail}', '{userJson}')", command =>
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException se) when (se.ErrorCode == (int)SQLiteErrorCode.Constraint)
                {
                    throw new ExistingUserException(user.Email);
                }
            });
        }

        public UserIdentifier GetUser(string userId)
        {
            string userEmail = Serializer.SerializeToBase64<string>(userId.ToUpperInvariant());
            return ExecuteCommand((command) => GetItem<UserIdentifier>(command, UsersTable, "email", userEmail, "userData",
                () => { throw new UserNotFoundException(userId); }));
        }

        public void UpdateUser(UserIdentifier user)
        {
            string userEmail = Serializer.SerializeToBase64<string>(user.Email.ToUpperInvariant());
            string userJson = Serializer.SerializeToBase64(user);
            ExecuteCommand($"UPDATE {UsersTable} SET userData = '{userJson}' WHERE email = '{userEmail}'", command =>
            {
                int rowsAffected = command.ExecuteNonQuery();
                logger.Info($"Update affected {rowsAffected} rows.");
                if (rowsAffected == 0)
                {
                    throw new UserNotFoundException(user.Email);
                }
            });
        }

        public void RemoveUser(UserIdentifier user)
        {
            string userEmail = Serializer.SerializeToBase64<string>(user.Email.ToUpperInvariant());
            ExecuteCommand($"DELETE FROM {UsersTable} WHERE email = '{userEmail}'", command =>
            {
                int rowsAffected = command.ExecuteNonQuery();
                logger.Info($"Delete affected {rowsAffected} rows.");
                if (rowsAffected == 0)
                {
                    throw new UserNotFoundException(user.Email);
                }
            });
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            string authorNameEncoded = Serializer.SerializeToBase64<string>(authorName.ToUpperInvariant());
            return ExecuteCommand((command) => GetItem(command, AuthorPackageTable, "authorName", authorNameEncoded, "packageVersionList",
                () => Enumerable.Empty<NapackVersionIdentifier>()));
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            string userIdEncoded = Serializer.SerializeToBase64<string>(userId.ToUpperInvariant());
            return ExecuteCommand((command) => GetItem(command, UserPackageTable, "userId", userIdEncoded, "packageNameList",
                () => Enumerable.Empty<string>()));
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            string pmvEncoded = Serializer.SerializeToBase64<string>(packageMajorVersion.ToString());
            return ExecuteCommand((command) => GetItem(command, PackageConsumersTable, "packageMajorVersionId", pmvEncoded, "consumingPackages",
                () => Enumerable.Empty<NapackVersionIdentifier>()));
        }

        public NapackStats GetPackageStatistics(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);
            return ExecuteCommand((command) => GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageNameEncoded, "packageStat",
                () => { throw new NapackStatsNotFoundException(packageName); }));
        }

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            string packageVersionEncoded = Serializer.SerializeToBase64<string>(packageVersion.GetFullName());
            return ExecuteCommand((command) => GetItem<NapackSpec>(command, PackageSpecsTable, "packageVersion", packageVersionEncoded, "packageSpec",
                () => { throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch); }));
        }

        public bool ContainsNapack(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);

            return ExecuteCommand($"SELECT packageName FROM {PackageMetadataTable} WHERE packageName = '{packageNameEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            });
        }

        public List<NapackSearchIndex> FindPackages(string searchPhrase, int skip, int top)
        {
            // Sanitize and return if the user didn't search for anything!
            List<string> searchKeys = searchPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(maxSearchTerms).Select(item => item.Replace("'", "''")).ToList();
            if (searchKeys.Count == 0)
            {
                return new List<NapackSearchIndex>();
            }

            // Build the SQL query
            StringBuilder searchString = new StringBuilder();
            searchString.Append($"SELECT metadata FROM {PackageMetadataTable} WHERE ");
            for (int i = 0; i < searchKeys.Count; i++)
            {
                searchString.Append($"descriptionAndTags LIKE '%{searchKeys[i]}%'");
                if (i != searchKeys.Count - 1)
                {
                    searchString.Append(" AND ");
                }
            }

            searchString.Append($" LIMIT {top} OFFSET {skip}");
            logger.Info($"Searching for {searchString.ToString()}");

            return ExecuteCommand((command) =>
            {
                List<NapackMetadata> metadatas = new List<NapackMetadata>();
                command.CommandText = searchString.ToString();
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        metadatas.Add(Serializer.DeserializeFromBase64<NapackMetadata>(reader.GetString(0)));
                    }
                }

                logger.Info($"Search returned {metadatas.Count} items.");

                return metadatas.Select(metadata =>
                {
                    string packageNameEncoded = Serializer.SerializeToBase64<string>(metadata.Name);
                    NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageNameEncoded, "packageStat",
                        () => { throw new NapackStatsNotFoundException(metadata.Name); });
                    return NapackSearchIndex.CreateFromMetadataAndStats(metadata, stats);
                }).ToList();
            });
        }

        public void IncrementPackageDownload(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);

            ExecuteTransactionCommand((command) =>
            {
                NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageNameEncoded, "packageStat",
                    () => { throw new NapackStatsNotFoundException(packageName); });
                stats.Downloads++;

                string packageStatsEncoded = Serializer.SerializeToBase64(stats);
                command.CommandText = $"UPDATE {PackageStatsTable} SET packageStat = '{packageStatsEncoded}' WHERE packageName = '{packageNameEncoded}'";
                command.ExecuteNonQuery();
            });

        }

        public NapackMetadata GetPackageMetadata(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);
            return ExecuteCommand((command) => GetItem<NapackMetadata>(command, PackageMetadataTable, "packageName", packageNameEncoded, "metadata",
                () => { throw new NapackNotFoundException(packageName); }));
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            string packageVersionEncoded = Serializer.SerializeToBase64<string>(packageVersion.GetFullName());
            return ExecuteCommand((command) => GetItem<NapackVersion>(command, PackageStoreTable, "packageVersion", packageVersionEncoded, "package",
                () => { throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch); }));
        }

        public void SaveNewNapack(string napackName, NewNapack newNapack, NapackSpec napackSpec)
        {
            string napackNameEncoded = Serializer.SerializeToBase64<string>(napackName);

            NapackVersionIdentifier version = new NapackVersionIdentifier(napackName, 1, 0, 0);
            NapackMetadata metadata = NapackMetadata.CreateFromNewNapack(napackName, newNapack);
            NapackVersion packageVersion = NapackVersion.CreateFromNewNapack(newNapack.NewNapackVersion);

            foreach (string author in newNapack.NewNapackVersion.Authors)
            {
                AddItem(author.ToUpperInvariant(), version, AuthorPackageTable, "authorName", "packageVersionList", false);
            }

            foreach (string userId in newNapack.AuthorizedUserIds)
            {
                AddItem(userId.ToUpperInvariant(), napackName, UserPackageTable, "userId", "packageNameList", true);
            }

            foreach (NapackMajorVersion consumedPackage in newNapack.NewNapackVersion.Dependencies)
            {
                AddItem(consumedPackage.ToString(), version, PackageConsumersTable, "packageMajorVersionId", "consumingPackages", false);
            }

            // Add the new napack to all the various stores.
            NapackStats stats = new NapackStats();
            stats.AddVersion(newNapack.NewNapackVersion);

            ExecuteTransactionCommand((command) =>
            {
                string statsEncoded = Serializer.SerializeToBase64(stats);
                command.CommandText = $"INSERT INTO {PackageStatsTable} VALUES ('{napackNameEncoded}', '{statsEncoded}')";
                command.ExecuteNonQuery();

                string metadataEncoded = Serializer.SerializeToBase64(metadata);
                string safeDescriptionAndTags = GetSafeDescriptionAndTags(metadata);
                command.CommandText = $"INSERT INTO {PackageMetadataTable} VALUES ('{napackNameEncoded}', '{safeDescriptionAndTags}', '{metadataEncoded}')";
                command.ExecuteNonQuery();

                string versionEncoded = Serializer.SerializeToBase64(version.GetFullName());
                string napackSpecEncoded = Serializer.SerializeToBase64(napackSpec);
                command.CommandText = $"INSERT INTO {PackageSpecsTable} VALUES ('{versionEncoded}', '{napackSpecEncoded}')";
                command.ExecuteNonQuery();

                string packageVersionEncoded = Serializer.SerializeToBase64(packageVersion);
                command.CommandText = $"INSERT INTO {PackageStoreTable} VALUES ('{versionEncoded}', '{packageVersionEncoded}')";
                command.ExecuteNonQuery();
            });
        }

        public void SaveNewNapackVersion(NapackMetadata package, NapackVersionIdentifier currentVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion, NapackSpec newVersionSpec)
        {
            NapackVersionIdentifier nextVersion = new NapackVersionIdentifier(currentVersion.NapackName, currentVersion.Major, currentVersion.Minor, currentVersion.Patch);
            NapackVersion packageVersion = NapackVersion.CreateFromNewNapack(newNapackVersion);
            
            foreach (string author in newNapackVersion.Authors)
            {
                AddItem(author.ToUpperInvariant(), nextVersion, AuthorPackageTable, "authorName", "packageVersionList", false);
            }

            // 
            // // Changes in user authorization do not occur through napack version updates.
            // 

            foreach (NapackMajorVersion consumedPackage in newNapackVersion.Dependencies)
            {
                AddItem(consumedPackage.ToString(), nextVersion, PackageConsumersTable, "packageMajorVersionId", "consumingPackages", false);
            }

            UpdatePackageMetadataStore(package, nextVersion, upversionType, newNapackVersion);
            
            ExecuteTransactionCommand((command) =>
            {
                string versionEncoded = Serializer.SerializeToBase64(nextVersion.GetFullName());
                string napackSpecEncoded = Serializer.SerializeToBase64(newVersionSpec);
                command.CommandText = $"INSERT INTO {PackageSpecsTable} VALUES ('{versionEncoded}', '{napackSpecEncoded}')";
                command.ExecuteNonQuery();

                string packageVersionEncoded = Serializer.SerializeToBase64(packageVersion);
                command.CommandText = $"INSERT INTO {PackageStoreTable} VALUES ('{versionEncoded}', '{packageVersionEncoded}')";
                command.ExecuteNonQuery();
            });
        }

        private void UpdatePackageMetadataStore(NapackMetadata package, NapackVersionIdentifier nextVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion)
        {
            if (upversionType == NapackAnalyst.UpversionType.Major)
            {
                NapackMajorVersionMetadata newMajorVersionMetadata = new NapackMajorVersionMetadata()
                {
                    Recalled = false,
                    Versions = new Dictionary<int, List<int>>
                    {
                        [0] = new List<int> { 0 }
                    },
                    License = newNapackVersion.License
                };

                package.Versions.Add(nextVersion.Major, newMajorVersionMetadata);
            }
            else if (upversionType == NapackAnalyst.UpversionType.Minor)
            {
                package.Versions[nextVersion.Major].Versions.Add(nextVersion.Minor, new List<int> { 0 });
            }
            else
            {
                package.Versions[nextVersion.Major].Versions[nextVersion.Minor].Add(nextVersion.Patch);
            }

            ExecuteTransactionCommand((command) =>
            {
                string packageNameEncoded = Serializer.SerializeToBase64<string>(package.Name);
                NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageNameEncoded, "packageStat",
                    () => { throw new NapackStatsNotFoundException(package.Name); });
                stats.AddVersion(newNapackVersion);

                string statsEncoded = Serializer.SerializeToBase64(stats);
                command.CommandText = $"UPDATE {PackageStatsTable} SET packageStat = '{statsEncoded}' WHERE packageName = '{packageNameEncoded}'";
                command.ExecuteNonQuery();
            });
            
            // TODO unfortunately we really should be getting the metadata and updating it here to avoid multi-user bugs.
            // I'm putting that down as a future redesign as we'll only hit this with unlucky concurrent updates to the same package.
            UpdatePackageMetadata(package);
        }

        public void UpdatePackageMetadata(NapackMetadata metadata)
        {
            string packageNameEncoded = Serializer.SerializeToBase64(metadata.Name);
            string metadataEncoded = Serializer.SerializeToBase64(metadata);

            ExecuteCommand($"UPDATE {PackageMetadataTable} SET metadata = '{metadataEncoded}', descriptionAndTags = '{GetSafeDescriptionAndTags(metadata)}' WHERE packageName = '{packageNameEncoded}'", command =>
            {
                int rowsUpdated = command.ExecuteNonQuery();
                if (rowsUpdated == 0)
                {
                    throw new NapackNotFoundException(metadata.Name);
                }
            });
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                database?.Close();
                database?.Dispose();
                database = null;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string GetSafeDescriptionAndTags(NapackMetadata metadata)
        {
            return (metadata.Description + " " + string.Join(" ", metadata.Tags)).Replace("'", "''");
        }

        /// <summary>
        /// Saves the SQLite DB to a local file.
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">Unused</param>
        public void RunDbBackup(object sender, ElapsedEventArgs e)
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