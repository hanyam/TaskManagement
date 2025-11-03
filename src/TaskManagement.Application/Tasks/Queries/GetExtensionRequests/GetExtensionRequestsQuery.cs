using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tasks.Queries.GetExtensionRequests;

/// <summary>
///     Query for getting extension requests.
/// </summary>
public record GetExtensionRequestsQuery : IQuery<List<ExtensionRequestDto>>
{
    public Guid? TaskId { get; init; }
    public ExtensionRequestStatus? Status { get; init; }
    public Guid? UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

