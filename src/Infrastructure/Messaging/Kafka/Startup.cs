using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Messaging.Kafka;

internal static class Startup
{
    public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaSection = configuration.GetSection(KafkaOptions.SectionName);

        if (!kafkaSection.Exists() ||
            string.IsNullOrWhiteSpace(kafkaSection[nameof(KafkaOptions.BootstrapServers)]))
        {
            Log.Warning("Kafka not configured; consumer hosted service not started.");
            return services;
        }

        services.Configure<KafkaOptions>(kafkaSection);
        services.AddHostedService<OrderPaidConsumerHostedService>();

        return services;
    }
}

