using Microsoft.EntityFrameworkCore;
using PriceUpdater.Domain.Entities;

namespace PriceUpdater.Infrastructure.Persistence;

public class PriceDbContext : DbContext
{
    public PriceDbContext(DbContextOptions<PriceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Price> Prices => Set<Price>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Price>(builder =>
        {
            builder.HasKey(price => price.Id);
            builder.Property(price => price.ProductId)
                .IsRequired();
            builder.Property(price => price.Amount)
                .IsRequired();
            builder.Property(price => price.CreatedAtUtc)
                .IsRequired();
            builder.HasIndex(price => new { price.ProductId, price.CreatedAtUtc });
        });
    }
}


