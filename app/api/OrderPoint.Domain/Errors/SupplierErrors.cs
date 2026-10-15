using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class SupplierErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Supplier.NotFound",
        "Supplier not found");

    internal static readonly Error NameIsRequired = Error.Validation(
        "Supplier.NameIsRequired",
        "Supplier name is required");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Supplier.NameAlreadyExists",
        "A supplier with this name already exists");

    public static readonly Error CannotDeleteWithDeliveries = Error.Conflict(
        "Supplier.CannotDeleteWithDeliveries",
        "Cannot delete a supplier with deliveries");
}