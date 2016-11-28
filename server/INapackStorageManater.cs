namespace Napack.Server
{
    /// <summary>
    /// Gets the specified napack package from the database.
    /// </summary>
    public interface INapackStorageManager
    {
        NapackPackage GetPackage(string packageName);
    }
}