#nullable enable
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}

public class Template
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
