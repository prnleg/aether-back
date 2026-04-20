namespace Aether.Domain.Interfaces;

public interface ISteamInventoryProvider
{
    Task<SteamInventoryResult> FetchInventoryAsync(string steamId, string appId, CancellationToken ct = default);
    Task<decimal?> FetchPriceAsync(string appId, string marketHashName, CancellationToken ct = default);
}

public record SteamInventoryResult(IReadOnlyList<SteamInventoryItem> Items);

public record SteamInventoryItem(
    string ExternalId,      // "{classid}_{instanceid}"
    string MarketHashName,
    string IconUrl,
    string RawJson);
