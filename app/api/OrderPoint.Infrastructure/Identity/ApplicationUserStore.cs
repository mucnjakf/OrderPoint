using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OrderPoint.Infrastructure.EfCore;

namespace OrderPoint.Infrastructure.Identity;

// The base store re-attaches the user on every update. Without auto-save that makes EF accept the unsaved changes
// as the original values, so a second update before IUnitOfWork saves (e.g. a password reset that also lifts the
// lockout) fails its concurrency check. Marking the user modified keeps the database's stamp as the original value.
internal sealed class ApplicationUserStore(ApplicationDbContext dbContext)
    : UserOnlyStore<ApplicationUser, ApplicationDbContext, Guid>(dbContext)
{
    public override Task<IdentityResult> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        Context.Update(user);

        return Task.FromResult(IdentityResult.Success);
    }
}