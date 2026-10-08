using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record DeleteBartenderCommand(Guid Id) : ICommand;

internal sealed class DeleteBartenderCommandHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
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

        bartenderRepository.Delete(bartender);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (bartender.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(bartender.ImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}