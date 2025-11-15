using Application.Features.Orders.Commands;
using Application.Services.Orders;
using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Handlers.Create
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private readonly IOrderService _orders;

        public CreateOrderHandler(IOrderService orders)
        {
            _orders = orders;
        }

        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            return await _orders.CreateOrderAsync(request.Order, cancellationToken);
        }
    }
}







