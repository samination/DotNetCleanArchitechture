using System;
using Domain.Entitites.Categories;
using Domain.Helpers;

namespace Domain.Entitites.Products
{
    public class Product : Base
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private double _price;
        private int _stock;
        private Guid _categoryId;

        protected Product()
        {
        }

        public Product(string name, string? description, double price, int stock, Guid categoryId)
        {
            Name = name;
            Description = description ?? string.Empty;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }

        public Product(Guid id, string name, string? description, double price, int stock, Guid categoryId)
            : base(id, DateTime.UtcNow)
        {
            Name = name;
            Description = description ?? string.Empty;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }

        public string Name
        {
            get => _name;
            set => _name = Guard.AgainstNullOrWhiteSpace(value, nameof(Name), minLength: 2, maxLength: 150);
        }

        public string Description
        {
            get => _description;
            set => _description = Guard.AgainstTooLong(value, nameof(Description), maxLength: 1000) ?? string.Empty;
        }

        public double Price
        {
            get => _price;
            set => _price = Guard.AgainstOutOfRange(value, nameof(Price), minimum: 0.01, maximum: 1_000_000);
        }

        public int Stock
        {
            get => _stock;
            set => _stock = Guard.AgainstOutOfRange(value, nameof(Stock), minimum: 0, maximum: 1_000_000);
        }

        // Relationship
        public Guid CategoryId
        {
            get => _categoryId;
            set => _categoryId = Guard.AgainstEmpty(value, nameof(CategoryId));
        }

        public Category Category { get; private set; } = null!;

        public void UpdateDetails(string name, string? description, double price, int stock, Guid categoryId)
        {
            Name = name;
            Description = description ?? string.Empty;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
            MarkUpdated();
        }
    }
}
