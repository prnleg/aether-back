using Aether.Domain.Common;

namespace Aether.Domain.Events;

public record AssetRemovedEvent(Guid PortfolioId, Guid AssetId) : IDomainEvent;
