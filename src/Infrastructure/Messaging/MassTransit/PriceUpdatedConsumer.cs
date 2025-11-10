using System.Linq;
using Application.IntegrationEvents.Prices;
using Application.IntegrationEvents.Products;
using Application.Services.Products;
using Data.Database;
using Domain.Entitites.Orders;
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.MassTransit;

internal sealed class PriceUpdatedConsumer : IConsumer<PriceUpdatedEvent>
{
    private readonly IProductService _productService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITopicProducer<Guid, ProductPriceChangedEvent> _priceChangedProducer;
    private readonly ILogger<PriceUpdatedConsumer> _logger;

    public PriceUpdatedConsumer(
        IProductService productService,
        ApplicationDbContext dbContext,
        ITopicProducer<Guid, ProductPriceChangedEvent> priceChangedProducer,
        ILogger<PriceUpdatedConsumer> logger)
    {
        _productService = productService;
        _dbContext = dbContext;
        _priceChangedProducer = priceChangedProducer;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PriceUpdatedEvent> context)
    {
        try
        {
            var result = await _productService.UpdatePriceIfNewerAsync(
                context.Message.ProductId,
                context.Message.Price,
                context.Message.CreatedAtUtc,
                context.CancellationToken).ConfigureAwait(false);

            if (!result.IsUpdated)
            {
                _logger.LogInformation(
                    "Price update ignored for product {ProductId}. Incoming timestamp {IncomingTimestamp} was not newer.",
                    context.Message.ProductId,
                    context.Message.CreatedAtUtc);
                return;
            }

            List<Guid> pendingOrderIds = await _dbContext.Orders
                .Where(o => o.ProductId == context.Message.ProductId && o.PaymentStatus == PaymentStatus.Pending)
                .Select(o => o.Id)
                .ToListAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (pendingOrderIds.Count > 0)
            {
                var priceChangedEvent = new ProductPriceChangedEvent(
                    context.Message.ProductId,
                    result.PreviousPrice,
                    result.CurrentPrice,
                    result.UpdatedAtUtc,
                    pendingOrderIds);

                await _priceChangedProducer
                    .Produce(context.Message.ProductId, priceChangedEvent, context.CancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation(
                    "Published ProductPriceChangedEvent for product {ProductId} affecting {OrdersCount} pending orders.",
                    context.Message.ProductId,
                    pendingOrderIds.Count);
            }

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

            throw new InvalidOperationException(
                string.Format(
                    "Failed to process price update for product '{0}' with incoming price '{1}' at '{2:O}'.",
                    context.Message.ProductId,
                    context.Message.Price,
                    context.Message.CreatedAtUtc),
                ex);
        }
    }
}


