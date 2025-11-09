using Domain.Entitites.Orders;

namespace DTO.Orders
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}


