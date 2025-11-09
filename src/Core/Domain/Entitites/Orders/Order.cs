using Domain.Entitites.Products;

namespace Domain.Entitites.Orders
{
    public class Order : Base
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}

