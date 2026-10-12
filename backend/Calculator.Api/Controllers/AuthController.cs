using Calculator.Api.Dtos;
using Calculator.Api.Extensions;
using Calculator.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Calculator.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(ServiceCollectionExtensions.LoginRateLimitPolicy)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return result is null
            ? Unauthorized(new ErrorResponse("Invalid email or password"))
            : Ok(new LoginResponse(result.Token, result.Email, result.ExpiresAtUtc));
    }
}
