using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Commands
{
    public record PayOrderCommand(Guid OrderId) : IRequest<Order>;
}







