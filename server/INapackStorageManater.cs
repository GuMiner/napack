namespace Napack.Server
{
    /// <summary>
    /// Gets the specified napack package from the database.
    /// </summary>
    public interface INapackStorageManater
    {
        NapackPackage GetPackage(string packageName);
    }
}