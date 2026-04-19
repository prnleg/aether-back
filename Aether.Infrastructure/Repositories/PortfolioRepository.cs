using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Aether.Infrastructure.Persistence;
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

    public async Task<IEnumerable<Portfolio>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Portfolios.Include(p => p.Assets).Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(Guid id)
    {
        return await _context.Portfolios.Include(p => p.Assets).FirstOrDefaultAsync(p => p.Id == id);
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
