using Application.Services.Orders;
using Data.Database;
using Domain.Entitites.Orders;
using Domain.Entitites.Products;
using Domain.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Orders
{
    internal class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;

        public OrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Order> CreateOrderAsync(Order order, CancellationToken ct)
        {
            Product? product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == order.ProductId, ct);

            if (product is null)
            {
                throw new KeyNotFoundException($"The product with ID: {order.ProductId} was not found in the database.");
            }

            if (product.Stock <= 0)
            {
                throw new CustomException($"The product '{product.Name}' is out of stock.");
            }

            order.PaymentStatus = PaymentStatus.Pending;
            order.CreatedAt = DateTime.UtcNow;
            order.MarkUpdated();

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            return order;
        }

        public async Task<Order> GetOrderByIdAsync(Guid orderId, CancellationToken ct)
        {
            Order? order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, ct);

            if (order is null)
            {
                throw new KeyNotFoundException($"The order with ID: {orderId} was not found in the database.");
            }

            return order;
        }

        public async Task<Order> PayOrderAsync(Guid orderId, CancellationToken ct)
        {
            Order? order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);

            if (order is null)
            {
                throw new KeyNotFoundException($"The order with ID: {orderId} was not found in the database.");
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                throw new CustomException("The order has already been paid.");
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.MarkUpdated();

            await _db.SaveChangesAsync(ct);

            return order;
        }
    }
}

