namespace Aether.Application.Features.Discovery;

public record GetDiscoveryItemsRequest(string? Status, int Page = 1, int PageSize = 20);
