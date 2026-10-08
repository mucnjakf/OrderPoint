using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error UserNotFound = Error.NotFound(
        "Auth.UserNotFound",
        "User not found");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials",
        "Email or password is incorrect");

    public static readonly Error LockedOut = Error.Unauthorized(
        "Auth.LockedOut",
        "Account is locked after too many failed login attempts, try again in a few minutes");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Auth.InvalidRefreshToken",
        "Refresh token is invalid or expired");

    public static readonly Error AccountInactive = Error.Forbidden(
        "Auth.AccountInactive",
        "Account is inactive");

    public static readonly Error PasswordChangeRequired = Error.Forbidden(
        "Auth.PasswordChangeRequired",
        "Password must be changed at first login");

    public static readonly Error PasswordChangeNotAllowed = Error.Forbidden(
        "Auth.PasswordChangeNotAllowed",
        "Password was already changed at first login");

    public static readonly Error InvalidEmail = Error.Validation(
        "Auth.InvalidEmail",
        "Email is invalid");

    public static readonly Error InvalidPassword = Error.Validation(
        "Auth.InvalidPassword",
        "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter and a digit");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Auth.EmailAlreadyExists",
        "User with this email already exists");
}