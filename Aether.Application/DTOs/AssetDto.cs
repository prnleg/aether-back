namespace Aether.Application.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public DateTime AcquisitionDate { get; set; }
    public decimal AcquisitionPrice { get; set; }
    public string AcquisitionCurrency { get; set; } = string.Empty;
    public decimal CurrentFloorPrice { get; set; }
    public string CurrentFloorCurrency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }

    // CryptoAsset
    public string? Symbol { get; set; }
    public decimal? Quantity { get; set; }

    // SteamSkinAsset
    public string? MarketHashName { get; set; }
    public string? AppId { get; set; }

    // PhysicalAsset
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Condition { get; set; }
}
