namespace Infrastructure.Messaging.MassTransit;

internal sealed class KafkaOptions
{
    internal const string SectionName = "Kafka";

    public string? BootstrapServers { get; init; }

    public string? ConsumerGroupId { get; init; }

    public string? OrderPaidTopic { get; init; }
}


