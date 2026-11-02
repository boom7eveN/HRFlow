using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace GatewayAPI.Kafka;

public class KafkaJsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data == null) return Array.Empty<byte>();;
        string json = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(json);
    }
}