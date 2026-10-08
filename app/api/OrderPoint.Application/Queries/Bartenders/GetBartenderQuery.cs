using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Bartenders;

public sealed record GetBartenderQuery(Guid Id) : IQuery<BartenderDto>;

internal sealed class GetBartenderQueryHandler(IBartenderRepository bartenderRepository)
    : IQueryHandler<GetBartenderQuery, BartenderDto>
{
    public async Task<Result<BartenderDto>> Handle(GetBartenderQuery query, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(query.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure<BartenderDto>(BartenderErrors.NotFound);
        }

        var bartenderDto = bartender.ToBartenderDto();

        return Result.Success(bartenderDto);
    }
}