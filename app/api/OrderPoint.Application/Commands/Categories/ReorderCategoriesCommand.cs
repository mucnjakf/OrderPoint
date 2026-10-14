using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record ReorderCategoriesCommand(IReadOnlyList<Guid> Ids)
    : ICommand, ICacheInvalidatingCommand, IMenuNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.CategoryChanges;
}

internal sealed class ReorderCategoriesCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderCategoriesCommand>
{
    public async Task<Result> Handle(ReorderCategoriesCommand command, CancellationToken cancellationToken)
    {
        int categoriesCount = await categoryRepository.CountAsync(cancellationToken);

        if (categoriesCount != command.Ids.Count)
        {
            return Result.Failure(CategoryErrors.DisplayOrderMustIncludeAllCategories);
        }

        IReadOnlyList<Category> categories = await categoryRepository.GetAsync(command.Ids, cancellationToken);

        if (categories.Count != command.Ids.Count)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        Dictionary<Guid, Category> categoriesById = categories.ToDictionary(category => category.Id);

        for (int i = 0; i < command.Ids.Count; i++)
        {
            categoriesById[command.Ids[i]].ChangeDisplayOrder(i + 1);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}