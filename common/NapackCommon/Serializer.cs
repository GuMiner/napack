using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Napack.Common
{
    /// <summary>
    /// Abstracts away Newtonsoft.JSON's interface.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Sets the global serialization settings.
        /// </summary>
        public static void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public static T Deserialize<T>(string @object)
        {
            return JsonConvert.DeserializeObject<T>(@object);
        }        

        public static string Serialize<T>(T @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        /// <summary>
        /// Reads an embedded resource from the specified assembly.
        /// </summary>
        public static string ReadFromAssembly(Assembly assembly, string resourceName)
        {
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
