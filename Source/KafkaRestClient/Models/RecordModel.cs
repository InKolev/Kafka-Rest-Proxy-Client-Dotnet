using Newtonsoft.Json;

namespace KafkaRestClient.Models
{
    internal class RecordModel<T>
    {
        public RecordModel(T value, string key)
        {
            Value = value;
            Key = key;
        }

        [JsonProperty("value")]
        public T Value { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
