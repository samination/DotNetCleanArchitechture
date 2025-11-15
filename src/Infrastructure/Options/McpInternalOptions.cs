namespace Infrastructure.Options;

public sealed class McpInternalOptions
{
    public const string SectionName = "McpInternal";

    public string SharedKey { get; set; } = string.Empty;
}

