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

public class BotGraphRunner : IDisposable
{
    private readonly Strategy _strategy;
    private readonly Dictionary<Guid, BotNode> _nodes;
    private readonly Dictionary<Guid, List<BotEdge>> _outgoingEdges;
    private readonly Dictionary<string, List<Guid>> _triggersBySymbol;
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _disposed;

    public BotGraphRunner(Strategy strategy, IServiceScopeFactory scopeFactory)
    {
        _strategy = strategy;
        _scopeFactory = scopeFactory;
        _nodes = strategy.BotNodes.ToDictionary(n => n.Id);
        _outgoingEdges = strategy.BotEdges
            .GroupBy(e => e.SourceNodeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        _triggersBySymbol = strategy.BotNodes
            .Where(n => n.Type == BotNodeType.Trigger)
            .Select(n =>
            {
                using var doc = JsonDocument.Parse(n.Config);
                var symbol = doc.RootElement.TryGetProperty("symbol", out var s)
                    ? s.GetString()
                    : null;
                return (Symbol: symbol, NodeId: n.Id);
            })
            .Where(x => x.Symbol != null)
            .GroupBy(x => x.Symbol!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.NodeId).ToList());
    }

    public bool HandlesSymbol(string symbol)
        => _triggersBySymbol.ContainsKey(symbol);

    public IReadOnlyList<Guid> GetTriggersForSymbol(string symbol)
        => _triggersBySymbol.GetValueOrDefault(symbol) ?? [];

    public async Task ExecuteAsync(MarketPriceEvent priceEvent, Guid triggerNodeId, CancellationToken ct)
    {
        if (_disposed) return;

        using var scope = _scopeFactory.CreateScope();
        var executionRepo = scope.ServiceProvider.GetRequiredService<IStrategyExecutionRepository>();

        var execution = new StrategyExecution(_strategy.Id);
        await executionRepo.AddAsync(execution);

        try
        {
            execution.UpdateProgress(triggerNodeId, null);

            var flag = new Dictionary<string, object>
            {
                ["symbol"] = priceEvent.Symbol,
                ["price"] = priceEvent.Price,
                ["timestamp"] = priceEvent.Timestamp
            };

            await WalkGraphAsync(triggerNodeId, flag, execution, executionRepo, ct);

            execution.Complete();
        }
        catch (Exception)
        {
            execution.Fail();
        }
        finally
        {
            executionRepo.Update(execution);
            await executionRepo.SaveChangesAsync();
        }
    }

    private async Task WalkGraphAsync(
        Guid currentNodeId,
        Dictionary<string, object> flag,
        StrategyExecution execution,
        IStrategyExecutionRepository executionRepo,
        CancellationToken ct)
    {
        var currentNode = _nodes.GetValueOrDefault(currentNodeId);
        if (currentNode == null) return;

        execution.UpdateProgress(currentNodeId, JsonSerializer.Serialize(flag));
        executionRepo.Update(execution);
        await executionRepo.SaveChangesAsync();

        switch (currentNode.Type)
        {
            case BotNodeType.Trigger:
                await FollowPortAsync(currentNodeId, NodePort.Out, flag, execution, executionRepo, ct);
                break;

            case BotNodeType.Condition:
                var conditionResult = await EvaluateConditionAsync(currentNode.Config, flag);
                var port = conditionResult ? NodePort.True : NodePort.False;
                flag["condition_result"] = conditionResult;
                await FollowPortAsync(currentNodeId, port, flag, execution, executionRepo, ct);
                break;

            case BotNodeType.Action:
                await ExecuteActionAsync(currentNode.Config, flag, _scopeFactory);
                break;

            case BotNodeType.Notification:
                await HandleNotificationAsync(currentNodeId, flag, _scopeFactory, ct);
                break;
        }
    }

    private async Task FollowPortAsync(
        Guid sourceNodeId,
        NodePort port,
        Dictionary<string, object> flag,
        StrategyExecution execution,
        IStrategyExecutionRepository executionRepo,
        CancellationToken ct)
    {
        if (_outgoingEdges.TryGetValue(sourceNodeId, out var edges))
        {
            var edge = edges.FirstOrDefault(e => e.SourcePort == port);
            if (edge != null)
            {
                await WalkGraphAsync(edge.TargetNodeId, flag, execution, executionRepo, ct);
            }
        }
    }

    private async Task HandleNotificationAsync(
        Guid currentNodeId,
        Dictionary<string, object> flag,
        IServiceScopeFactory scopeFactory,
        CancellationToken ct)
    {
        if (!_outgoingEdges.TryGetValue(currentNodeId, out var edges)) return;

        var nextEdge = edges.FirstOrDefault(e => e.SourcePort == NodePort.Out);
        if (nextEdge == null) return;

        var nextNode = _nodes.GetValueOrDefault(nextEdge.TargetNodeId);
        if (nextNode == null || nextNode.Type != BotNodeType.Action) return;

        using var scope = scopeFactory.CreateScope();
        var pendingRepo = scope.ServiceProvider.GetRequiredService<IPendingOperationRepository>();

        try
        {
            using var doc = JsonDocument.Parse(nextNode.Config);
            var root = doc.RootElement;
            var actionType = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
            var symbol = (flag.GetValueOrDefault("symbol") as string) ?? "";
            var price = flag.TryGetValue("price", out var p) && decimal.TryParse(p?.ToString(), out var dp) ? dp : 0m;
            var quantity = root.TryGetProperty("quantity", out var q) ? q.GetDecimal() : 0m;

            if (string.IsNullOrEmpty(symbol) || price <= 0 || quantity <= 0) return;

            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            var pendingOp = new PendingOperation(
                Guid.NewGuid(),
                _strategy.PortfolioId,
                _strategy.Id,
                _strategy.Name,
                symbol,
                actionType,
                quantity,
                price,
                expiresAt);

            await pendingRepo.AddAsync(pendingOp);
            await pendingRepo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
            logger?.LogWarning(ex, "Failed to create PendingOperation for strategy {StrategyId}", _strategy.Id);
        }
    }

    private Task<bool> EvaluateConditionAsync(string configJson, Dictionary<string, object> flag)
    {
        try
        {
            using var doc = JsonDocument.Parse(configJson);
            var root = doc.RootElement;
            var indicator = root.TryGetProperty("indicator", out var i) ? i.GetString() ?? "price" : "price";
            var operatorStr = root.TryGetProperty("operator", out var o) ? o.GetString() ?? "greater_than" : "greater_than";
            var comparisonValue = root.TryGetProperty("value", out var v) ? v.GetDecimal() : 0m;

            var currentValue = indicator switch
            {
                "price" when flag.TryGetValue("price", out var p) && decimal.TryParse(p?.ToString(), out var dp) => dp,
                _ => 0m
            };

            return Task.FromResult(operatorStr switch
            {
                "greater_than" => currentValue > comparisonValue,
                "less_than" => currentValue < comparisonValue,
                "equal_to" => currentValue == comparisonValue,
                "greater_or_equal" => currentValue >= comparisonValue,
                "less_or_equal" => currentValue <= comparisonValue,
                "cross_above" => currentValue > comparisonValue,
                "cross_below" => currentValue < comparisonValue,
                _ => currentValue > comparisonValue
            });
        }
        catch (Exception ex)
        {
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
            logger?.LogWarning(ex, "Condition evaluation failed. Config: {Config}", configJson);
            return Task.FromResult(true);
        }
    }

    private async Task ExecuteActionAsync(string configJson, Dictionary<string, object> flag, IServiceScopeFactory scopeFactory)
    {
        try
        {
            using var doc = JsonDocument.Parse(configJson);
            var root = doc.RootElement;
            var type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";

            if (type is "buy" or "sell")
            {
                await ExecuteTradeAsync(type, root, flag, scopeFactory);
            }
        }
        catch (Exception ex)
        {
            using var logScope = scopeFactory.CreateScope();
            var logger = logScope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
            logger?.LogWarning(ex, "Action execution failed for strategy {StrategyId}. Config: {Config}",
                _strategy.Id, configJson);
        }
    }

    private async Task ExecuteTradeAsync(string type, JsonElement config, Dictionary<string, object> flag, IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();

        var symbol = (flag.GetValueOrDefault("symbol") as string) ?? "";
        var price = flag.TryGetValue("price", out var p) && decimal.TryParse(p?.ToString(), out var parsedPrice) ? parsedPrice : 0m;
        var quantity = config.TryGetProperty("quantity", out var q) ? q.GetDecimal() : 0m;

        if (string.IsNullOrEmpty(symbol) || price <= 0 || quantity <= 0)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
            logger?.LogWarning("Invalid {Type} params for strategy {Sid}: Symbol={Sym}, Price={Prc}, Qty={Qty}",
                type, _strategy.Id, symbol, price, quantity);
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var commissionService = scope.ServiceProvider.GetRequiredService<ICommissionService>();

        var portfolio = await db.Portfolios
            .Include(p => p.Positions)
            .Include(p => p.Orders)
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == _strategy.PortfolioId);

        if (portfolio == null)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
            logger?.LogWarning("Portfolio {Pid} not found for {Type} action (strategy {Sid})",
                _strategy.PortfolioId, type, _strategy.Id);
            return;
        }

        if (type == "buy")
            portfolio.BuyPosition(symbol, quantity, price, commissionService);
        else
            portfolio.SellPosition(symbol, quantity, price, commissionService);

        await db.SaveChangesAsync();
        var logger2 = scope.ServiceProvider.GetService<ILogger<BotGraphRunner>>();
        logger2?.LogInformation("Bot {Type} executed: {Symbol} x {Qty} @ {Price} for strategy {Sid}",
            type, symbol, quantity, price, _strategy.Id);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
