using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public abstract class Asset
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public string Name { get; protected set; } = string.Empty;
    public DateTime AcquisitionDate { get; protected set; }
    public Money AcquisitionPrice { get; protected set; } = null!;
    public Money CurrentFloorPrice { get; protected set; } = null!;
    public DateTime LastUpdated { get; protected set; }

    protected Asset() { }

    protected Asset(string name, DateTime acquisitionDate, Money acquisitionPrice)
    {
        Name = name;
        AcquisitionDate = acquisitionDate;
        AcquisitionPrice = acquisitionPrice;
        CurrentFloorPrice = Money.Zero(acquisitionPrice.Currency);
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateFloorPrice(Money newPrice)
    {
        CurrentFloorPrice = newPrice;
        LastUpdated = DateTime.UtcNow;
    }
}
