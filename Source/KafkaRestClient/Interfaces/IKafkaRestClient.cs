using System.Net.Http;
using System.Threading.Tasks;
using KafkaRestClient.Models;

namespace KafkaRestClient.Interfaces
{
    public interface IKafkaRestClient
    {
        Task<HttpResponseMessage> PostSingleRecordAsync<TRecord>(PostSingleRecordRequest<TRecord> request)
            where TRecord : IGetPartitionKey;

        Task<HttpResponseMessage> PostMultipleRecordsAsync<TRecord>(PostMultipleRecordsRequest<TRecord> request)
            where TRecord : IGetPartitionKey;
    }
}
