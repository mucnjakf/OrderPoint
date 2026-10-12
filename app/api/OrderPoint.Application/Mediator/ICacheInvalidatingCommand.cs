namespace OrderPoint.Application.Mediator;

// A command whose change shows up in cached reads; CacheInvalidationBehavior removes these tags once it succeeds
internal interface ICacheInvalidatingCommand
{
    IReadOnlyList<string> InvalidatedCacheTags { get; }
}