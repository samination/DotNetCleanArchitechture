using Domain.Entitites.Orders;
using System;

namespace DTO.Orders
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}


