using Application.Features.Orders.Commands;
using Application.Features.Orders.Handlers.Update;
using Application.IntegrationEvents.Orders;
using Application.Services.Orders;
using Domain.Entitites.Orders;
using Moq;
using Shouldly;

namespace CrudApi.Application.UnitTests.Orders.Handlers;

public class PayOrderHandlerTests
{
    private readonly Mock<IOrderService> _orderServiceMock = new();
    private readonly Mock<IOrderEventPublisher> _eventPublisherMock = new();
    private readonly PayOrderHandler _sut;

    public PayOrderHandlerTests()
    {
        _sut = new PayOrderHandler(_orderServiceMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPayOrderAndPublishEvent()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var order = new Order
        {
            ProductId = Guid.NewGuid()
        };

        _orderServiceMock
            .Setup(service => service.PayOrderAsync(order.Id, cancellationToken))
            .ReturnsAsync(order);

        var command = new PayOrderCommand(order.Id);

        // Act
        Order result = await _sut.Handle(command, cancellationToken);

        // Assert
        result.ShouldBe(order);

        _orderServiceMock.Verify(
            service => service.PayOrderAsync(order.Id, cancellationToken),
            Times.Once);

        _eventPublisherMock.Verify(
            publisher => publisher.PublishOrderPaidAsync(
                It.Is<OrderPaidEvent>(evt =>
                    evt.OrderId == order.Id &&
                    evt.ProductId == order.ProductId),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPropagateCancellation()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        var command = new PayOrderCommand(Guid.NewGuid());

        _orderServiceMock
            .Setup(service => service.PayOrderAsync(command.OrderId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act
        var action = () => _sut.Handle(command, cancellationToken);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(action);

        _eventPublisherMock.Verify(
            publisher => publisher.PublishOrderPaidAsync(
                It.IsAny<OrderPaidEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}


