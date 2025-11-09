using MediatR;
using PriceUpdater.Infrastructure;
using PriceUpdater.Application.Prices.Commands;
using PriceUpdater.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(typeof(CreatePriceCommand).Assembly);

builder.WebHost.UseUrls("http://localhost:5090");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PriceDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();


