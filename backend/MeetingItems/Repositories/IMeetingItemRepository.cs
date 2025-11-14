#nullable enable
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Repositories;

public interface IMeetingItemRepository
{
    Task<MeetingItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<MeetingItem?> GetByIdWithDocumentsAsync(string id, CancellationToken cancellationToken = default);
    Task<List<MeetingItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(MeetingItem meetingItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(MeetingItem meetingItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
