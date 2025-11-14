# Meeting Items Module

This document describes the complete structure of the Meeting Items module, covering the first 3 sections (General, Details, and Documents tabs) with both frontend and backend implementation.

## 📁 Project Structure

```
VueProject/
├── backend/
│   └── MeetingItems/
│       ├── Common/
│       │   ├── BusinessResult.cs
│       │   └── Abstractions/
│       │       ├── ICommand.cs
│       │       ├── IQuery.cs
│       │       ├── IUnitOfWork.cs
│       │       ├── IUserSessionProvider.cs
│       │       └── IDocumentStorageService.cs
│       ├── Models/
│       │   └── MeetingItem.cs (Domain model with Document)
│       ├── Features/
│       │   ├── CreateMeetingItem.cs (Command, Validator, Handler)
│       │   ├── UpdateMeetingItem.cs (Command, Validator, Handler)
│       │   ├── GetMeetingItem.cs (Query, Validator, Handler)
│       │   └── UploadDocument.cs (Command, Validator, Handler)
│       ├── Repositories/
│       │   ├── IMeetingItemRepository.cs
│       │   ├── IDecisionBoardRepository.cs
│       │   └── ITemplateRepository.cs
│       └── Controllers/
│           └── MeetingItemsController.cs
└── src/
    └── modules/
        └── meetingItems/
            ├── models/
            │   └── meetingItem.ts (TypeScript interfaces and enums)
            ├── services/
            │   └── meetingItemsApi.ts (API service layer)
            └── components/
                └── MeetingItemForm.vue (3-tab form component)
```

## 🎯 Backend Architecture

### Pattern: Vertical Slice Architecture with CQRS

Each feature is self-contained in a single file following the pattern from `CreateMeetingItem.cs`.

### Key Components

#### 1. **Domain Models** (`Models/MeetingItem.cs`)
- Encapsulated domain entities with private setters
- Static factory methods (`Create`)
- Business logic methods (`Update`, `AddDocument`, `RemoveDocument`)
- Rich domain model pattern

```csharp
public class MeetingItem
{
    public string Id { get; private set; }

    public static MeetingItem Create(...) { }
    public void Update(...) { }
    public void AddDocument(Document document) { }
}
```

#### 2. **Features** (CQRS Commands/Queries)

Each feature file contains:
- **Validator**: FluentValidation rules with async repository checks
- **Command/Query**: Record type with input parameters
- **Response**: Record type for output
- **Handler**: Business logic using primary constructor injection

**Example: CreateMeetingItem.cs**
```csharp
public class CreateMeetingItem
{
    public class Validator : AbstractValidator<Command> { }
    public record Command(...) : ICommand<Response>;
    public record Response(string MeetingItemId);
    internal class Handler(...) : ICommandHandler<Command, Response> { }
}
```

#### 3. **Validators**
- FluentValidation with `AbstractValidator<T>`
- Cascade mode for stopping on first error
- Async validation for repository checks
- Custom validation messages using `BusinessErrorMessage`

```csharp
RuleFor(c => c.Topic)
    .NotEmpty()
    .WithMessage(BusinessErrorMessage.Required)
    .MaximumLength(200)
    .WithMessage("Topic must not exceed 200 characters");
```

#### 4. **Business Result Pattern**
```csharp
public class BusinessResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }
}
```

#### 5. **API Controller**
- Minimal controller with MediatR
- Business Result handling
- Proper HTTP status codes

```csharp
[HttpPost]
public async Task<IActionResult> CreateMeetingItem([FromBody] CreateMeetingItem.Command command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetMeetingItem), new { id = result.Data!.MeetingItemId }, result.Data)
        : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
}
```

## 🎨 Frontend Architecture

### Pattern: Module-based with TypeScript

### Key Components

#### 1. **TypeScript Models** (`models/meetingItem.ts`)
- Enums for Status and Outcome
- Interfaces matching backend DTOs
- Factory functions for creating empty objects

```typescript
export interface MeetingItem {
  id: string | null
  topic: string
  purpose: string
  outcome: MeetingItemOutcome
  duration: number
  // ... other properties
}

export function createEmptyMeetingItem(): MeetingItem { }
```

#### 2. **API Service** (`services/meetingItemsApi.ts`)
- Centralized API calls
- Type-safe requests/responses
- Error handling

```typescript
class MeetingItemsApiService {
  async getMeetingItem(id: string): Promise<MeetingItem> { }
  async createMeetingItem(dto: CreateMeetingItemDto): Promise<{meetingItemId: string}> { }
  async updateMeetingItem(id: string, dto: UpdateMeetingItemDto): Promise<{meetingItemId: string}> { }
  async uploadDocument(dto: UploadDocumentDto): Promise<Document> { }
}
```

#### 3. **Vue Component** (`components/MeetingItemForm.vue`)
- 3 tabs: General, Details, Documents
- Type-safe with TypeScript
- Quasar Framework components
- All text wrapped in `$t()` for i18n

**Tab Structure:**
1. **General Tab**: Topic, Purpose, Outcome, Duration, Digital Product, Participants
2. **Details Tab**: Submission Date, Status
3. **Documents Tab**: Document upload, list, delete

## 📋 Features Implemented

### Backend Features

1. **Create Meeting Item** (`CreateMeetingItem.cs`)
   - Validates all fields
   - Checks Decision Board exists
   - Creates draft meeting item

2. **Update Meeting Item** (`UpdateMeetingItem.cs`)
   - Validates meeting item exists
   - Updates fields
   - Validates status transitions

3. **Get Meeting Item** (`GetMeetingItem.cs`)
   - Retrieves by ID
   - Includes documents

4. **Upload Document** (`UploadDocument.cs`)
   - Base64 file upload
   - File size validation (max 10 MB)
   - Content type validation

### Frontend Features

1. **General Tab**
   - Form inputs for all general fields
   - Field validation with rules
   - Auto-fill requestor from session

2. **Details Tab**
   - Read-only submission date
   - Status selector (conditional editing)

3. **Documents Tab**
   - Upload dialog with file selector
   - Document list with metadata
   - Download and delete actions
   - File size formatting
   - File icon based on extension

## 🔧 Validation Rules

### Backend (FluentValidation)

| Field | Rules |
|-------|-------|
| Topic | Required, Max 200 chars |
| Purpose | Required, Min 10 chars, Max 2000 chars |
| Outcome | Required, Must be Decision/Discussion/Information |
| Duration | Required, > 0, ≤ 480 minutes |
| Digital Product | Required, Max 200 chars |
| Requestor | Required, Max 50 chars |
| Owner/Presenter | Required, Max 50 chars |
| Sponsor | Optional, Max 50 chars |
| Decision Board ID | Required, Must exist in database |
| Template ID | Optional, Must be active if provided |
| File Size | Required, > 0, ≤ 10 MB |

## 🚀 Usage Examples

### Backend Usage

```csharp
// In your API controller or service
var command = new CreateMeetingItem.Command(
    decisionBoardId: "board-123",
    templateId: null,
    topic: "IT Security Review",
    purpose: "Discuss security measures...",
    outcome: "Decision",
    digitalProduct: "Banking App",
    duration: 45,
    requestor: "user123",
    ownerPresenter: "user123",
    sponsor: null
);

var result = await _mediator.Send(command);

if (result.IsSuccess)
{
    var meetingItemId = result.Data.MeetingItemId;
}
```

### Frontend Usage

```vue
<template>
  <MeetingItemForm
    :meeting-item="meetingItem"
    :can-change-status="isAdmin"
    @update:meeting-item="handleUpdate"
    @document-uploaded="handleDocumentUploaded"
    @document-deleted="handleDocumentDeleted"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import MeetingItemForm from '@/modules/meetingItems/components/MeetingItemForm.vue'
import { meetingItemsApi } from '@/modules/meetingItems/services/meetingItemsApi'
import { createEmptyMeetingItem } from '@/modules/meetingItems/models/meetingItem'
import type { MeetingItem, Document } from '@/modules/meetingItems/models/meetingItem'

const meetingItem = ref<MeetingItem>(createEmptyMeetingItem())

onMounted(async () => {
  if (route.params.id) {
    meetingItem.value = await meetingItemsApi.getMeetingItem(route.params.id as string)
  }
})

const handleUpdate = (updated: MeetingItem) => {
  meetingItem.value = updated
}

const handleDocumentUploaded = (document: Document) => {
  meetingItem.value.documents.push(document)
}

const handleDocumentDeleted = (documentId: string) => {
  meetingItem.value.documents = meetingItem.value.documents.filter(d => d.id !== documentId)
}
</script>
```

## 📦 Dependencies

### Backend
- MediatR (for CQRS)
- FluentValidation (for validation)
- Microsoft.AspNetCore.Mvc (for API)

### Frontend
- Vue 3 (Composition API)
- TypeScript
- Quasar Framework
- Fetch API (for HTTP requests)

## 🔐 Security Considerations

1. **Authentication**: `IUserSessionProvider` should be implemented to get current user
2. **Authorization**: Add authorization checks in validators or handlers
3. **File Upload**: Validate file types and scan for malware
4. **CORS**: Configure CORS policy in backend
5. **Rate Limiting**: Consider adding rate limiting for uploads

## 🧪 Testing

### Backend Testing
```csharp
public class CreateMeetingItemTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateMeetingItem.Command(...);
        var handler = new CreateMeetingItem.Handler(...);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data.MeetingItemId);
    }
}
```

### Frontend Testing
```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MeetingItemForm from './MeetingItemForm.vue'

describe('MeetingItemForm', () => {
  it('renders all three tabs', () => {
    const wrapper = mount(MeetingItemForm, {
      props: {
        meetingItem: createEmptyMeetingItem()
      }
    })

    expect(wrapper.text()).toContain('General')
    expect(wrapper.text()).toContain('Details')
    expect(wrapper.text()).toContain('Documents')
  })
})
```

## 📝 Notes

- All text strings use `$t()` placeholder for future i18n implementation
- IDs are strings (GUIDs) for consistency
- Dates are ISO 8601 strings
- File upload uses base64 encoding
- Business Result pattern provides consistent error handling
- Validators inject repositories for database checks

## 🔄 Future Enhancements

1. Add field value support (dynamic template fields)
2. Implement actual document storage (blob storage)
3. Add versioning for documents
4. Add audit trail for changes
5. Implement workflow/approval process
6. Add email notifications
7. Add search and filtering
8. Add bulk operations

---

**Author**: Claude AI
**Date**: 2025-11-14
**Version**: 1.0
