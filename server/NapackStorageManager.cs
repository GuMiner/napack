using System;
using System.Collections.Generic;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files.
    /// </summary>
    public class NapackStorageManager : INapackStorageManager
    {
        public void AddUser(UserIdentifier user)
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

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
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

        public NapackStats GetPackageStatistics(string packageName)
        {
            throw new NotImplementedException();
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            throw new NotImplementedException();
        }

        public UserIdentifier GetUser(string userId)
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

        public void UpdateUser(UserIdentifier user)
        {
            throw new NotImplementedException();
        }
    }
}