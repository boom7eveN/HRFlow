using Confluent.Kafka;
using Contracts;

namespace GatewayAPI.Kafka;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private const string Topic = "application-events";

    private readonly IProducer<string, ApplicationSubmittedEvent> _producer;

    public KafkaProducer(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "kafka:29092",
            Acks = Acks.All,
            EnableIdempotence = true,
            AllowAutoCreateTopics = true
        };

        _producer = new ProducerBuilder<string, ApplicationSubmittedEvent>(producerConfig)
            .SetValueSerializer(new KafkaJsonSerializer<ApplicationSubmittedEvent>())
            .Build();
    }

    public async Task SendAsync(ApplicationSubmittedEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, ApplicationSubmittedEvent>
        {
            Key = evt.CandidateId.ToString(),
            Value = evt
        };

        var result = await _producer.ProduceAsync(Topic, message, ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}