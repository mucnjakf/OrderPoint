using Microsoft.AspNetCore.Identity;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public UserRole Role { get; set; }

    public bool MustChangePassword { get; set; }
}