namespace Aether.Application.Features.Discovery;

public record SyncInventoryRequest(IReadOnlyList<string> AppIds);
