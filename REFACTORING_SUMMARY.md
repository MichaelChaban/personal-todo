# Meeting Items Refactoring Summary

## Overview

This document describes the major refactoring performed on the Meeting Items application, transitioning from a DDD approach to a simpler architecture with public setters, implementing blob storage for documents, creating a TypeScript API client from Swagger, setting up Pinia store, and splitting the frontend component into separate sections.

## 🔄 Backend Changes

### 1. Domain Models - Simplified Architecture

**Location**: `backend/MeetingItems/Models/MeetingItem.cs`

**Changes**:
- Removed private setters, replaced with public setters
- Removed readonly collections (`IReadOnlyList<Document>` → `List<Document>`)
- Removed domain methods (Create, Update, AddDocument, RemoveDocument)
- Removed factory methods
- Simplified to plain POCO models with public properties

**Before**:
```csharp
public class MeetingItem
{
    public string Id { get; private set; }
    private readonly List<Document> _documents = new();
    public IReadOnlyList<Document> Documents => _documents.AsReadOnly();

    public static MeetingItem Create(...) { }
    public void Update(...) { }
    public void AddDocument(Document document) { }
}
```

**After**:
```csharp
public class MeetingItem
{
    public string Id { get; set; }
    public List<Document> Documents { get; set; } = new();
    // All properties with public setters
}
```

### 2. Blob Storage Integration

**Location**:
- `backend/MeetingItems/Features/CreateMeetingItem.cs`
- `backend/MeetingItems/Features/UpdateMeetingItem.cs`

**Changes**:
- Replaced `IDocumentStorageService` with `IBlobContainerClientFactory`
- Using `IBlobContainerClient` for upload/delete operations
- Parallel document uploads using `Task.WhenAll`
- Blob names are now GUIDs instead of hierarchical paths

**Implementation**:
```csharp
internal class Handler(
    IMeetingItemRepository meetingItemRepository,
    IBlobContainerClientFactory containerClientFactory,
    IUserSessionProvider userSessionProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
{
    public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var client = containerClientFactory.GetBlobContainerClient("meeting-item-documents");
        var documents = await UploadDocumentsAsync(client, meetingItem.Id, command.Documents, currentUserId, cancellationToken);
        meetingItem.Documents.AddRange(documents);
    }

    private static async Task<List<Document>> UploadDocumentsAsync(
        IBlobContainerClient client,
        string meetingItemId,
        IEnumerable<DocumentDto> documentDtos,
        string uploadedBy,
        CancellationToken cancellationToken)
    {
        var uploadTasks = documentDtos.Select(async docDto =>
        {
            var blobName = Guid.NewGuid().ToString();
            using var stream = new MemoryStream(fileContent);
            await client.UploadAsync(blobName, stream, cancellationToken);

            return new Document { /* ... */ };
        });

        return (await Task.WhenAll(uploadTasks)).ToList();
    }
}
```

### 3. GetMeetingItemsByDecisionBoard Query

**Location**: `backend/MeetingItems/Features/GetMeetingItemsByDecisionBoard.cs`

**New Feature**:
- Query to fetch all meeting items for a specific decision board
- Returns list with document count (not full documents)
- Endpoint: `GET /api/meeting-items/decision-board/{decisionBoardId}`

**Response**:
```csharp
public record Response(List<MeetingItemDto> MeetingItems);

public record MeetingItemDto(
    string Id,
    string Topic,
    string Purpose,
    // ... other fields
    int DocumentCount, // Count only, not full documents
    DateTime CreatedAt,
    string CreatedBy);
```

### 4. Updated GetMeetingItem

**Location**: `backend/MeetingItems/Features/GetMeetingItem.cs`

**Changes**:
- Already includes full document information in response
- Documents include: Id, FileName, FileSize, ContentType, UploadDate, UploadedBy

### 5. Controller Updates

**Location**: `backend/MeetingItems/Controllers/MeetingItemsController.cs`

**Changes**:
- Added `GetMeetingItemsByDecisionBoard` endpoint
- Reordered endpoints (list before detail to avoid route conflicts)

```csharp
[HttpGet("decision-board/{decisionBoardId}")]
public async Task<IActionResult> GetMeetingItemsByDecisionBoard(string decisionBoardId, CancellationToken cancellationToken)

[HttpGet("{id}")]
public async Task<IActionResult> GetMeetingItem(string id, CancellationToken cancellationToken)
```

## 🎨 Frontend Changes

### 1. Generated TypeScript API Client

**Location**: `src/api/generated/meetingItemsApi.ts`

**Features**:
- Auto-generated from Swagger/OpenAPI specification
- Type-safe DTOs matching backend contracts
- Singleton instance `meetingItemsApiClient`
- All API endpoints:
  - `getMeetingItemsByDecisionBoard(decisionBoardId)`
  - `getMeetingItem(id)`
  - `createMeetingItem(command)`
  - `updateMeetingItem(id, command)`

**DTOs**:
```typescript
export interface DocumentDto {
  fileName: string
  contentType: string
  fileSize: number
  base64Content: string
}

export interface CreateMeetingItemCommand {
  decisionBoardId: string
  templateId?: string | null
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor?: string | null
  documents?: DocumentDto[]
}

export interface UpdateMeetingItemCommand {
  id: string
  // ... other fields
  newDocuments?: DocumentDto[]
  documentsToDelete?: string[]
}
```

### 2. Pinia Store

**Location**: `src/modules/meetingItems/stores/meetingItemsStore.ts`

**Features**:
- Centralized state management for meeting items
- Loading and error states
- Auto-refresh after create/update operations
- Filtered getters (by status)

**State**:
```typescript
const meetingItems = ref<MeetingItemDto[]>([])
const currentMeetingItem = ref<GetMeetingItemResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
```

**Actions**:
```typescript
async function fetchMeetingItemsByDecisionBoard(decisionBoardId: string)
async function fetchMeetingItem(id: string)
async function createMeetingItem(command: CreateMeetingItemCommand)
async function updateMeetingItem(id: string, command: UpdateMeetingItemCommand)
function clearCurrentMeetingItem()
function clearError()
```

**Getters**:
```typescript
const getMeetingItemsByStatus = computed(() => (status: string) => ...)
const draftMeetingItems = computed(() => ...)
const submittedMeetingItems = computed(() => ...)
```

### 3. Component Separation

**Location**: `src/modules/meetingItems/components/sections/`

The monolithic form component has been split into three separate section components:

#### GeneralSection.vue
- Topic
- Purpose
- Outcome (Decision/Discussion/Information)
- Duration
- Digital Product

#### DetailsSection.vue
- Requestor (readonly after creation)
- Owner/Presenter
- Sponsor
- Status (with icon and color coding)
- Submission Date

**Features**:
- Conditional status editing based on permissions
- Status icons and colors
- Readonly mode for certain fields

#### DocumentsSection.vue
- Document list with file icons
- Upload dialog with file picker
- Delete confirmation
- File size formatting
- File type icons (PDF, Word, Excel, etc.)
- 10 MB file size validation

**Features**:
```typescript
@upload: [file: File]  // Emits file object
@delete: [documentId: string]  // Emits document ID to delete
```

### 4. Main Form Component

**Location**: `src/modules/meetingItems/components/MeetingItemFormRefactored.vue`

**Features**:
- Combines all three section components
- Tab-based navigation
- Document badge showing count
- Handles create and edit modes
- Manages temporary documents (uploads/deletes before save)
- Converts files to DocumentDto using helpers

**Usage**:
```vue
<MeetingItemFormRefactored
  :meeting-item="item"  // Optional for edit mode
  :decision-board-id="boardId"
  :can-change-status="true"
  :readonly="false"
  @submit="handleSubmit"
  @cancel="handleCancel"
/>
```

### 5. List Page Example

**Location**: `src/modules/meetingItems/pages/MeetingItemsListPage.vue`

**Features**:
- Uses Pinia store for data management
- Tab-based filtering (All/Draft/Submitted)
- Loading and error states
- Create and edit dialogs
- Status badges with colors
- Document count display

### 6. File Helpers (Unchanged)

**Location**: `src/modules/meetingItems/utils/fileHelpers.ts`

- `fileToBase64()` - Converts File to base64
- `fileToDocumentDto()` - Converts File to DocumentDto
- `filesToDocumentDtos()` - Batch conversion
- `formatFileSize()` - Human-readable sizes
- `isFileSizeValid()` - Max 10 MB validation
- `getFileExtension()` - Extract extension

### 7. Removed Files

**Removed**:
- `src/modules/meetingItems/services/meetingItemsApi.ts` (replaced by generated client)
- `src/modules/meetingItems/models/meetingItem.ts` (replaced by DTOs in generated client)

## 📋 Migration Guide

### Backend Migration

1. **Update DI Container**:
```csharp
services.AddScoped<IBlobContainerClientFactory, BlobContainerClientFactory>();
```

2. **Remove old service**:
```csharp
// REMOVE: services.AddScoped<IDocumentStorageService, ...>();
```

3. **Update Repository**:
```csharp
public interface IMeetingItemRepository
{
    Task<MeetingItem?> GetByIdWithDocumentsAsync(string id, CancellationToken cancellationToken);
    Task<List<MeetingItem>> GetByDecisionBoardIdAsync(string decisionBoardId, CancellationToken cancellationToken);
}
```

### Frontend Migration

1. **Install Pinia** (if not already):
```bash
npm install pinia
```

2. **Setup Pinia in main.ts**:
```typescript
import { createPinia } from 'pinia'
app.use(createPinia())
```

3. **Update component imports**:
```typescript
// OLD
import { meetingItemsApi } from '@/modules/meetingItems/services/meetingItemsApi'

// NEW
import { useMeetingItemsStore } from '@/modules/meetingItems/stores/meetingItemsStore'
const store = useMeetingItemsStore()
```

4. **Update API calls**:
```typescript
// OLD
const result = await meetingItemsApi.createMeetingItem(data)

// NEW
const result = await store.createMeetingItem(data)
// Store automatically refreshes the list
```

## 🎯 Key Benefits

1. **Simpler Backend**:
   - No domain complexity
   - Direct property access
   - Standard EF Core patterns

2. **Better Document Handling**:
   - Parallel uploads for performance
   - Proper blob storage integration
   - Consistent with existing patterns

3. **Type-Safe Frontend**:
   - Generated client matches backend exactly
   - No manual DTO synchronization needed
   - Compile-time type checking

4. **Better State Management**:
   - Centralized data in Pinia store
   - Automatic refresh after mutations
   - Loading and error states built-in

5. **Modular Components**:
   - Each section is self-contained
   - Easy to test individually
   - Reusable across different forms

6. **Decision Board Filtering**:
   - Each board has its own meeting items
   - Efficient filtering at API level
   - No client-side filtering needed

## 🔐 Security Considerations

1. **File Upload**: Max 10 MB enforced client and server-side
2. **Blob Storage**: Uses secure blob names (GUIDs)
3. **Authorization**: Should validate user has access to decision board
4. **Content Type**: Should whitelist allowed file types
5. **Virus Scanning**: Consider adding before storage

## 📝 API Endpoints Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/meeting-items/decision-board/{decisionBoardId}` | Get all items for a board |
| GET | `/api/meeting-items/{id}` | Get single item with documents |
| POST | `/api/meeting-items` | Create item with documents |
| PUT | `/api/meeting-items/{id}` | Update item, add/delete documents |

---

**Author**: Claude AI
**Date**: 2025-11-14
**Version**: 2.0
