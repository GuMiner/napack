using System;
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

        public static T DeserializeFromBase64<T>(string @object)
        {
            return Deserialize<T>(Encoding.UTF8.GetString(Convert.FromBase64String(@object)));
        }

        public static string SerializeToBase64<T>(T @object)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@object)));
        }
    }
}
