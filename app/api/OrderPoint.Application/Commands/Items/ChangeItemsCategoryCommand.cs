using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record ChangeItemsCategoryCommand(IReadOnlyList<Guid> Ids, Guid CategoryId) : ICommand;

internal sealed class ChangeItemsCategoryCommandHandler(
    IItemRepository itemRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeItemsCategoryCommand>
{
    public async Task<Result> Handle(ChangeItemsCategoryCommand command, CancellationToken cancellationToken)
    {
        bool categoryExists = await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        IReadOnlyList<Item> items = await itemRepository.GetAsync(command.Ids, cancellationToken);

        if (items.Count != command.Ids.Count)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        foreach (Item item in items)
        {
            item.ChangeCategory(command.CategoryId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}