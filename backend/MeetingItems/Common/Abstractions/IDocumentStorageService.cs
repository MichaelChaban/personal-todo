#nullable enable
namespace MeetingItemsApp.Common.Abstractions;

public interface IDocumentStorageService
{
    Task<string> UploadAsync(string filePath, byte[] fileContent, string contentType, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
