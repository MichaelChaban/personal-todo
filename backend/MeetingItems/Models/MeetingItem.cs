#nullable enable
using System;
using System.Collections.Generic;

namespace MeetingItemsApp.MeetingItems.Models;

public class MeetingItem
{
    public string Id { get; set; } = string.Empty;

    // General Information
    public string Topic { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string DigitalProduct { get; set; } = string.Empty;

    // Participants
    public string Requestor { get; set; } = string.Empty;
    public string OwnerPresenter { get; set; } = string.Empty;
    public string? Sponsor { get; set; }

    // System Information
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = "Draft";

    // References
    public string DecisionBoardId { get; set; } = string.Empty;
    public string? TemplateId { get; set; }

    // Documents
    public List<Document> Documents { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Factory method to create a new MeetingItem
    /// </summary>
    public static MeetingItem Create(
        string decisionBoardId,
        string? templateId,
        string topic,
        string purpose,
        string outcome,
        string digitalProduct,
        int duration,
        string requestor,
        string ownerPresenter,
        string? sponsor,
        string createdBy)
    {
        return new MeetingItem
        {
            Id = Guid.NewGuid().ToString(),
            DecisionBoardId = decisionBoardId,
            TemplateId = templateId,
            Topic = topic,
            Purpose = purpose,
            Outcome = outcome,
            DigitalProduct = digitalProduct,
            Duration = duration,
            Requestor = requestor,
            OwnerPresenter = ownerPresenter,
            Sponsor = sponsor,
            SubmissionDate = DateTime.UtcNow,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            Documents = new List<Document>()
        };
    }

    /// <summary>
    /// Update the meeting item with new values
    /// </summary>
    public void Update(
        string topic,
        string purpose,
        string outcome,
        string digitalProduct,
        int duration,
        string ownerPresenter,
        string? sponsor,
        string status,
        string updatedBy)
    {
        Topic = topic;
        Purpose = purpose;
        Outcome = outcome;
        DigitalProduct = digitalProduct;
        Duration = duration;
        OwnerPresenter = ownerPresenter;
        Sponsor = sponsor;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

public class Document
{
    public string Id { get; set; } = string.Empty;
    public string MeetingItemId { get; set; } = string.Empty;

    // Original filename as uploaded by user
    public string OriginalFileName { get; set; } = string.Empty;

    // Backend-generated filename for storage
    public string FileName { get; set; } = string.Empty;

    // Full storage path/blob name
    public string StoragePath { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;

    // Version tracking
    public int Version { get; set; } = 1;
    public string? BaseDocumentId { get; set; } // Links to the original document for versions
    public bool IsLatestVersion { get; set; } = true;

    // Audit fields
    public DateTime UploadDate { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Factory method to create a new document (Version 1)
    /// </summary>
    public static Document Create(
        string meetingItemId,
        string originalFileName,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string uploadedBy)
    {
        return new Document
        {
            Id = Guid.NewGuid().ToString(),
            MeetingItemId = meetingItemId,
            OriginalFileName = originalFileName,
            FileName = fileName,
            StoragePath = storagePath,
            FileSize = fileSize,
            ContentType = contentType,
            Version = 1,
            BaseDocumentId = null,
            IsLatestVersion = true,
            UploadDate = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Factory method to create a new version of an existing document
    /// </summary>
    public static Document CreateVersion(
        string meetingItemId,
        string baseDocumentId,
        int version,
        string originalFileName,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string uploadedBy)
    {
        return new Document
        {
            Id = Guid.NewGuid().ToString(),
            MeetingItemId = meetingItemId,
            OriginalFileName = originalFileName,
            FileName = fileName,
            StoragePath = storagePath,
            FileSize = fileSize,
            ContentType = contentType,
            Version = version,
            BaseDocumentId = baseDocumentId,
            IsLatestVersion = true,
            UploadDate = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Mark this document as deleted (soft delete)
    /// </summary>
    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsLatestVersion = false;
    }

    /// <summary>
    /// Mark this document as not the latest version
    /// </summary>
    public void MarkAsOldVersion()
    {
        IsLatestVersion = false;
    }
}
