# Entity Configurations and Document Handling Updates

## Overview

This document describes the Entity Framework Core configurations and the consolidation of document handling into `CreateMeetingItem` and `UpdateMeetingItem` commands.

## ✅ Changes Implemented

### 1. Entity Framework Core Configurations

#### MeetingItemConfiguration.cs
**Location**: `backend/MeetingItems/Infrastructure/Configurations/MeetingItemConfiguration.cs`

- Configures `MeetingItem` entity with proper column types and constraints
- Sets up indexes for performance (DecisionBoardId, Status, CreatedAt, SubmissionDate)
- Defines cascade delete for documents
- Property configurations:
  - String lengths (Topic: 200, Purpose: 2000, etc.)
  - Required fields
  - Default values (Status: "Draft")
  - Relationships (One-to-Many with Documents)

```csharp
builder.HasMany(m => m.Documents)
    .WithOne()
    .HasForeignKey(d => d.MeetingItemId)
    .OnDelete(DeleteBehavior.Cascade);
```

#### DocumentConfiguration.cs
**Location**: `backend/MeetingItems/Infrastructure/Configurations/DocumentConfiguration.cs`

- Configures `Document` entity
- Sets up foreign key to MeetingItem
- Indexes for MeetingItemId and UploadDate
- Property constraints:
  - FileName: Max 255 characters
  - FilePath: Max 500 characters
  - ContentType: Max 100 characters

#### MeetingItemsDbContext.cs
**Location**: `backend/MeetingItems/Infrastructure/MeetingItemsDbContext.cs`

- DbContext with MeetingItems and Documents DbSets
- Applies entity configurations in OnModelCreating

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new MeetingItemConfiguration());
    modelBuilder.ApplyConfiguration(new DocumentConfiguration());
}
```

### 2. Document Handling in CreateMeetingItem

#### Updated Command
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
    List<DocumentDto>? Documents = null) : ICommand<Response>;
```

#### DocumentDto
```csharp
public record DocumentDto(
    string FileName,
    string ContentType,
    long FileSize,
    string Base64Content);
```

#### Updated Response
```csharp
public record Response(
    string MeetingItemId,
    List<DocumentResponseDto> UploadedDocuments);

public record DocumentResponseDto(
    string Id,
    string FileName,
    long FileSize,
    string ContentType);
```

#### Document Validator
- Nested `DocumentValidator` class using FluentValidation
- Validates each document in the list:
  - FileName: Required, Max 255 chars
  - ContentType: Required
  - FileSize: Required, > 0, ≤ 10 MB
  - Base64Content: Required, Valid base64 format

#### Handler Logic
1. Creates meeting item
2. Loops through documents (if provided)
3. For each document:
   - Extracts base64 content
   - Generates unique file path
   - Uploads to storage
   - Creates Document entity
   - Adds to MeetingItem
4. Saves everything in one transaction

### 3. Document Handling in UpdateMeetingItem

#### Updated Command
```csharp
public record Command(
    string Id,
    string Topic,
    string Purpose,
    string Outcome,
    string DigitalProduct,
    int Duration,
    string OwnerPresenter,
    string? Sponsor,
    string Status,
    List<CreateMeetingItem.DocumentDto>? NewDocuments = null,
    List<string>? DocumentsToDelete = null) : ICommand<Response>;
```

#### Updated Response
```csharp
public record Response(
    string MeetingItemId,
    List<CreateMeetingItem.DocumentResponseDto> NewlyUploadedDocuments,
    List<string> DeletedDocumentIds);
```

#### Handler Logic
1. Retrieves existing meeting item with documents
2. Updates meeting item fields
3. **Deletes documents** (if specified):
   - Loops through DocumentsToDelete
   - Deletes from storage
   - Removes from entity
4. **Uploads new documents** (if provided):
   - Same logic as CreateMeetingItem
   - Adds to existing documents
5. Saves everything in one transaction

#### Key Feature: Document Deletion
```csharp
if (command.DocumentsToDelete?.Any() == true)
{
    foreach (var documentId in command.DocumentsToDelete)
    {
        var document = meetingItem.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document != null)
        {
            await documentStorage.DeleteAsync(document.FilePath, cancellationToken);
            meetingItem.RemoveDocument(documentId);
            deletedDocumentIds.Add(documentId);
        }
    }
}
```

### 4. Removed UploadDocument Command

The standalone `UploadDocument.cs` command is **no longer needed** as document upload is now integrated into:
- `CreateMeetingItem` - for initial document uploads
- `UpdateMeetingItem` - for adding/deleting documents

### 5. Updated API Controller

**Removed endpoint**:
```csharp
// DELETED: POST /api/meeting-items/{meetingItemId}/documents
```

**Updated endpoints**:
```csharp
// POST /api/meeting-items
// Now accepts Documents array in body

// PUT /api/meeting-items/{id}
// Now accepts NewDocuments and DocumentsToDelete arrays in body
```

## 📋 Database Schema

### MeetingItems Table
```sql
CREATE TABLE MeetingItems (
    Id NVARCHAR(50) PRIMARY KEY,
    Topic NVARCHAR(200) NOT NULL,
    Purpose NVARCHAR(2000) NOT NULL,
    Outcome NVARCHAR(50) NOT NULL,
    Duration INT NOT NULL,
    DigitalProduct NVARCHAR(200) NOT NULL,
    Requestor NVARCHAR(50) NOT NULL,
    OwnerPresenter NVARCHAR(50) NOT NULL,
    Sponsor NVARCHAR(50) NULL,
    SubmissionDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    DecisionBoardId NVARCHAR(50) NOT NULL,
    TemplateId NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(50) NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL
);

CREATE INDEX IX_MeetingItems_DecisionBoardId ON MeetingItems(DecisionBoardId);
CREATE INDEX IX_MeetingItems_Status ON MeetingItems(Status);
CREATE INDEX IX_MeetingItems_CreatedAt ON MeetingItems(CreatedAt);
CREATE INDEX IX_MeetingItems_SubmissionDate ON MeetingItems(SubmissionDate);
```

### Documents Table
```sql
CREATE TABLE Documents (
    Id NVARCHAR(50) PRIMARY KEY,
    MeetingItemId NVARCHAR(50) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    UploadDate DATETIME2 NOT NULL,
    UploadedBy NVARCHAR(50) NOT NULL,
    FOREIGN KEY (MeetingItemId) REFERENCES MeetingItems(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Documents_MeetingItemId ON Documents(MeetingItemId);
CREATE INDEX IX_Documents_UploadDate ON Documents(UploadDate);
```

## 🔄 API Usage Examples

### Create Meeting Item with Documents

**Request**:
```http
POST /api/meeting-items
Content-Type: application/json

{
  "decisionBoardId": "board-123",
  "templateId": null,
  "topic": "IT Security Review",
  "purpose": "Discuss security measures for Q4",
  "outcome": "Decision",
  "digitalProduct": "Banking App",
  "duration": 45,
  "requestor": "user123",
  "ownerPresenter": "user123",
  "sponsor": null,
  "documents": [
    {
      "fileName": "security-report.pdf",
      "contentType": "application/pdf",
      "fileSize": 1024000,
      "base64Content": "data:application/pdf;base64,JVBERi0xLjQKJeLjz9M..."
    }
  ]
}
```

**Response**:
```json
{
  "meetingItemId": "abc-123",
  "uploadedDocuments": [
    {
      "id": "doc-456",
      "fileName": "security-report.pdf",
      "fileSize": 1024000,
      "contentType": "application/pdf"
    }
  ]
}
```

### Update Meeting Item with Document Management

**Request**:
```http
PUT /api/meeting-items/abc-123
Content-Type: application/json

{
  "id": "abc-123",
  "topic": "IT Security Review (Updated)",
  "purpose": "Discuss security measures for Q4 with additional focus on API security",
  "outcome": "Decision",
  "digitalProduct": "Banking App",
  "duration": 60,
  "ownerPresenter": "user456",
  "sponsor": "user789",
  "status": "Submitted",
  "newDocuments": [
    {
      "fileName": "api-security-guidelines.pdf",
      "contentType": "application/pdf",
      "fileSize": 512000,
      "base64Content": "data:application/pdf;base64,..."
    }
  ],
  "documentsToDelete": ["doc-old-123"]
}
```

**Response**:
```json
{
  "meetingItemId": "abc-123",
  "newlyUploadedDocuments": [
    {
      "id": "doc-789",
      "fileName": "api-security-guidelines.pdf",
      "fileSize": 512000,
      "contentType": "application/pdf"
    }
  ],
  "deletedDocumentIds": ["doc-old-123"]
}
```

## 🎯 Benefits

1. **Single Transaction**: All operations (meeting item + documents) in one atomic transaction
2. **Better Validation**: Documents validated along with meeting item data
3. **Simplified API**: No separate document upload endpoint needed
4. **Better UX**: Create meeting item with attachments in one call
5. **Consistency**: Document management integrated into domain logic
6. **Type Safety**: Reusable DocumentDto across commands

## 📝 Migration Guide

### For Frontend Developers

**Old approach** (separate calls):
```typescript
// 1. Create meeting item
const item = await createMeetingItem(data)

// 2. Upload each document separately
for (const doc of documents) {
  await uploadDocument(item.id, doc)
}
```

**New approach** (single call):
```typescript
import { filesToDocumentDtos } from '@/modules/meetingItems/utils/fileHelpers'

// Create meeting item with documents in one call
const documents = await filesToDocumentDtos(selectedFiles)
const result = await meetingItemsApi.createMeetingItem({
  ...data,
  documents
})

console.log(`Created item ${result.meetingItemId}`)
console.log(`Uploaded ${result.uploadedDocuments.length} documents`)
```

**Update with documents**:
```typescript
import { filesToDocumentDtos } from '@/modules/meetingItems/utils/fileHelpers'

// Update meeting item with new documents and delete old ones
const newDocuments = await filesToDocumentDtos(newFiles)
const result = await meetingItemsApi.updateMeetingItem(id, {
  ...data,
  newDocuments,
  documentsToDelete: ['doc-id-1', 'doc-id-2']
})

console.log(`Uploaded ${result.newlyUploadedDocuments.length} new documents`)
console.log(`Deleted ${result.deletedDocumentIds.length} documents`)
```

### For Backend Developers

1. **Remove** `UploadDocument.cs` feature file
2. **Update** API calls to use new command structure
3. **Run migrations** to create database schema
4. **Implement** `IDocumentStorageService` for your storage provider

### Frontend Changes Summary

#### Updated Files:

1. **src/modules/meetingItems/models/meetingItem.ts**
   - Added `DocumentDto` interface
   - Added `DocumentResponseDto` interface
   - Updated `CreateMeetingItemDto` to include optional `documents` array
   - Updated `UpdateMeetingItemDto` to include `newDocuments` and `documentsToDelete`
   - Removed standalone `UploadDocumentDto` interface

2. **src/modules/meetingItems/services/meetingItemsApi.ts**
   - Updated `createMeetingItem` return type to include `uploadedDocuments`
   - Updated `updateMeetingItem` return type to include `newlyUploadedDocuments` and `deletedDocumentIds`
   - Removed `uploadDocument` method
   - Removed `deleteDocument` method

3. **src/env.d.ts** (new file)
   - Added TypeScript definitions for Vite environment variables
   - Defines `VITE_API_BASE_URL` environment variable

4. **src/modules/meetingItems/utils/fileHelpers.ts** (new file)
   - `fileToBase64()` - Converts File to base64 string
   - `fileToDocumentDto()` - Converts File to DocumentDto
   - `filesToDocumentDtos()` - Converts File array to DocumentDto array
   - `formatFileSize()` - Human-readable file size formatting
   - `isFileSizeValid()` - Validates file size (max 10 MB)
   - `getFileExtension()` - Extracts file extension

## 🔐 Security Considerations

1. **File Size Validation**: Max 10 MB enforced
2. **Content Type Validation**: Should be validated against whitelist
3. **Virus Scanning**: Consider adding virus scan before storage
4. **Access Control**: Validate user permissions before document access
5. **Secure Storage**: Use encrypted storage for sensitive documents

---

**Author**: Claude AI
**Date**: 2025-11-14
**Version**: 1.1
