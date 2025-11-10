using Application.Services.Common;
using Application.Services.Products;

namespace Application.IntegrationEvents.Orders;

public class OrderPaidEventProcessor : IOrderPaidEventProcessor, IScopedService
{
    private readonly IProductService _productService;

    public OrderPaidEventProcessor(IProductService productService)
    {
        _productService = productService;
    }

    public async Task HandleAsync(OrderPaidEvent orderPaidEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(orderPaidEvent);

        await _productService.DecrementStockAsync(orderPaidEvent.ProductId, 1, cancellationToken)
            .ConfigureAwait(false);
    }
}

