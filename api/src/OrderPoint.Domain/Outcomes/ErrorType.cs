namespace OrderPoint.Domain.Outcomes;

public enum ErrorType
{
    Failure,
    RequestValidation,
    Validation,
    NotFound,
    Conflict
}