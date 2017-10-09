# Kafka-Rest-Proxy-Client-Dotnet
Simple .NET client for easy access to Kafka Rest Proxy.  

Currently supports only fast *publish* of messages.  
The speed of each publish is pretty much network bound (also varies depending on the size of the message).  
If the Kafka Rest Proxy is set up in the same data center as the client application, you can reach average publish speed around 2-3 ms per single message (tested with messages of size between 1 and 5kbs).
# Example usage

## Initialize client
```cs
// Wrapper over the standard .NET HttpClient, addressing some of the issues of the native client.
IRestClient restClient = new RestClient();

// Wrapper over Newtonsoft's JsonConvert class
ISerializer serializer = new JsonSerializer();

// Specify the URL from which Kafka Rest Proxy is accessible.
var kafkaRestProxyUrl = "http://36.189.204.236:8082";

// Specify the supported request content type for publishing messages to Kafka Rest Proxy. (it might be different for different versions of Kafka Rest Proxy)
var kafkaRestProxyRequestContentType = "application/vnd.kafka.json.v1+json";

// Initialize the client.
var client = new KafkaRestClient(
    restClient,
    serializer,
    kafkaRestProxyUrl,
    kafkaRestProxyRequestContentType);
```

## Post single record
```cs
// Create a single record that will be published to Kafka.
var monster = new Monster { Name = "Scarlet van Halisha", MonsterType = MonsterType.RaidBoss };

// Create a request that contains the record and the target topic.
var request = new PostSingleRecordRequest<Monster>()
    .WithDestination<MonstersTopic>()
    .WithRecord(monster);

// Send publish request to Kafka Rest Proxy.
HttpResponseMessage response = await client.PostSingleRecordAsync(request);
```

## Post multiple records
```cs
// Create a list of records that will be published to Kafka.
var monsters = new List<Monster>
{
    new Monster { Name = "Scarlet van Halisha", MonsterType = MonsterType.RaidBoss },
    new Monster { Name = "Lidia von Hellman", MonsterType = MonsterType.Monster },
    new Monster { Name = "Queen Ant", MonsterType = MonsterType.Minion }
};

// Create a request that contains all the records and the target topic.
var request = new PostMultipleRecordsRequest<Monster>()
    .WithDestination<MonstersTopic>()
    .WithRecords(monsters);

// Send publish request to Kafka Rest Proxy.
HttpResponseMessage response = await client.PostMultipleRecordsAsync(request);
```

## Topic
```cs
// If you want to use strongly typed topics, as shown in the examples above,
// you should create an empty class which implements the "ITopic" interface.
public class MonstersTopic : ITopic { }
```

## Record with partition key
```cs
// In order to define a partition key for the record which will be published to Kafka,
// your class should implement "IGetPartitionKey" interface and provide implementation for the GetPartitionKey() method.
public class Monster : IGetPartitionKey
{
   public string Name { get; set; }

   public MonsterType MonsterType { get; set; }

   public string GetPartitionKey()
   {
       return MonsterType.ToString();
   }
}
```
