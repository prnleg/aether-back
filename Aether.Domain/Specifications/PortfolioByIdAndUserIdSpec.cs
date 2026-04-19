using Aether.Domain.Entities;

namespace Aether.Domain.Specifications;

public class PortfolioByIdAndUserIdSpec : BaseSpecification<Portfolio>
{
    public PortfolioByIdAndUserIdSpec(Guid id, Guid userId)
    {
        AddCriteria(p => p.Id == id && p.UserId == userId);
        AddInclude(p => p.Assets);
    }
}
