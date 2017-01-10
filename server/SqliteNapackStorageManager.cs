using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
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

        private IDbConnection database;
        
        private const string UsersCollection = "users";
        private const string AuthorPackageMapCollection = "authorPackageMap";
        private const string UserAuthorizedPackageCollection = "userAuthorizedPackageMap";
        private const string PackageStatsCollection = "packageStats";

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
            string usersTable = "users (" +
                "email TEXT PRIMARY KEY COLLATE NOCASE NOT NULL, " +
                "userData TEXT NOT NULL)";
            CreateTable(usersTable);

            
        }

        private void CreateTable(string tableCreationLogic)
        {
            using (IDbCommand command = database.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {tableCreationLogic}";
                command.ExecuteNonQuery();
            }
        }

        public void AddUser(UserIdentifier user)
        {
            //using (IDbCommand command = database.CreateCommand())
            //{
            //    command.ExecuteNonQuery();
            //}
                // try
                // {
                //     user.Id = UserIdentifier.GetSafeId(user.Email);
                //     LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(RavenDbNapackStorageManager.UsersCollection);
                //     users.Insert(user);
                // }
                // catch (LiteException le)
                // {
                //     logger.Warn(le);
                //     if (le.ErrorCode == LiteException.INDEX_DUPLICATE_KEY)
                //     {
                //         throw new ExistingUserException(user.Email);
                //     }
                // 
                //     throw ;
                // }
                throw new NotImplementedException();
        }

        public UserIdentifier GetUser(string userId)
        {
            // LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(RavenDbNapackStorageManager.UsersCollection);
            // UserIdentifier user = users.FindById(UserIdentifier.GetSafeId(userId));
            // if (user == null)
            // {
            //     logger.Warn($"User not found {userId}.");
            //     throw new UserNotFoundException(userId);
            // }
            // 
            // return user;

            throw new NotImplementedException();
        }

        public void UpdateUser(UserIdentifier user)
        {
            // user.Id = UserIdentifier.GetSafeId(user.Email);
            // LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(RavenDbNapackStorageManager.UsersCollection);
            // if (!users.Update(user))
            // {
            //     throw new UserNotFoundException(user.Email);
            // }

            throw new NotImplementedException();
        }

        public void RemoveUser(UserIdentifier user)
        {
            // LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(RavenDbNapackStorageManager.UsersCollection);
            // if (!users.Delete(UserIdentifier.GetSafeId(user.Email)))
            // {
            //     throw new UserNotFoundException(user.Email);
            // }

            throw new NotImplementedException();
        }

        public class AuthorPackageMap
        {
            public AuthorPackageMap()
            {
            }
            
            public string AuthorNameBase64 { get; set; }

            public List<NapackVersionIdentifier> AuthoredPackage { get; set; }
        }

        public class UserAuthorizedPackageMap
        {
            public UserAuthorizedPackageMap()
            {
            }
            
            public string UserNameBase64 { get; set; }

            public List<string> AuthorizedPackages { get; set; }
        }

        public class PackageConsumerMap
        {
            public PackageConsumerMap()
            {
            }
            
            public string PackageMajorVersion { get; set; }

            public List<NapackVersionIdentifier> PackageConsumers { get; set; }
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            //LiteCollection<AuthorPackageMap> authorPackageMap = database.GetCollection<AuthorPackageMap>(RavenDbNapackStorageManager.AuthorPackageMapCollection);
            //AuthorPackageMap map = authorPackageMap.FindById(Convert.ToBase64String(Encoding.UTF8.GetBytes(authorName.ToUpperInvariant())));
            //return map?.AuthoredPackage ?? new List<NapackVersionIdentifier>();
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            throw new NotImplementedException();
            // LiteCollection<UserAuthorizedPackageMap> userAuthorizedPackageMap = database.GetCollection<UserAuthorizedPackageMap>(RavenDbNapackStorageManager.UserAuthorizedPackageCollection);
            // UserAuthorizedPackageMap map = userAuthorizedPackageMap.FindById(Convert.ToBase64String(Encoding.UTF8.GetBytes(userId)));
            // return map?.AuthorizedPackages ?? new List<string>();
            throw new NotImplementedException();
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            throw new NotImplementedException();
            // LiteCollection<PackageConsumerMap> packageConsumerMap = database.GetCollection<PackageConsumerMap>(RavenDbNapackStorageManager.UserAuthorizedPackageCollection);
            // PackageConsumerMap map = packageConsumerMap.FindById(packageMajorVersion.ToString());
            // return map?.PackageConsumers ?? new List<NapackVersionIdentifier>();
        }

        public NapackStats GetPackageStatistics(string packageName)
        {
            throw new NotImplementedException();
            // LiteCollection<NapackStats> packageStatsCollection = database.GetCollection<NapackStats>(RavenDbNapackStorageManager.PackageStatsCollection);
            // NapackStats stats = packageStatsCollection.FindById(packageName);
            // if (stats == null)
            // {
            //     logger.Warn($"Package stats not found {packageName}.");
            //     throw new NapackStatsNotFoundException(packageName);
            // }
            // 
            // return stats;
        }

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            throw new NotImplementedException();
        }

        public bool ContainsNapack(string packageName)
        {
            throw new NotImplementedException();
        }

        public List<NapackSearchIndex> FindPackages(string searchPhrase, int skip, int top)
        {
            throw new NotImplementedException();
        }        

        public NapackMetadata GetPackageMetadata(string packageName)
        {
            throw new NotImplementedException();
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            throw new NotImplementedException();
        }
        
        public void IncrementPackageDownload(string packageName)
        {
            throw new NotImplementedException();
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