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
public class AuthController(
    IAuthService authService,
    IPasswordResetService passwordResetService,
    IDtoMapper mapper) : ControllerBase
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

    /// <remarks>
    /// Answers 200 for known and unknown emails alike. Email delivery is simulated: the reset token is
    /// returned in the body instead of being emailed (see <see cref="ForgotPasswordResponse"/>).
    /// </remarks>
    [HttpPost("forgot-password")]
    [EnableRateLimiting(ServiceCollectionExtensions.PasswordResetRateLimitPolicy)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await passwordResetService.RequestResetAsync(request.Email, cancellationToken);
        return Ok(mapper.ToForgotPasswordResponse(ticket));
    }

    /// <remarks>204 when the link can be used; an invalid or expired one gets 400 from
    /// <see cref="Infrastructure.PasswordResetExceptionHandler"/>.</remarks>
    [HttpPost("reset-password/validate")]
    [EnableRateLimiting(ServiceCollectionExtensions.PasswordResetRateLimitPolicy)]
    public async Task<IActionResult> ValidateResetToken(
        ResetPasswordTokenRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.ValidateTokenAsync(request.Email, request.Token, cancellationToken);
        return NoContent();
    }

    /// <remarks>204 on success; a bad link or an unacceptable password gets 400 from
    /// <see cref="Infrastructure.PasswordResetExceptionHandler"/>.</remarks>
    [HttpPost("reset-password")]
    [EnableRateLimiting(ServiceCollectionExtensions.PasswordResetRateLimitPolicy)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await passwordResetService.ResetPasswordAsync(
            request.Email, request.Token, request.NewPassword, cancellationToken);
        return NoContent();
    }
}
