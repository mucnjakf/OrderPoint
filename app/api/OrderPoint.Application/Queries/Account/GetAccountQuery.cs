using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Account;

public sealed record GetAccountQuery(Guid UserId) : IQuery<AccountDto>;

internal sealed class GetAccountQueryHandler(IIdentityService identityService, IBartenderRepository bartenderRepository)
    : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<Result<AccountDto>> Handle(GetAccountQuery query, CancellationToken cancellationToken)
    {
        UserDto? user = await identityService.GetUserAsync(query.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AccountDto>(AuthErrors.UserNotFound);
        }

        if (user.Role != UserRole.Bartender)
        {
            return Result.Success(new AccountDto(user.Email, user.ImageUrl, null, null, null));
        }

        // A bartender's profile is set by the admin on the bartender, whose id is the user's id
        Bartender? bartender = await bartenderRepository.GetAsync(user.Id, cancellationToken);

        return Result.Success(new AccountDto(
            user.Email,
            bartender?.ImageUrl,
            bartender?.FirstName,
            bartender?.LastName,
            bartender?.PhoneNumber));
    }
}