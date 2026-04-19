namespace Aether.Application.Features.Portfolio;

public class AddPhysicalAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public decimal AcquisitionPrice { get; set; }
    public string Currency { get; set; } = "USD";
}
