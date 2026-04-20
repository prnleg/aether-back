using Aether.Domain.Enums;
using Aether.Domain.Interfaces;
using Aether.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aether.Infrastructure.BackgroundServices;

public class PriceEnrichmentWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceEnrichmentWorker> _logger;
    private readonly decimal _priceThreshold;
    private readonly int _batchSize;
    private readonly TimeSpan _runInterval;
    private readonly TimeSpan _delayBetweenItems;

    public PriceEnrichmentWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<PriceEnrichmentWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _priceThreshold = configuration.GetValue<decimal>("Discovery:PriceThreshold", 0.05m);
        _batchSize = configuration.GetValue<int>("Discovery:PriceEnrichment:BatchSize", 20);
        _runInterval = TimeSpan.FromMinutes(configuration.GetValue<int>("Discovery:PriceEnrichment:IntervalMinutes", 5));
        _delayBetweenItems = TimeSpan.FromMilliseconds(configuration.GetValue<int>("Discovery:PriceEnrichment:DelayBetweenItemsMs", 1500));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnrichBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PriceEnrichmentWorker encountered an unexpected error.");
            }

            await Task.Delay(_runInterval, stoppingToken);
        }
    }

    private async Task EnrichBatchAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var discoveryRepo = scope.ServiceProvider.GetRequiredService<IDiscoveryRepository>();
        var steamProvider = scope.ServiceProvider.GetRequiredService<ISteamInventoryProvider>();

        var items = await discoveryRepo.GetUnpricedItemsAsync(_batchSize, ct);
        if (items.Count == 0) return;

        _logger.LogInformation("PriceEnrichmentWorker: enriching {Count} unpriced items.", items.Count);

        int enriched = 0, failed = 0;

        foreach (var item in items)
        {
            if (ct.IsCancellationRequested) break;

            if (enriched > 0)
                await Task.Delay(_delayBetweenItems, ct);

            decimal? price;
            try
            {
                price = await steamProvider.FetchPriceAsync(item.AppId, item.MarketHashName, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Price fetch failed for {MarketHashName}, will retry next cycle.", item.MarketHashName);
                failed++;
                continue;
            }

            if (price.HasValue)
            {
                item.UpdatePrice(new Money(price.Value, "USD"));

                // Apply threshold: cheap items → Available (hidden from review feed, not rejected)
                if (price.Value < _priceThreshold && item.Status == AssetStatus.PendingApproval)
                    item.MarkAvailable();
            }
            // If price is null (item not on market), leave MarketPrice null but keep PendingApproval
            // so the user can still manually review it

            await discoveryRepo.UpdateAsync(item, ct);
            enriched++;
        }

        _logger.LogInformation("PriceEnrichmentWorker: enriched {Enriched}, failed {Failed} (will retry next cycle).",
            enriched, failed);
    }
}
