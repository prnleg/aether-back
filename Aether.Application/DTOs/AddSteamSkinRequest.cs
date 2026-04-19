namespace Aether.Application.DTOs;

public class AddSteamSkinRequest
{
    public string Name { get; set; } = string.Empty;
    public string MarketHashName { get; set; } = string.Empty;
    public decimal AcquisitionPrice { get; set; }
    public string Currency { get; set; } = "USD";
}
