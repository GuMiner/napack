using System.IO;
using Nancy;
using Napack.Common;

namespace Napack.Server.Utils
{
    /// <summary>
    /// Adds serialization extensions to ensure Newtonsoft.Json performs the deserialization,
    ///  with request checking, of Nancy objects.
    /// </summary>
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(NancyContext context)
        {
            return Serializer.Deserialize<T>(new StreamReader(context.Request.Body).ReadToEnd());
        }
    }
}
