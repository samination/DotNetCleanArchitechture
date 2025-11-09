namespace PriceUpdater.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string? BootstrapServers { get; init; }
    public string? PriceUpdatedTopic { get; init; }
}


