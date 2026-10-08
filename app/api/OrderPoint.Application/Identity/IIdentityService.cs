using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Identity;

public interface IIdentityService
{
    Task<Result<UserDto>> CheckPasswordAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Result> CreateUserAsync(
        Guid id,
        string email,
        string password,
        UserRole role,
        bool mustChangePassword,
        CancellationToken cancellationToken = default);

    Task<Result> ChangeInitialPasswordAsync(
        Guid id,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default);

    Task<Result> UpdateEmailAsync(Guid id, string email, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}