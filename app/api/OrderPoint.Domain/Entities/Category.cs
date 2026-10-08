using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

public sealed class Category : Entity
{
    public string Name { get; private set; }

    public string Description { get; private set; }

    public CategoryStatus Status { get; private set; }

    public string? ImageUrl { get; private set; }

    private readonly List<Item> _items = [];

    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    private Category(
        Guid id,
        string name,
        string description,
        CategoryStatus status,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Name = name;
        Description = description;
        Status = status;
    }

    public static Result<Category> Create(
        string name,
        string description,
        CategoryStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Category>(CategoryErrors.NameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Category>(CategoryErrors.DescriptionIsRequired);
        }

        Category category = new(Guid.CreateVersion7(), name, description, status, DateTimeOffset.UtcNow);

        return Result.Success(category);
    }

    public Result Update(
        string name,
        string description,
        CategoryStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(CategoryErrors.NameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(CategoryErrors.DescriptionIsRequired);
        }

        Name = name;
        Description = description;
        Status = status;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result SetImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Result.Failure(CategoryErrors.ImageUrlIsRequired);
        }

        ImageUrl = imageUrl;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void RemoveImage()
    {
        ImageUrl = null;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}