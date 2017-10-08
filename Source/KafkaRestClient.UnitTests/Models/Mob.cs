using KafkaRestClient.Interfaces;

namespace KafkaRestClient.UnitTests.Models
{
    public class Mob : IGetPartitionKey
    {
        public string Name { get; set; }

        public MobType MobType { get; set; }

        public string GetPartitionKey()
        {
            return MobType.ToString();
        }
    }
}