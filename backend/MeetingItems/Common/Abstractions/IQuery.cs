#nullable enable
using MediatR;

namespace MeetingItemsApp.Common.Abstractions;

/// <summary>
/// Marker interface for queries
/// </summary>
public interface IQuery<TResponse> : IRequest<BusinessResult<TResponse>>
{
}

/// <summary>
/// Handler interface for queries
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, BusinessResult<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
