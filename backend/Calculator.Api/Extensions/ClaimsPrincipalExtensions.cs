using System.Security.Claims;

namespace Calculator.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the id of the authenticated user (the JWT <c>nameid</c> claim).</summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The authenticated user has no NameIdentifier claim.");
}
