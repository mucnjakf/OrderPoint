namespace OrderPoint.Admin.Features.Shared.Dtos;

public sealed record PaginationDto<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    internal int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    internal bool HasPreviousPage => PageNumber > 1;

    internal bool HasNextPage => PageNumber < TotalPages;
}