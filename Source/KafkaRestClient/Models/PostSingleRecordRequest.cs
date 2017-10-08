using KafkaRestClient.Interfaces;

namespace KafkaRestClient.Models
{
    public class PostSingleRecordRequest<TRecord>
        where TRecord : IGetPartitionKey
    {
        public string Topic { get; private set; }

        public TRecord Record { get; private set; }

        public PostSingleRecordRequest<TRecord> WithRecord(TRecord record)
        {
            this.Record = record;
            return this;
        }

        public PostSingleRecordRequest<TRecord> WithDestination<TTopic>()
        {
            this.Topic = typeof(TTopic).Name;
            return this;
        }

        public PostSingleRecordRequest<TRecord> WithDestination(string topic)
        {
            this.Topic = topic;
            return this;
        }
    }
}
