using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Fixtures;

public sealed record UpdateFixtureCommand(Guid Id, FixtureType Type, string? Label)
    : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class UpdateFixtureCommandHandler(IFixtureRepository fixtureRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateFixtureCommand>
{
    public async Task<Result> Handle(UpdateFixtureCommand command, CancellationToken cancellationToken)
    {
        Fixture? fixture = await fixtureRepository.GetAsync(command.Id, cancellationToken);

        if (fixture is null)
        {
            return Result.Failure(FixtureErrors.NotFound);
        }

        fixture.Update(command.Type, command.Label);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}