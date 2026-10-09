using Calculator.Data.Entities;

namespace Calculator.Data;

internal static class SeedUsers
{
    // Password for both seeded accounts is "1111", hashed with ASP.NET Core Identity's PasswordHasher.
    public static readonly ApplicationUser[] Users =
    [
        new ApplicationUser
        {
            Id = "6f6a9e3a-9a4b-4c7c-8e1a-2f6a3f5c2c01",
            UserName = "andrew.t",
            NormalizedUserName = "ANDREW.T",
            Email = "andrew.t@intobi.com",
            NormalizedEmail = "ANDREW.T@INTOBI.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEL9diNczBBRYb9mxTZo68nifdurb+b/3OQ4ZFLbNNZ3zjqmG3kR9kQr7d2RspmOS7A==",
            SecurityStamp = "3F2C9A1E-6B4D-4E3A-9B1C-7A2D8E5F1A01",
            ConcurrencyStamp = "8D1E4C2B-5A3F-4B6D-9E7C-1F2A3B4C5D01",
        },
        new ApplicationUser
        {
            Id = "b2c4d6e8-1f3a-4b5c-8d9e-0a1b2c3d4e02",
            UserName = "arty.l",
            NormalizedUserName = "ARTY.L",
            Email = "arty.l@intobi.com",
            NormalizedEmail = "ARTY.L@INTOBI.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEKK6TGbYkgOP56z9leMug3NKD5Csjas9rm9sLlUH3o+0Albt1F7KEPpEvILKIV/onA==",
            SecurityStamp = "1A2B3C4D-5E6F-4071-8293-A4B5C6D7E802",
            ConcurrencyStamp = "9E8D7C6B-5A4F-4312-8091-2B3C4D5E6F02",
        },
    ];
}
