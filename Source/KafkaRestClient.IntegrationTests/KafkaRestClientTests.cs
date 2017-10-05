using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaRestClient.Interfaces;
using KafkaRestClient.Models;
using NUnit.Framework;

namespace KafkaRestClient.IntegrationTests
{
    [TestFixture]
    public class KafkaRestClientTests
    {
        private IKafkaRestClient _client;

        private IRestClient _restClient;

        private ISerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            _restClient = new RestClient();
            _serializer = new JsonSerializer();

            //"PLAINTEXT://35.189.203.232:9092", "PLAINTEXT://35.187.15.9:9092", "PLAINTEXT://104.199.88.226:9092"
            // Load all strings and nums from config
            _client = new KafkaRestClient(
                _restClient,
                _serializer,
                "http://35.189.203.232:8082",
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
            response.EnsureSuccessStatusCode();

            StringAssert.Contains("offsets", response.Content.ReadAsStringAsync().Result);
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
            response.EnsureSuccessStatusCode();

            StringAssert.Contains("offsets", response.Content.ReadAsStringAsync().Result);
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
