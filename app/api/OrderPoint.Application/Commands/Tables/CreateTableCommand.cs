using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Tables;

public sealed record CreateTableCommand(
    string Code,
    Guid AreaId,
    int Seats,
    TableStatus Status,
    TableShape Shape,
    int X,
    int Y,
    int Width,
    int Height)
    : ICommand<TableDto>, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed class CreateTableCommandHandler(
    ITableRepository tableRepository,
    IAreaRepository areaRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTableCommand, TableDto>
{
    public async Task<Result<TableDto>> Handle(CreateTableCommand command, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetAsync(command.AreaId, cancellationToken);

        if (area is null)
        {
            return Result.Failure<TableDto>(AreaErrors.NotFound);
        }

        bool codeExists = await tableRepository.ExistsByCodeAsync(command.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<TableDto>(TableErrors.CodeAlreadyExists);
        }

        Result<Table> result = Table.Create(
            command.Code,
            area,
            command.Seats,
            command.Status,
            command.Shape,
            command.X,
            command.Y,
            command.Width,
            command.Height);

        if (result.IsFailure)
        {
            return Result.Failure<TableDto>(result.Error);
        }

        await tableRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tableDto = result.Value.ToTableDto();

        return Result.Success(tableDto);
    }
}