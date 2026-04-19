namespace Aether.Application.DTOs;

public class PortfolioDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<AssetDto> Assets { get; set; } = new();
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = "USD";
}
