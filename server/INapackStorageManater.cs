using System.Collections.Generic;

namespace Napack.Server
{
    /// <summary>
    /// Manages Napack storage.
    /// </summary>
    /// <remarks>
    /// This section is still in design. While retrieving the entire Napack package (with all of its versions) is simple, it will
    ///  grow unwieldly when a large package is updated continually. 
    /// 
    /// Quickly retrieving package dependencies -- in addition to package consumers -- will also need to be properly designed.
    /// All of these function signatures should be considered as very temporary.
    /// </remarks>
    public interface INapackStorageManager
    {
        NapackPackage GetPackage(string packageName);

        void UpdatePackage(NapackPackage package);

        List<NapackMajorVersion> GetPackageDependencies(string packageName);

        List<NapackMajorVersion> GetFlattenedPackageDependencies(string packageName);

    }
}