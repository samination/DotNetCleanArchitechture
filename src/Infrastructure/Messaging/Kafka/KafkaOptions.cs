namespace Infrastructure.Messaging.Kafka
{
    public class KafkaOptions
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; set; } = "localhost:9092";
        public string ConsumerGroupId { get; set; } = "dotnet-crud-api";
        public string OrderPaidTopic { get; set; } = "order-paid";
    }
}

