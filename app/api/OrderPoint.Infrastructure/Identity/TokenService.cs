using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Infrastructure.EfCore;

namespace OrderPoint.Infrastructure.Identity;

// Changes are tracked on the DbContext and persisted by the handler's IUnitOfWork.SaveChangesAsync
internal sealed class TokenService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> jwtOptions)
    : ITokenService
{
    // ASP.NET Core maps the short "role" claim to ClaimTypes.Role when it validates the token
    private const string RoleClaimName = "role";

    private const int RefreshTokenByteLength = 64;

    public async Task<AuthTokensDto> CreateTokensAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        DateTimeOffset accessTokenExpiresAtUtc = nowUtc.Add(jwtOptions.Value.AccessTokenLifetime);

        string accessToken = CreateAccessToken(user, accessTokenExpiresAtUtc);
        string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(RefreshTokenByteLength));

        // Expired tokens are cleaned up whenever the user gets new ones
        List<RefreshToken> expiredRefreshTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.ExpiresAtUtc <= nowUtc)
            .ToListAsync(cancellationToken);

        dbContext.RefreshTokens.RemoveRange(expiredRefreshTokens);

        await dbContext.RefreshTokens.AddAsync(
            new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                TokenHash = HashRefreshToken(refreshToken),
                ExpiresAtUtc = nowUtc.Add(jwtOptions.Value.RefreshTokenLifetime),
                CreatedAtUtc = nowUtc
            },
            cancellationToken);

        return new AuthTokensDto(accessToken, accessTokenExpiresAtUtc, refreshToken, user.Email, user.Role);
    }

    public async Task<Result<AuthTokensDto>> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        RefreshToken? storedRefreshToken = await GetRefreshTokenAsync(refreshToken, cancellationToken);

        if (storedRefreshToken is null || storedRefreshToken.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<AuthTokensDto>(AuthErrors.InvalidRefreshToken);
        }

        ApplicationUser? user = await userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<AuthTokensDto>(AuthErrors.InvalidRefreshToken);
        }

        // A refresh token works only once; the client gets a new one with every refresh
        dbContext.RefreshTokens.Remove(storedRefreshToken);

        AuthTokensDto tokens = await CreateTokensAsync(user.ToUserDto(), cancellationToken);

        return Result.Success(tokens);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken? storedRefreshToken = await GetRefreshTokenAsync(refreshToken, cancellationToken);

        if (storedRefreshToken is not null)
        {
            dbContext.RefreshTokens.Remove(storedRefreshToken);
        }
    }

    public async Task RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<RefreshToken> refreshTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId)
            .ToListAsync(cancellationToken);

        dbContext.RefreshTokens.RemoveRange(refreshTokens);
    }

    private string CreateAccessToken(UserDto user, DateTimeOffset expiresAtUtc)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtOptions.Value.Issuer,
            Audience = jwtOptions.Value.Audience,
            Expires = expiresAtUtc.UtcDateTime,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [RoleClaimName] = user.Role.ToString()
            }
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    private Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string tokenHash = HashRefreshToken(refreshToken);

        return dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    // Only the hash is stored, so a leaked database does not leak usable refresh tokens
    private static string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}