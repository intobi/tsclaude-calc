namespace Calculator.Contracts.Auth;

/// <summary>
/// JWT settings (configuration section <c>Jwt</c>), shared by the token issuer and the API that validates tokens.
/// </summary>
public class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (<c>iss</c>).</summary>
    public string Issuer { get; set; } = null!;

    /// <summary>Token audience (<c>aud</c>).</summary>
    public string Audience { get; set; } = null!;

    /// <summary>HMAC-SHA256 signing key; at least 32 characters.</summary>
    public string Key { get; set; } = null!;

    /// <summary>Token lifetime in minutes.</summary>
    public int ExpiryMinutes { get; set; }
}
