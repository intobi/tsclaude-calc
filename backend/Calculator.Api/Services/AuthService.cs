using Calculator.Api.Dtos;
using Calculator.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return null;
        }

        var (token, expiresAtUtc) = tokenService.CreateToken(user);
        return new LoginResponse(token, user.Email!, expiresAtUtc);
    }
}
