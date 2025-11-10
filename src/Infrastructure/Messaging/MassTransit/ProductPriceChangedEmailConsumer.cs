using Application.IntegrationEvents.Products;
using Application.Services.Notifications;
using Data.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.MassTransit;

internal sealed class ProductPriceChangedEmailConsumer : IConsumer<ProductPriceChangedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<ProductPriceChangedEmailConsumer> _logger;

    public ProductPriceChangedEmailConsumer(
        ApplicationDbContext dbContext,
        IEmailService emailService,
        ILogger<ProductPriceChangedEmailConsumer> logger)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductPriceChangedEvent> context)
    {
        try
        {
            var orders = await _dbContext.Orders
                .Where(o => context.Message.AffectedOrderIds.Contains(o.Id))
                .ToListAsync(context.CancellationToken)
                .ConfigureAwait(false);

            await _emailService.SendProductPriceChangedNotificationAsync(
                context.Message,
                orders,
                context.CancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Processed ProductPriceChangedEvent for product {ProductId} affecting {OrderCount} orders.",
                context.Message.ProductId,
                context.Message.AffectedOrderIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process ProductPriceChangedEvent for product {ProductId}.",
                context.Message.ProductId);

            throw new InvalidOperationException(
                string.Format(
                    "Failed to process ProductPriceChangedEvent for product '{0}' affecting {1} orders.",
                    context.Message.ProductId,
                    context.Message.AffectedOrderIds.Count),
                ex);
        }
    }
}


