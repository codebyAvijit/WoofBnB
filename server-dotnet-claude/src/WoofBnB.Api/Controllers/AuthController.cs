using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WoofBnB.Application.Auth;
using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.Api.Controllers;

/// <summary>Mirrors server/src/modules/auth/auth.routes.js: POST /login, GET /me.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IApiResponseFactory _responseFactory;

    public AuthController(IAuthService authService, IApiResponseFactory responseFactory)
    {
        _authService = authService;
        _responseFactory = responseFactory;
    }

    [EnableRateLimiting(RateLimitPolicies.Login)]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(_responseFactory.Success(HttpStatusCodes.Ok, "Login successful", result));
    }

    [Authorize(Policy = "ActiveUser")]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirst("id")!.Value);
        var user = await _authService.GetCurrentUserAsync(userId);

        return Ok(_responseFactory.Success(HttpStatusCodes.Ok, "User fetched successfully", user));
    }
}
