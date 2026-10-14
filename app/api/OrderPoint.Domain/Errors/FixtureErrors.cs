using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class FixtureErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Fixture.NotFound",
        "Fixture not found");

    internal static readonly Error OutsideArea = Error.Validation(
        "Fixture.OutsideArea",
        "Fixture must lie inside its area");
}