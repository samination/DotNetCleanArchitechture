namespace Application.IntegrationEvents.Prices;

public record PriceUpdatedEvent(Guid ProductId, double Price, DateTime CreatedAtUtc);


