using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record CreateBartenderCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes)
    : ICommand<BartenderDto>;

internal sealed class CreateBartenderCommandHandler(IBartenderRepository bartenderRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBartenderCommand, BartenderDto>
{
    public async Task<Result<BartenderDto>> Handle(CreateBartenderCommand command, CancellationToken cancellationToken)
    {
        bool emailExists = await bartenderRepository.ExistsByEmailAsync(command.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<BartenderDto>(BartenderErrors.EmailAlreadyExists);
        }

        Result<Bartender> result = Bartender.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Status,
            command.Notes);

        if (result.IsFailure)
        {
            return Result.Failure<BartenderDto>(result.Error);
        }

        await bartenderRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var bartenderDto = result.Value.ToBartenderDto(ordersCount: 0);

        return Result.Success(bartenderDto);
    }
}