using System.Net;
using System.Net.Mail;
using System.Text;
using Application.IntegrationEvents.Products;
using Application.Services.Notifications;
using Domain.Entitites.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Notifications;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendProductPriceChangedNotificationAsync(ProductPriceChangedEvent priceChangedEvent, IEnumerable<Order> affectedOrders, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning("Email notification skipped: SMTP host is not configured.");
            return;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl
        };

        client.UseDefaultCredentials = false;

        if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = $"Product price updated - Product {priceChangedEvent.ProductId}",
            Body = BuildEmailBody(priceChangedEvent, affectedOrders),
            IsBodyHtml = false
        };

        message.To.Add(_options.NotificationRecipient);

        try
        {
            await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Sent product price change notification email for product {ProductId} to {Recipient}.",
                priceChangedEvent.ProductId,
                _options.NotificationRecipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send product price change notification email for product {ProductId}.",
                priceChangedEvent.ProductId);

            throw new InvalidOperationException(
                string.Format(
                    "Failed to send product price change notification email for product '{0}' to '{1}'.",
                    priceChangedEvent.ProductId,
                    _options.NotificationRecipient),
                ex);
        }
    }

    private static string BuildEmailBody(ProductPriceChangedEvent priceChangedEvent, IEnumerable<Order> affectedOrders)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Product price update notification");
        builder.AppendLine("---------------------------------");
        builder.AppendLine($"Product ID      : {priceChangedEvent.ProductId}");
        builder.AppendLine($"Previous Price  : {priceChangedEvent.OldPrice:C}");
        builder.AppendLine($"New Price       : {priceChangedEvent.NewPrice:C}");
        builder.AppendLine($"Updated At (UTC): {priceChangedEvent.UpdatedAtUtc:O}");
        builder.AppendLine();
        builder.AppendLine("Affected pending orders:");

        foreach (Order order in affectedOrders)
        {
            builder.AppendLine($" - Order ID: {order.Id}");
        }

        return builder.ToString();
    }
}


