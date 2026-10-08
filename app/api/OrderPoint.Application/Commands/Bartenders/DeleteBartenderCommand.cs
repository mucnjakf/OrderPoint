using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record DeleteBartenderCommand(Guid Id) : ICommand;

internal sealed class DeleteBartenderCommandHandler(IBartenderRepository bartenderRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBartenderCommand>
{
    public async Task<Result> Handle(DeleteBartenderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        bartenderRepository.Delete(bartender);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}