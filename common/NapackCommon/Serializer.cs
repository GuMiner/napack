using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
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
    }
}
