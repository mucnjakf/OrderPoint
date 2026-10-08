using OrderPoint.Application.Identity;
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
    string? Notes) : ICommand;

internal sealed class UpdateBartenderCommandHandler(
    IBartenderRepository bartenderRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBartenderCommand>
{
    public async Task<Result> Handle(UpdateBartenderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        bool isEmailChanged = command.Email != bartender.Email;
        bool isDeactivated = bartender.Status == BartenderStatus.Active && command.Status == BartenderStatus.Inactive;

        if (isEmailChanged)
        {
            bool emailExists = await bartenderRepository.ExistsByEmailAsync(command.Email, cancellationToken);

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
            command.Notes);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (isEmailChanged)
        {
            Result userResult = await identityService.UpdateEmailAsync(bartender.Id, command.Email, cancellationToken);

            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Error);
            }
        }

        if (isDeactivated)
        {
            await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}