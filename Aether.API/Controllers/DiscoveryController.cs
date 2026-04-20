using Aether.API.Common;
using Aether.Application.Features.Discovery;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aether.API.Controllers;

[ApiController]
[Route("api/discovery")]
[Authorize]
public class DiscoveryController : ControllerBase
{
    private readonly IDiscoveryService _discoveryService;
    private readonly IValidator<SyncInventoryRequest> _syncValidator;

    public DiscoveryController(IDiscoveryService discoveryService, IValidator<SyncInventoryRequest> syncValidator)
    {
        _discoveryService = discoveryService;
        _syncValidator = syncValidator;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncInventoryRequest request, CancellationToken ct)
    {
        await _syncValidator.ValidateAndThrowAsync(request, ct);
        var result = await _discoveryService.SyncAsync(request, CurrentUserId, ct);
        return result.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var request = new GetDiscoveryItemsRequest(status, page, pageSize);
        var result = await _discoveryService.GetItemsAsync(request, CurrentUserId, ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _discoveryService.ApproveAsync(id, CurrentUserId, ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        var result = await _discoveryService.RejectAsync(id, CurrentUserId, ct);
        return result.ToActionResult(this);
    }
}
