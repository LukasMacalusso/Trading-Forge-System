using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TraderForge.Domain.Repositories;

namespace TraderForge.Infrastructure.Services;
public class TrialExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrialExpirationService> _logger;
    public TrialExpirationService(IServiceProvider serviceProvider, ILogger<TrialExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredTrials(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
    private async Task ProcessExpiredTrials(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var traderRepository = scope.ServiceProvider.GetRequiredService<ITraderRepository>();
            var expiredTraders = await traderRepository.GetExpiredTrialsAsync();
            foreach (var trader in expiredTraders)
                trader.ClearSubscriptionPlan();
            await traderRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing expired free trials.");
        }
    }
}
