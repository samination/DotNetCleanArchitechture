namespace DotnetcleanArchMcpServer.Options;

public sealed class CrudApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;

    public string InternalKey { get; set; } = string.Empty;
}

