using Calculator.Data.Entities;
using Calculator.Services.Models;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Services.Auth;

internal class AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        var (token, expiresAtUtc) = tokenService.CreateToken(user);
        return new AuthResult(token, user.Email!, expiresAtUtc);
    }
}
