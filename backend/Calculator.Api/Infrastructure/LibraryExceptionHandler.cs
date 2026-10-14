using Calculator.Contracts;
using Calculator.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Calculator.Api.Infrastructure;

/// <summary>
/// Turns broken book/author rules into 400 (unknown author, date of birth in the future)
/// and 409 (author still has books).
/// </summary>
internal sealed class LibraryExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int? statusCode = exception switch
        {
            UnknownAuthorException or InvalidDateOfBirthException => StatusCodes.Status400BadRequest,
            AuthorHasBooksException => StatusCodes.Status409Conflict,
            _ => null,
        };
        if (statusCode is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode.Value;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(exception.Message), cancellationToken);
        return true;
    }
}
