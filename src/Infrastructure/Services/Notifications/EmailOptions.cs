namespace Infrastructure.Services.Notifications;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 25;
    public bool UseSsl { get; init; } = true;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string FromAddress { get; init; } = "no-reply@dotnet-crud-api.local";
    public string FromName { get; init; } = "Dotnet CRUD API";
    public string NotificationRecipient { get; init; } = "sami-n-roses@hotmail.com";
}


