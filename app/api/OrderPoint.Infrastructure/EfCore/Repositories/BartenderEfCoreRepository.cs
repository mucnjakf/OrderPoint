using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class BartenderEfCoreRepository(ApplicationDbContext dbContext) : IBartenderRepository
{
    public async Task<(IReadOnlyList<Bartender>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        BartenderStatus? status = null,
        BartenderSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Bartender> query = dbContext.Bartenders
            .AsNoTracking();

        query = SearchBartenders(query, searchQuery);
        query = FilterBartenders(query, status);
        query = SortBartenders(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Bartender> bartenders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (bartenders.AsReadOnly(), totalCount);
    }

    public async Task<Bartender?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Bartenders.SingleOrDefaultAsync(bartender => bartender.Id == id, cancellationToken);

    public async Task CreateAsync(Bartender bartender, CancellationToken cancellationToken = default)
        => await dbContext.Bartenders.AddAsync(bartender, cancellationToken);

    public void Delete(Bartender bartender)
        => dbContext.Bartenders.Remove(bartender);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await dbContext.Bartenders.AnyAsync(bartender => bartender.Email == email, cancellationToken);

    private static IQueryable<Bartender> SearchBartenders(IQueryable<Bartender> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(bartender =>
                bartender.FirstName.ToLower().Contains(normalizedSearchQuery) ||
                bartender.LastName.ToLower().Contains(normalizedSearchQuery) ||
                bartender.Email.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Bartender> FilterBartenders(IQueryable<Bartender> query, BartenderStatus? status)
    {
        if (status.HasValue)
        {
            query = query.Where(bartender => bartender.Status == status.Value);
        }

        return query;
    }

    private static IQueryable<Bartender> SortBartenders(IQueryable<Bartender> query, BartenderSortBy? sortBy)
    {
        IOrderedQueryable<Bartender> orderedQuery = sortBy switch
        {
            BartenderSortBy.NameAsc => query
                .OrderBy(bartender => bartender.LastName)
                .ThenBy(bartender => bartender.FirstName),
            BartenderSortBy.NameDesc => query
                .OrderByDescending(bartender => bartender.LastName)
                .ThenByDescending(bartender => bartender.FirstName),
            BartenderSortBy.CreatedAtUtcAsc => query.OrderBy(bartender => bartender.CreatedAtUtc),
            BartenderSortBy.CreatedAtUtcDesc => query.OrderByDescending(bartender => bartender.CreatedAtUtc),
            _ => query.OrderByDescending(bartender => bartender.CreatedAtUtc)
        };

        return orderedQuery.ThenBy(bartender => bartender.Id);
    }
}