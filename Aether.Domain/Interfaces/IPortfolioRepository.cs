using Aether.Domain.Entities;
using Aether.Domain.Specifications;

namespace Aether.Domain.Interfaces;

public interface IPortfolioRepository
{
    Task<IEnumerable<Portfolio>> GetAllAsync();
    Task<Portfolio?> FindAsync(ISpecification<Portfolio> spec);
    Task<IEnumerable<Portfolio>> ListAsync(ISpecification<Portfolio> spec);
    Task AddAsync(Portfolio portfolio);
    Task UpdateAsync(Portfolio portfolio);
}
