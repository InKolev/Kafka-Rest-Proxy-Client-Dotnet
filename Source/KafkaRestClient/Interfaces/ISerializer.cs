namespace KafkaRestClient.Interfaces
{
    public interface ISerializer
    {
        T Deserialize<T>(string obj);

        string Serialize<T>(T obj);
    }
}
