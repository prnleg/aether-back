using Aether.Domain.Entities;

namespace Aether.Domain.Specifications;

public class PortfolioByIdSpec : BaseSpecification<Portfolio>
{
    public PortfolioByIdSpec(Guid id)
    {
        AddCriteria(p => p.Id == id);
        AddInclude(p => p.Assets);
    }
}
