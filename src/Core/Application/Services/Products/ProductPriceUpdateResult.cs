namespace Application.Services.Products;

public sealed record ProductPriceUpdateResult(
    bool IsUpdated,
    double PreviousPrice,
    double CurrentPrice,
    DateTime UpdatedAtUtc);


