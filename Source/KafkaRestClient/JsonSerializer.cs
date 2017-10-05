using System.Collections.Generic;
using KafkaRestClient.Interfaces;
using Newtonsoft.Json;

namespace KafkaRestClient
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonSerializer()
            : this(new List<JsonConverter>())
        {
        }

        public JsonSerializer(IEnumerable<JsonConverter> converters)
        {
            _serializerSettings = new JsonSerializerSettings();
            foreach (var converter in converters)
            {
                _serializerSettings.Converters.Add(converter);
            }

            _serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
        }

        public T Deserialize<T>(string obj) => JsonConvert.DeserializeObject<T>(obj, _serializerSettings);

        public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, _serializerSettings);
    }
}
