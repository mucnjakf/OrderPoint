using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class CategoryErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Category.NotFound",
        "Category not found");

    internal static readonly Error NameIsRequired = Error.Validation(
        "Category.NameIsRequired",
        "Category name is required");

    internal static readonly Error DescriptionIsRequired = Error.Validation(
        "Category.DescriptionIsRequired",
        "Category description is required");

    public static readonly Error CannotDeleteCategoryWithItems = Error.Conflict(
        "Category.CannotDeleteCategoryWithItems",
        "Cannot delete category with items");
}