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
    public const string PasswordResetRateLimitPolicy = "password-reset";

    /// <summary>Login attempts allowed per client IP address per minute (brute-force protection).</summary>
    private const int LoginAttemptsPerMinute = 10;

    /// <summary>
    /// Password reset calls (requesting, checking or using a link) allowed per client IP address per minute;
    /// limits token guessing and link spamming.
    /// </summary>
    private const int PasswordResetAttemptsPerMinute = 5;

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
            .AddExceptionHandler<PasswordResetExceptionHandler>()
            .AddProblemDetails();

    /// <summary>Per-IP limits for the anonymous auth endpoints (login and password reset).</summary>
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
                new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                    new ErrorResponse("Too many attempts. Try again in a minute."),
                    cancellationToken));

            options.AddPolicy(LoginRateLimitPolicy, httpContext => PerIpPerMinute(httpContext, LoginAttemptsPerMinute));
            options.AddPolicy(
                PasswordResetRateLimitPolicy,
                httpContext => PerIpPerMinute(httpContext, PasswordResetAttemptsPerMinute));
        });

    private static RateLimitPartition<string> PerIpPerMinute(HttpContext httpContext, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
}
