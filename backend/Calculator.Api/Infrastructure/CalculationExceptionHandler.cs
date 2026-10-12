using Calculator.Api.Dtos;
using Calculator.Contracts.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Calculator.Api.Infrastructure;

/// <summary>Turns calculation errors caused by user input into 400 responses.</summary>
internal sealed class CalculationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CalculationException calculationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(calculationException.Message), cancellationToken);
        return true;
    }
}
