using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Tables;

// A drag on the floor plan editor: position and size, possibly in another area
public sealed record MoveTableCommand(
    Guid Id,
    Guid AreaId,
    int X,
    int Y,
    int Width,
    int Height) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class MoveTableCommandHandler(
    ITableRepository tableRepository,
    IAreaRepository areaRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MoveTableCommand>
{
    public async Task<Result> Handle(MoveTableCommand command, CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetAsync(command.Id, cancellationToken);

        if (table is null)
        {
            return Result.Failure(TableErrors.NotFound);
        }

        Area? area = await areaRepository.GetAsync(command.AreaId, cancellationToken);

        if (area is null)
        {
            return Result.Failure(AreaErrors.NotFound);
        }

        Result result = table.Place(area, command.X, command.Y, command.Width, command.Height);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}