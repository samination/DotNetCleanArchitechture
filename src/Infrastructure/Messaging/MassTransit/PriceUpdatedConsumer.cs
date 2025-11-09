using Application.IntegrationEvents.Prices;
using Application.Services.Products;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.MassTransit;

internal sealed class PriceUpdatedConsumer : IConsumer<PriceUpdatedEvent>
{
    private readonly IProductService _productService;
    private readonly ILogger<PriceUpdatedConsumer> _logger;

    public PriceUpdatedConsumer(IProductService productService, ILogger<PriceUpdatedConsumer> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PriceUpdatedEvent> context)
    {
        try
        {
            await _productService.UpdatePriceIfNewerAsync(
                context.Message.ProductId,
                context.Message.Price,
                context.Message.CreatedAtUtc,
                context.CancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Processed price update for product {ProductId} with price {Price} created at {CreatedAtUtc}",
                context.Message.ProductId,
                context.Message.Price,
                context.Message.CreatedAtUtc);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process price update for product {ProductId}",
                context.Message.ProductId);
            throw;
        }
    }
}


