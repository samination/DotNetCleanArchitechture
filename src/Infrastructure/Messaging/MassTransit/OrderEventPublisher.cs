using System;
using Application.IntegrationEvents.Orders;
using Application.Services.Orders;
using MassTransit;

namespace Infrastructure.Messaging.MassTransit;

internal sealed class OrderEventPublisher : IOrderEventPublisher
{
    private readonly ITopicProducer<Guid, OrderPaidEvent> _producer;

    public OrderEventPublisher(ITopicProducer<Guid, OrderPaidEvent> producer)
    {
        _producer = producer;
    }

    public Task PublishOrderPaidAsync(OrderPaidEvent @event, CancellationToken ct)
    {
        return _producer.Produce(@event.OrderId, @event, ct);
    }
}

