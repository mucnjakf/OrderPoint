using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class FixtureEfCoreRepository(ApplicationDbContext dbContext) : IFixtureRepository
{
    public async Task<Fixture?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Fixtures.SingleOrDefaultAsync(fixture => fixture.Id == id, cancellationToken);

    public async Task CreateAsync(Fixture fixture, CancellationToken cancellationToken = default)
        => await dbContext.Fixtures.AddAsync(fixture, cancellationToken);

    public void Delete(Fixture fixture)
        => dbContext.Fixtures.Remove(fixture);
}