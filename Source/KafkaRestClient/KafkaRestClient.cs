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
        private readonly string _requestContentType;

        public KafkaRestClient(
            IRestClient restClient,
            ISerializer serializer,
            string kafkaRestProxyUrl,
            string requestContentType)
        {
            _restClient = restClient;
            _serializer = serializer;
            _kafkaRestProxyUrl = kafkaRestProxyUrl;
            _requestContentType = requestContentType;
        }

        public Task<HttpResponseMessage> PostSingleRecordAsync<TRecord>(
            PostSingleRecordRequest<TRecord> request)
            where TRecord : IGetPartitionKey
        {
            if (request == null)
                throw new ArgumentNullException(
                    $"Argument \"{nameof(request)}\" must not be null.");

            if (request.Record == null)
                throw new ArgumentNullException(
                    $"Argument \"{nameof(request)}.{nameof(request.Record)}\" must not be null.");

            if (string.IsNullOrWhiteSpace(request.Topic))
                throw new ArgumentException(
                    $"Argument \"{nameof(request)}.{nameof(request.Topic)}\" must not be null, empty or whitespace.");

            Uri destinationUri;
            var destinationUriString = $@"{_kafkaRestProxyUrl}/topics/{request.Topic}";
            if (!IsValidUri(destinationUriString, out destinationUri))
            {
                throw new ArgumentException(
                    $"Argument \"{nameof(destinationUriString)}\" with value \"{destinationUriString}\" does not represent a valid URI.");
            }

            var requestContent = CreateMessageWithSingleRecord(request.Record);
            var requestContentSerialized = _serializer.Serialize(requestContent);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, destinationUri)
            {
                Content = new StringContent(
                    requestContentSerialized,
                    Encoding.UTF8,
                    _requestContentType)
            };

            return _restClient.SendAsync(httpRequest);
        }

        public Task<HttpResponseMessage> PostMultipleRecordsAsync<TRecord>(
            PostMultipleRecordsRequest<TRecord> request)
            where TRecord : IGetPartitionKey
        {
            if (request == null)
                throw new ArgumentNullException(
                    $"Argument \"{nameof(request)}\" must not be null.");

            if (request.Records == null)
                throw new ArgumentNullException(
                    $"Argument \"{nameof(request)}.{nameof(request.Records)}\" must not be null.");

            if (request.Records.Any(record => record == null))
                throw new ArgumentNullException(
                    $"Argument \"{nameof(request)}.{nameof(request.Records)}\" must not contain null records.");

            if (string.IsNullOrWhiteSpace(request.Topic))
                throw new ArgumentException(
                    $"Argument \"{nameof(request)}.{nameof(request.Topic)}\" must not be null, empty or whitespace.");

            Uri destinationUri;
            var destinationUriString = $@"{_kafkaRestProxyUrl}/topics/{request.Topic}";
            if (!IsValidUri(destinationUriString, out destinationUri))
            {
                throw new ArgumentException(
                    $"Argument \"{nameof(destinationUriString)}\" with value \"{destinationUriString}\" does not represent a valid URI.");
            }

            var requestContent = CreateMessageWithMultipleRecords(request.Records);
            var requestContentSerialized = _serializer.Serialize(requestContent);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, destinationUri)
            {
                Content = new StringContent(
                    requestContentSerialized,
                    Encoding.UTF8,
                    _requestContentType)
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

        public void Dispose()
        {
            _restClient?.Dispose();
        }
    }
}
