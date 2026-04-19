using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public class PhysicalAsset : Asset
{
    public string Category { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Condition { get; private set; } = string.Empty;

    private PhysicalAsset() { }

    public PhysicalAsset(string name, DateTime acquisitionDate, Money acquisitionPrice, string category, string brand, string condition) 
        : base(name, acquisitionDate, acquisitionPrice)
    {
        Category = category;
        Brand = brand;
        Condition = condition;
    }
}
