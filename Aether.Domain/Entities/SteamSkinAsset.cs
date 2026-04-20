using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public class SteamSkinAsset : Asset
{
    public string MarketHashName { get; private set; } = string.Empty;
    public string AppId { get; private set; } = "730"; // Default CS2

    private SteamSkinAsset() { }

    public SteamSkinAsset(string name, DateTime acquisitionDate, Money acquisitionPrice, string marketHashName, string appId = "730")
        : base(name, acquisitionDate, acquisitionPrice)
    {
        MarketHashName = marketHashName;
        AppId = appId;
    }
}
