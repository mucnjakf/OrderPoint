using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.TableRequests;

// A table's open requests, for the guests at it; not cached, TablesChanged says when to reload
public sealed record GetTableRequestsByCodeQuery(string TableCode) : IQuery<IReadOnlyList<TableRequestDto>>;

internal sealed class GetTableRequestsByCodeQueryHandler(
    ITableRepository tableRepository,
    ITableRequestRepository tableRequestRepository)
    : IQueryHandler<GetTableRequestsByCodeQuery, IReadOnlyList<TableRequestDto>>
{
    public async Task<Result<IReadOnlyList<TableRequestDto>>> Handle(
        GetTableRequestsByCodeQuery query,
        CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetByCodeAsync(query.TableCode, cancellationToken);

        if (table is null)
        {
            return Result.Failure<IReadOnlyList<TableRequestDto>>(TableErrors.NotFound);
        }

        IReadOnlyList<TableRequest> requests =
            await tableRequestRepository.GetByTableAsync(table.Id, cancellationToken);

        return Result.Success<IReadOnlyList<TableRequestDto>>(requests
            .Where(request => request.Status == TableRequestStatus.Open)
            .Select(request => request.ToTableRequestDto(table.Code))
            .ToList());
    }
}