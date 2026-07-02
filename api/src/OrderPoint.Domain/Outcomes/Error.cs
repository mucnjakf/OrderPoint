namespace OrderPoint.Domain.Outcomes;

public sealed class Error
{
    internal static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    internal static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Failure);

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    public static Error RequestValidation(string code, string description)
        => new(code, description, ErrorType.RequestValidation);

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);
}