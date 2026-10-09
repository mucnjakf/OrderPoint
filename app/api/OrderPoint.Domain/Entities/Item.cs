using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

public sealed class Item : Entity
{
    public string Name { get; private set; }

    public string Description { get; private set; }

    public double Portion { get; private set; }

    public decimal Price { get; private set; }

    public ItemStatus Status { get; private set; }

    public string? ImageUrl { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    private readonly List<OrderItem> _orderItems = [];

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public bool IsAvailable => Status == ItemStatus.Active && Category.Status == CategoryStatus.Active;

    private Item(
        Guid id,
        string name,
        string description,
        double portion,
        decimal price,
        ItemStatus status,
        Guid categoryId,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Name = name;
        Description = description;
        Portion = portion;
        Price = price;
        Status = status;
        CategoryId = categoryId;
    }

    public static Result<Item> Create(
        string name,
        string description,
        double portion,
        decimal price,
        ItemStatus status,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Item>(ItemErrors.NameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Item>(ItemErrors.DescriptionIsRequired);
        }

        if (portion <= 0)
        {
            return Result.Failure<Item>(ItemErrors.PortionMustBePositive);
        }

        if (price <= 0)
        {
            return Result.Failure<Item>(ItemErrors.PriceMustBePositive);
        }

        Item item = new(
            Guid.CreateVersion7(),
            name,
            description,
            portion,
            price,
            status,
            categoryId,
            DateTimeOffset.UtcNow);

        return Result.Success(item);
    }

    public Result Update(
        string name,
        string description,
        double portion,
        decimal price,
        ItemStatus status,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ItemErrors.NameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(ItemErrors.DescriptionIsRequired);
        }

        if (portion <= 0)
        {
            return Result.Failure(ItemErrors.PortionMustBePositive);
        }

        if (price <= 0)
        {
            return Result.Failure(ItemErrors.PriceMustBePositive);
        }

        Name = name;
        Description = description;
        Portion = portion;
        Price = price;
        Status = status;
        CategoryId = categoryId;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result SetImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Result.Failure(ItemErrors.ImageUrlIsRequired);
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