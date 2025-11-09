namespace PriceUpdater.Application.Prices.Contracts;

public record CreatePriceRequest(Guid ProductId, double Amount, DateTime? CreatedAtUtc);


