using Application.IntegrationEvents.Products;
using Application.Services.Common;
using Domain.Entitites.Orders;

namespace Application.Services.Notifications;

public interface IEmailService : ISingletonService
{
    Task SendProductPriceChangedNotificationAsync(ProductPriceChangedEvent priceChangedEvent, IEnumerable<Order> affectedOrders, CancellationToken cancellationToken);
}


