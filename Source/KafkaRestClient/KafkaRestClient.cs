using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KafkaRestClient.Interfaces;
using KafkaRestClient.Models;

namespace KafkaRestClient
{
    public class KafkaRestClient : IKafkaRestClient
    {
        private readonly IRestClient _restClient;
        private readonly ISerializer _serializer;
        private readonly string _kafkaRestProxyUrl;
        private readonly string _defaultRequestContentType;

        public KafkaRestClient(
            IRestClient restClient,
            ISerializer serializer,
            string kafkaRestProxyUrl,
            string defaultRequestContentType)
        {
            _restClient = restClient;
            _serializer = serializer;
            _kafkaRestProxyUrl = kafkaRestProxyUrl;
            _defaultRequestContentType = defaultRequestContentType;
        }

        public Task<HttpResponseMessage> PostSingleRecordAsync<TRecord>(
            PostSingleRecordRequest<TRecord> request)
            where TRecord : IGetPartitionKey
        {
            if (request == null)
                throw new ArgumentNullException();

            if (request.Record == null)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(request.Topic))
                throw new ArgumentException();

            Uri destinationUri;
            var destinationUriString = $@"{_kafkaRestProxyUrl}/topics/{request.Topic}";
            if (!IsValidUri(destinationUriString, out destinationUri))
            {
                throw new ArgumentException();
            }

            var requestContent = CreateMessageWithSingleRecord(request.Record);
            var requestContentSerialized = _serializer.Serialize(requestContent);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, destinationUri)
            {
                Content = new StringContent(
                    requestContentSerialized,
                    Encoding.UTF8,
                    _defaultRequestContentType)
            };

            return _restClient.SendAsync(httpRequest);
        }

        public Task<HttpResponseMessage> PostMultipleRecordsAsync<TRecord>(
            PostMultipleRecordsRequest<TRecord> request)
            where TRecord : IGetPartitionKey
        {
            if (request == null)
                throw new ArgumentNullException();

            if (request.Records == null)
                throw new ArgumentNullException();

            if (request.Records.Any(record => record == null))
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(request.Topic))
                throw new ArgumentException();

            Uri destinationUri;
            var destinationUriString = $@"{_kafkaRestProxyUrl}/topics/{request.Topic}";
            if (!IsValidUri(destinationUriString, out destinationUri))
            {
                throw new ArgumentException($"Parameter {nameof(destinationUriString)} does not represent a valid URI.");
            }

            var requestContent = CreateMessageWithMultipleRecords(request.Records);
            var requestContentSerialized = _serializer.Serialize(requestContent);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, destinationUri)
            {
                Content = new StringContent(
                    requestContentSerialized,
                    Encoding.UTF8,
                    _defaultRequestContentType)
            };

            return _restClient.SendAsync(httpRequest);
        }

        private PostRecordsModel<TRecord> CreateMessageWithSingleRecord<TRecord>(TRecord record)
            where TRecord : IGetPartitionKey
        {
            return new PostRecordsModel<TRecord>(
                new RecordModel<TRecord>(record, record.GetPartitionKey()));
        }

        private PostRecordsModel<TRecord> CreateMessageWithMultipleRecords<TRecord>(IEnumerable<TRecord> records)
            where TRecord : IGetPartitionKey
        {
            return new PostRecordsModel<TRecord>(
                records.Select(record => new RecordModel<TRecord>(record, record.GetPartitionKey())).ToList());
        }

        private bool IsValidUri(string uri, out Uri destinationUri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out destinationUri);
        }
    }
}
