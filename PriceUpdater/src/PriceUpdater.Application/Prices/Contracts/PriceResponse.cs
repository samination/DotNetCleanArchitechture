namespace PriceUpdater.Application.Prices.Contracts;

public record PriceResponse(Guid Id, Guid ProductId, double Amount, DateTime CreatedAtUtc);


