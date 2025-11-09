using System;
using System.Linq;

namespace Domain.Entitites
{
    public abstract class Base
    {
        protected Base()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            RowVersion = Array.Empty<byte>();
        }

        protected Base(Guid id, DateTime createdAtUtc)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("The identifier must not be empty.", nameof(id));
            }

            Id = id;
            CreatedAt = EnsureUtc(createdAtUtc);
            UpdatedAt = CreatedAt;
            RowVersion = Array.Empty<byte>();
        }

        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public byte[] RowVersion { get; private set; }

        public void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUpdatedAt(DateTime dateTimeUtc)
        {
            UpdatedAt = EnsureUtc(dateTimeUtc);
        }

        public void MarkDeleted()
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            MarkUpdated();
        }

        public void Restore()
        {
            if (!IsDeleted)
            {
                return;
            }

            IsDeleted = false;
            DeletedAt = null;
            MarkUpdated();
        }

        public void SetConcurrencyToken(byte[]? rowVersion)
        {
            RowVersion = rowVersion is { Length: > 0 }
                ? rowVersion.ToArray()
                : Array.Empty<byte>();
        }

        protected static DateTime EnsureUtc(DateTime timestamp)
        {
            return timestamp.Kind switch
            {
                DateTimeKind.Utc => timestamp,
                DateTimeKind.Local => timestamp.ToUniversalTime(),
                _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            };
        }
    }
}
