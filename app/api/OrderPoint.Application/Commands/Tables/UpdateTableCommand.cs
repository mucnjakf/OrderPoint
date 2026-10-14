using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Tables;

public sealed record UpdateTableCommand(
    Guid Id,
    string Code,
    int Seats,
    TableStatus Status,
    TableShape Shape) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class UpdateTableCommandHandler(ITableRepository tableRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTableCommand>
{
    public async Task<Result> Handle(UpdateTableCommand command, CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetAsync(command.Id, cancellationToken);

        if (table is null)
        {
            return Result.Failure(TableErrors.NotFound);
        }

        if (command.Code != table.Code)
        {
            bool codeExists = await tableRepository.ExistsByCodeAsync(command.Code, cancellationToken);

            if (codeExists)
            {
                return Result.Failure(TableErrors.CodeAlreadyExists);
            }
        }

        Result result = table.Update(command.Code, command.Seats, command.Status, command.Shape);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}