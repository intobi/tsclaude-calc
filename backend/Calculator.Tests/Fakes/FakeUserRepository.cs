using Calculator.Data.Entities;
using Calculator.Data.Repositories;

namespace Calculator.Tests.Fakes;

internal class FakeUserRepository(ApplicationUser user, string password) : IUserRepository
{
    public Task<ApplicationUser?> FindByCredentialsAsync(
        string email,
        string candidatePassword,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(email == user.Email && candidatePassword == password ? user : null);
}
