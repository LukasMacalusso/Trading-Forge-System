using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using TraderForge.Domain.Events;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Services;

public class StrategyEngineService : BackgroundService, IStrategyEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMarketDataEventBus _eventBus;
    private readonly ConcurrentDictionary<Guid, BotGraphRunner> _activeStrategies = new();

    public StrategyEngineService(IServiceScopeFactory scopeFactory, IMarketDataEventBus eventBus)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadActiveStrategiesAsync(stoppingToken);

        await foreach (var priceEvent in _eventBus.ReadAllAsync(stoppingToken))
        {
            ReactToPriceEvent(priceEvent, stoppingToken);
        }
    }

    private async Task LoadActiveStrategiesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var strategyRepo = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
        var activeStrategies = await strategyRepo.GetActiveWithEngineRunningAsync();

        foreach (var strategy in activeStrategies)
        {
            var runner = new BotGraphRunner(strategy, _scopeFactory);
            _activeStrategies[strategy.Id] = runner;
        }
    }

    private void ReactToPriceEvent(MarketPriceEvent priceEvent, CancellationToken ct)
    {
        foreach (var (_, runner) in _activeStrategies)
        {
            if (!runner.HandlesSymbol(priceEvent.Symbol))
                continue;

            foreach (var triggerNodeId in runner.GetTriggersForSymbol(priceEvent.Symbol))
            {
                _ = runner.ExecuteAsync(priceEvent, triggerNodeId, ct);
            }
        }
    }

    public async Task StartStrategyAsync(Guid strategyId)
    {
        using var scope = _scopeFactory.CreateScope();
        var strategyRepo = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
        var executionRepo = scope.ServiceProvider.GetRequiredService<IStrategyExecutionRepository>();

        var strategy = await strategyRepo.GetByIdWithGraphAsync(strategyId);
        if (strategy == null) return;

        strategy.StartEngine();

        var existingExecution = await executionRepo.GetActiveByStrategyIdAsync(strategyId);
        if (existingExecution != null)
        {
            existingExecution.Pause();
            await executionRepo.SaveChangesAsync();
        }

        var runner = new BotGraphRunner(strategy, _scopeFactory);
        _activeStrategies[strategyId] = runner;

        await strategyRepo.SaveChangesAsync();
    }

    public async Task StopStrategyAsync(Guid strategyId)
    {
        if (_activeStrategies.TryRemove(strategyId, out var runner))
        {
            runner.Dispose();
        }

        using var scope = _scopeFactory.CreateScope();
        var strategyRepo = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
        var executionRepo = scope.ServiceProvider.GetRequiredService<IStrategyExecutionRepository>();

        var strategy = await strategyRepo.GetByIdAsync(strategyId);
        if (strategy == null) return;

        strategy.StopEngine();

        var execution = await executionRepo.GetActiveByStrategyIdAsync(strategyId);
        if (execution != null)
        {
            execution.Pause();
            await executionRepo.SaveChangesAsync();
        }

        await strategyRepo.SaveChangesAsync();
    }

    public bool IsStrategyRunning(Guid strategyId) => _activeStrategies.ContainsKey(strategyId);
}
