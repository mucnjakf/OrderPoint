using Microsoft.EntityFrameworkCore;
using OrderPoint.Infrastructure.EfCore;

namespace OrderPoint.Api.Extensions;

internal static class DatabaseExtensions
{
    internal static void ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}