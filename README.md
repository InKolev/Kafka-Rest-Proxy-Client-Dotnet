# Kafka-Rest-Proxy-Client-Dotnet
Simple .NET client for easy access to Kafka Rest Proxy.

# Examples

## Initialize client
```cs
var restClient = new RestClient();
var serializer = new JsonSerializer();
var kafkaRestProxyUrl = "http://36.189.204.236:8082";
var kafkaRestProxyRequestContentType = "application/vnd.kafka.json.v1+json";

var client = new KafkaRestClient(
    restClient,
    serializer,
    kafkaRestProxyUrl,
    kafkaRestProxyRequestContentType);
```

## Post single record
```cs
var monster = new Monster { Name = "Scarlet van Halisha", MonsterType = MonsterType.RaidBoss };

var request = new PostSingleRecordRequest<Monster>()
    .WithRecord(monster)
    .WithDestination<MonstersTopic>();

HttpResponseMessage response = await client.PostSingleRecordAsync(request);
```

## Post multiple records
```cs
var monsters = new List<Monster>
{
    new Monster { Name = "Scarlet van Halisha", MonsterType = MonsterType.RaidBoss },
    new Monster { Name = "Lidia von Hellman", MonsterType = MonsterType.Monster },
    new Monster { Name = "Queen Ant", MonsterType = MonsterType.Minion }
};

var request = new PostMultipleRecordsRequest<Monster>()
    .WithRecords(monsters)
    .WithDestination<MonstersTopic>();

HttpResponseMessage response = await client.PostMultipleRecordsAsync(request);
```

## Topic
```cs
public class MonstersTopic : ITopic { }
```

## Record with key
```cs
public class Monster : IGetPartitionKey
{
   public string Name { get; set; }

   public MonsterType MonsterType { get; set; }

   public string GetPartitionKey()
   {
       return MonsterType.ToString();
   }
}

public enum MonsterType
{
   Minion,
   Monster,
   RaidBoss
}
```
