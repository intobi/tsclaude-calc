using Calculator.Api.Dtos;
using Calculator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);
        return result is null
            ? Unauthorized(new ErrorResponse("Invalid email or password"))
            : Ok(result);
    }
}
