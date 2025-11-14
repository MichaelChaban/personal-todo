#nullable enable
namespace MeetingItemsApp.MeetingItems.Repositories;

public interface IDecisionBoardRepository
{
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
