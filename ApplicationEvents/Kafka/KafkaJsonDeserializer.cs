using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace ApplicationEvents.Kafka;

public class KafkaJsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull || data.IsEmpty) return default!;

        string json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}