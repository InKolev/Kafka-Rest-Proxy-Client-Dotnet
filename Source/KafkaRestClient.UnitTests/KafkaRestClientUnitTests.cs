using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KafkaRestClient.Interfaces;
using KafkaRestClient.Models;
using KafkaRestClient.UnitTests.Models;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace KafkaRestClient.UnitTests
{
    [TestFixture]
    public class KafkaRestClientUnitTests
    {
        private IKafkaRestClient _client;

        private Mock<IRestClient> _restClientMock;

        private Mock<ISerializer> _serializerMock;

        private KafkaRestClientUnitTestsConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = GetConfig();

            _restClientMock = new Mock<IRestClient>();
            _serializerMock = new Mock<ISerializer>();

            _client = new KafkaRestClient(
                _restClientMock.Object,
                _serializerMock.Object,
                _config.KafkaRestProxyUrl,
                _config.RequestContentType);
        }

        [TearDown]
        public void TearDown()
        {
            _config = null;
            _restClientMock = null;
            _serializerMock = null;

            _client.Dispose();
            _client = null;
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.NoContent)]
        public void PostSingleRecordAsync_ValidRequestAndHttpResponseIndicatesSuccess_DoesNotThrowException(
            HttpStatusCode responseStatusCode)
        {
            // Arrange
            var record = new Mob
            {
                Name = "Scarlet van Halisha",
                MobType = MobType.RaidBoss
            };

            var request = new PostSingleRecordRequest<Mob>()
                .WithDestination<MobsTopic>()
                .WithRecord(record);

            _restClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            _serializerMock
                .Setup(x => x.Serialize(It.IsAny<object>()))
                .Returns(JsonConvert.SerializeObject(request));

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () =>
                {
                    var response = await _client.PostSingleRecordAsync(request);
                    response.EnsureSuccessStatusCode();
                });

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
                Times.Once());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()),
                Times.Once());
        }

        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.GatewayTimeout)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public void PostSingleRecordAsync_ValidRequestButHttpResponseIndicatesFailure_ThrowsHttpRequestException(
            HttpStatusCode responseStatusCode)
        {
            // Arrange
            var record = new Mob
            {
                Name = "Scarlet van Halisha",
                MobType = MobType.RaidBoss
            };

            var request = new PostSingleRecordRequest<Mob>()
                .WithDestination<MobsTopic>()
                .WithRecord(record);

            _restClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(responseStatusCode)));

            _serializerMock
                .Setup(x => x.Serialize(It.IsAny<object>()))
                .Returns(JsonConvert.SerializeObject(request));

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(
                async () =>
                {
                    var response = await _client.PostSingleRecordAsync(request);
                    response.EnsureSuccessStatusCode();
                });

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
                Times.Once());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()),
                Times.Once());
        }

        [Test]
        public void PostSingleRecordAsync_RecordIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var request = new PostSingleRecordRequest<Mob>()
                .WithDestination<MobsTopic>()
                .WithRecord(null);

            // Act & Assert
            var exc = Assert.ThrowsAsync<ArgumentNullException>(
                 async () => await _client.PostSingleRecordAsync(request));
           
            StringAssert.Contains(nameof(request.Record), exc.Message);

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
                Times.Never());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()),
                Times.Never());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void PostSingleRecordAsync_DestinationIsNullEmptyOrWhitespace_ThrowsArgumentException(string destination)
        {
            // Arrange
            var record = new Mob
            {
                MobType = MobType.Monster,
                Name = "Lidia von Hellman"
            };

            var request = new PostSingleRecordRequest<Mob>()
                .WithDestination(destination)
                .WithRecord(record);

            // Act & Assert
            var exc = Assert.ThrowsAsync<ArgumentException>(
                async () => await _client.PostSingleRecordAsync(request));

            StringAssert.Contains(nameof(request.Topic), exc.Message);

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
                Times.Never());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()),
                Times.Never());
        }

        [Test]
        public async Task PostMultipleRecords()
        {
            var records = new List<Mob>
            {
                new Mob { Name = "Istina", MobType = MobType.RaidBoss },
                new Mob { Name = "Scarlet van Halisha", MobType = MobType.RaidBoss},
                new Mob { Name = "Lidia von Hellman", MobType = MobType.RaidBoss }
            };

            var request = new PostMultipleRecordsRequest<Mob>()
                .WithRecords(records)
                .WithDestination<MobsTopic>();

            _restClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            _serializerMock
                .Setup(x => x.Serialize(It.IsAny<object>()))
                .Returns(JsonConvert.SerializeObject(request));

            var response = await _client.PostMultipleRecordsAsync(request);

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
                Times.Once());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()),
                Times.Once());
        }

        private KafkaRestClientUnitTestsConfig GetConfig()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configLocation = $@"{assemblyLocation}\{nameof(KafkaRestClientUnitTestsConfig)}.json";
            if (!File.Exists(configLocation))
            {
                throw new ArgumentException($"Could not find \"{configLocation}\".");
            }

            var configContent = File.ReadAllText(configLocation);

            return JsonConvert.DeserializeObject<KafkaRestClientUnitTestsConfig>(configContent);
        }
    }
}
