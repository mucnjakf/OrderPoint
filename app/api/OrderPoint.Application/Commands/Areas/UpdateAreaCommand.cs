using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Areas;

public sealed record UpdateAreaCommand(Guid Id, string Name, int Columns, int Rows)
    : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class UpdateAreaCommandHandler(IAreaRepository areaRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAreaCommand>
{
    public async Task<Result> Handle(UpdateAreaCommand command, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetAsync(command.Id, cancellationToken);

        if (area is null)
        {
            return Result.Failure(AreaErrors.NotFound);
        }

        if (command.Name != area.Name)
        {
            bool nameExists = await areaRepository.ExistsByNameAsync(command.Name, cancellationToken);

            if (nameExists)
            {
                return Result.Failure(AreaErrors.NameAlreadyExists);
            }
        }

        Result result = area.Update(command.Name, command.Columns, command.Rows);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}