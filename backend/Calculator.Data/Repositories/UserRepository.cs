using Calculator.Contracts.Persistence;
using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Data.Repositories;

/// <summary>ASP.NET Core Identity implementation of <see cref="IUserRepository"/>.</summary>
public class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public async Task<UserAccount?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return new UserAccount(user.Id, user.Email!);
    }
}
