using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Aether.Domain.Specifications;
using Aether.Infrastructure.Persistence;
using Aether.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Aether.Infrastructure.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AetherDbContext _context;

    public PortfolioRepository(AetherDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Portfolio>> GetAllAsync()
    {
        return await _context.Portfolios.Include(p => p.Assets).ToListAsync();
    }

    public async Task<Portfolio?> FindAsync(ISpecification<Portfolio> spec)
    {
        return await SpecificationEvaluator<Portfolio>
            .GetQuery(_context.Portfolios.AsQueryable(), spec)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Portfolio>> ListAsync(ISpecification<Portfolio> spec)
    {
        return await SpecificationEvaluator<Portfolio>
            .GetQuery(_context.Portfolios.AsQueryable(), spec)
            .ToListAsync();
    }

    public async Task AddAsync(Portfolio portfolio)
    {
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Portfolio portfolio)
    {
        _context.Portfolios.Update(portfolio);
        await _context.SaveChangesAsync();
    }
}
