using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record ChangeCategoriesStatusCommand(IReadOnlyList<Guid> Ids, CategoryStatus Status) : ICommand;

internal sealed class ChangeCategoriesStatusCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeCategoriesStatusCommand>
{
    public async Task<Result> Handle(ChangeCategoriesStatusCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await categoryRepository.GetAsync(command.Ids, cancellationToken);

        if (categories.Count != command.Ids.Count)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        foreach (Category category in categories)
        {
            category.ChangeStatus(command.Status);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}