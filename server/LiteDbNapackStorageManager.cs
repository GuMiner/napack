using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files.
    /// </summary>
    public class LiteDbNapackStorageManager : INapackStorageManager, IDisposable
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private LiteDatabase database;
        private const string UsersCollection = "users";
        private const string AuthorPackageMapCollection = "authorPackageMap";
        private const string UserAuthorizedPackageCollection = "userAuthorizedPackageMap";
        private const string PackageStatsCollection = "packageStats";

        public LiteDbNapackStorageManager(string databaseFileName)
        {
            database = new LiteDatabase(databaseFileName);

            // Allow us to serialize URIs in documents.
            // Interestingly, this is used as an example in the docs here: https://github.com/mbdavid/LiteDB/wiki/Object-Mapping
            BsonMapper.Global.RegisterType<Uri>((uri) => uri.AbsoluteUri, (bson) => new Uri(bson.AsString));
            BsonMapper.Global.RegisterType<DateTime>((time) => time.ToString("o"), (bson) => DateTime.Parse(bson.AsString));
            BsonMapper.Global.RegisterType<TimeSpan>((span) => span.ToString(), (bson) => TimeSpan.Parse(bson.AsString));
        }

        public void AddUser(UserIdentifier user)
        {
            try
            {
                user.Id = UserIdentifier.GetSafeId(user.Email);
                LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(LiteDbNapackStorageManager.UsersCollection);
                users.Insert(user);
            }
            catch (LiteException le)
            {
                logger.Warn(le);
                if (le.ErrorCode == LiteException.INDEX_DUPLICATE_KEY)
                {
                    throw new ExistingUserException(user.Email);
                }

                throw ;
            }
        }

        public UserIdentifier GetUser(string userId)
        {
            LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(LiteDbNapackStorageManager.UsersCollection);
            UserIdentifier user = users.FindById(UserIdentifier.GetSafeId(userId));
            if (user == null)
            {
                logger.Warn($"User not found {userId}.");
                throw new UserNotFoundException(userId);
            }

            return user;
        }

        public void UpdateUser(UserIdentifier user)
        {
            user.Id = UserIdentifier.GetSafeId(user.Email);
            LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(LiteDbNapackStorageManager.UsersCollection);
            if (!users.Update(user))
            {
                throw new UserNotFoundException(user.Email);
            }
        }

        public void RemoveUser(UserIdentifier user)
        {
            LiteCollection<UserIdentifier> users = database.GetCollection<UserIdentifier>(LiteDbNapackStorageManager.UsersCollection);
            if (!users.Delete(UserIdentifier.GetSafeId(user.Email)))
            {
                throw new UserNotFoundException(user.Email);
            }
        }

        public class AuthorPackageMap
        {
            public AuthorPackageMap()
            {
            }

            [BsonId]
            public string AuthorNameBase64 { get; set; }

            public List<NapackVersionIdentifier> AuthoredPackage { get; set; }
        }

        public class UserAuthorizedPackageMap
        {
            public UserAuthorizedPackageMap()
            {
            }

            [BsonId]
            public string UserNameBase64 { get; set; }

            public List<string> AuthorizedPackages { get; set; }
        }

        public class PackageConsumerMap
        {
            public PackageConsumerMap()
            {
            }

            [BsonId]
            public string PackageMajorVersion { get; set; }

            public List<NapackVersionIdentifier> PackageConsumers { get; set; }
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            LiteCollection<AuthorPackageMap> authorPackageMap = database.GetCollection<AuthorPackageMap>(LiteDbNapackStorageManager.AuthorPackageMapCollection);
            AuthorPackageMap map = authorPackageMap.FindById(Convert.ToBase64String(Encoding.UTF8.GetBytes(authorName.ToUpperInvariant())));
            return map?.AuthoredPackage ?? new List<NapackVersionIdentifier>();
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            LiteCollection<UserAuthorizedPackageMap> userAuthorizedPackageMap = database.GetCollection<UserAuthorizedPackageMap>(LiteDbNapackStorageManager.UserAuthorizedPackageCollection);
            UserAuthorizedPackageMap map = userAuthorizedPackageMap.FindById(Convert.ToBase64String(Encoding.UTF8.GetBytes(userId)));
            return map?.AuthorizedPackages ?? new List<string>();
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            LiteCollection<PackageConsumerMap> packageConsumerMap = database.GetCollection<PackageConsumerMap>(LiteDbNapackStorageManager.UserAuthorizedPackageCollection);
            PackageConsumerMap map = packageConsumerMap.FindById(packageMajorVersion.ToString());
            return map?.PackageConsumers ?? new List<NapackVersionIdentifier>();
        }

        public NapackStats GetPackageStatistics(string packageName)
        {
            LiteCollection<NapackStats> packageStatsCollection = database.GetCollection<NapackStats>(LiteDbNapackStorageManager.PackageStatsCollection);
            NapackStats stats = packageStatsCollection.FindById(packageName);
            if (stats == null)
            {
                logger.Warn($"Package stats not found {packageName}.");
                throw new NapackStatsNotFoundException(packageName);
            }

            return stats;
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

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
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