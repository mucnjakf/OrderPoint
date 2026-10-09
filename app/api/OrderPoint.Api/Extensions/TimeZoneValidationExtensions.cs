using FluentValidation;

namespace OrderPoint.Api.Extensions;

internal static class TimeZoneValidationExtensions
{
    internal static IRuleBuilderOptions<T, string> MustBeValidTimeZone<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("TimeZone is required")
            .MaximumLength(100).WithMessage("TimeZone must be at most 100 characters")
            .Must(timeZone => TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out _))
            .WithMessage("TimeZone is invalid");
    }
}