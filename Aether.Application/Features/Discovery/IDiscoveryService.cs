using Aether.Application.Features.Portfolio;
using Aether.Domain.Common;

namespace Aether.Application.Features.Discovery;

public interface IDiscoveryService
{
    Task<Result<SyncResultDto>> SyncAsync(SyncInventoryRequest request, Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<DiscoveryItemDto>>> GetItemsAsync(GetDiscoveryItemsRequest request, Guid userId, CancellationToken ct = default);
    Task<Result<AssetDto>> ApproveAsync(Guid itemId, Guid userId, CancellationToken ct = default);
    Task<Result> RejectAsync(Guid itemId, Guid userId, CancellationToken ct = default);
}
