using Contracts;

namespace GatewayAPI.Kafka;

public interface IKafkaProducer
{
    Task SendAsync(ApplicationSubmittedEvent evt, CancellationToken ct = default);
}