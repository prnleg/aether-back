using Aether.Domain.ValueObjects;

namespace Aether.Domain.Entities;

public class CryptoAsset : Asset
{
    public string Symbol { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }

    private CryptoAsset() { }

    public CryptoAsset(string name, DateTime acquisitionDate, Money acquisitionPrice, string symbol, decimal quantity) 
        : base(name, acquisitionDate, acquisitionPrice)
    {
        Symbol = symbol.ToUpper();
        Quantity = quantity;
    }
}
