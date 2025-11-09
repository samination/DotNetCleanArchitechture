using System.Collections.Concurrent;
using System.Reflection;
using Application.Features.Orders.Commands;
using Application.Features.Orders.Handlers.Update;
using Application.IntegrationEvents.Orders;
using Application.Services.Orders;
using Domain.Entitites;
using Domain.Entitites.Orders;
using Infrastructure.Messaging.MassTransit;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CrudApi.IntegrationTests.Orders;

public class OrderPaidEventIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private BusHandle? _busHandle;
    private readonly OrderPaidEventSink _sink;
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public OrderPaidEventIntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(new KafkaOptions
        {
            BootstrapServers = "localhost:9092",
            ConsumerGroupId = "integration-test-consumer",
            OrderPaidTopic = "order-paid",
            PriceUpdatedTopic = "price-updated"
        });

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<OrderPaidEventConsumer>();

            busConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.ReceiveEndpoint("order-paid-test", endpoint =>
                {
                    endpoint.ConfigureConsumer<OrderPaidEventConsumer>(context);
                });
            });
        });

        services.AddScoped<IOrderEventPublisher, TestOrderEventPublisher>();

        services.AddScoped<IOrderService>(_ =>
            new TestOrderService(_orderId, _productId));

        services.AddScoped<PayOrderHandler>();

        _sink = new OrderPaidEventSink();
        services.AddSingleton(_sink);
        services.AddScoped<IOrderPaidEventProcessor, TestOrderPaidEventProcessor>();

        _provider = services.BuildServiceProvider(validateScopes: true);
    }

    public async Task InitializeAsync()
    {
        var busControl = _provider.GetRequiredService<IBusControl>();
        _busHandle = await busControl.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_busHandle is not null)
        {
            await _busHandle.StopAsync();
        }

        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task PayOrder_ShouldPublishAndConsumeOrderPaidEvent()
    {
        using var scope = _provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<PayOrderHandler>();

        var command = new PayOrderCommand(_orderId);

        Order order = await handler.Handle(command, CancellationToken.None);

        order.PaymentStatus.ShouldBe(PaymentStatus.Paid);
        order.ProductId.ShouldBe(_productId);

        OrderPaidEvent processedEvent = await _sink.WaitAsync(TimeSpan.FromSeconds(5));

        processedEvent.OrderId.ShouldBe(order.Id);
        processedEvent.ProductId.ShouldBe(_productId);
    }

    private sealed class TestOrderEventPublisher : IOrderEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public TestOrderEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishOrderPaidAsync(OrderPaidEvent @event, CancellationToken ct)
        {
            return _publishEndpoint.Publish(@event, ct);
        }
    }

    private sealed class TestOrderService : IOrderService
    {
        private readonly ConcurrentDictionary<Guid, Order> _orders;

        public TestOrderService(Guid orderId, Guid productId)
        {
            _orders = new ConcurrentDictionary<Guid, Order>();
            _orders[orderId] = CreateOrder(orderId, productId);
        }

        public Task<Order> CreateOrderAsync(Order order, CancellationToken ct) =>
            throw new NotImplementedException();

        public Task<Order> GetOrderByIdAsync(Guid orderId, CancellationToken ct) =>
            throw new NotImplementedException();

        public Task<Order> PayOrderAsync(Guid orderId, CancellationToken ct)
        {
            if (!_orders.TryGetValue(orderId, out var order))
            {
                throw new KeyNotFoundException($"Order with id '{orderId}' not found.");
            }

            order.MarkPaid(DateTime.UtcNow);

            return Task.FromResult(order);
        }

        private static Order CreateOrder(Guid id, Guid productId)
        {
            var order = new Order(productId);

            SetOrderId(order, id);

            return order;
        }

        private static void SetOrderId(Order order, Guid id)
        {
            var property = typeof(Base).GetProperty(nameof(Base.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(order, id);
        }
    }

    private sealed class TestOrderPaidEventProcessor : IOrderPaidEventProcessor
    {
        private readonly OrderPaidEventSink _sink;

        public TestOrderPaidEventProcessor(OrderPaidEventSink sink)
        {
            _sink = sink;
        }

        public Task HandleAsync(OrderPaidEvent orderPaidEvent, CancellationToken cancellationToken)
        {
            _sink.Add(orderPaidEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class OrderPaidEventSink
    {
        private readonly ConcurrentQueue<OrderPaidEvent> _events = new();
        private readonly TaskCompletionSource<OrderPaidEvent> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Add(OrderPaidEvent orderPaidEvent)
        {
            _events.Enqueue(orderPaidEvent);
            _tcs.TrySetResult(orderPaidEvent);
        }

        public async Task<OrderPaidEvent> WaitAsync(TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                return await _tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Timed out waiting for OrderPaidEvent after {timeout}.");
            }
        }
    }
}


