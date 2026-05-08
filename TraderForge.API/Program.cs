using Microsoft.EntityFrameworkCore;
using TraderForge.API.Hubs;
using TraderForge.API.Services;
using TraderForge.Application.Handlers;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Domain.Services;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddTransient<GetMarketPricesQueryHandler>();

builder.Services.AddHttpClient<IMarketDataProvider, BinanceMarketProvider>();
builder.Services.AddSingleton<IMarketService, CachedMarketService>();
builder.Services.AddHostedService<BackgroundMarketPollingService>();
builder.Services.AddSingleton<IMarketDataBroadcaster, SignalRMarketDataBroadcaster>();
builder.Services.AddSignalR();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketAssetRepository, MarketAssetRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(_ => true) // Adjust this for production security
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // REQUIRED FOR SIGNALR
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<MarketDataHub>("/hubs/market");

app.Run();