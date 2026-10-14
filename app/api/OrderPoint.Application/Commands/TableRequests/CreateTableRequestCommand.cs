using Microsoft.Extensions.Logging;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.TableRequests;

public sealed record CreateTableRequestCommand(string TableCode, TableRequestType Type)
    : ICommand<TableRequestDto>, ITablesNotifyingCommand;

internal sealed partial class CreateTableRequestCommandHandler(
    ITableRepository tableRepository,
    ITableRequestRepository tableRequestRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateTableRequestCommandHandler> logger)
    : ICommandHandler<CreateTableRequestCommand, TableRequestDto>
{
    public async Task<Result<TableRequestDto>> Handle(
        CreateTableRequestCommand command,
        CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetByCodeAsync(command.TableCode, cancellationToken);

        if (table is null)
        {
            return Result.Failure<TableRequestDto>(TableErrors.NotFound);
        }

        IReadOnlyList<TableRequest> requests =
            await tableRequestRepository.GetByTableAsync(table.Id, cancellationToken);

        // Asking again while a bartender is on the way changes nothing
        TableRequest? openRequest = requests.FirstOrDefault(request =>
            request.Type == command.Type && request.Status == TableRequestStatus.Open);

        if (openRequest is not null)
        {
            return Result.Success(openRequest.ToTableRequestDto(table.Code));
        }

        Result<TableRequest> result = TableRequest.Create(table, command.Type);

        if (result.IsFailure)
        {
            return Result.Failure<TableRequestDto>(result.Error);
        }

        await tableRequestRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogTableRequestCreated(command.Type, table.Code);

        return Result.Success(result.Value.ToTableRequestDto(table.Code));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{RequestType} requested at table {TableCode}")]
    private partial void LogTableRequestCreated(TableRequestType requestType, string tableCode);
}