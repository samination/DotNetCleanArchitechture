using Application.Services.Common;
using Domain.Entitites.Orders;

namespace Application.Services.Orders
{
    public interface IOrderService : ITransientService
    {
        Task<Order> CreateOrderAsync(Order order, CancellationToken ct);
        Task<Order> GetOrderByIdAsync(Guid orderId, CancellationToken ct);
        Task<Order> PayOrderAsync(Guid orderId, CancellationToken ct);
    }
}

