using System;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PriceUpdater.Application.Prices;
using PriceUpdater.Application.Prices.Events;
using PriceUpdater.Infrastructure.Messaging;
using PriceUpdater.Infrastructure.Persistence;
using PriceUpdater.Infrastructure.Services;

namespace PriceUpdater.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "server=localhost;port=3306;database=priceupdate;user=root;password=root;TreatTinyAsBoolean=true;";

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<PriceDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<IPriceService, PriceService>();

        var kafkaSection = configuration.GetSection(KafkaOptions.SectionName);
        var kafkaOptions = kafkaSection.Get<KafkaOptions>()
            ?? throw new InvalidOperationException("Kafka configuration section is missing.");
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
                rider.AddProducer<Guid, PriceUpdatedEvent>(kafkaOptions.PriceUpdatedTopic!);

                rider.UsingKafka((context, kafka) =>
                {
                    var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
                    ValidateKafkaOptions(options);

                    kafka.Host(options.BootstrapServers);
                });
            });
        });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(options => options.WaitUntilStarted = true);

        return services;
    }

    private static void ValidateKafkaOptions(KafkaOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.PriceUpdatedTopic))
        {
            throw new InvalidOperationException("Kafka price updated topic configuration is missing.");
        }
    }
}


