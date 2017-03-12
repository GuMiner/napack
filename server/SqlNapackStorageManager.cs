using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
    /// Defines how to perform Napack storage management in a SQL environment.
    /// </summary>
    public abstract class SqlNapackStorageManager : INapackStorageManager, IDisposable
    {
        // If you're using more than this, you're searching for the wrong thing.
        private const int maxSearchTerms = 20;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected const string UsersTable = "users";
        protected const string AuthorPackageTable = "authorPackage";
        protected const string UserPackageTable = "userPackage";
        protected const string PackageConsumersTable = "packageConsumers";
        protected const string PackageStatsTable = "packageStats";
        protected const string PackageSpecsTable = "packageSpecs";
        protected const string PackageMetadataTable = "packageMetadata";
        protected const string PackageStoreTable = "packageStore";

        private const string PackageMetadataDescriptionAndTagsIndex = "packageMetadataDATIndex";

        public abstract bool PerformsAutomatedBackups { get; }

        protected abstract IDbConnection DatabaseConnection { get; }

        protected abstract string BeginTransactionString { get; }

        protected abstract string EndTransactionString { get; }

        protected abstract string RollbackTransactionString { get; }

        protected void ExecuteCommand(string commandText, Action<IDbCommand> commandAction)
        {
            ExecuteCommand(commandText, (command) =>
            {
                logger.Info(command.ToString());
                commandAction(command);
                return 0;
            });
        }

        protected T ExecuteCommand<T>(string commandText, Func<IDbCommand, T> commandAction)
        {
            return ExecuteCommand((command) =>
            {
                command.CommandText = commandText;
                return commandAction(command);
            });
        }

        protected T ExecuteCommand<T>(Func<IDbCommand, T> commandAction)
        {
            using (IDbCommand command = this.DatabaseConnection.CreateCommand())
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

        protected void ExecuteTransactionCommand(Action<IDbCommand> commandAction)
        {
            using (IDbCommand command = this.DatabaseConnection.CreateCommand())
            {
                try
                {
                    command.CommandText = BeginTransactionString;
                    command.ExecuteNonQuery();

                    commandAction(command);

                    command.CommandText = EndTransactionString;
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException se)
                {
                    try
                    {
                        command.CommandText = RollbackTransactionString;
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
        
        protected T GetItem<T>(IDbCommand currentCommand, string table, string keyName, string key, string valueName)
            where T: class
        {
            currentCommand.Parameters.Add(key);
            currentCommand.CommandText = $"SELECT {valueName} FROM {table} WHERE {keyName} = $1";
            T item = null;
            using (IDataReader reader = currentCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    item = Serializer.Deserialize<T>(reader.GetString(0));
                }
            }

            currentCommand.Parameters.Clear();
            return item;
        }

        protected T GetItem<T>(IDbCommand currentCommand, string table, string keyName, string key, string valueName, Func<T> missingAction)
            where T : class
        {
            currentCommand.Parameters.Add(key);
            currentCommand.CommandText = $"SELECT {valueName} FROM {table} WHERE {keyName} = $1";
            T item = null;
            using (IDataReader reader = currentCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    item = Serializer.Deserialize<T>(reader.GetString(0));
                }
                else
                {
                    item = missingAction();
                }
            }

            currentCommand.Parameters.Clear();
            return item;
        }

        protected void AddItem<T>(string key, T item, string table, string keyName, string valueName, bool distinct)
        {
            ExecuteTransactionCommand((command) =>
            {
                List<T> items = GetItem<List<T>>(command, table, keyName, key, valueName);
                if (items == null)
                {
                    string itemsEncoded = Serializer.Serialize(new List<T>() { item });
                    command.Parameters.Add(key);
                    command.Parameters.Add(itemsEncoded);
                    command.CommandText = $"INSERT INTO {table} VALUES ($1, $2)";
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                else
                {
                    items.Add(item);
                    items = distinct ? items.Distinct().ToList() : items;

                    string itemsEncoded = Serializer.Serialize(items);
                    command.Parameters.Add(itemsEncoded);
                    command.Parameters.Add(key);
                    command.CommandText = $"UPDATE {table} SET {valueName} = $1 WHERE {keyName} = $2";
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            });
        }
        
        protected void CreateTablesAndIndices()
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
            using (IDbCommand command = this.DatabaseConnection.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {tableCreationLogic}";
                command.ExecuteNonQuery();
                logger.Info($"Created table {tableName}.");
            }
        }

        private void CreateIndex(string indexName, string tableName, string columnName)
        {
            using (IDbCommand command = this.DatabaseConnection.CreateCommand())
            {
                command.CommandText = $"CREATE INDEX {indexName} ON {tableName} ({columnName} COLLATE NOCASE)";
                command.ExecuteNonQuery();
                logger.Info($"Created index {indexName} on {tableName}");
            }
        }

        public void AddUser(UserIdentifier user)
        {
            string userEmail = user.Email.ToUpperInvariant();
            string userJson = Serializer.Serialize(user);

            ExecuteCommand($"INSERT INTO {UsersTable} VALUES ($1, $2)", command =>
            {
                try
                {
                    command.Parameters.Add(userEmail);
                    command.Parameters.Add(userJson);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                catch (SQLiteException se) when (se.ErrorCode == (int)SQLiteErrorCode.Constraint)
                {
                    throw new ExistingUserException(user.Email);
                }
            });
        }

        public UserIdentifier GetUser(string userId)
        {
            string userEmail = Serializer.Serialize<string>(userId.ToUpperInvariant());
            return ExecuteCommand((command) => GetItem<UserIdentifier>(command, UsersTable, "email", userEmail, "userData",
                () => { throw new UserNotFoundException(userId); }));
        }

        public void UpdateUser(UserIdentifier user)
        {
            string userEmail = user.Email.ToUpperInvariant();
            string userJson = Serializer.Serialize(user);
            ExecuteCommand($"UPDATE {UsersTable} SET userData = $1 WHERE email = $2", command =>
            {
                command.Parameters.Add(userJson);
                command.Parameters.Add(userEmail);
                int rowsAffected = command.ExecuteNonQuery();
                logger.Info($"Update affected {rowsAffected} rows.");
                if (rowsAffected == 0)
                {
                    throw new UserNotFoundException(user.Email);
                }

                command.Parameters.Clear();
            });
        }

        public void RemoveUser(UserIdentifier user)
        {
            string userEmail = user.Email.ToUpperInvariant();
            ExecuteCommand($"DELETE FROM {UsersTable} WHERE email = $1", command =>
            {
                command.Parameters.Add(userEmail);
                int rowsAffected = command.ExecuteNonQuery();
                logger.Info($"Delete affected {rowsAffected} rows.");
                if (rowsAffected == 0)
                {
                    throw new UserNotFoundException(user.Email);
                }

                command.Parameters.Clear();
            });
        }

        public bool ContainsNapack(string packageName)
        {
            return ExecuteCommand($"SELECT packageName FROM {PackageMetadataTable} WHERE packageName = $1", command =>
            {
                bool hasNapack = false;
                command.Parameters.Add(packageName);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hasNapack = true;
                    }
                }

                command.Parameters.Clear();
                return hasNapack;
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
                        metadatas.Add(Serializer.Deserialize<NapackMetadata>(reader.GetString(0)));
                    }
                }

                logger.Info($"Search returned {metadatas.Count} items.");

                return metadatas.Select(metadata =>
                {
                    NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", metadata.Name, "packageStat",
                        () => { throw new NapackStatsNotFoundException(metadata.Name); });
                    return NapackSearchIndex.CreateFromMetadataAndStats(metadata, stats);
                }).ToList();
            });
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            return ExecuteCommand((command) => GetItem(command, AuthorPackageTable, "authorName", authorName.ToUpperInvariant(), "packageVersionList",
                () => Enumerable.Empty<NapackVersionIdentifier>()));
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            return ExecuteCommand((command) => GetItem(command, UserPackageTable, "userId", userId.ToUpperInvariant(), "packageNameList",
                () => Enumerable.Empty<string>()));
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            return ExecuteCommand((command) => GetItem(command, PackageConsumersTable, "packageMajorVersionId", packageMajorVersion.ToString(), "consumingPackages",
                () => Enumerable.Empty<NapackVersionIdentifier>()));
        }

        public NapackStats GetPackageStatistics(string packageName)
        {
            return ExecuteCommand((command) => GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageName, "packageStat",
                () => { throw new NapackStatsNotFoundException(packageName); }));
        }

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            return ExecuteCommand((command) => GetItem<NapackSpec>(command, PackageSpecsTable, "packageVersion", packageVersion.GetFullName(), "packageSpec",
                () => { throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch); }));
        }

        public void IncrementPackageDownload(string packageName)
        {
            ExecuteTransactionCommand((command) =>
            {
                NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", packageName, "packageStat",
                    () => { throw new NapackStatsNotFoundException(packageName); });
                stats.Downloads++;

                string packageStatsEncoded = Serializer.Serialize(stats);
                command.Parameters.Add(packageStatsEncoded);
                command.Parameters.Add(packageName);
                command.CommandText = $"UPDATE {PackageStatsTable} SET packageStat = $1 WHERE packageName = $2";
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });

        }

        public NapackMetadata GetPackageMetadata(string packageName, bool lockPackage)
        {
            // Before we do any updates, we need to verify someone else isn't concurrently updating the same Napack.
            // If someone else is, we will exit with a concurrency exception instead of performing all our calculations again.
            NapackMetadata package = null;
            ExecuteTransactionCommand((command) =>
            {
                package = this.GetItem<NapackMetadata>(command, PackageMetadataTable, "packageName", packageName, "metadata",
                    () => { throw new NapackNotFoundException(packageName); });

                if (lockPackage)
                {
                    if (package.ConcurrentLock + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
                    {
                        // Honestly, this is pretty hacky. Someone could *theoretically* make the code from here on down take > 10 minutes, causing a concurrency problem.
                        // Ideally, this entire workflow needs another redesign to fail faster -- or I need to implement an ETag-style concurrency mechanism with my data.
                        throw new ConcurrentOperationException();
                    }

                    // Take a transient lock
                    package.ConcurrentLock = DateTime.UtcNow;
                    string metadataEncoded = Serializer.Serialize(package);

                    command.CommandText = $"UPDATE {PackageMetadataTable} SET metadata = $1, descriptionAndTags = $2 WHERE packageName = $3";
                    command.Parameters.Add(metadataEncoded);
                    command.Parameters.Add(GetSafeDescriptionAndTags(package));
                    command.Parameters.Add(package.Name);
                    int rowsUpdated = command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    if (rowsUpdated == 0)
                    {
                        throw new NapackNotFoundException(package.Name);
                    }
                }
            });

            return package;
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            return ExecuteCommand((command) => GetItem<NapackVersion>(command, PackageStoreTable, "packageVersion", packageVersion.GetFullName(), "package",
                () => { throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch); }));
        }

        public void SaveNewNapack(string napackName, NewNapack newNapack, NapackSpec napackSpec)
        {
            NapackVersionIdentifier version = new NapackVersionIdentifier(napackName, 1, 0, 0);
            NapackMetadata metadata = NapackMetadata.CreateFromNewNapack(napackName, newNapack);
            NapackVersion packageVersion = NapackVersion.CreateFromNewNapack(newNapack.NewNapackVersion);

            foreach (string author in newNapack.NewNapackVersion.Authors)
            {
                AddItem(author.ToUpperInvariant(), version, AuthorPackageTable, "authorName", "packageVersionList", false);
            }

            foreach (string userId in newNapack.metadata.AuthorizedUserIds)
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
                string statsEncoded = Serializer.Serialize(stats);
                command.Parameters.Add(napackName);
                command.Parameters.Add(statsEncoded);
                command.CommandText = $"INSERT INTO {PackageStatsTable} VALUES ($1, $2)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                string metadataEncoded = Serializer.Serialize(metadata);
                string safeDescriptionAndTags = GetSafeDescriptionAndTags(metadata);
                command.Parameters.Add(napackName);
                command.Parameters.Add(safeDescriptionAndTags);
                command.Parameters.Add(metadataEncoded);
                command.CommandText = $"INSERT INTO {PackageMetadataTable} VALUES ($1, $2, $3)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                string napackSpecEncoded = Serializer.Serialize(napackSpec);
                command.Parameters.Add(version.GetFullName());
                command.Parameters.Add(napackSpecEncoded);
                command.CommandText = $"INSERT INTO {PackageSpecsTable} VALUES ($1, $2)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                string packageVersionEncoded = Serializer.Serialize(packageVersion);
                command.Parameters.Add(version.GetFullName());
                command.Parameters.Add(packageVersionEncoded);
                command.CommandText = $"INSERT INTO {PackageStoreTable} VALUES ($1, $2)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();
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

            ExecuteTransactionCommand((command) =>
            {
                string napackSpecEncoded = Serializer.Serialize(newVersionSpec);
                command.Parameters.Add(nextVersion.GetFullName());
                command.Parameters.Add(napackSpecEncoded);
                command.CommandText = $"INSERT INTO {PackageSpecsTable} VALUES ($1, $2)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                string packageVersionEncoded = Serializer.Serialize(packageVersion);
                command.Parameters.Add(nextVersion.GetFullName());
                command.Parameters.Add(packageVersionEncoded);
                command.CommandText = $"INSERT INTO {PackageStoreTable} VALUES ($1, $2)";
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });

            // Our lock is cleared at last here.
            UpdatePackageMetadataStore(package, nextVersion, upversionType, newNapackVersion);
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
                NapackStats stats = GetItem<NapackStats>(command, PackageStatsTable, "packageName", package.Name, "packageStat",
                    () => { throw new NapackStatsNotFoundException(package.Name); });
                stats.AddVersion(newNapackVersion);

                string statsEncoded = Serializer.Serialize(stats);
                command.Parameters.Add(statsEncoded);
                command.Parameters.Add(package.Name);
                command.CommandText = $"UPDATE {PackageStatsTable} SET packageStat = $1 WHERE packageName = $2";
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });

            UpdatePackageMetadata(package);
        }

        public void UpdatePackageMetadata(NapackMetadata package)
        {
            // Updating a package *always* clears the lock.
            package.ConcurrentLock = DateTime.MinValue;
            string metadataEncoded = Serializer.Serialize(package);
            ExecuteCommand($"UPDATE {PackageMetadataTable} SET metadata = $1, descriptionAndTags = $2 WHERE packageName = $3", command =>
            {
                command.Parameters.Add(metadataEncoded);
                command.Parameters.Add(GetSafeDescriptionAndTags(package));
                command.Parameters.Add(package.Name);
                int rowsUpdated = command.ExecuteNonQuery();
                command.Parameters.Clear();
                if (rowsUpdated == 0)
                {
                    throw new NapackNotFoundException(package.Name);
                }
            });
        }
        
        public void UpdatePackageVersion(NapackVersionIdentifier packageVersion, NapackVersion updatedVersion)
        {
            ExecuteCommand($"UPDATE {PackageStoreTable} SET package = $1 WHERE packageVersion = $2", (command) =>
            {
                command.Parameters.Add(Serializer.Serialize(updatedVersion));
                command.Parameters.Add(packageVersion.GetFullName());
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        }

        public void RemovePackageVersion(NapackVersionIdentifier packageVersion)
        {
            ExecuteCommand($"DELETE FROM {PackageStoreTable} WHERE packageVersion = $1", command =>
            {
                command.Parameters.Add(packageVersion.GetFullName());
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        }

        public void RemovePackageSpecification(NapackVersionIdentifier packageVersion)
        {
            ExecuteCommand($"DELETE FROM {PackageSpecsTable} WHERE packageVersion = $1", command =>
            {
                command.Parameters.Add(packageVersion.GetFullName());
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        }

        public void RemovePackageStatistics(string packageName)
        {
            ExecuteCommand($"DELETE FROM {PackageStatsTable} WHERE packageName = $1", command =>
            {
                command.Parameters.Add(packageName);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        }

        public void RemoveAuthoredPackages(string authorName, string packageName)
        {
            ExecuteTransactionCommand((command) =>
            {
                List<NapackVersionIdentifier> authoredPackages = this.GetAuthoredPackages(authorName)
                    .Where(item => !item.NapackName.Equals(packageName))
                    .ToList();

                command.Parameters.Add(Serializer.Serialize(authoredPackages));
                command.Parameters.Add(authorName.ToUpperInvariant());
                command.CommandText = $"UPDATE {AuthorPackageTable} SET packageVersionList = $1 WHERE authorName = $2";
                command.Parameters.Clear();
            });
        }

        public abstract void RunDbBackup(object sender, ElapsedEventArgs e);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DatabaseConnection?.Close();
                this.DatabaseConnection?.Dispose();
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
    }
}