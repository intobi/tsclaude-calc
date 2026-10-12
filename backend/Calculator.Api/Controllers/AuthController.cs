using Calculator.Api.Extensions;
using Calculator.Api.Mapping;
using Calculator.Contracts;
using Calculator.Contracts.Auth;
using Calculator.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Calculator.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IDtoMapper mapper) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(ServiceCollectionExtensions.LoginRateLimitPolicy)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return result is null
            ? Unauthorized(new ErrorResponse("Invalid email or password"))
            : Ok(mapper.ToLoginResponse(result));
    }
}
