using System.Text.Json;
using Application.IntegrationEvents.Orders;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging.Kafka
{
    internal class OrderPaidConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly KafkaOptions _options;
        private readonly ILogger<OrderPaidConsumerHostedService> _logger;

        public OrderPaidConsumerHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<KafkaOptions> options,
            ILogger<OrderPaidConsumerHostedService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
                {
                    _logger.LogWarning("Kafka bootstrap servers configuration missing. OrderPaid consumer will not start.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_options.OrderPaidTopic))
                {
                    _logger.LogWarning("Kafka topic configuration missing. OrderPaid consumer will not start.");
                    return;
                }

                await Task.Yield();

                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = _options.BootstrapServers,
                    GroupId = string.IsNullOrWhiteSpace(_options.ConsumerGroupId) ? "dotnet-crud-api-order-consumer" : _options.ConsumerGroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
                consumer.Subscribe(_options.OrderPaidTopic);

                _logger.LogInformation("OrderPaid Kafka consumer started.");

                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;

                    try
                    {
                        result = consumer.Consume(stoppingToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                        continue;
                    }
                    catch (KafkaException ex)
                    {
                        _logger.LogError(ex, "Kafka broker error: {Reason}", ex.Error.Reason);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, "Unexpected error while consuming OrderPaid events.");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    if (result is null)
                    {
                        continue;
                    }

                    try
                    {
                        OrderPaidEvent? message = JsonSerializer.Deserialize<OrderPaidEvent>(result.Message.Value);

                        if (message is null)
                        {
                            _logger.LogWarning("Received empty order paid message.");
                            continue;
                        }

                        using IServiceScope scope = _serviceScopeFactory.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<IOrderPaidEventProcessor>();

                        await processor.HandleAsync(message, stoppingToken);

                        consumer.Commit(result);

                        _logger.LogInformation("Processed OrderPaid event for OrderId {OrderId} and ProductId {ProductId}", message.OrderId, message.ProductId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process OrderPaid message with key {Key}", result.Message.Key);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("OrderPaid Kafka consumer stopping due to cancellation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderPaid Kafka consumer terminated unexpectedly.");
            }
        }
    }
}

