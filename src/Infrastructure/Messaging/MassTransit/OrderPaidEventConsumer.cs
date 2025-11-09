using System;
using Application.IntegrationEvents.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.MassTransit;

internal sealed class OrderPaidEventConsumer : IConsumer<OrderPaidEvent>
{
    private readonly IOrderPaidEventProcessor _processor;
    private readonly ILogger<OrderPaidEventConsumer> _logger;

    public OrderPaidEventConsumer(IOrderPaidEventProcessor processor, ILogger<OrderPaidEventConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPaidEvent> context)
    {
        try
        {
            await _processor.HandleAsync(context.Message, context.CancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Processed OrderPaid event for OrderId {OrderId} and ProductId {ProductId}",
                context.Message.OrderId,
                context.Message.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process OrderPaid message for OrderId {OrderId}",
                context.Message.OrderId);
            throw;
        }
    }
}


