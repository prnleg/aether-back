using Aether.Domain.Common;
using Aether.Domain.Events;
using Aether.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aether.Domain.Entities;

public class Portfolio : IHasDomainEvents
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    [NotMapped]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Portfolio() { }

    public Portfolio(string name, Guid userId)
    {
        Name = name;
        UserId = userId;
        _domainEvents.Add(new PortfolioCreatedEvent(Id, name, userId));
    }

    public void AddAsset(Asset asset)
    {
        _assets.Add(asset);
        _domainEvents.Add(new AssetAddedEvent(Id, asset.Id, asset.GetType().Name));
    }

    public bool RemoveAsset(Guid assetId)
    {
        var asset = _assets.FirstOrDefault(a => a.Id == assetId);
        if (asset == null) return false;
        _assets.Remove(asset);
        _domainEvents.Add(new AssetRemovedEvent(Id, assetId));
        return true;
    }

    public Money CalculateTotalValue(string currency)
    {
        decimal total = _assets
            .Where(a => a.CurrentFloorPrice.Currency == currency)
            .Sum(a => a.CurrentFloorPrice.Amount);

        return new Money(total, currency);
    }
}
