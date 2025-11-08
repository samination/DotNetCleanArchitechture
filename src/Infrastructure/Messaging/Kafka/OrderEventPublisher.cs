using System.Text.Json;
using Application.IntegrationEvents.Orders;
using Application.Services.Orders;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging.Kafka
{
    internal sealed class OrderEventPublisher : IOrderEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaOptions _options;
        private bool _disposed;

        public OrderEventPublisher(IOptions<KafkaOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
            {
                throw new InvalidOperationException("Kafka bootstrap servers are not configured.");
            }

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task PublishOrderPaidAsync(OrderPaidEvent @event, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_options.OrderPaidTopic))
            {
                throw new InvalidOperationException("Kafka order paid topic is not configured.");
            }

            string payload = JsonSerializer.Serialize(@event);

            var message = new Message<string, string>
            {
                Key = @event.OrderId.ToString(),
                Value = payload
            };

            try
            {
                DeliveryResult<string, string> result = await _producer.ProduceAsync(_options.OrderPaidTopic, message, ct);

                if (result.Status != PersistenceStatus.Persisted)
                {
                    throw new KafkaException(new Error(ErrorCode.Unknown, "Failed to persist message."));
                }
            }
            catch (ProduceException<string, string> ex)
            {
                throw new KafkaException(ex.Error);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();

            _disposed = true;
        }
    }
}

