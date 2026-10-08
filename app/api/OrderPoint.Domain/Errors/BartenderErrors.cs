using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class BartenderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Bartender.NotFound",
        "Bartender not found");

    internal static readonly Error FirstNameIsRequired = Error.Validation(
        "Bartender.FirstNameIsRequired",
        "Bartender first name is required");

    internal static readonly Error LastNameIsRequired = Error.Validation(
        "Bartender.LastNameIsRequired",
        "Bartender last name is required");

    internal static readonly Error EmailIsRequired = Error.Validation(
        "Bartender.EmailIsRequired",
        "Bartender email is required");

    internal static readonly Error ImageUrlIsRequired = Error.Validation(
        "Bartender.ImageUrlIsRequired",
        "Bartender image URL is required");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Bartender.EmailAlreadyExists",
        "Bartender with this email already exists");
}