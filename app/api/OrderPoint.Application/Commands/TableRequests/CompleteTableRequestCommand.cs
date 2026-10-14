using Microsoft.Extensions.Logging;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.TableRequests;

public sealed record CompleteTableRequestCommand(Guid Id, Guid BartenderId) : ICommand, ITablesNotifyingCommand;

internal sealed partial class CompleteTableRequestCommandHandler(
    ITableRequestRepository tableRequestRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ILogger<CompleteTableRequestCommandHandler> logger)
    : ICommandHandler<CompleteTableRequestCommand>
{
    public async Task<Result> Handle(CompleteTableRequestCommand command, CancellationToken cancellationToken)
    {
        TableRequest? request = await tableRequestRepository.GetAsync(command.Id, cancellationToken);

        if (request is null)
        {
            return Result.Failure(TableRequestErrors.NotFound);
        }

        Bartender? bartender = await bartenderRepository.GetAsync(command.BartenderId, cancellationToken);

        // The access token outlives a deletion by up to 15 minutes
        if (bartender is null)
        {
            return Result.Failure(AuthErrors.AccountDeleted);
        }

        // The access token outlives a deactivation by up to 15 minutes
        if (bartender.Status == BartenderStatus.Inactive)
        {
            return Result.Failure(AuthErrors.AccountInactive);
        }

        Result result = request.Complete();

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogTableRequestCompleted(request.Id, command.BartenderId);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Table request {TableRequestId} done by bartender {BartenderId}")]
    private partial void LogTableRequestCompleted(Guid tableRequestId, Guid bartenderId);
}