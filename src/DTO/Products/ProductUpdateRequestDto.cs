using System;

namespace DTO.Products
{
    public class ProductUpdateRequestDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        public Guid CategoryId { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
