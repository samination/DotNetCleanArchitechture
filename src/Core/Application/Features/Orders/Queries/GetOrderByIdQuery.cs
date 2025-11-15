using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Queries
{
    public record GetOrderByIdQuery(Guid OrderId) : IRequest<Order>;
}







