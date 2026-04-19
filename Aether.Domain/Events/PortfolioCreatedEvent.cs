using Aether.Domain.Common;

namespace Aether.Domain.Events;

public record PortfolioCreatedEvent(Guid PortfolioId, string Name, Guid UserId) : IDomainEvent;
