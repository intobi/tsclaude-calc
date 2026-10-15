using Calculator.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Calculator.Data.Repositories;

public class UserRepository(
    UserManager<ApplicationUser> userManager,
    IOptions<DataProtectionTokenProviderOptions> tokenOptions) : IUserRepository
{
    public async Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.CheckPasswordAsync(user, password) ? user : null;
    }

    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        userManager.FindByEmailAsync(email);

    public TimeSpan PasswordResetTokenLifespan => tokenOptions.Value.TokenLifespan;

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) =>
        userManager.GeneratePasswordResetTokenAsync(user);

    public Task<bool> IsPasswordResetTokenValidAsync(ApplicationUser user, string token) =>
        userManager.VerifyUserTokenAsync(
            user,
            userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
            token);

    public Task<bool> IsCurrentPasswordAsync(ApplicationUser user, string password) =>
        userManager.CheckPasswordAsync(user, password);

    public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) =>
        userManager.ResetPasswordAsync(user, token, newPassword);
}
