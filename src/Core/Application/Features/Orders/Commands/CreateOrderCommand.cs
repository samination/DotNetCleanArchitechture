using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Commands
{
    public record CreateOrderCommand(Order Order) : IRequest<Order>;
}







