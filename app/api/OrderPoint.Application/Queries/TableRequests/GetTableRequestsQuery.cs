using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.TableRequests;

// Every open request, oldest first, for the bar app; not cached, TablesChanged says when to reload
public sealed record GetTableRequestsQuery : IQuery<IReadOnlyList<TableRequestDto>>;

internal sealed class GetTableRequestsQueryHandler(ITableRequestRepository tableRequestRepository)
    : IQueryHandler<GetTableRequestsQuery, IReadOnlyList<TableRequestDto>>
{
    public async Task<Result<IReadOnlyList<TableRequestDto>>> Handle(
        GetTableRequestsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TableRequest> requests =
            await tableRequestRepository.GetByStatusAsync(TableRequestStatus.Open, cancellationToken);

        return Result.Success<IReadOnlyList<TableRequestDto>>(requests
            .Select(request => request.ToTableRequestDto(request.Table.Code))
            .ToList());
    }
}