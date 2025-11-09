using Application.Features.Orders.Queries;
using Application.Services.Orders;
using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Handlers.Read
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Order>
    {
        private readonly IOrderService _orders;

        public GetOrderByIdHandler(IOrderService orders)
        {
            _orders = orders;
        }

        public async Task<Order> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _orders.GetOrderByIdAsync(request.OrderId, cancellationToken);
        }
    }
}

