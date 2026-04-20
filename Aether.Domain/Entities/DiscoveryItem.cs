using Aether.Domain.Enums;
using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public class DiscoveryItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ExternalId { get; private set; } = string.Empty;   // "{classid}_{instanceid}"
    public string MarketHashName { get; private set; } = string.Empty;
    public string AppId { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;
    public string? RawMetadata { get; private set; }
    public AssetStatus Status { get; private set; }
    public Money? MarketPrice { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private DiscoveryItem() { }

    public DiscoveryItem(
        string externalId,
        string marketHashName,
        string appId,
        string iconUrl,
        string? rawMetadata,
        AssetStatus status,
        Money? marketPrice,
        Guid userId)
    {
        ExternalId = externalId;
        MarketHashName = marketHashName;
        AppId = appId;
        IconUrl = iconUrl;
        RawMetadata = rawMetadata;
        Status = status;
        MarketPrice = marketPrice;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        LastSeenAt = DateTime.UtcNow;
    }

    public void TouchLastSeen() => LastSeenAt = DateTime.UtcNow;

    public void Approve() => Status = AssetStatus.Approved;

    public void Reject() => Status = AssetStatus.Rejected;

    public void MarkAvailable() => Status = AssetStatus.Available;

    public void UpdatePrice(Money price) => MarketPrice = price;
}
