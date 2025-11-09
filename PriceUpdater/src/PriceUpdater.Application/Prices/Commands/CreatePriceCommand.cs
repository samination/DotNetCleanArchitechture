using MediatR;
using PriceUpdater.Application.Prices.Contracts;

namespace PriceUpdater.Application.Prices.Commands;

public record CreatePriceCommand(Guid ProductId, double Amount, DateTime? CreatedAtUtc) : IRequest<PriceResponse>;


