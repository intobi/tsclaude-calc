using Calculator.Contracts.Auth;
using Calculator.Contracts.Calculations;
using Calculator.Contracts.History;
using Calculator.Services.Auth;
using Calculator.Services.Calculations;
using Calculator.Services.History;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Services;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the business service implementations of the <c>Calculator.Contracts</c> interfaces.
    /// Requires implementations of the <c>Calculator.Contracts.Persistence</c> interfaces to be registered as well.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32, "Jwt:Key must be at least 32 characters")
            .Validate(o => o.ExpiryMinutes > 0, "Jwt:ExpiryMinutes must be positive")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICalculatorService, CalculatorService>();
        services.AddScoped<IHistoryService, HistoryService>();

        return services;
    }
}
