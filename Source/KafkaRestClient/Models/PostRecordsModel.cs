using System.Collections.Generic;
using Newtonsoft.Json;

namespace KafkaRestClient.Models
{
    internal class PostRecordsModel<T>
    {
        public PostRecordsModel(RecordModel<T> recordModel)
        {
            Records = new List<RecordModel<T>> { recordModel };
        }

        public PostRecordsModel(IEnumerable<RecordModel<T>> records)
        {
            Records = records;
        }

        [JsonProperty("records")]
        public IEnumerable<RecordModel<T>> Records { get; set; }
    }
}
