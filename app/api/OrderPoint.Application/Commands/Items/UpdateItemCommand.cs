using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record UpdateItemCommand(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId) : ICommand;

internal sealed class UpdateItemCommandHandler(
    IItemRepository itemRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateItemCommand>
{
    public async Task<Result> Handle(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        bool categoryExists = await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        Result result = item.Update(
            command.Name,
            command.Description,
            command.Portion,
            command.Price,
            command.ImageUrl,
            command.CategoryId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}