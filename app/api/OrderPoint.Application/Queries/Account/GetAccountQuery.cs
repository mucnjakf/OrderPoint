using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Account;

public sealed record GetAccountQuery(Guid UserId) : IQuery<AccountDto>;

internal sealed class GetAccountQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<Result<AccountDto>> Handle(GetAccountQuery query, CancellationToken cancellationToken)
    {
        UserDto? user = await identityService.GetUserAsync(query.UserId, cancellationToken);

        return user is null
            ? Result.Failure<AccountDto>(AuthErrors.UserNotFound)
            : Result.Success(new AccountDto(user.Email, user.ImageUrl));
    }
}