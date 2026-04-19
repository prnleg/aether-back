using Aether.Domain.Common;

namespace Aether.Domain.Events;

public record AssetAddedEvent(Guid PortfolioId, Guid AssetId, string AssetType) : IDomainEvent;
