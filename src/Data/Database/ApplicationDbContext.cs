using System;
using System.Linq.Expressions;
using Data.Identity;
using Domain.Entitites.Categories;
using Domain.Entitites.Orders;
using Domain.Entitites.Products;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entitites;

namespace Data.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ApplyGlobalEntityConfiguration(builder);

            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Product)
                    .WithMany()
                    .HasForeignKey(o => o.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.PaymentStatus)
                    .HasConversion<int>();

                entity.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });
        }

        private static void ApplyGlobalEntityConfiguration(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(Base).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                builder.Entity(entityType.ClrType)
                    .Property<DateTime>(nameof(Base.CreatedAt))
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                builder.Entity(entityType.ClrType)
                    .Property<DateTime>(nameof(Base.UpdatedAt))
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                builder.Entity(entityType.ClrType)
                    .Property<DateTime?>(nameof(Base.DeletedAt))
                    .HasConversion(
                        v => v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

                builder.Entity(entityType.ClrType)
                    .Property<bool>(nameof(Base.IsDeleted))
                    .HasDefaultValue(false);

                builder.Entity(entityType.ClrType)
                    .Property<byte[]>(nameof(Base.RowVersion))
                    .IsRowVersion();

                var filter = CreateIsDeletedRestriction(entityType.ClrType);
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        private static LambdaExpression CreateIsDeletedRestriction(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var property = Expression.Property(parameter, nameof(Base.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            return Expression.Lambda(condition, parameter);
        }
    }
}
