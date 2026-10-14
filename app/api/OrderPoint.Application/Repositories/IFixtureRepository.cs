using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface IFixtureRepository
{
    Task<Fixture?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Fixture fixture, CancellationToken cancellationToken = default);

    void Delete(Fixture fixture);
}