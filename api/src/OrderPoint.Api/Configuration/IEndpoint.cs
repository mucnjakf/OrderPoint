namespace OrderPoint.Api.Configuration;

internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}