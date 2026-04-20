using System.Text.Json;
using Aether.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aether.Infrastructure.Services;

public class SteamInventoryProvider : ISteamInventoryProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SteamInventoryProvider> _logger;

    public SteamInventoryProvider(IHttpClientFactory httpClientFactory, ILogger<SteamInventoryProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Steam");
        _logger = logger;
    }

    public async Task<SteamInventoryResult> FetchInventoryAsync(string steamId, string appId, CancellationToken ct = default)
    {
        const int pageSize = 1000;
        var allItems = new List<SteamInventoryItem>();
        var allDescriptions = new Dictionary<string, JsonElement>();
        var seen = new HashSet<string>();
        string? lastAssetId = null;
        int page = 0;

        while (true)
        {
            var url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/2?l=english&count={pageSize}";
            if (lastAssetId != null)
                url += $"&start_assetid={lastAssetId}";

            HttpResponseMessage response;
            try
            {
                // Steam rate-limits aggressive pagination — small delay after first page
                if (page > 0)
                    await Task.Delay(1500, ct);

                response = await _httpClient.GetAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Steam inventory page {Page} for {SteamId}/{AppId}", page, steamId, appId);
                throw new InvalidOperationException($"Could not reach Steam API: {ex.Message}", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var hint = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => "Inventory is private. Go to Steam → Settings → Privacy → Game Details → set to Public.",
                    System.Net.HttpStatusCode.BadRequest => "Inventory not accessible. It may be private or this game has no items on this account.",
                    System.Net.HttpStatusCode.TooManyRequests => "Steam rate limit hit. Wait a few minutes and try again.",
                    System.Net.HttpStatusCode.Unauthorized => "Steam denied access. Inventory is likely private.",
                    _ => $"Steam returned HTTP {(int)response.StatusCode}."
                };

                _logger.LogWarning("Steam inventory page {Page} returned {Status} for {SteamId}/{AppId}: {Hint}",
                    page, (int)response.StatusCode, steamId, appId, hint);

                throw new InvalidOperationException(hint);
            }

            var json = await response.Content.ReadAsStringAsync(ct);

            if (json.Contains("\"success\":false") || json.Contains("\"success\": false"))
                throw new InvalidOperationException("Inventory not accessible. It may be private or this account has no items for this game.");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Accumulate descriptions across pages
            if (root.TryGetProperty("descriptions", out var descArray))
            {
                foreach (var desc in descArray.EnumerateArray())
                {
                    var classId = desc.TryGetProperty("classid", out var c) ? c.GetString() : null;
                    var instanceId = desc.TryGetProperty("instanceid", out var i) ? i.GetString() : null;
                    if (classId != null && instanceId != null)
                        allDescriptions[$"{classId}_{instanceId}"] = desc.Clone();
                }
            }

            // Accumulate assets
            if (root.TryGetProperty("assets", out var assetsArray))
            {
                foreach (var asset in assetsArray.EnumerateArray())
                {
                    var classId = asset.TryGetProperty("classid", out var c) ? c.GetString() : null;
                    var instanceId = asset.TryGetProperty("instanceid", out var i) ? i.GetString() : null;
                    if (classId == null || instanceId == null) continue;

                    var externalId = $"{classId}_{instanceId}";
                    if (!seen.Add(externalId)) continue;

                    if (!allDescriptions.TryGetValue(externalId, out var desc)) continue;

                    var marketHashName = desc.TryGetProperty("market_hash_name", out var mhn)
                        ? mhn.GetString() ?? string.Empty
                        : string.Empty;

                    var iconUrl = desc.TryGetProperty("icon_url", out var icon)
                        ? $"https://steamcommunity-a.akamaihd.net/economy/image/{icon.GetString()}"
                        : string.Empty;

                    allItems.Add(new SteamInventoryItem(externalId, marketHashName, iconUrl, desc.GetRawText()));
                }
            }

            // Check for more pages
            bool hasMore = root.TryGetProperty("more_items", out var moreEl) && moreEl.GetInt32() == 1;
            if (!hasMore) break;

            lastAssetId = root.TryGetProperty("last_assetid", out var lastEl)
                ? lastEl.GetString()
                : null;

            if (lastAssetId == null) break;

            page++;
            _logger.LogInformation("Steam inventory page {Page} fetched ({Count} items so far), continuing...", page, allItems.Count);
        }

        _logger.LogInformation("Steam inventory fetch complete: {Total} items for {SteamId}/{AppId}", allItems.Count, steamId, appId);
        return new SteamInventoryResult(allItems);
    }

    public async Task<decimal?> FetchPriceAsync(string appId, string marketHashName, CancellationToken ct = default)
    {
        var encoded = Uri.EscapeDataString(marketHashName);
        var url = $"https://steamcommunity.com/market/priceoverview/?appid={appId}&currency=1&market_hash_name={encoded}";

        const int maxRetries = 4;
        var delay = TimeSpan.FromSeconds(3);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? delay;
                    // Steam rarely sends Retry-After, so use exponential backoff floor of 30s
                    var wait = retryAfter.TotalSeconds < 5 ? delay : retryAfter;
                    _logger.LogWarning("Steam price API rate limited (attempt {Attempt}/{Max}). Waiting {Wait}s before retry.",
                        attempt + 1, maxRetries, (int)wait.TotalSeconds);
                    await Task.Delay(wait, ct);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // exponential backoff
                    continue;
                }

                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("success", out var success) || !success.GetBoolean())
                    return null;

                if (root.TryGetProperty("lowest_price", out var priceEl))
                {
                    var priceStr = priceEl.GetString() ?? string.Empty;
                    var cleaned = new string(priceStr.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray())
                        .Replace(",", ".");
                    if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var price))
                        return price;
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch price for {MarketHashName} (attempt {Attempt}/{Max})",
                    marketHashName, attempt + 1, maxRetries);
                if (attempt == maxRetries - 1) return null;
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
            }
        }

        return null;
    }

}
