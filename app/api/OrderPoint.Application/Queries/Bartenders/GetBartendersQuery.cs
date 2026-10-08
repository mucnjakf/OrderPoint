using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Bartenders;

public sealed record GetBartendersQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    BartenderStatus? Status,
    BartenderSortBy? SortBy)
    : IQuery<PaginationDto<BartenderDto>>;

internal sealed class GetBartendersQueryHandler(IBartenderRepository bartenderRepository)
    : IQueryHandler<GetBartendersQuery, PaginationDto<BartenderDto>>
{
    public async Task<Result<PaginationDto<BartenderDto>>> Handle(
        GetBartendersQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Bartender> bartenders, int totalCount) = await bartenderRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<BartenderDto>(
            bartenders.Select(bartender => bartender.ToBartenderDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}