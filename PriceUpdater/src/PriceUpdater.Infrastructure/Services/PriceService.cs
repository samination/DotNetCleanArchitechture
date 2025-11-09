using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PriceUpdater.Application.Prices;
using PriceUpdater.Application.Prices.Contracts;
using PriceUpdater.Application.Prices.Events;
using PriceUpdater.Domain.Entities;
using PriceUpdater.Infrastructure.Persistence;

namespace PriceUpdater.Infrastructure.Services;

internal sealed class PriceService : IPriceService
{
    private readonly PriceDbContext _dbContext;
    private readonly ITopicProducer<Guid, PriceUpdatedEvent> _producer;
    private readonly ILogger<PriceService> _logger;

    public PriceService(
        PriceDbContext dbContext,
        ITopicProducer<Guid, PriceUpdatedEvent> producer,
        ILogger<PriceService> logger)
    {
        _dbContext = dbContext;
        _producer = producer;
        _logger = logger;
    }

    public async Task<PriceResponse> CreateAsync(CreatePriceRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Amount), "Price amount must be non-negative.");
        }

        DateTime createdAtUtc = request.CreatedAtUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        var entity = new Price
        {
            ProductId = request.ProductId,
            Amount = request.Amount,
            CreatedAtUtc = createdAtUtc
        };

        await _dbContext.Prices.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var priceUpdatedEvent = new PriceUpdatedEvent(entity.ProductId, entity.Amount, entity.CreatedAtUtc);

        await _producer.Produce(entity.ProductId, priceUpdatedEvent, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Created price entry {PriceId} for product {ProductId} with amount {Amount} at {CreatedAtUtc}",
            entity.Id,
            entity.ProductId,
            entity.Amount,
            entity.CreatedAtUtc);

        return new PriceResponse(entity.Id, entity.ProductId, entity.Amount, entity.CreatedAtUtc);
    }
}


