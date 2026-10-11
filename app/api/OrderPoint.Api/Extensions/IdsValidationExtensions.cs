using FluentValidation;

namespace OrderPoint.Api.Extensions;

internal static class IdsValidationExtensions
{
    private const int MaxIdsCount = 100;

    internal static IRuleBuilderOptions<T, IReadOnlyList<Guid>> MustBeValidIds<T>(
        this IRuleBuilderInitial<T, IReadOnlyList<Guid>> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ids are required")
            .Must(ids => ids.Count <= MaxIdsCount).WithMessage("Ids must contain at most 100 ids")
            .Must(ids => !ids.Contains(Guid.Empty)).WithMessage("Ids must not contain an empty id")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Ids must be unique");
    }
}