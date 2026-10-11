namespace OrderPoint.Application.Dtos;

/// <summary>A page of results.</summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="PageNumber">The page number, starting at 1.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <param name="TotalCount">The number of items matching the filters, across all pages.</param>
public sealed record PaginationDto<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    /// <summary>The number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a page before this one exists.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Whether a page after this one exists.</summary>
    public bool HasNextPage => PageNumber < TotalPages;
}