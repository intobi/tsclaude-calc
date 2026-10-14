using System.Text;
using System.Threading.RateLimiting;
using Calculator.Api.Infrastructure;
using Calculator.Api.Mapping;
using Calculator.Contracts;
using Calculator.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Calculator.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "FrontendDev";
    public const string LoginRateLimitPolicy = "login";

    /// <summary>Login attempts allowed per client IP address per minute (brute-force protection).</summary>
    private const int LoginAttemptsPerMinute = 10;

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Missing '{JwtOptions.SectionName}' configuration section.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddFrontendCors(this IServiceCollection services) =>
        services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy =>
                policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

    /// <summary>Registers the DTO ⇄ entity mapper used by the controllers. It is stateless, so one instance is shared.</summary>
    public static IServiceCollection AddDtoMapping(this IServiceCollection services) =>
        services.AddSingleton<IDtoMapper, DtoMapper>();

    public static IServiceCollection AddApiErrorHandling(this IServiceCollection services) =>
        services
            .AddExceptionHandler<CalculationExceptionHandler>()
            .AddExceptionHandler<LibraryExceptionHandler>()
            .AddProblemDetails();

    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
                new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                    new ErrorResponse("Too many login attempts. Try again in a minute."),
                    cancellationToken));

            options.AddPolicy(LoginRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = LoginAttemptsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });
}
