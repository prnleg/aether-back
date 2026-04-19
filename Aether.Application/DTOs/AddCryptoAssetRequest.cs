namespace Aether.Application.DTOs;

public class AddCryptoAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AcquisitionPrice { get; set; }
    public string Currency { get; set; } = "USD";
}
