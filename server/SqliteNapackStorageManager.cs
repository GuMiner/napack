using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
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

        private IDbConnection database;
        
        private const string UsersTable = "users";
        private const string AuthorPackageTable = "authorPackage";
        private const string UserPackageTable = "userPackage";
        private const string PackageConsumersTable = "packageConsumers";
        private const string PackageStatsTable = "packageStats";
        private const string PackageSpecsTable = "packageSpecs";
        private const string PackageMetadataTable = "packageMetadata";
        private const string PackageStoreTable = "packageStore";

        private const string PackageMetadataDescriptionIndex = "packageMetadataDescriptionIndex";
        private const string PackageMetadataTagsIndex = "packageMetadataTagsIndex";

        public SqliteNapackStorageManager(string databaseFileName)
        {
            bool createTablesAndIndices = false;

            logger.Info($"SQLite version: {SqliteConnection.SQLiteVersion}");
            if (!File.Exists(databaseFileName))
            {
                SqliteConnection.SetConfig(SQLiteConfig.MultiThread);
                SqliteConnection.CreateFile(databaseFileName);
                createTablesAndIndices = true;
            }

            database = new SqliteConnection($"Data Source={databaseFileName}");
            database.Open();

            if (createTablesAndIndices)
            {
                CreateTablesAndIndices();
            }
        }
        
        private void CreateTablesAndIndices()
        {
            string usersTable = $"{UsersTable} (" +
                "email TEXT PRIMARY KEY NOT NULL, " +
                "userData TEXT NOT NULL)";
            CreateTable(usersTable);

            string authorPackageTable = $"{AuthorPackageTable} (" +
                "authorName TEXT PRIMARY KEY NOT NULL, " +
                "packageVersionList TEXT NOT NULL)";
            CreateTable(authorPackageTable);

            string userPackageTable = $"{UserPackageTable} (" +
                "userId TEXT PRIMARY KEY NOT NULL, " +
                "packageNameList TEXT NOT NULL)";
            CreateTable(userPackageTable);

            string packageConsumersTable = $"{PackageConsumersTable} (" +
                "packageMajorVersionId TEXT PRIMARY KEY NOT NULL, " +
                "consumingPackages TEXT NOT NULL)";
            CreateTable(packageConsumersTable);

            string statsTable = $"{PackageStatsTable} (" +
                "packageName TEXT PRIMARY KEY NOT NULL, " +
                "packageStat TEXT NOT NULL)";
            CreateTable(statsTable);

            string specsTable = $"{PackageSpecsTable} (" +
                "packageVersion TEXT PRIMARY KEY NOT NULL, " +
                "packageSpec TEXT NOT NULL)";
            CreateTable(specsTable);

            string packageMetadataTable = $"{PackageMetadataTable} (" +
                "packageName TEXT PRIMARY KEY NOT NULL, " +
                "description TEXT COLLATE NOCASE NOT NULL, " +
                "tags TEXT COLLATE NOCASE NOT NULL, " +
                "metadata TEXT NOT NULL)";
            CreateTable(packageMetadataTable);
            CreateIndex(PackageMetadataDescriptionIndex, PackageMetadataTable, "description");
            CreateIndex(PackageMetadataDescriptionIndex, PackageMetadataTagsIndex, "tags");

            string packageStoreTable = $"{PackageStoreTable} (" +
                "packageVersion TEXT PRIMARY KEY NOT NULL, " +
                "package TEXT NOT NULL)";
            CreateTable(packageStoreTable);
        }

        private void CreateTable(string tableCreationLogic)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {tableCreationLogic}";
                command.ExecuteNonQuery();
            }
        }

        private void CreateIndex(string indexName, string tableName, string columnName)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                command.CommandText = $"CREATE INDEX {indexName} ON {tableName} ({columnName} COLLATE NOCASE)";
                command.ExecuteNonQuery();
            }
        }

        private void ExecuteCommand(string commandText, Action<IDbCommand> commandAction)
        {
            ExecuteCommand(commandText, (command) =>
            {
                commandAction(command);
                return 0;
            });
        }

        private T ExecuteCommand<T>(string commandText, Func<IDbCommand, T> commandAction)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                try
                {
                    command.CommandText = commandText;
                    return commandAction(command);
                }
                catch (SqliteException se)
                {
                    logger.Warn(se);
                    throw;
                }
            }
        }
        
        public void AddUser(UserIdentifier user)
        {
            string userEmail = Serializer.SerializeToBase64<string>(user.Email.ToUpperInvariant());
            string userJson = Serializer.SerializeToBase64(user);
            ExecuteCommand($"INSERT INTO {UsersTable} ('{userEmail}', '{userJson}')", command =>
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException se) when (se.ErrorCode == SQLiteErrorCode.Constraint)
                {
                    throw new ExistingUserException(user.Email);
                }
            });
        }

        public UserIdentifier GetUser(string userId)
        {
            string userEmail = Serializer.SerializeToBase64<string>(userId.ToUpperInvariant());
            return ExecuteCommand($"SELECT userData FROM {UsersTable} WHERE email = '{userEmail}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<UserIdentifier>(reader.GetString(0));
                    }

                    throw new UserNotFoundException(userId);
                }
            });
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
            
            return ExecuteCommand($"SELECT packageVersionList FROM {AuthorPackageTable} WHERE authorName = '{authorNameEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<List<NapackVersionIdentifier>>(reader.GetString(0));
                    }

                    return Enumerable.Empty<NapackVersionIdentifier>();
                }
            });
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            string userIdEncoded = Serializer.SerializeToBase64<string>(userId.ToUpperInvariant());

            return ExecuteCommand($"SELECT packageNameList FROM {UserPackageTable} WHERE userId = '{userIdEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<List<string>>(reader.GetString(0));
                    }

                    return Enumerable.Empty<string>();
                }
            });
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            string pmvEncoded = Serializer.SerializeToBase64<string>(packageMajorVersion.ToString());

            return ExecuteCommand($"SELECT consumingPackages FROM {PackageConsumersTable} WHERE packageMajorVersionId = '{pmvEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<List<NapackVersionIdentifier>>(reader.GetString(0));
                    }

                    return Enumerable.Empty<NapackVersionIdentifier>();
                }
            });
        }

        public NapackStats GetPackageStatistics(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);

            return ExecuteCommand($"SELECT packageStat FROM {PackageStatsTable} WHERE packageName = '{packageNameEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<NapackStats>(reader.GetString(0));
                    }

                    throw new NapackStatsNotFoundException(packageName);
                }
            });
        }

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            string packageVersionEncoded = Serializer.SerializeToBase64<string>(packageVersion.GetFullName());

            return ExecuteCommand($"SELECT packageSpec FROM {PackageSpecsTable} WHERE packageVersion = '{packageVersionEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<NapackSpec>(reader.GetString(0));
                    }

                    throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch);
                }
            });
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
            IEnumerable<string> searchKeys = searchPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(maxSearchTerms);
            // TODO SELECT ITEMS FROM TABLE WHERE COLUMN = 'key' AND ... LIMIT SKIP OFFSET TOP

            // For each metadata item found, retrieve the NapackStats.

            throw new NotImplementedException();
        }

        public void IncrementPackageDownload(string packageName)
        {
            NapackStats stats = GetPackageStatistics(packageName);
            stats.Downloads++;

            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);
            string packageStatsEncoded = Serializer.SerializeToBase64(stats);

            ExecuteCommand($"UPDATE {PackageStatsTable} SET packageStat = '{packageStatsEncoded}' WHERE packageName = '{packageNameEncoded}'", command =>
            {
                int rowsAffected = command.ExecuteNonQuery();
                logger.Info($"Update affected {rowsAffected} rows.");
                if (rowsAffected == 0)
                {
                    throw new NapackStatsNotFoundException(packageName);
                }
            });

        }

        public NapackMetadata GetPackageMetadata(string packageName)
        {
            string packageNameEncoded = Serializer.SerializeToBase64<string>(packageName);

            return ExecuteCommand($"SELECT metadata FROM {PackageMetadataTable} WHERE packageName = '{packageNameEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<NapackMetadata>(reader.GetString(0));
                    }

                    throw new NapackNotFoundException(packageName);
                }
            });
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            string packageVersionEncoded = Serializer.SerializeToBase64<string>(packageVersion.GetFullName());

            return ExecuteCommand($"SELECT package FROM {PackageStoreTable} WHERE packageVersion = '{packageVersionEncoded}'", command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return Serializer.DeserializeFromBase64<NapackVersion>(reader.GetString(0));
                    }

                    throw new NapackVersionNotFoundException(packageVersion.Major, packageVersion.Minor, packageVersion.Patch);
                }
            });
        }
        
        public void SaveNewNapack(string napackName, NewNapack newNapack, NapackSpec napackSpec)
        {
            throw new NotImplementedException();
        }

        public void SaveNewNapackVersion(NapackMetadata package, NapackVersionIdentifier currentVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion, NapackSpec newVersionSpec)
        {
            throw new NotImplementedException();
        }

        public void UpdatePackageMetadata(NapackMetadata metadata)
        {
            throw new NotImplementedException();
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
    }
}