namespace PriceUpdater.Application.Prices.Events;

public record PriceUpdatedEvent(Guid ProductId, double Price, DateTime CreatedAtUtc);


