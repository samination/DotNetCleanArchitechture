using Application.IntegrationEvents.Orders;
using Application.Services.Common;

namespace Application.Services.Orders
{
    public interface IOrderEventPublisher : ISingletonService
    {
        Task PublishOrderPaidAsync(OrderPaidEvent @event, CancellationToken ct);
    }
}

