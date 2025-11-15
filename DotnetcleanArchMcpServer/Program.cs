using System.Net.Http.Headers;
using DotnetcleanArchMcpServer.Options;
using DotnetcleanArchMcpServer.Prompts;
using DotnetcleanArchMcpServer.Resources;
using DotnetcleanArchMcpServer.Tools;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<CrudApiOptions>()
    .Bind(builder.Configuration.GetSection("CrudApi"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "CrudApi:BaseUrl must be provided.")
    .Validate(options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute), "CrudApi:BaseUrl must be an absolute URI.")
    .Validate(options => options.TimeoutSeconds > 0, "CrudApi:TimeoutSeconds must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddHttpClient("CrudApi", (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<CrudApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (!string.IsNullOrWhiteSpace(options.InternalKey))
    {
        client.DefaultRequestHeaders.Remove("X-Mcp-Internal-Key");
        client.DefaultRequestHeaders.Add("X-Mcp-Internal-Key", options.InternalKey);
    }
});

builder.Services.AddMcpServer()
    .AddAuthorizationFilters()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(CategoryTools).Assembly)
    .WithPromptsFromAssembly(typeof(CategoryPrompts).Assembly)
    .WithResourcesFromAssembly(typeof(CategoryResources).Assembly);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapMcp("/mcp");

app.Run();
