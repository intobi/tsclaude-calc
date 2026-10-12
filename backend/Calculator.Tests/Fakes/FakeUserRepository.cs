using Calculator.Contracts.Persistence;

namespace Calculator.Tests.Fakes;

internal class FakeUserRepository(UserAccount account, string password) : IUserRepository
{
    public Task<UserAccount?> FindByCredentialsAsync(
        string email,
        string candidatePassword,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(email == account.Email && candidatePassword == password ? account : null);
}
