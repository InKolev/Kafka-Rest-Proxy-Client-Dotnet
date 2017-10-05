using System.Collections.Generic;
using KafkaRestClient.Interfaces;

namespace KafkaRestClient.Models
{
    public class PostMultipleRecordsRequest<TRecord>
        where TRecord : IGetPartitionKey
    {
        public string Topic { get; private set; }

        public IEnumerable<TRecord> Records { get; private set; }

        public PostMultipleRecordsRequest<TRecord> WithRecords(IEnumerable<TRecord> records)
        {
            this.Records = records;
            return this;
        }

        public PostMultipleRecordsRequest<TRecord> WithDestination<TTopic>()
        {
            this.Topic = typeof(TTopic).Name;
            return this;
        }
    }
}
