using MediatR;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

internal interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>, IBaseCommandHandler
    where TCommand : ICommand;

internal interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>, IBaseCommandHandler
    where TCommand : ICommand<TResponse>;

internal interface IBaseCommandHandler;