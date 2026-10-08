using OrderPoint.Application.Dtos;

namespace OrderPoint.Infrastructure.Identity;

internal static class ApplicationUserMapper
{
    internal static UserDto ToUserDto(this ApplicationUser user) => new(
        user.Id,
        user.Email!,
        user.Role,
        user.MustChangePassword);
}