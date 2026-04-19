using Aether.Domain.Entities;

namespace Aether.Domain.Specifications;

public class PortfoliosByUserIdSpec : BaseSpecification<Portfolio>
{
    public PortfoliosByUserIdSpec(Guid userId)
    {
        AddCriteria(p => p.UserId == userId);
        AddInclude(p => p.Assets);
    }
}
