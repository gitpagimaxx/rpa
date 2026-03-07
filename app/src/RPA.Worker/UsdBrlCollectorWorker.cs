using Microsoft.Extensions.Options;
using Rpa.Application.UseCases.CollectUsdBrl;
using Rpa.Worker.Options;

namespace Rpa.Worker;

public sealed class UsdBrlCollectorWorker(
    ILogger<UsdBrlCollectorWorker> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<WorkerOptions> options)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, options.Value.IntervalSeconds));
        logger.LogInformation("Worker started. Interval={IntervalSeconds}s", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var started = DateTimeOffset.UtcNow;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<CollectUsdBrlHandler>();
                var rate = await useCase.HandleAsync(stoppingToken);

                logger.LogInformation(
                    "Collected {Base}/{Quote} rate={Rate} at={AtUtc} source={Source}",
                    rate.BaseCurrency, rate.QuoteCurrency, rate.Rate, rate.CollectedAtUtc, rate.Source);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Collect failed");
            }

            var delay = interval - (DateTimeOffset.UtcNow - started);
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            try { await Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }
    }
}