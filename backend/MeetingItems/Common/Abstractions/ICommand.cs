#nullable enable
using MediatR;

namespace MeetingItemsApp.Common.Abstractions;

/// <summary>
/// Marker interface for commands
/// </summary>
public interface ICommand<TResponse> : IRequest<BusinessResult<TResponse>>
{
}

/// <summary>
/// Handler interface for commands
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, BusinessResult<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
