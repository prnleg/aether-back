using Aether.Application.Features.Discovery;
using Aether.Application.Features.Portfolio;
using Aether.Domain.Common;
using Aether.Domain.Entities;
using Aether.Domain.Enums;
using Aether.Domain.Interfaces;
using Aether.Domain.Specifications;
using Aether.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aether.Infrastructure.Services;

public class DiscoveryService : IDiscoveryService
{
    private readonly IDiscoveryRepository _discoveryRepo;
    private readonly IUserProfileRepository _userProfileRepo;
    private readonly ISteamInventoryProvider _steamProvider;
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly ILogger<DiscoveryService> _logger;
    private readonly int _maxItemsPerSync;

    public DiscoveryService(
        IDiscoveryRepository discoveryRepo,
        IUserProfileRepository userProfileRepo,
        ISteamInventoryProvider steamProvider,
        IPortfolioRepository portfolioRepo,
        IConfiguration configuration,
        ILogger<DiscoveryService> logger)
    {
        _discoveryRepo = discoveryRepo;
        _userProfileRepo = userProfileRepo;
        _steamProvider = steamProvider;
        _portfolioRepo = portfolioRepo;
        _logger = logger;
        _maxItemsPerSync = configuration.GetValue<int>("Discovery:MaxItemsPerSync", 200);
    }

    public async Task<Result<SyncResultDto>> SyncAsync(SyncInventoryRequest request, Guid userId, CancellationToken ct = default)
    {
        var profile = await _userProfileRepo.GetByUserIdAsync(userId, ct);
        if (profile?.SteamId == null)
            return Result.Failure<SyncResultDto>(Error.Validation("SteamId not set. Update your profile first."));

        int totalAdded = 0, totalUpdated = 0, totalSkipped = 0;

        foreach (var appId in request.AppIds)
        {
            SteamInventoryResult inventory;
            try
            {
                inventory = await _steamProvider.FetchInventoryAsync(profile.SteamId, appId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inventory fetch failed for appId {AppId}", appId);
                return Result.Failure<SyncResultDto>(Error.Validation($"Failed to fetch Steam inventory for appId {appId}: {ex.Message}"));
            }

            var existingIds = await _discoveryRepo.GetExternalIdsByUserAndAppIdAsync(userId, appId, ct);

            var toAdd = new List<DiscoveryItem>();
            int newItemCount = 0;

            foreach (var steamItem in inventory.Items)
            {
                if (existingIds.Contains(steamItem.ExternalId))
                {
                    totalUpdated++;
                    continue;
                }

                if (newItemCount >= _maxItemsPerSync)
                {
                    totalSkipped++;
                    continue;
                }

                // No price fetching here — PriceEnrichmentWorker handles it in background
                toAdd.Add(new DiscoveryItem(
                    steamItem.ExternalId,
                    steamItem.MarketHashName,
                    appId,
                    steamItem.IconUrl,
                    steamItem.RawJson,
                    AssetStatus.PendingApproval,
                    null,
                    userId));

                newItemCount++;
            }

            if (toAdd.Count > 0)
            {
                await _discoveryRepo.AddRangeAsync(toAdd, ct);
                totalAdded += toAdd.Count;
            }
        }

        return Result.Success(new SyncResultDto(totalAdded, totalUpdated, totalSkipped));
    }

    public async Task<Result<IReadOnlyList<DiscoveryItemDto>>> GetItemsAsync(GetDiscoveryItemsRequest request, Guid userId, CancellationToken ct = default)
    {
        AssetStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AssetStatus>(request.Status, true, out var parsed))
            status = parsed;

        var items = await _discoveryRepo.GetByUserAndStatusAsync(userId, status, request.Page, request.PageSize, ct);
        var dtos = items.Select(MapToDto).ToList();
        return Result.Success<IReadOnlyList<DiscoveryItemDto>>(dtos);
    }

    public async Task<Result<AssetDto>> ApproveAsync(Guid itemId, Guid userId, CancellationToken ct = default)
    {
        var item = await _discoveryRepo.FindByIdAsync(itemId, ct);
        if (item == null || item.UserId != userId)
            return Result.Failure<AssetDto>(Error.NotFound($"Discovery item {itemId} not found."));

        if (item.Status == AssetStatus.Approved)
            return Result.Failure<AssetDto>(Error.Conflict("Item is already approved."));

        // Get or create default portfolio
        var portfolios = (await _portfolioRepo.ListAsync(new PortfoliosByUserIdSpec(userId))).ToList();
        Portfolio portfolio;
        if (portfolios.Count > 0)
        {
            portfolio = portfolios[0];
        }
        else
        {
            portfolio = new Portfolio("My Portfolio", userId);
            await _portfolioRepo.AddAsync(portfolio);
        }

        var acquisitionPrice = Money.Zero("USD");
        var floorPrice = item.MarketPrice ?? Money.Zero("USD");

        var asset = new SteamSkinAsset(
            item.MarketHashName,
            DateTime.UtcNow,
            acquisitionPrice,
            item.MarketHashName,
            item.AppId);

        asset.UpdateFloorPrice(floorPrice);

        portfolio.AddAsset(asset);
        await _portfolioRepo.UpdateAsync(portfolio);

        item.Approve();
        await _discoveryRepo.UpdateAsync(item, ct);

        return Result.Success(new AssetDto
        {
            Id = asset.Id,
            Name = asset.Name,
            AssetType = "SteamSkin",
            AcquisitionDate = asset.AcquisitionDate,
            AcquisitionPrice = asset.AcquisitionPrice.Amount,
            AcquisitionCurrency = asset.AcquisitionPrice.Currency,
            CurrentFloorPrice = asset.CurrentFloorPrice.Amount,
            CurrentFloorCurrency = asset.CurrentFloorPrice.Currency,
            LastUpdated = asset.LastUpdated,
            MarketHashName = asset.MarketHashName,
            AppId = asset.AppId
        });
    }

    public async Task<Result> RejectAsync(Guid itemId, Guid userId, CancellationToken ct = default)
    {
        var item = await _discoveryRepo.FindByIdAsync(itemId, ct);
        if (item == null || item.UserId != userId)
            return Result.Failure(Error.NotFound($"Discovery item {itemId} not found."));

        item.Reject();
        await _discoveryRepo.UpdateAsync(item, ct);
        return Result.Success();
    }

    private static DiscoveryItemDto MapToDto(DiscoveryItem item) => new(
        item.Id,
        item.ExternalId,
        item.MarketHashName,
        item.AppId,
        item.IconUrl,
        item.Status.ToString(),
        item.MarketPrice?.Amount,
        item.MarketPrice?.Currency,
        item.LastSeenAt,
        item.CreatedAt);
}
