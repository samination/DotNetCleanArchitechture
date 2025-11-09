using System;
using Application.IntegrationEvents.Orders;
using Application.IntegrationEvents.Prices;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Infrastructure.Messaging.MassTransit;

internal static class Startup
{
    public static IServiceCollection AddMassTransitInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("Configuring MassTransit infrastructure");

        var kafkaSection = configuration.GetSection(KafkaOptions.SectionName);
        var kafkaOptions = kafkaSection.Get<KafkaOptions>();

        if (kafkaOptions is null)
        {
            throw new InvalidOperationException("Kafka configuration section is missing.");
        }

        ValidateKafkaOptions(kafkaOptions);

        services.Configure<KafkaOptions>(kafkaSection);

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            busConfigurator.AddRider(rider =>
            {
                rider.AddConsumer<OrderPaidEventConsumer>();
                rider.AddConsumer<PriceUpdatedConsumer>();

                rider.AddProducer<Guid, OrderPaidEvent>(kafkaOptions.OrderPaidTopic!);

                rider.UsingKafka((context, kafka) =>
                {
                    var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
                    ValidateKafkaOptions(options);

                    kafka.Host(options.BootstrapServers);

                    var groupId = string.IsNullOrWhiteSpace(options.ConsumerGroupId)
                        ? "dotnet-crud-api-order-consumer"
                        : options.ConsumerGroupId;

                    kafka.TopicEndpoint<Guid, OrderPaidEvent>(
                        options.OrderPaidTopic!,
                        groupId,
                        endpoint =>
                        {
                            endpoint.ConfigureConsumer<OrderPaidEventConsumer>(context);
                        });

                    kafka.TopicEndpoint<Guid, PriceUpdatedEvent>(
                        options.PriceUpdatedTopic!,
                        groupId,
                        endpoint =>
                        {
                            endpoint.ConfigureConsumer<PriceUpdatedConsumer>(context);
                        });
                });
            });
        });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
            });

        return services;
    }

    private static void ValidateKafkaOptions(KafkaOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.OrderPaidTopic))
        {
            throw new InvalidOperationException("Kafka order paid topic configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.PriceUpdatedTopic))
        {
            throw new InvalidOperationException("Kafka price updated topic configuration is missing.");
        }
    }
}

