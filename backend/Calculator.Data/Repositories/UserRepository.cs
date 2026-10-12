using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Calculator.Data.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public async Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.CheckPasswordAsync(user, password) ? user : null;
    }
}
