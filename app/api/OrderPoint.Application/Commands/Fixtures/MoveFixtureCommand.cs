using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Fixtures;

// A drag on the floor plan editor: position and size, possibly in another area
public sealed record MoveFixtureCommand(
    Guid Id,
    Guid AreaId,
    int X,
    int Y,
    int Width,
    int Height) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class MoveFixtureCommandHandler(
    IFixtureRepository fixtureRepository,
    IAreaRepository areaRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MoveFixtureCommand>
{
    public async Task<Result> Handle(MoveFixtureCommand command, CancellationToken cancellationToken)
    {
        Fixture? fixture = await fixtureRepository.GetAsync(command.Id, cancellationToken);

        if (fixture is null)
        {
            return Result.Failure(FixtureErrors.NotFound);
        }

        Area? area = await areaRepository.GetAsync(command.AreaId, cancellationToken);

        if (area is null)
        {
            return Result.Failure(AreaErrors.NotFound);
        }

        Result result = fixture.Place(area, command.X, command.Y, command.Width, command.Height);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}