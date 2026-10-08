using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Identity;

public interface ITokenService
{
    Task<AuthTokensDto> CreateTokensAsync(UserDto user, CancellationToken cancellationToken = default);

    Task<Result<AuthTokensDto>> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}