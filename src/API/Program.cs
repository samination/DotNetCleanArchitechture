using API.Configurations;
using API.Filters;
using API.Validation.Auth;
using Data.Database;
using Infrastructure;
using Infrastructure.Common;
using MediatR;
using Serilog;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

StaticLogger.EnsureLoggerIsInitialized();
Log.Information("Starting Web API...");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.Host.AddConfigurations();

// Add controllers with some extra options
builder.Services.AddControllers(options =>
{
    options.Filters.Add<StringNormalizationActionFilter>();
}).AddJsonOptions(x =>
{
    // Serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // Ignore omitted parameters on models to enable optional params (e.g. User update)
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddProblemDetails();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssembly(typeof(LoginRequestDtoValidator).Assembly);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Validation");

        var validationErrors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => new
            {
                Field = kvp.Key,
                Errors = kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            })
            .ToArray();

        logger.LogWarning(
            "Validation failed for request {Path}. TraceId={TraceId}. Errors={Errors}",
            context.HttpContext.Request.Path,
            context.HttpContext.TraceIdentifier,
            validationErrors);

        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problemDetails);
    };
});

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add mediator and mapper
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Endpoint explorer and Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the application
var app = builder.Build();

// Seed database fixtures
await app.Services.SeedDatabaseAsync();

// Configure the HTTP request pipeline.
// Use Swagger and it's UI
app.UseSwagger();
app.UseSwaggerUI();

// Use infrastructure services: middlware, etc...
app.UseInfrastructure(builder.Configuration);

// Enable HTTPS Redirection
app.UseHttpsRedirection();

// Enable authentication
app.UseAuthentication();

// Enable authorization capabilities
app.UseAuthorization();

// Map controllers
app.MapControllers();


// All done - let's run the app / API
Log.Information("The Web API is now ready to accept incoming requests!");
await app.RunAsync();
