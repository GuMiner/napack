using System;
using System.Collections.Generic;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files.
    /// </summary>
    public class NapackStorageManager : INapackStorageManager
    {
        public IDictionary<string, float> FindPackages(string searchPhrase, int skip, int top)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAuthorizedPackages(string userHash)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NapackMajorVersion> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
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
    }
}