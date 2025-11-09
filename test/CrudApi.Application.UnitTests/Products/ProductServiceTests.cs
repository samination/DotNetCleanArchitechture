using Application.Services.Categories;
using Infrastructure.Services.Products;
using Data.Database;
using Domain.Entitites.Products;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace CrudApi.Application.UnitTests.Products;

public class ProductServiceTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock = new();

    private static ApplicationDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task UpdatePriceIfNewerAsync_ShouldUpdatePrice_WhenEventIsNewer()
    {
        // Arrange
        await using var dbContext = CreateDbContext(nameof(UpdatePriceIfNewerAsync_ShouldUpdatePrice_WhenEventIsNewer));
        var service = new ProductService(dbContext, _categoryServiceMock.Object);

        var product = new Product
        {
            Name = "Test Product",
            Description = "Description",
            Price = 10.0,
            Stock = 5,
            CategoryId = Guid.NewGuid()
        };

        product.SetUpdatedAt(DateTime.UtcNow.AddHours(-2));

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var newPrice = 15.5;
        var eventTimestamp = DateTime.UtcNow;

        // Act
        await service.UpdatePriceIfNewerAsync(product.Id, newPrice, eventTimestamp, CancellationToken.None);

        // Assert
        Product updated = await dbContext.Products.FirstAsync(p => p.Id == product.Id);

        updated.Price.ShouldBe(newPrice);
        updated.UpdatedAt.ShouldBe(eventTimestamp);
    }

    [Fact]
    public async Task UpdatePriceIfNewerAsync_ShouldIgnore_WhenEventIsOlder()
    {
        // Arrange
        await using var dbContext = CreateDbContext(nameof(UpdatePriceIfNewerAsync_ShouldIgnore_WhenEventIsOlder));
        var service = new ProductService(dbContext, _categoryServiceMock.Object);

        var product = new Product
        {
            Name = "Existing Product",
            Description = "Description",
            Price = 20.0,
            Stock = 10,
            CategoryId = Guid.NewGuid()
        };

        var existingUpdatedAt = DateTime.UtcNow;
        product.SetUpdatedAt(existingUpdatedAt);

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var olderEventTimestamp = existingUpdatedAt.AddMinutes(-30);
        var newPrice = 25.0;

        // Act
        await service.UpdatePriceIfNewerAsync(product.Id, newPrice, olderEventTimestamp, CancellationToken.None);

        // Assert
        Product updated = await dbContext.Products.FirstAsync(p => p.Id == product.Id);

        updated.Price.ShouldBe(20.0);
        updated.UpdatedAt.ShouldBe(existingUpdatedAt);
    }
}


