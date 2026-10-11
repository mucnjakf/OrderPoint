using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record DeleteBartendersCommand(IReadOnlyList<Guid> Ids) : ICommand;

internal sealed class DeleteBartendersCommandHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBartendersCommand>
{
    public async Task<Result> Handle(DeleteBartendersCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Bartender> bartenders = await bartenderRepository.GetAsync(command.Ids, cancellationToken);

        if (bartenders.Count != command.Ids.Count)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        bool haveOrders = await orderRepository.ExistsByBartendersAsync(command.Ids, cancellationToken);

        if (haveOrders)
        {
            return Result.Failure(BartenderErrors.CannotDeleteBartenderWithOrders);
        }

        foreach (Bartender bartender in bartenders)
        {
            await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
            await identityService.DeleteUserAsync(bartender.Id, cancellationToken);
            bartenderRepository.Delete(bartender);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Bartender bartender in bartenders.Where(bartender => bartender.ImageUrl is not null))
        {
            await imageStorage.DeleteAsync(bartender.ImageUrl!, cancellationToken);
        }

        return Result.Success();
    }
}