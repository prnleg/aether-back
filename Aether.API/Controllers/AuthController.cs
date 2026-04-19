using Aether.Application.Features.Auth;
using Aether.Application.Features.Portfolio;
using Aether.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aether.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IPortfolioService _portfolioService;
    private readonly IValidator<AuthRequest> _validator;

    public AuthController(
        UserManager<IdentityUser<Guid>> userManager,
        IJwtService jwtService,
        IPortfolioService portfolioService,
        IValidator<AuthRequest> validator)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _portfolioService = portfolioService;
        _validator = validator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] AuthRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Conflict(new { error = "Conflict", message = "Email is already registered." });

        var user = new IdentityUser<Guid> { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(new { error = "RegistrationFailed", message = errors });
        }

        var portfolioResult = await _portfolioService.GetOrCreateDefaultPortfolioIdAsync(user.Id);

        var token = _jwtService.GenerateToken(user.Id, user.Email!);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            PortfolioId = portfolioResult.Value
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { error = "Unauthorized", message = "Invalid email or password." });

        var portfolioResult = await _portfolioService.GetOrCreateDefaultPortfolioIdAsync(user.Id);

        var token = _jwtService.GenerateToken(user.Id, user.Email!);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            PortfolioId = portfolioResult.Value
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();
        return Ok(new UserDto { Id = user.Id, Name = user.UserName!, Email = user.Email! });
    }
}
