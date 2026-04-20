using Aether.Application.Features.Auth;
using Aether.Application.Features.Users;
using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aether.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUserProfileRepository _userProfileRepo;

    public UsersController(UserManager<IdentityUser<Guid>> userManager, IUserProfileRepository userProfileRepo)
    {
        _userManager = userManager;
        _userProfileRepo = userProfileRepo;
    }

    [HttpPatch("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "ValidationError", message = "Name cannot be empty." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "ValidationError", message = "Email cannot be empty." });

        var nameResult = await _userManager.SetUserNameAsync(user, request.Name);
        if (!nameResult.Succeeded)
        {
            var errors = string.Join("; ", nameResult.Errors.Select(e => e.Description));
            return BadRequest(new { error = "UpdateFailed", message = errors });
        }

        var emailResult = await _userManager.SetEmailAsync(user, request.Email);
        if (!emailResult.Succeeded)
        {
            var errors = string.Join("; ", emailResult.Errors.Select(e => e.Description));
            return BadRequest(new { error = "UpdateFailed", message = errors });
        }

        return Ok(new UserDto { Id = user.Id, Name = user.UserName!, Email = user.Email! });
    }

    [HttpPatch("me/steam-id")]
    public async Task<IActionResult> UpdateSteamId([FromBody] UpdateSteamIdRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SteamId))
            return BadRequest(new { error = "ValidationError", message = "SteamId cannot be empty." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _userProfileRepo.GetByUserIdAsync(userId, ct) ?? new UserProfile(userId);
        profile.SetSteamId(request.SteamId.Trim());
        await _userProfileRepo.UpsertAsync(profile, ct);

        return Ok(new { steamId = profile.SteamId });
    }
}
