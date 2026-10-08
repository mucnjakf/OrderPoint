using OrderPoint.Application.Identity;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Extensions;

internal static class IdentityExtensions
{
    private const string AdminEmailKey = "Admin:Email";

    private const string AdminPasswordKey = "Admin:Password";

    internal static async Task SeedAdminAsync(this WebApplication app)
    {
        string? email = app.Configuration[AdminEmailKey];
        string? password = app.Configuration[AdminPasswordKey];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            app.Logger.LogWarning(
                "Admin is not seeded because {EmailKey} or {PasswordKey} is not configured",
                AdminEmailKey,
                AdminPasswordKey);

            return;
        }

        using IServiceScope scope = app.Services.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        bool adminExists = await identityService.ExistsByEmailAsync(email);

        if (adminExists)
        {
            return;
        }

        Result result = await identityService.CreateUserAsync(
            Guid.CreateVersion7(),
            email,
            password,
            UserRole.Admin,
            mustChangePassword: false);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Seeding admin {email} failed: {result.Error.Description}");
        }

        await unitOfWork.SaveChangesAsync();
    }
}