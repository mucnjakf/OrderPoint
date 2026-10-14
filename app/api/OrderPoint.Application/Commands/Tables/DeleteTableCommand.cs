using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Tables;

public sealed record DeleteTableCommand(Guid Id) : ICommand, ICacheInvalidatingCommand, ITablesNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.TableChanges;
}

internal sealed partial class DeleteTableCommandHandler(
    ITableRepository tableRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteTableCommandHandler> logger)
    : ICommandHandler<DeleteTableCommand>
{
    public async Task<Result> Handle(DeleteTableCommand command, CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetAsync(command.Id, cancellationToken);

        if (table is null)
        {
            return Result.Failure(TableErrors.NotFound);
        }

        bool hasOrders = await orderRepository.ExistsByTableAsync(table.Id, cancellationToken);

        if (hasOrders)
        {
            return Result.Failure(TableErrors.CannotDeleteTableWithOrders);
        }

        tableRepository.Delete(table);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogTableDeleted(table.Id, table.Code);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Table {TableId} ({Code}) deleted")]
    private partial void LogTableDeleted(Guid tableId, string code);
}