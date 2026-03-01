using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private Portfolio() { }

    public Portfolio(string name, Guid userId)
    {
        Name = name;
        UserId = userId;
    }

    public void AddAsset(Asset asset)
    {
        _assets.Add(asset);
    }

    public Money CalculateTotalValue(string currency)
    {
        decimal total = _assets
            .Where(a => a.CurrentFloorPrice.Currency == currency)
            .Sum(a => a.CurrentFloorPrice.Amount);
        
        return new Money(total, currency);
    }
}
