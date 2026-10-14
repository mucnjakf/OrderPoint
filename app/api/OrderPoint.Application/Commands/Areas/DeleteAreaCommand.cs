using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Areas;

public sealed record DeleteAreaCommand(Guid Id) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed partial class DeleteAreaCommandHandler(
    IAreaRepository areaRepository,
    ITableRepository tableRepository,
    IFixtureRepository fixtureRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteAreaCommandHandler> logger)
    : ICommandHandler<DeleteAreaCommand>
{
    public async Task<Result> Handle(DeleteAreaCommand command, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetAsync(command.Id, cancellationToken);

        if (area is null)
        {
            return Result.Failure(AreaErrors.NotFound);
        }

        bool containsTables = await tableRepository.ExistsByAreaAsync(area.Id, cancellationToken);

        if (containsTables)
        {
            return Result.Failure(AreaErrors.CannotDeleteAreaWithTables);
        }

        // Fixtures only belong to their area's floor plan, so they go with it
        foreach (Fixture fixture in area.Fixtures)
        {
            fixtureRepository.Delete(fixture);
        }

        areaRepository.Delete(area);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogAreaDeleted(area.Id, area.Name);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Area {AreaId} ({Name}) deleted")]
    private partial void LogAreaDeleted(Guid areaId, string name);
}