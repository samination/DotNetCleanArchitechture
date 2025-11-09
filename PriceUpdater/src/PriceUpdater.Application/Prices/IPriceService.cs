using PriceUpdater.Application.Prices.Contracts;

namespace PriceUpdater.Application.Prices;

public interface IPriceService
{
    Task<PriceResponse> CreateAsync(CreatePriceRequest request, CancellationToken cancellationToken);
}


