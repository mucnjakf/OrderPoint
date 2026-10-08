using Microsoft.AspNetCore.Identity;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Infrastructure.Identity;

// UserManager does not take cancellation tokens; the store is registered without auto-save, so every change
// is persisted by the handler's IUnitOfWork.SaveChangesAsync
internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    private static readonly string[] PasswordErrorCodes =
    [
        nameof(IdentityErrorDescriber.PasswordTooShort),
        nameof(IdentityErrorDescriber.PasswordRequiresDigit),
        nameof(IdentityErrorDescriber.PasswordRequiresLower),
        nameof(IdentityErrorDescriber.PasswordRequiresUpper),
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric),
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars)
    ];

    public async Task<Result<UserDto>> CheckPasswordAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        // A user of the other app is treated like an unknown user, so an app never learns about the other's accounts
        if (user is null || user.Role != role)
        {
            return Result.Failure<UserDto>(AuthErrors.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<UserDto>(AuthErrors.LockedOut);
        }

        bool isPasswordValid = await userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(user);

            return await userManager.IsLockedOutAsync(user)
                ? Result.Failure<UserDto>(AuthErrors.LockedOut)
                : Result.Failure<UserDto>(AuthErrors.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success(user.ToUserDto());
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<Result> CreateUserAsync(
        Guid id,
        string email,
        string password,
        UserRole role,
        bool mustChangePassword,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser user = new()
        {
            Id = id,
            UserName = email,
            Email = email,
            Role = role,
            MustChangePassword = mustChangePassword
        };

        IdentityResult result = await userManager.CreateAsync(user, password);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(ToError(result));
    }

    public async Task<Result> ChangeInitialPasswordAsync(
        Guid id,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        IdentityResult result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(ToError(result));
        }

        user.MustChangePassword = false;

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        Guid id,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // Remove + add sets a password without the old one and without reset tokens; nothing is saved on failure
        await userManager.RemovePasswordAsync(user);

        IdentityResult result = await userManager.AddPasswordAsync(user, newPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(ToError(result));
        }

        // The new password is temporary again, and a lockout from forgotten-password attempts is lifted
        user.MustChangePassword = true;
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success();
    }

    public async Task<Result> UpdateEmailAsync(Guid id, string email, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        user.UserName = email;
        user.Email = email;

        IdentityResult result = await userManager.UpdateAsync(user);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(ToError(result));
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return;
        }

        IdentityResult result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Deleting user {id} failed: {result}");
        }
    }

    private static Error ToError(IdentityResult result)
    {
        string[] codes = result.Errors.Select(error => error.Code).ToArray();

        if (codes.Contains(nameof(IdentityErrorDescriber.DuplicateEmail)) ||
            codes.Contains(nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return AuthErrors.EmailAlreadyExists;
        }

        if (codes.Contains(nameof(IdentityErrorDescriber.InvalidEmail)) ||
            codes.Contains(nameof(IdentityErrorDescriber.InvalidUserName)))
        {
            return AuthErrors.InvalidEmail;
        }

        if (codes.Contains(nameof(IdentityErrorDescriber.PasswordMismatch)))
        {
            return AuthErrors.InvalidCredentials;
        }

        if (codes.Any(PasswordErrorCodes.Contains))
        {
            return AuthErrors.InvalidPassword;
        }

        throw new InvalidOperationException($"Unexpected Identity failure: {result}");
    }
}