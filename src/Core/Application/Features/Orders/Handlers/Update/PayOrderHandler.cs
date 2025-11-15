using Application.Features.Orders.Commands;
using Application.IntegrationEvents.Orders;
using Application.Services.Orders;
using Domain.Entitites.Orders;
using MediatR;

namespace Application.Features.Orders.Handlers.Update
{
    public class PayOrderHandler : IRequestHandler<PayOrderCommand, Order>
    {
        private readonly IOrderService _orders;
        private readonly IOrderEventPublisher _eventPublisher;

        public PayOrderHandler(IOrderService orders, IOrderEventPublisher eventPublisher)
        {
            _orders = orders;
            _eventPublisher = eventPublisher;
        }

        public async Task<Order> Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            Order order = await _orders.PayOrderAsync(request.OrderId, cancellationToken);

            await _eventPublisher.PublishOrderPaidAsync(new OrderPaidEvent(order.Id, order.ProductId), cancellationToken);

            return order;
        }
    }
}







