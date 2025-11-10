namespace Application.IntegrationEvents.Orders;

public interface IOrderPaidEventProcessor
{
    Task HandleAsync(OrderPaidEvent orderPaidEvent, CancellationToken cancellationToken);
}

