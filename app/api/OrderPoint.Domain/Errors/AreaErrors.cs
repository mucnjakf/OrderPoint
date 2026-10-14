using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class AreaErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Area.NotFound",
        "Area not found");

    internal static readonly Error NameIsRequired = Error.Validation(
        "Area.NameIsRequired",
        "Area name is required");

    internal static readonly Error GridSizeMustBePositive = Error.Validation(
        "Area.GridSizeMustBePositive",
        "Area columns and rows must be positive");

    internal static readonly Error TooSmallForContents = Error.Conflict(
        "Area.TooSmallForContents",
        "Area is too small for the tables and fixtures on it");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Area.NameAlreadyExists",
        "Area with this name already exists");

    public static readonly Error CannotDeleteAreaWithTables = Error.Conflict(
        "Area.CannotDeleteAreaWithTables",
        "Cannot delete area with tables");
}