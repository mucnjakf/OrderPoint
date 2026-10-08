using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    bool MustChangePassword);