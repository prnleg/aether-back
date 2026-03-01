using Aether.Domain.Entities;
using Aether.Domain.ValueObjects;
using Aether.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly AetherDbContext _context;

    public PortfolioController(AetherDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolios()
    {
        return await _context.Portfolios.Include(p => p.Assets).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Portfolio>> CreatePortfolio(string name)
    {
        // For now, using a hardcoded user ID until we have real auth
        var userId = Guid.NewGuid(); 
        var portfolio = new Portfolio(name, userId);
        
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPortfolios), new { id = portfolio.Id }, portfolio);
    }

    [HttpPost("{id}/assets/steam")]
    public async Task<IActionResult> AddSteamSkin(Guid id, string name, decimal price, string hashName)
    {
        var portfolio = await _context.Portfolios.FindAsync(id);
        if (portfolio == null) return NotFound();

        var asset = new SteamSkinAsset(name, DateTime.UtcNow, new Money(price, "USD"), hashName);
        portfolio.AddAsset(asset);
        
        await _context.SaveChangesAsync();
        return Ok(portfolio);
    }
}
