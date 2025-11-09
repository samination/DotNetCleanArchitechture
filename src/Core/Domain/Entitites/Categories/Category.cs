using System;
using Domain.Helpers;

namespace Domain.Entitites.Categories
{
    public class Category : Base
    {
        private string _name = string.Empty;
        private string _description = string.Empty;

        // EF Core parameterless constructor
        protected Category()
        {
        }

        public Category(string name, string? description = null)
        {
            Name = name;
            Description = description ?? string.Empty;
        }

        public Category(Guid id, string name, string? description = null)
            : base(id, DateTime.UtcNow)
        {
            Name = name;
            Description = description ?? string.Empty;
        }

        public string Name
        {
            get => _name;
            set => _name = Guard.AgainstNullOrWhiteSpace(value, nameof(Name), minLength: 2, maxLength: 100);
        }

        public string Description
        {
            get => _description;
            set => _description = Guard.AgainstTooLong(value, nameof(Description), maxLength: 500) ?? string.Empty;
        }

        public void UpdateDetails(string name, string? description = null)
        {
            Name = name;
            Description = description ?? string.Empty;
            MarkUpdated();
        }
    }
}
