using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record UpdateBartenderCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes,
    string? ImageUrl) : ICommand;

internal sealed class UpdateBartenderCommandHandler(IBartenderRepository bartenderRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBartenderCommand>
{
    public async Task<Result> Handle(UpdateBartenderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        if (command.Email != bartender.Email)
        {
            bool emailExists = await bartenderRepository.ExistsAsync(command.Email, cancellationToken);

            if (emailExists)
            {
                return Result.Failure(BartenderErrors.EmailAlreadyExists);
            }
        }

        Result result = bartender.Update(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Status,
            command.Notes,
            command.ImageUrl);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}