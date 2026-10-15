using Calculator.Contracts;
using Calculator.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Calculator.Api.Infrastructure;

/// <summary>Turns rejected password resets (bad or expired link, unacceptable new password) into 400 responses.</summary>
internal sealed class PasswordResetExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not PasswordResetException passwordResetException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(passwordResetException.Message), cancellationToken);
        return true;
    }
}
