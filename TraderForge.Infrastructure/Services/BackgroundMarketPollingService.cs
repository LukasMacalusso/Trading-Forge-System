using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Services;
using TraderForge.Domain.Repositories;

namespace TraderForge.Infrastructure.Services;

public class BackgroundMarketPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMarketDataProvider _dataProvider;
    private readonly IMemoryCache _cache;
    private readonly IMarketDataBroadcaster _broadcaster;
    
    public BackgroundMarketPollingService(
        IServiceScopeFactory scopeFactory, 
        IMarketDataProvider dataProvider, 
        IMemoryCache cache,
        IMarketDataBroadcaster broadcaster)
    {
        _scopeFactory = scopeFactory;
        _dataProvider = dataProvider;
        _cache = cache;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecutePollingCycle(stoppingToken);
        }
    }
    
    private async Task ExecutePollingCycle(CancellationToken stoppingToken)
    {
        try
        {
            var allPrices = await _dataProvider.GetPricesAsync();
            SaveToCache(allPrices);
            await SaveToDatabase(allPrices);
            await _broadcaster.BroadCastPricesAsync(allPrices, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Polling Error]: {ex.Message}");
        }
    }
    
    private void SaveToCache(Dictionary<string, decimal> allPrices)
    {
        _cache.Set(CacheKeys.MarketPrices, allPrices, TimeSpan.FromMinutes(1));
    }

    private async Task SaveToDatabase(Dictionary<string, decimal> allPrices)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMarketAssetRepository>();

        foreach (var symbol in SupportedAssets.Symbols)
        {
            if (allPrices.TryGetValue(symbol, out var price))
            {
                var asset = new MarketAsset
                {
                    Symbol = symbol, 
                    Name = symbol, 
                    CurrentPrice = price, 
                    LastUpdated = DateTime.UtcNow
                };

                await repository.AddAsync(asset);
            }
        }
    }
}