# Factory Methods and Domain Methods Summary

## Overview

Added factory methods and domain methods to `MeetingItem` and `Document` models to encapsulate object creation and state changes.

## MeetingItem Factory and Domain Methods

### Factory Method: Create
Creates a new meeting item with all required fields and proper defaults.

```csharp
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
        Status = "Draft",                    // Default status
        CreatedAt = DateTime.UtcNow,
        CreatedBy = createdBy,
        Documents = new List<Document>()     // Empty collection
    };
}
```

**Usage in CreateMeetingItem**:
```csharp
var meetingItem = MeetingItem.Create(
    command.DecisionBoardId,
    command.TemplateId,
    command.Topic,
    command.Purpose,
    command.Outcome,
    command.DigitalProduct,
    command.Duration,
    command.Requestor,
    command.OwnerPresenter,
    command.Sponsor,
    currentUserId);
```

### Domain Method: Update
Updates meeting item fields and sets audit information.

```csharp
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
```

**Usage in UpdateMeetingItem**:
```csharp
meetingItem.Update(
    command.Topic,
    command.Purpose,
    command.Outcome,
    command.DigitalProduct,
    command.Duration,
    command.OwnerPresenter,
    command.Sponsor,
    command.Status,
    currentUserId);
```

## Document Factory and Domain Methods

### Factory Method: Create
Creates a new document (Version 1).

```csharp
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
        Version = 1,                    // Always 1 for new documents
        BaseDocumentId = null,          // No base for original
        IsLatestVersion = true,
        UploadDate = DateTime.UtcNow,
        UploadedBy = uploadedBy,
        IsDeleted = false
    };
}
```

**Usage in DocumentService.UploadDocumentsAsync**:
```csharp
return Document.Create(
    meetingItemId,
    docDto.FileName,
    fileName,
    storagePath,
    fileContent.Length,
    docDto.ContentType,
    uploadedBy);
```

### Factory Method: CreateVersion
Creates a new version of an existing document.

```csharp
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
        Version = version,                   // Provided version number
        BaseDocumentId = baseDocumentId,     // Links to original
        IsLatestVersion = true,              // New version is latest
        UploadDate = DateTime.UtcNow,
        UploadedBy = uploadedBy,
        IsDeleted = false
    };
}
```

**Usage in DocumentService.UploadNewVersionAsync**:
```csharp
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
```

### Domain Method: MarkAsDeleted
Soft deletes a document.

```csharp
public void MarkAsDeleted(string deletedBy)
{
    IsDeleted = true;
    DeletedDate = DateTime.UtcNow;
    DeletedBy = deletedBy;
    IsLatestVersion = false;    // Deleted docs aren't latest
}
```

**Usage in DocumentService.DeleteDocumentAsync**:
```csharp
document.MarkAsDeleted(deletedBy);
```

### Domain Method: MarkAsOldVersion
Marks document as no longer the latest version.

```csharp
public void MarkAsOldVersion()
{
    IsLatestVersion = false;
}
```

**Usage in UpdateMeetingItem when uploading new version**:
```csharp
baseDocument.MarkAsOldVersion();
```

## Benefits

### 1. Encapsulation
- Object creation logic is centralized
- State transitions are explicit and controlled
- Business rules enforced at creation time

### 2. Consistency
- Same creation pattern everywhere
- Audit fields always set correctly
- Default values guaranteed

### 3. Testability
- Easy to test factory methods in isolation
- Clear contracts for object creation
- Mockable for unit tests

### 4. Maintainability
- Single place to update creation logic
- Changes propagate automatically
- Reduces code duplication

### 5. Domain-Driven Design
- Factories are a DDD pattern
- Encapsulates domain logic
- Models express business intent

## Before and After Comparison

### Before (Direct Instantiation)
```csharp
// CreateMeetingItem - Verbose and error-prone
var meetingItem = new MeetingItem
{
    Id = Guid.NewGuid().ToString(),
    DecisionBoardId = command.DecisionBoardId,
    TemplateId = command.TemplateId,
    Topic = command.Topic,
    Purpose = command.Purpose,
    Outcome = command.Outcome,
    DigitalProduct = command.DigitalProduct,
    Duration = command.Duration,
    Requestor = command.Requestor,
    OwnerPresenter = command.OwnerPresenter,
    Sponsor = command.Sponsor,
    SubmissionDate = DateTime.UtcNow,
    Status = "Draft",
    CreatedAt = DateTime.UtcNow,
    CreatedBy = currentUserId
};

// UpdateMeetingItem - Manual field updates
meetingItem.Topic = command.Topic;
meetingItem.Purpose = command.Purpose;
meetingItem.Outcome = command.Outcome;
meetingItem.DigitalProduct = command.DigitalProduct;
meetingItem.Duration = command.Duration;
meetingItem.OwnerPresenter = command.OwnerPresenter;
meetingItem.Sponsor = command.Sponsor;
meetingItem.Status = command.Status;
meetingItem.UpdatedAt = DateTime.UtcNow;
meetingItem.UpdatedBy = currentUserId;
```

### After (Factory Methods)
```csharp
// CreateMeetingItem - Clean and intention-revealing
var meetingItem = MeetingItem.Create(
    command.DecisionBoardId,
    command.TemplateId,
    command.Topic,
    command.Purpose,
    command.Outcome,
    command.DigitalProduct,
    command.Duration,
    command.Requestor,
    command.OwnerPresenter,
    command.Sponsor,
    currentUserId);

// UpdateMeetingItem - Domain method handles complexity
meetingItem.Update(
    command.Topic,
    command.Purpose,
    command.Outcome,
    command.DigitalProduct,
    command.Duration,
    command.OwnerPresenter,
    command.Sponsor,
    command.Status,
    currentUserId);
```

## Key Principles

1. **Factory methods** for **creation** (static methods)
2. **Domain methods** for **state changes** (instance methods)
3. **Audit fields** set automatically
4. **Business rules** enforced in one place
5. **Immutable after creation** where appropriate (e.g., Id, CreatedAt, CreatedBy)

---

**Date**: 2025-11-14
**Version**: 1.0
