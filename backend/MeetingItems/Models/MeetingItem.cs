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
}
