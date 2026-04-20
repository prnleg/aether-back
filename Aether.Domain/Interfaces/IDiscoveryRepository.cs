using Aether.Domain.Entities;
using Aether.Domain.Enums;

namespace Aether.Domain.Interfaces;

public interface IDiscoveryRepository
{
    Task<HashSet<string>> GetExternalIdsByUserAndAppIdAsync(Guid userId, string appId, CancellationToken ct = default);
    Task<IReadOnlyList<DiscoveryItem>> GetUnpricedItemsAsync(int batchSize, CancellationToken ct = default);
    Task<IReadOnlyList<DiscoveryItem>> GetByUserAndStatusAsync(Guid userId, AssetStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<DiscoveryItem?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<DiscoveryItem> items, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<DiscoveryItem> items, CancellationToken ct = default);
    Task UpdateAsync(DiscoveryItem item, CancellationToken ct = default);
}
