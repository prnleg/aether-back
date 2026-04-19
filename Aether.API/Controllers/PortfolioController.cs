using Aether.Application.DTOs;
using Aether.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aether.API.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetPortfolios()
    {
        var portfolios = await _portfolioService.GetUserPortfoliosAsync(CurrentUserId);
        return Ok(portfolios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDto>> GetPortfolio(Guid id)
    {
        var portfolio = await _portfolioService.GetPortfolioByIdAsync(id, CurrentUserId);
        if (portfolio == null) return NotFound();
        return Ok(portfolio);
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioDto>> CreatePortfolio([FromBody] CreatePortfolioRequest request)
    {
        var portfolio = await _portfolioService.CreatePortfolioAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
    }

    [HttpPost("{id}/assets/crypto")]
    public async Task<ActionResult<AssetDto>> AddCryptoAsset(Guid id, [FromBody] AddCryptoAssetRequest request)
    {
        var asset = await _portfolioService.AddCryptoAssetAsync(id, request, CurrentUserId);
        return Ok(asset);
    }

    [HttpPost("{id}/assets/steam")]
    public async Task<ActionResult<AssetDto>> AddSteamSkin(Guid id, [FromBody] AddSteamSkinRequest request)
    {
        var asset = await _portfolioService.AddSteamSkinAsync(id, request, CurrentUserId);
        return Ok(asset);
    }

    [HttpPost("{id}/assets/physical")]
    public async Task<ActionResult<AssetDto>> AddPhysicalAsset(Guid id, [FromBody] AddPhysicalAssetRequest request)
    {
        var asset = await _portfolioService.AddPhysicalAssetAsync(id, request, CurrentUserId);
        return Ok(asset);
    }

    [HttpDelete("{id}/assets/{assetId}")]
    public async Task<IActionResult> RemoveAsset(Guid id, Guid assetId)
    {
        await _portfolioService.RemoveAssetAsync(id, assetId, CurrentUserId);
        return NoContent();
    }
}
