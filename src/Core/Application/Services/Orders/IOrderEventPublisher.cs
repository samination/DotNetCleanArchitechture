using Application.IntegrationEvents.Orders;
using Application.Services.Common;

namespace Application.Services.Orders
{
    public interface IOrderEventPublisher : IScopedService
    {
        Task PublishOrderPaidAsync(OrderPaidEvent @event, CancellationToken ct);
    }
}

