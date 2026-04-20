using Aether.Domain.Entities;
using Aether.Domain.Enums;
using Aether.Domain.Interfaces;
using Aether.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aether.Infrastructure.Repositories;

public class DiscoveryRepository : IDiscoveryRepository
{
    private readonly AetherDbContext _context;

    public DiscoveryRepository(AetherDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetExternalIdsByUserAndAppIdAsync(Guid userId, string appId, CancellationToken ct = default)
    {
        var ids = await _context.DiscoveryItems
            .Where(d => d.UserId == userId && d.AppId == appId)
            .Select(d => d.ExternalId)
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task<IReadOnlyList<DiscoveryItem>> GetByUserAndStatusAsync(Guid userId, AssetStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.DiscoveryItems.Where(d => d.UserId == userId);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        return await query
            .OrderByDescending(d => d.LastSeenAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<DiscoveryItem?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DiscoveryItems.FindAsync(new object[] { id }, ct);
    }

    public async Task AddRangeAsync(IEnumerable<DiscoveryItem> items, CancellationToken ct = default)
    {
        _context.DiscoveryItems.AddRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<DiscoveryItem> items, CancellationToken ct = default)
    {
        _context.DiscoveryItems.UpdateRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DiscoveryItem item, CancellationToken ct = default)
    {
        _context.DiscoveryItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DiscoveryItem>> GetUnpricedItemsAsync(int batchSize, CancellationToken ct = default)
    {
        return await _context.DiscoveryItems
            .Where(d => d.MarketPrice == null && d.Status != AssetStatus.Rejected)
            .OrderBy(d => d.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}
