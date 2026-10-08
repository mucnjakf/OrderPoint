using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Auth;

public sealed record RefreshTokensCommand(string RefreshToken) : ICommand<AuthTokensDto>;

internal sealed class RefreshTokensCommandHandler(ITokenService tokenService, IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokensCommand, AuthTokensDto>
{
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokensCommand command, CancellationToken cancellationToken)
    {
        Result<AuthTokensDto> result = await tokenService.RefreshTokensAsync(command.RefreshToken, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value);
    }
}