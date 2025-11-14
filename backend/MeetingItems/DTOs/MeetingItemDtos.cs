#nullable enable
using System;
using System.Collections.Generic;

namespace MeetingItemsApp.MeetingItems.DTOs;

/// <summary>
/// DTO for listing meeting items (without full details)
/// </summary>
public record MeetingItemListItem(
    string Id,
    string Topic,
    string Purpose,
    string Outcome,
    string DigitalProduct,
    int Duration,
    string Requestor,
    string OwnerPresenter,
    string? Sponsor,
    DateTime SubmissionDate,
    string Status,
    string? TemplateId,
    int DocumentCount,
    DateTime CreatedAt,
    string CreatedBy);

/// <summary>
/// DTO for full meeting item details (with all documents)
/// </summary>
public record MeetingItemDetail(
    string Id,
    string Topic,
    string Purpose,
    string Outcome,
    string DigitalProduct,
    int Duration,
    string Requestor,
    string OwnerPresenter,
    string? Sponsor,
    DateTime SubmissionDate,
    string Status,
    string DecisionBoardId,
    string? TemplateId,
    List<DocumentDetail> Documents,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

/// <summary>
/// DTO for document details with version information
/// </summary>
public record DocumentDetail(
    string Id,
    string FileName,
    string OriginalFileName,
    string StoragePath,
    long FileSize,
    string ContentType,
    int Version,
    DateTime UploadDate,
    string UploadedBy,
    bool IsLatestVersion);

/// <summary>
/// DTO for document upload (used in commands)
/// </summary>
public record DocumentUploadDto(
    string FileName,
    string ContentType,
    long FileSize,
    string Base64Content);

/// <summary>
/// DTO for document upload response
/// </summary>
public record DocumentUploadResponse(
    string Id,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    int Version);
