using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Fixtures;

public sealed record CreateFixtureCommand(
    Guid AreaId,
    FixtureType Type,
    string? Label,
    int X,
    int Y,
    int Width,
    int Height)
    : ICommand<FixtureDto>, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class CreateFixtureCommandHandler(
    IFixtureRepository fixtureRepository,
    IAreaRepository areaRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateFixtureCommand, FixtureDto>
{
    public async Task<Result<FixtureDto>> Handle(CreateFixtureCommand command, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetAsync(command.AreaId, cancellationToken);

        if (area is null)
        {
            return Result.Failure<FixtureDto>(AreaErrors.NotFound);
        }

        Result<Fixture> result = Fixture.Create(
            area,
            command.Type,
            command.Label,
            command.X,
            command.Y,
            command.Width,
            command.Height);

        if (result.IsFailure)
        {
            return Result.Failure<FixtureDto>(result.Error);
        }

        await fixtureRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var fixtureDto = result.Value.ToFixtureDto();

        return Result.Success(fixtureDto);
    }
}