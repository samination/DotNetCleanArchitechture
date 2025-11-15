using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using DotnetcleanArchMcpServer.Tools;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DotnetcleanArchMcpServer.Resources;

[McpServerResourceType]
public sealed class CategoryResources
{
    private readonly CategoryTools _categoryTools;

    public CategoryResources(CategoryTools categoryTools)
    {
        _categoryTools = categoryTools;
    }

    [McpServerResource(Name = "resource://categories/summary")]
    [Description("Short dashboard-style summary of categories fetched via the CRUD API.")]
    public async Task<ResourceContents> GetSummaryAsync(
        [Description("Optional keyword to highlight while summarizing.")] string? focus = null,
        CancellationToken cancellationToken = default)
    {
        var firstPage = await _categoryTools.ListCategoriesAsync(
            bearerToken: null,
            pageNumber: 1,
            pageSize: 25,
            cancellationToken);

        var total = firstPage.GetProperty("totalCount").GetInt32();
        var items = firstPage.GetProperty("items");
        var sb = new StringBuilder()
            .AppendLine($"Total categories: {total}")
            .AppendLine($"Showing first {items.GetArrayLength()} entries.")
            .AppendLine();

        if (!string.IsNullOrWhiteSpace(focus))
        {
            sb.AppendLine($"Focus keyword: {focus}");
        }

        foreach (var element in items.EnumerateArray())
        {
            var name = element.GetProperty("name").GetString();
            var description = element.TryGetProperty("description", out var desc)
                ? desc.GetString()
                : null;
            sb.AppendLine($"- {name}: {description}");
        }

        return new TextResourceContents
        {
            Text = sb.ToString()
        };
    }
}

