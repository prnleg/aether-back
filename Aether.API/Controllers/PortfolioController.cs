using Aether.API.Common;
using Aether.Application.Features.Portfolio;
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
    public async Task<IActionResult> GetPortfolios()
    {
        var result = await _portfolioService.GetUserPortfoliosAsync(CurrentUserId);
        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPortfolio(Guid id)
    {
        var result = await _portfolioService.GetPortfolioByIdAsync(id, CurrentUserId);
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioRequest request)
    {
        var result = await _portfolioService.CreatePortfolioAsync(request, CurrentUserId);
        if (result.IsFailure) return result.ToActionResult(this);
        return CreatedAtAction(nameof(GetPortfolio), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id}/assets/crypto")]
    public async Task<IActionResult> AddCryptoAsset(Guid id, [FromBody] AddCryptoAssetRequest request)
    {
        var result = await _portfolioService.AddCryptoAssetAsync(id, request, CurrentUserId);
        return result.ToActionResult(this);
    }

    [HttpPost("{id}/assets/steam")]
    public async Task<IActionResult> AddSteamSkin(Guid id, [FromBody] AddSteamSkinRequest request)
    {
        var result = await _portfolioService.AddSteamSkinAsync(id, request, CurrentUserId);
        return result.ToActionResult(this);
    }

    [HttpPost("{id}/assets/physical")]
    public async Task<IActionResult> AddPhysicalAsset(Guid id, [FromBody] AddPhysicalAssetRequest request)
    {
        var result = await _portfolioService.AddPhysicalAssetAsync(id, request, CurrentUserId);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id}/assets/{assetId}")]
    public async Task<IActionResult> RemoveAsset(Guid id, Guid assetId)
    {
        var result = await _portfolioService.RemoveAssetAsync(id, assetId, CurrentUserId);
        return result.ToActionResult(this);
    }
}
