using ApplicationEvents.Storage;
using Confluent.Kafka;
using Contracts;
using HRFlow.Contracts;

namespace ApplicationEvents.Kafka;

public class KafkaBackgroundConsumer : BackgroundService
{
    private const string Topic = "application-events";

    private readonly IConsumer<string, ApplicationSubmittedEvent> _consumer;
    private readonly IApplicationStorage _storage;

    public KafkaBackgroundConsumer(IConfiguration config, IApplicationStorage storage)
    {
        _storage = storage;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "kafka:29092",
            GroupId = config["Kafka:GroupId"] ?? "gateway-application-events",
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true
        };

        _consumer = new ConsumerBuilder<string, ApplicationSubmittedEvent>(consumerConfig)
            .SetValueDeserializer(new KafkaJsonDeserializer<ApplicationSubmittedEvent>())
            .Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        try
        {
            _consumer.Subscribe(Topic);
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, ApplicationSubmittedEvent>? result = _consumer.Consume(stoppingToken);;
                if (result?.Message is null) continue;
                var evnt = result.Message.Value;
                _storage.Add(new Application { Id = evnt.CandidateId.ToString(), CandidateName = evnt.CandidateName });
            }
        }
        finally
        {
            _consumer.Close();
        }

        return Task.CompletedTask;
    }
    
    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}