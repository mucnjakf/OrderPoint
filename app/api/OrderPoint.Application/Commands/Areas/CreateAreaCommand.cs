using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Areas;

public sealed record CreateAreaCommand(string Name, int Columns, int Rows)
    : ICommand<AreaDto>, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class CreateAreaCommandHandler(IAreaRepository areaRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAreaCommand, AreaDto>
{
    public async Task<Result<AreaDto>> Handle(CreateAreaCommand command, CancellationToken cancellationToken)
    {
        bool nameExists = await areaRepository.ExistsByNameAsync(command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<AreaDto>(AreaErrors.NameAlreadyExists);
        }

        int maxDisplayOrder = await areaRepository.GetMaxDisplayOrderAsync(cancellationToken);

        Result<Area> result = Area.Create(command.Name, command.Columns, command.Rows, maxDisplayOrder + 1);

        if (result.IsFailure)
        {
            return Result.Failure<AreaDto>(result.Error);
        }

        await areaRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var areaDto = result.Value.ToAreaDto();

        return Result.Success(areaDto);
    }
}