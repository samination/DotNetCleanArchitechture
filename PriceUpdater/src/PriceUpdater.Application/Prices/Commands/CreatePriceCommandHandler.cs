using MediatR;
using PriceUpdater.Application.Prices;
using PriceUpdater.Application.Prices.Contracts;

namespace PriceUpdater.Application.Prices.Commands;

internal sealed class CreatePriceCommandHandler : IRequestHandler<CreatePriceCommand, PriceResponse>
{
    private readonly IPriceService _priceService;

    public CreatePriceCommandHandler(IPriceService priceService)
    {
        _priceService = priceService;
    }

    public Task<PriceResponse> Handle(CreatePriceCommand request, CancellationToken cancellationToken)
    {
        var dto = new CreatePriceRequest(request.ProductId, request.Amount, request.CreatedAtUtc);
        return _priceService.CreateAsync(dto, cancellationToken);
    }
}


