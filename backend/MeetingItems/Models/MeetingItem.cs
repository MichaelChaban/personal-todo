#nullable enable
using System;
using System.Collections.Generic;

namespace MeetingItemsApp.MeetingItems.Models;

public class MeetingItem
{
    public string Id { get; private set; } = string.Empty;

    // General Information
    public string Topic { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public string Outcome { get; private set; } = string.Empty;
    public int Duration { get; private set; }
    public string DigitalProduct { get; private set; } = string.Empty;

    // Participants
    public string Requestor { get; private set; } = string.Empty;
    public string OwnerPresenter { get; private set; } = string.Empty;
    public string? Sponsor { get; private set; }

    // System Information
    public DateTime SubmissionDate { get; private set; }
    public string Status { get; private set; } = "Draft";

    // References
    public string DecisionBoardId { get; private set; } = string.Empty;
    public string? TemplateId { get; private set; }

    // Documents
    private readonly List<Document> _documents = new();
    public IReadOnlyList<Document> Documents => _documents.AsReadOnly();

    // Audit
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Private constructor for EF Core
    private MeetingItem() { }

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
            CreatedBy = createdBy
        };
    }

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

    public void AddDocument(Document document)
    {
        _documents.Add(document);
    }

    public void RemoveDocument(string documentId)
    {
        var document = _documents.Find(d => d.Id == documentId);
        if (document != null)
        {
            _documents.Remove(document);
        }
    }
}

public class Document
{
    public string Id { get; private set; } = string.Empty;
    public string MeetingItemId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime UploadDate { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;

    // Private constructor for EF Core
    private Document() { }

    public static Document Create(
        string meetingItemId,
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        string uploadedBy)
    {
        return new Document
        {
            Id = Guid.NewGuid().ToString(),
            MeetingItemId = meetingItemId,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            ContentType = contentType,
            UploadDate = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };
    }
}
