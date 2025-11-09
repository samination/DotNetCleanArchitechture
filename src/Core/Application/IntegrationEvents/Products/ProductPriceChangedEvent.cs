using System.Collections.Generic;

namespace Application.IntegrationEvents.Products;

public record ProductPriceChangedEvent(
    Guid ProductId,
    double OldPrice,
    double NewPrice,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<Guid> AffectedOrderIds);


