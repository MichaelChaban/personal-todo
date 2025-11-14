#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.DTOs;
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Services;

public interface IDocumentService
{
    Task<List<Document>> UploadDocumentsAsync(
        string meetingItemId,
        IEnumerable<DocumentUploadDto> documents,
        string uploadedBy,
        CancellationToken cancellationToken);

    Task<Document> UploadNewVersionAsync(
        string meetingItemId,
        string baseDocumentId,
        int version,
        DocumentUploadDto document,
        string uploadedBy,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(
        Document document,
        string deletedBy,
        CancellationToken cancellationToken);

    string GenerateFileName(string originalFileName);
    string GenerateStoragePath(string meetingItemId, string fileName);
}

public class DocumentService : IDocumentService
{
    private readonly IBlobContainerClientFactory _containerClientFactory;
    private const string ContainerName = "meeting-item-documents";

    public DocumentService(IBlobContainerClientFactory containerClientFactory)
    {
        _containerClientFactory = containerClientFactory ?? throw new ArgumentNullException(nameof(containerClientFactory));
    }

    public async Task<List<Document>> UploadDocumentsAsync(
        string meetingItemId,
        IEnumerable<DocumentUploadDto> documents,
        string uploadedBy,
        CancellationToken cancellationToken)
    {
        var client = _containerClientFactory.GetBlobContainerClient(ContainerName);

        var uploadTasks = documents.Select(async docDto =>
        {
            // Extract base64 content
            var base64Content = ExtractBase64Content(docDto.Base64Content);
            var fileContent = Convert.FromBase64String(base64Content);

            // Generate backend-controlled filename and storage path
            var fileName = GenerateFileName(docDto.FileName);
            var storagePath = GenerateStoragePath(meetingItemId, fileName);

            // Upload to blob storage
            using var stream = new MemoryStream(fileContent);
            await client.UploadAsync(storagePath, stream, cancellationToken);

            // Create document entity using factory method
            return Document.Create(
                meetingItemId,
                docDto.FileName,
                fileName,
                storagePath,
                fileContent.Length,
                docDto.ContentType,
                uploadedBy);
        });

        return (await Task.WhenAll(uploadTasks)).ToList();
    }

    public async Task<Document> UploadNewVersionAsync(
        string meetingItemId,
        string baseDocumentId,
        int version,
        DocumentUploadDto document,
        string uploadedBy,
        CancellationToken cancellationToken)
    {
        var client = _containerClientFactory.GetBlobContainerClient(ContainerName);

        // Extract base64 content
        var base64Content = ExtractBase64Content(document.Base64Content);
        var fileContent = Convert.FromBase64String(base64Content);

        // Generate filename and storage path
        var fileName = GenerateFileName(document.FileName);
        var storagePath = GenerateStoragePath(meetingItemId, fileName);

        // Upload to blob storage
        using var stream = new MemoryStream(fileContent);
        await client.UploadAsync(storagePath, stream, cancellationToken);

        // Create new version document using factory method
        return Document.CreateVersion(
            meetingItemId,
            baseDocumentId,
            version,
            document.FileName,
            fileName,
            storagePath,
            fileContent.Length,
            document.ContentType,
            uploadedBy);
    }

    public async Task DeleteDocumentAsync(
        Document document,
        string deletedBy,
        CancellationToken cancellationToken)
    {
        var client = _containerClientFactory.GetBlobContainerClient(ContainerName);

        // Soft delete using domain method
        document.MarkAsDeleted(deletedBy);

        // Also delete from blob storage
        await client.DeleteAsync(document.StoragePath, cancellationToken);
    }

    public string GenerateFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        // Sanitize filename: remove special characters
        var sanitized = SanitizeFileName(fileNameWithoutExtension);

        // Generate unique filename: {sanitized-name}_{timestamp}_{guid}{extension}
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

        return $"{sanitized}_{timestamp}_{uniqueId}{extension}";
    }

    public string GenerateStoragePath(string meetingItemId, string fileName)
    {
        // Structure: meeting-items/{meetingItemId}/{fileName}
        return $"meeting-items/{meetingItemId}/{fileName}";
    }

    private static string ExtractBase64Content(string base64Content)
    {
        // Remove data URL prefix if present (e.g., "data:image/png;base64,...")
        return base64Content.Contains(',')
            ? base64Content.Split(',')[1]
            : base64Content;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters and limit length
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Replace spaces with underscores
        sanitized = sanitized.Replace(" ", "_");

        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }

        return sanitized;
    }
}
