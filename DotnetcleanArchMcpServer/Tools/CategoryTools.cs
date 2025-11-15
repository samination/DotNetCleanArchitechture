using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace DotnetcleanArchMcpServer.Tools;

[McpServerToolType]
public sealed class CategoryTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoryTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Fetches paginated categories from the CRUD API.")]
    public async Task<JsonElement> ListCategoriesAsync(
        [Description("Optional JWT bearer token without the 'Bearer ' prefix.")] string? bearerToken = null,
        [Description("1-based page number for pagination.")] int pageNumber = 1,
        [Description("Number of items to return per page.")] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var httpClient = _httpClientFactory.CreateClient("CrudApi");
        var requestUri = $"api/Category/get?pageNumber={pageNumber}&pageSize={pageSize}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddAuthorizationHeader(request, bearerToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        return await ReadJsonElementAsync(response, cancellationToken);
    }

    [McpServerTool]
    [Description("Creates a category by forwarding the request to the CRUD API.")]
    public async Task<JsonElement> CreateCategoryAsync(
        [Description("JWT bearer token without the 'Bearer ' prefix. Required.")] string bearerToken,
        [Description("Category name.")] string name,
        [Description("Category description.")] string description,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var httpClient = _httpClientFactory.CreateClient("CrudApi");

        var payload = JsonSerializer.Serialize(new
        {
            Name = name,
            Description = description ?? string.Empty
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/Category/add")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        AddAuthorizationHeader(request, bearerToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        return await ReadJsonElementAsync(response, cancellationToken);
    }

    private static void AddAuthorizationHeader(HttpRequestMessage request, string? bearerToken)
    {
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }

    private static async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var reason = string.IsNullOrWhiteSpace(errorBody) ? response.ReasonPhrase : errorBody;
        throw new InvalidOperationException($"CRUD API request failed ({(int)response.StatusCode}): {reason}");
    }

    private static async Task<JsonElement> ReadJsonElementAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.Clone();
    }
}

