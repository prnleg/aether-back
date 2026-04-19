using Aether.Domain.Entities;

namespace Aether.Domain.Interfaces;

public interface IPortfolioRepository
{
    Task<IEnumerable<Portfolio>> GetAllAsync();
    Task<IEnumerable<Portfolio>> GetAllByUserIdAsync(Guid userId);
    Task<Portfolio?> GetByIdAsync(Guid id);
    Task AddAsync(Portfolio portfolio);
    Task UpdateAsync(Portfolio portfolio);
}
