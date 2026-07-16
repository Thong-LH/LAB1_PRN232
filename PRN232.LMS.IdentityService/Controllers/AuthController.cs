using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.IdentityService.Models;
using PRN232.LMS.IdentityService.Services;
using Asp.Versioning;

namespace PRN232.LMS.IdentityService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> Login([FromBody] LoginRequest request)
    {
        var authToken = await _authService.LoginAsync(request);

        if (authToken is null)
        {
            return Unauthorized(ApiResponse<AuthTokenResponse>.Fail("Invalid username or password."));
        }

        return Ok(ApiResponse<AuthTokenResponse>.Ok(authToken, "Login successful."));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authToken = await _authService.RefreshTokenAsync(request);

        if (authToken is null)
        {
            return Unauthorized(ApiResponse<AuthTokenResponse>.Fail("Invalid refresh token."));
        }

        return Ok(ApiResponse<AuthTokenResponse>.Ok(authToken, "Token refreshed successfully."));
    }
}
