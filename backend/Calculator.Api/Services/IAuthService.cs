using Calculator.Api.Dtos;

namespace Calculator.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
}
