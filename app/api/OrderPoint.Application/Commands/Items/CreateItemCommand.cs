using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record CreateItemCommand(
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId)
    : ICommand<ItemDto>;

internal sealed class CreateItemCommandHandler(
    IItemRepository itemRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateItemCommand, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        bool categoryExists = await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<ItemDto>(CategoryErrors.NotFound);
        }

        Result<Item> result = Item.Create(
            command.Name,
            command.Description,
            command.Portion,
            command.Price,
            command.ImageUrl,
            command.CategoryId);

        if (result.IsFailure)
        {
            return Result.Failure<ItemDto>(result.Error);
        }

        await itemRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Item item = (await itemRepository.GetAsync(result.Value.Id, cancellationToken))!;

        var itemDto = item.ToItemDto();

        return Result.Success(itemDto);
    }
}