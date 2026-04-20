namespace Aether.Application.Features.Discovery;

public record DiscoveryItemDto(
    Guid Id,
    string ExternalId,
    string MarketHashName,
    string AppId,
    string IconUrl,
    string Status,
    decimal? MarketPriceAmount,
    string? MarketPriceCurrency,
    DateTime LastSeenAt,
    DateTime CreatedAt);
