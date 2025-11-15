namespace Application.IntegrationEvents.Orders
{
    public record OrderPaidEvent(Guid OrderId, Guid ProductId);
}







