# Shared DTOs and Document Versioning Implementation

## Overview

This document describes the implementation of shared DTOs, backend-controlled file naming, and comprehensive document versioning system for the Meeting Items application.

## 🎯 Key Changes

### 1. Shared DTOs (`backend/MeetingItems/DTOs/MeetingItemDtos.cs`)

All response models are now centralized in shared DTOs instead of being defined within each feature class.

#### MeetingItemListItem
Used for listing meeting items (without full document details):
- Includes all meeting item fields
- `DocumentCount` instead of full document list
- Used in list views for performance

#### MeetingItemDetail
Used for full meeting item details:
- Includes all meeting item fields
- Full `List<DocumentDetail>` with version information
- Includes audit fields (UpdatedAt, UpdatedBy)

#### DocumentDetail
Used in detail responses with full version tracking:
```csharp
public record DocumentDetail(
    string Id,
    string FileName,              // Backend-generated filename
    string OriginalFileName,      // User's original filename
    string StoragePath,           // Full blob storage path
    long FileSize,
    string ContentType,
    int Version,                  // Version number (1, 2, 3...)
    DateTime UploadDate,
    string UploadedBy,
    bool IsLatestVersion          // Flag for latest version
);
```

#### DocumentUploadDto
Used in commands for uploading documents:
```csharp
public record DocumentUploadDto(
    string FileName,              // User's filename
    string ContentType,
    long FileSize,
    string Base64Content
);
```

#### DocumentUploadResponse
Returned after successful upload:
```csharp
public record DocumentUploadResponse(
    string Id,
    string FileName,              // Backend-generated
    string OriginalFileName,      // User's original
    long FileSize,
    string ContentType,
    int Version
);
```

### 2. Enhanced Document Model

The `Document` entity now supports full version tracking:

```csharp
public class Document
{
    public string Id { get; set; }
    public string MeetingItemId { get; set; }

    // File naming - backend controlled
    public string OriginalFileName { get; set; }  // "report.pdf"
    public string FileName { get; set; }          // "report_20250114123456_a1b2c3d4.pdf"
    public string StoragePath { get; set; }       // "meeting-items/{id}/report_20250114123456_a1b2c3d4.pdf"

    public long FileSize { get; set; }
    public string ContentType { get; set; }

    // Version tracking
    public int Version { get; set; } = 1;
    public string? BaseDocumentId { get; set; }   // Links to original document
    public bool IsLatestVersion { get; set; } = true;

    // Audit and soft delete
    public DateTime UploadDate { get; set; }
    public string UploadedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### 3. DocumentService

Centralized service for all document operations with backend-controlled naming:

#### File Naming Strategy

**GenerateFileName**:
```csharp
// Input: "My Report.pdf"
// Output: "My_Report_20250114123456_a1b2c3d4.pdf"

var sanitized = SanitizeFileName(fileNameWithoutExtension);
var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
return $"{sanitized}_{timestamp}_{uniqueId}{extension}";
```

**GenerateStoragePath**:
```csharp
// Output: "meeting-items/{meetingItemId}/{fileName}"
return $"meeting-items/{meetingItemId}/{fileName}";
```

#### Key Methods

**UploadDocumentsAsync** - For new documents:
- Generates backend-controlled filename
- Creates storage path
- Uploads to blob storage
- Creates Document entity with Version = 1

**UploadNewVersionAsync** - For document versions:
- Takes BaseDocumentId parameter
- Generates new filename (different from original)
- Links to base document via BaseDocumentId
- Version number set by caller

**DeleteDocumentAsync** - Soft delete:
- Sets `IsDeleted = true`
- Sets `DeletedDate` and `DeletedBy`
- Sets `IsLatestVersion = false`
- Removes from blob storage

### 4. Create Meeting Item

**Command**:
```csharp
public record Command(
    string DecisionBoardId,
    string? TemplateId,
    string Topic,
    string Purpose,
    string Outcome,
    string DigitalProduct,
    int Duration,
    string Requestor,
    string OwnerPresenter,
    string? Sponsor,
    List<DocumentUploadDto>? Documents = null) : ICommand<Response>;
```

**Response**:
```csharp
public record Response(
    string MeetingItemId,
    List<DocumentUploadResponse> UploadedDocuments);
```

**Behavior**:
- Only uploads new documents (no versioning needed for new items)
- Backend generates all filenames
- All documents get Version = 1

### 5. Update Meeting Item

**Command**:
```csharp
public record DocumentVersionUpdate(
    string BaseDocumentId,
    DocumentUploadDto Document);

public record Command(
    string Id,
    string Topic,
    // ... other fields
    List<DocumentUploadDto>? NewDocuments = null,        // Brand new documents
    List<DocumentVersionUpdate>? DocumentVersions = null, // New versions of existing docs
    List<string>? DocumentsToDelete = null) : ICommand<Response>;
```

**Response**:
```csharp
public record Response(
    string MeetingItemId,
    List<DocumentUploadResponse> NewlyUploadedDocuments,  // Brand new docs
    List<DocumentUploadResponse> VersionedDocuments,      // New versions
    List<string> DeletedDocumentIds);
```

**Three Types of Document Operations**:

1. **Delete Documents** (`DocumentsToDelete`):
   - Soft deletes specified documents
   - Removes from blob storage
   - Keeps in database with IsDeleted = true

2. **Upload New Documents** (`NewDocuments`):
   - Brand new documents (not versions)
   - Get Version = 1
   - Independent of existing documents

3. **Upload Document Versions** (`DocumentVersions`):
   - New version of existing document
   - Links via BaseDocumentId
   - Previous version marked as IsLatestVersion = false
   - Version number incremented

**Version Logic**:
```csharp
// Find all versions of the document
var allVersions = meetingItem.Documents
    .Where(d => (d.Id == baseDocumentId || d.BaseDocumentId == baseDocumentId) && !d.IsDeleted)
    .ToList();

// Get next version number
var maxVersion = allVersions.Max(d => d.Version);
newVersion.Version = maxVersion + 1;

// Mark old version as not latest
baseDocument.IsLatestVersion = false;
```

### 6. Entity Framework Configuration

Update `DocumentConfiguration.cs` to include new fields:

```csharp
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        // File fields
        builder.Property(d => d.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(300).IsRequired();
        builder.Property(d => d.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();

        // Version tracking
        builder.Property(d => d.Version).IsRequired().HasDefaultValue(1);
        builder.Property(d => d.BaseDocumentId).HasMaxLength(50);
        builder.Property(d => d.IsLatestVersion).IsRequired().HasDefaultValue(true);

        // Soft delete
        builder.Property(d => d.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.DeletedBy).HasMaxLength(50);

        // Indexes
        builder.HasIndex(d => d.MeetingItemId);
        builder.HasIndex(d => d.BaseDocumentId);
        builder.HasIndex(d => d.IsLatestVersion);
        builder.HasIndex(d => d.IsDeleted);
        builder.HasIndex(d => d.UploadDate);
    }
}
```

## 📋 Usage Examples

### Creating Meeting Item with Documents

**Request**:
```json
{
  "decisionBoardId": "board-123",
  "topic": "IT Security Review",
  "purpose": "Discuss Q4 security measures",
  "outcome": "Decision",
  "digitalProduct": "Banking App",
  "duration": 45,
  "requestor": "user123",
  "ownerPresenter": "user456",
  "sponsor": null,
  "documents": [
    {
      "fileName": "Security Report.pdf",
      "contentType": "application/pdf",
      "fileSize": 1024000,
      "base64Content": "data:application/pdf;base64,JVBERi0..."
    }
  ]
}
```

**Response**:
```json
{
  "meetingItemId": "item-abc",
  "uploadedDocuments": [
    {
      "id": "doc-123",
      "fileName": "Security_Report_20250114123456_a1b2c3d4.pdf",
      "originalFileName": "Security Report.pdf",
      "fileSize": 1024000,
      "contentType": "application/pdf",
      "version": 1
    }
  ]
}
```

### Updating with Document Operations

**Request**:
```json
{
  "id": "item-abc",
  "topic": "IT Security Review (Updated)",
  // ... other fields
  "newDocuments": [
    {
      "fileName": "Additional Notes.docx",
      "contentType": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "fileSize": 512000,
      "base64Content": "data:application/...,UEsDBBQABgA..."
    }
  ],
  "documentVersions": [
    {
      "baseDocumentId": "doc-123",
      "document": {
        "fileName": "Security Report.pdf",
        "contentType": "application/pdf",
        "fileSize": 1100000,
        "base64Content": "data:application/pdf;base64,JVBERi0..."
      }
    }
  ],
  "documentsToDelete": ["doc-old-456"]
}
```

**Response**:
```json
{
  "meetingItemId": "item-abc",
  "newlyUploadedDocuments": [
    {
      "id": "doc-789",
      "fileName": "Additional_Notes_20250114124500_b2c3d4e5.docx",
      "originalFileName": "Additional Notes.docx",
      "fileSize": 512000,
      "contentType": "application/vnd.openxmlformats...",
      "version": 1
    }
  ],
  "versionedDocuments": [
    {
      "id": "doc-124",
      "fileName": "Security_Report_20250114124530_c3d4e5f6.pdf",
      "originalFileName": "Security Report.pdf",
      "fileSize": 1100000,
      "contentType": "application/pdf",
      "version": 2
    }
  ],
  "deletedDocumentIds": ["doc-old-456"]
}
```

## 🔍 Document Version Tracking

For a document with multiple versions:

| ID | OriginalFileName | FileName | Version | BaseDocumentId | IsLatestVersion | IsDeleted |
|----|------------------|----------|---------|----------------|-----------------|-----------|
| doc-1 | report.pdf | report_20250114100000_aa11bb22.pdf | 1 | null | false | false |
| doc-2 | report.pdf | report_20250114110000_bb22cc33.pdf | 2 | doc-1 | false | false |
| doc-3 | report.pdf | report_20250114120000_cc33dd44.pdf | 3 | doc-1 | true | false |

**Querying**:
- Latest version only: `WHERE IsLatestVersion = true AND IsDeleted = false`
- All versions: `WHERE (Id = 'doc-1' OR BaseDocumentId = 'doc-1') AND IsDeleted = false`
- Version history: `ORDER BY Version ASC`

## 🎯 Benefits

1. **Backend-Controlled Naming**:
   - Prevents filename conflicts
   - Maintains original filename for display
   - Unique, sortable filenames

2. **Full Version History**:
   - Track all document versions
   - Link versions together
   - Easy to find latest version

3. **Soft Delete**:
   - Audit trail preserved
   - Can restore if needed
   - Blob storage cleaned up

4. **Shared DTOs**:
   - No duplication across features
   - Single source of truth
   - Easy to maintain

5. **Separation of Concerns**:
   - DocumentService handles all document logic
   - Features focus on business logic
   - Testable in isolation

## 🔐 Security & Performance

1. **File Size**: Max 10 MB enforced
2. **Filename Sanitization**: Removes invalid characters
3. **Unique Names**: Timestamp + GUID prevents collisions
4. **Soft Delete**: Maintains audit trail
5. **Indexes**: Optimized queries for versions and deletions
6. **Parallel Uploads**: Uses Task.WhenAll for performance

## 📝 DI Registration

```csharp
services.AddScoped<IDocumentService, DocumentService>();
services.AddScoped<IBlobContainerClientFactory, BlobContainerClientFactory>();
```

---

**Date**: 2025-11-14
**Version**: 3.0
