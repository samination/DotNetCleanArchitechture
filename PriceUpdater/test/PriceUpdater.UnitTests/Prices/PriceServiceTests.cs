using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PriceUpdater.Application.Prices.Contracts;
using PriceUpdater.Application.Prices.Events;
using PriceUpdater.Infrastructure.Persistence;
using PriceUpdater.Infrastructure.Services;
using Shouldly;

namespace PriceUpdater.UnitTests.Prices;

public class PriceServiceTests
{
    private static PriceDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PriceDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new PriceDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPriceAndPublishEvent()
    {
        // Arrange
        await using var dbContext = CreateDbContext(nameof(CreateAsync_ShouldPersistPriceAndPublishEvent));

        var producerMock = new Mock<ITopicProducer<Guid, PriceUpdatedEvent>>();
        var loggerMock = new Mock<ILogger<PriceService>>();

        var service = new PriceService(dbContext, producerMock.Object, loggerMock.Object);

        var request = new CreatePriceRequest(
            Guid.NewGuid(),
            55.75,
            DateTime.UtcNow);

        // Act
        var response = await service.CreateAsync(request, CancellationToken.None);

        // Assert - entity persisted
        var entity = await dbContext.Prices.FirstOrDefaultAsync(p => p.Id == response.Id);
        entity.ShouldNotBeNull();
        entity.ProductId.ShouldBe(request.ProductId);
        entity.Amount.ShouldBe(request.Amount);

        // Assert - event published with matching payload
        producerMock.Verify(
            producer => producer.Produce(
                request.ProductId,
                It.Is<PriceUpdatedEvent>(evt =>
                    evt.ProductId == request.ProductId &&
                    evt.Price == request.Amount &&
                    evt.CreatedAtUtc == entity.CreatedAtUtc),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAt_WhenTimestampIsNull()
    {
        // Arrange
        await using var dbContext = CreateDbContext(nameof(CreateAsync_ShouldSetCreatedAt_WhenTimestampIsNull));

        var producerMock = new Mock<ITopicProducer<Guid, PriceUpdatedEvent>>();
        var loggerMock = new Mock<ILogger<PriceService>>();

        var service = new PriceService(dbContext, producerMock.Object, loggerMock.Object);

        var request = new CreatePriceRequest(
            Guid.NewGuid(),
            19.99,
            null);

        // Act
        var response = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        var entity = await dbContext.Prices.FirstOrDefaultAsync(p => p.Id == response.Id);
        entity.ShouldNotBeNull();
        entity.CreatedAtUtc.ShouldBe(response.CreatedAtUtc);
        entity.CreatedAtUtc.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task CreateAsync_WhenCancellationRequested_ShouldNotPersistOrPublish()
    {
        // Arrange
        await using var dbContext = CreateDbContext(nameof(CreateAsync_WhenCancellationRequested_ShouldNotPersistOrPublish));

        var producerMock = new Mock<ITopicProducer<Guid, PriceUpdatedEvent>>();
        var loggerMock = new Mock<ILogger<PriceService>>();

        var service = new PriceService(dbContext, producerMock.Object, loggerMock.Object);

        var request = new CreatePriceRequest(
            Guid.NewGuid(),
            60.0,
            DateTime.UtcNow);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var action = () => service.CreateAsync(request, cts.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(action);

        (await dbContext.Prices.CountAsync()).ShouldBe(0);

        producerMock.Verify(
            producer => producer.Produce(
                It.IsAny<Guid>(),
                It.IsAny<PriceUpdatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}


