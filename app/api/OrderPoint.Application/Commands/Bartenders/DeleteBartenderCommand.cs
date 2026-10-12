using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record DeleteBartenderCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.BartenderChanges;
}

internal sealed partial class DeleteBartenderCommandHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBartenderCommandHandler> logger)
    : ICommandHandler<DeleteBartenderCommand>
{
    public async Task<Result> Handle(DeleteBartenderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        bool hasOrders = await orderRepository.ExistsByBartenderAsync(bartender.Id, cancellationToken);

        if (hasOrders)
        {
            return Result.Failure(BartenderErrors.CannotDeleteBartenderWithOrders);
        }

        await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
        await identityService.DeleteUserAsync(bartender.Id, cancellationToken);
        bartenderRepository.Delete(bartender);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (bartender.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(bartender.ImageUrl, cancellationToken);
        }

        LogBartenderDeleted(bartender.Id, bartender.Email);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Bartender {BartenderId} ({Email}) and their login deleted")]
    private partial void LogBartenderDeleted(Guid bartenderId, string email);
}