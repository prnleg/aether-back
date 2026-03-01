using Aether.Domain.Entities;
using Aether.Domain.ValueObjects;

namespace Aether.Domain.Interfaces;

public interface IMarketProvider
{
    string ProviderName { get; }
    Task<Money> GetCurrentPriceAsync(Asset asset);
}
