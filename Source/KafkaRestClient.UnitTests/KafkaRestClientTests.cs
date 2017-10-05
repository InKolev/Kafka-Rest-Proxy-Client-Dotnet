using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KafkaRestClient.Interfaces;
using KafkaRestClient.Models;
using Moq;
using NUnit.Framework;

namespace KafkaRestClient.UnitTests
{
    [TestFixture]
    public class KafkaRestClientTests
    {
        private IKafkaRestClient _client;

        private Mock<IRestClient> _restClientMock;

        private Mock<ISerializer> _serializerMock;

        [SetUp]
        public void SetUp()
        {
            _restClientMock = new Mock<IRestClient>();
            _serializerMock = new Mock<ISerializer>();

            _serializerMock.Setup(x => x.Serialize(It.IsAny<object>()))
                .Returns(Guid.NewGuid().ToString());    
                
            _client = new KafkaRestClient(
                _restClientMock.Object,
                _serializerMock.Object,
                "http://kafkarestproxy:8082",
                "application/vnd.kafka.json.v1+json");
        }
        [Test]
        public async Task PostSingleRecord()
        {
            var record = new Mob { Name = "Scarlet van Halisha", MobType = MobType.RaidBoss };

            var request = new PostSingleRecordRequest<Mob>()
                .WithDestination<RaidBossesTopic>()
                .WithRecord(record);

            var response = await _client.PostSingleRecordAsync(request);
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
                .WithDestination<RaidBossesTopic>();

            var response = await _client.PostMultipleRecordsAsync(request);

            _restClientMock.Verify(
                x => x.SendAsync(It.IsAny<HttpRequestMessage>()), 
                Times.Once());

            _serializerMock.Verify(
                x => x.Serialize(It.IsAny<object>()), 
                Times.Once());
        }
    }

    public class Mob : IGetPartitionKey
    {
        public string Name { get; set; }

        public MobType MobType { get; set; }

        public string GetPartitionKey()
        {
            return MobType.ToString();
        }
    }

    public enum MobType
    {
        Monster,
        Minion,
        RaidBoss
    }

    public class RaidBossesTopic : ITopic { }

    public class MonstersTopic : ITopic { }

    public class MinionsTopic : ITopic { }

    public class NpCsTopic : ITopic { }
}
