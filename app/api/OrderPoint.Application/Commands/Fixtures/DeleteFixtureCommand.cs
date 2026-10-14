using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Fixtures;

public sealed record DeleteFixtureCommand(Guid Id) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class DeleteFixtureCommandHandler(IFixtureRepository fixtureRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteFixtureCommand>
{
    public async Task<Result> Handle(DeleteFixtureCommand command, CancellationToken cancellationToken)
    {
        Fixture? fixture = await fixtureRepository.GetAsync(command.Id, cancellationToken);

        if (fixture is null)
        {
            return Result.Failure(FixtureErrors.NotFound);
        }

        fixtureRepository.Delete(fixture);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}