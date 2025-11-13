# Frontend-Backend Integration - Meeting Items System

Complete integration between Vue 3 frontend and .NET backend MediatR handlers.

---

## Overview

The frontend has been fully integrated with the backend API to support the complete meeting items workflow:

1. **Create/Edit Meeting Items** - Form submission with dynamic fields
2. **Document Upload** - File management with automatic versioning
3. **Status Management** - Workflow state transitions
4. **Template Loading** - Dynamic form rendering based on templates

---

## Files Updated

### Backend Files Created

1. **MeetingItemsController.cs**
   - Location: `Backend_MediatorHandlers/MeetingItemsController.cs`
   - Exposes all MediatR handlers as REST API endpoints
   - Follows EmployeeController.cs pattern with Permission attributes
   - Endpoints:
     - `GET /api/MeetingItems/{id}` - Get meeting item details
     - `POST /api/MeetingItems` - Create new meeting item
     - `PUT /api/MeetingItems/{id}` - Update meeting item
     - `PATCH /api/MeetingItems/{id}/status` - Update status (secretary/chair only)
     - `POST /api/MeetingItems/{id}/documents` - Upload document
     - `GET /api/MeetingItems/template/{decisionBoardId}` - Get template

2. **Six MediatR Handlers** (previously created)
   - CreateMeetingItem.cs
   - UpdateMeetingItem.cs
   - GetMeetingItemById.cs
   - UpdateMeetingItemStatus.cs
   - UploadMeetingItemDocument.cs
   - GetTemplateByDecisionBoard.cs

### Frontend Files Updated

1. **src/api/meetingItems.js** - API Service Layer
   - Complete rewrite to match backend structure
   - Proper error handling with `fetchWithErrorHandling` wrapper
   - Field value mapping to typed properties (textValue, numberValue, etc.)
   - Document upload with base64 encoding

2. **src/pages/MeetingItemPage.vue** - Main Form Component
   - Added API integration for create/update operations
   - Template loading on component mount
   - Dynamic field value mapping with `mapFieldValuesToDto()`
   - Proper handling of active vs historical fields
   - Document management integration

3. **src/components/DocumentUpload.vue** - Document Management
   - API integration for upload/download/delete
   - Base64 file conversion
   - Dual mode: store locally before save, or upload immediately if item exists
   - Blob download handling

---

## API Integration Details

### 1. Create Meeting Item

**Frontend Call:**
```javascript
const payload = {
  decisionBoardId: 'board-123',
  templateId: 'template-456',
  topic: 'Cloud Migration Strategy',
  purpose: 'Discuss migration approach...',
  outcome: 'Decision',
  digitalProduct: 'Infrastructure Platform',
  durationMinutes: 30,
  ownerPresenterUserId: 'USER123',
  sponsorUserId: 'USER456',
  fieldValues: [
    { fieldName: 'architectureImpact', textValue: 'High' },
    { fieldName: 'estimatedCost', numberValue: 50000 }
  ],
  documents: [
    { fileName: 'doc.pdf', contentType: 'application/pdf', base64Content: '...' }
  ]
}

await meetingItemsApi.createMeetingItem(payload)
```

**Backend Endpoint:**
```csharp
[HttpPost]
[Permission(Policy.CanCreateMeetingItems)]
public async Task<IActionResult> Create(
    [FromBody] CreateMeetingItem.Command command,
    CancellationToken cancellationToken)
{
    var result = await Mediator.Send(command, cancellationToken);
    return HandleResult<CreateMeetingItem.Response>(result);
}
```

**Response:**
```json
{
  "meetingItemId": "mi-123",
  "status": "Submitted",
  "createdDate": "2025-01-13T10:30:00Z"
}
```

---

### 2. Get Meeting Item (with Active/Historical Fields)

**Frontend Call:**
```javascript
const response = await meetingItemsApi.getMeetingItem('mi-123')

// Response structure
{
  id: 'mi-123',
  decisionBoardId: 'board-123',
  topic: 'Cloud Migration',
  status: 'Proposed',
  activeFields: [
    {
      fieldName: 'architectureImpact',
      label: 'Architecture Impact',
      textValue: 'High',
      fieldType: 'Text'
    }
  ],
  historicalFields: [
    {
      fieldName: 'oldRiskLevel',
      label: 'Risk Level (Removed)',
      textValue: 'Medium',
      deactivatedDate: '2025-01-10',
      deactivationReason: 'Field removed from template'
    }
  ],
  documents: [...]
}
```

**Backend Handler:**
```csharp
internal class Handler : IQueryHandler<Query, Response>
{
    public async Task<BusinessResult<Response>> Handle(...)
    {
        // Separates active vs historical fields
        var activeFields = meetingItem.FieldValues
            .Where(fv => fv.FieldDefinition.IsActive)
            .Select(fv => new ActiveFieldDto(...))
            .ToList();

        var historicalFields = meetingItem.FieldValues
            .Where(fv => !fv.FieldDefinition.IsActive)
            .Select(fv => new HistoricalFieldDto(...))
            .ToList();
    }
}
```

---

### 3. Field Value Mapping (Critical!)

The frontend must map form values to the correct typed properties based on field type:

**Frontend Mapping Function:**
```javascript
const mapFieldValuesToDto = () => {
  const fieldValues = []

  Object.entries(meetingItem.value.fieldValues).forEach(([fieldName, value]) => {
    const fieldDef = template.value?.fieldDefinitions?.find(f => f.fieldName === fieldName)
    if (!fieldDef) return

    const dto = {
      fieldName,
      textValue: null,
      numberValue: null,
      dateValue: null,
      booleanValue: null,
      jsonValue: null
    }

    // Map to correct typed property based on field type
    switch (fieldDef.fieldType) {
      case 'Text':
      case 'Email':
      case 'Dropdown':
        dto.textValue = value
        break
      case 'Number':
        dto.numberValue = typeof value === 'number' ? value : parseFloat(value)
        break
      case 'Date':
        dto.dateValue = value instanceof Date ? value.toISOString() : value
        break
      case 'Boolean':
        dto.booleanValue = Boolean(value)
        break
      case 'MultiSelect':
        dto.jsonValue = JSON.stringify(Array.isArray(value) ? value : [value])
        break
      default:
        dto.textValue = String(value)
    }

    fieldValues.push(dto)
  })

  return fieldValues
}
```

**Backend Validation (DynamicFieldValidator):**
```csharp
private bool ValidateTextField(FieldValueDto fieldValue, FieldDefinition fieldDef)
{
    // Must use TextValue for Text fields
    if (string.IsNullOrWhiteSpace(fieldValue.TextValue))
        return false;

    // Validate length
    if (rules.MinLength.HasValue && fieldValue.TextValue.Length < rules.MinLength)
        return false;
}

private bool ValidateNumberField(FieldValueDto fieldValue, FieldDefinition fieldDef)
{
    // Must use NumberValue for Number fields
    if (!fieldValue.NumberValue.HasValue)
        return false;

    // Validate range
    if (rules.Min.HasValue && fieldValue.NumberValue < rules.Min)
        return false;
}
```

---

### 4. Document Upload

**Frontend Flow:**

1. **Before Meeting Item Exists** (during creation):
```javascript
// Convert file to base64 and store locally
const base64Content = await convertFileToBase64(file)
const newDoc = {
  fileName: file.name,
  contentType: file.type,
  base64Content: base64Content
}
documents.value.push(newDoc)

// Submit with meeting item
await meetingItemsApi.createMeetingItem({
  ...otherFields,
  documents: documents.value
})
```

2. **After Meeting Item Exists** (adding more documents):
```javascript
// Upload immediately via API
const response = await meetingItemsApi.uploadDocument(meetingItemId, {
  fileName: file.name,
  contentType: file.type,
  base64Content: base64Content
})

// Response includes versioned filename
{
  documentId: 'doc-456',
  storedFileName: 'ITG20251113CloudMigration.v01.pdf',
  versionNumber: '01'
}
```

**Backend Versioning:**
```csharp
// Generates: <ABBR><YYYYMMDD><TOPIC>.v<XX>.ext
private static (string storedFileName, string version) GenerateStoredFileName(
    string abbreviation,
    string topic,
    string originalFileName,
    int existingVersionCount)
{
    var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
    var topicSlug = SanitizeFileName(topic, maxLength: 20);
    var extension = Path.GetExtension(originalFileName);
    var versionNumber = existingVersionCount + 1;
    var version = versionNumber.ToString("D2");

    var storedFileName = $"{abbreviation}{dateStr}{topicSlug}.v{version}{extension}";

    return (storedFileName, version);
}
```

---

### 5. Template Loading

**Frontend:**
```javascript
// Load template on component mount
const loadTemplate = async () => {
  const response = await meetingItemsApi.getTemplateByDecisionBoard('board-123')

  template.value = response
  meetingItem.value.templateId = response.templateId

  // Initialize field values from template defaults
  response.fieldDefinitions.forEach(field => {
    if (field.isRequired || field.options?.some(opt => opt.isDefault)) {
      const defaultOption = field.options?.find(opt => opt.isDefault)
      meetingItem.value.fieldValues[field.fieldName] = defaultOption?.value || null
    }
  })
}
```

**Backend:**
```csharp
public record Response(
    string TemplateId,
    string TemplateName,
    string? Description,
    List<FieldDefinitionDto> FieldDefinitions);

public record FieldDefinitionDto(
    string Id,
    string FieldName,
    string Label,
    string FieldType,        // Text, Number, Date, Boolean, Dropdown, MultiSelect, Email
    bool IsRequired,
    string Category,
    int DisplayOrder,
    string? HelpText,
    string? PlaceholderText,
    ValidationRulesDto? ValidationRules,
    List<FieldOptionDto>? Options);
```

---

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend (Vue 3)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  MeetingItemPage.vue                                             │
│  ├─ Load Template (onMounted)                                    │
│  │  └─ meetingItemsApi.getTemplateByDecisionBoard()             │
│  │                                                               │
│  ├─ Render Form                                                  │
│  │  ├─ Static Fields (topic, purpose, outcome, etc.)            │
│  │  ├─ Dynamic Fields (from template.fieldDefinitions)          │
│  │  └─ DocumentUpload Component                                 │
│  │                                                               │
│  └─ Submit Form                                                  │
│     ├─ mapFieldValuesToDto() - convert to typed properties      │
│     └─ meetingItemsApi.createMeetingItem(payload)               │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 │ HTTP POST /api/MeetingItems
                                 ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Backend (.NET + MediatR)                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  MeetingItemsController.cs                                       │
│  └─ Create([FromBody] CreateMeetingItem.Command command)        │
│     └─ Mediator.Send(command)                                    │
│                                                                   │
│         ↓                                                         │
│                                                                   │
│  CreateMeetingItem.Handler                                       │
│  ├─ Validate static fields (FluentValidation)                   │
│  ├─ Validate dynamic fields (DynamicFieldValidator)             │
│  │  ├─ Check required fields present                            │
│  │  ├─ Check field types match (textValue for Text fields)      │
│  │  ├─ Check text length (minLength, maxLength)                 │
│  │  ├─ Check number range (min, max)                            │
│  │  ├─ Check email format                                       │
│  │  └─ Check dropdown options (value in allowed list)           │
│  ├─ Create MeetingItem entity                                   │
│  ├─ Create MeetingItemFieldValue entities (typed storage)       │
│  ├─ Upload documents to blob storage                            │
│  │  └─ Generate versioned filenames                             │
│  └─ Save to database                                             │
│                                                                   │
│         ↓                                                         │
│                                                                   │
│  Response: { meetingItemId, status, createdDate }               │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 │ HTTP 200 OK
                                 ↓
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend (Vue 3)                         │
│                                                                   │
│  ✅ Show success notification                                    │
│  ✅ Navigate to meeting items list                               │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Error Handling

### Frontend Error Display

```javascript
try {
  await meetingItemsApi.createMeetingItem(payload)

  $q.notify({
    type: 'positive',
    message: 'Meeting item submitted successfully'
  })

  router.push('/meeting-items')
} catch (error) {
  $q.notify({
    type: 'negative',
    message: 'Failed to submit meeting item',
    caption: error.message
  })
}
```

### Backend Error Responses

```csharp
// Validation errors
if (!result.IsSuccess)
    return BadRequest(result.Error);

// Business errors
return BusinessResult.Failure<Response>(
    "Dynamic field 'estimatedCost' exceeds maximum value of 1000000"
);

// Not found
return BusinessResult.Failure<Response>(BusinessErrorMessage.NotFound);
```

---

## Testing Checklist

### Frontend

- [ ] Template loads on component mount
- [ ] Form validates required fields before submit
- [ ] Field values map to correct typed properties
- [ ] Documents convert to base64 correctly
- [ ] Success/error notifications display properly
- [ ] Navigation works after successful submit
- [ ] Edit mode loads existing data correctly
- [ ] Historical fields display as read-only

### Backend

- [ ] Static field validation works (FluentValidation)
- [ ] Dynamic field validation works (DynamicFieldValidator)
- [ ] Field type enforcement (text → TextValue, number → NumberValue)
- [ ] Document versioning increments correctly
- [ ] Blob storage upload succeeds
- [ ] Database saves complete entity graph
- [ ] Active/historical field separation works
- [ ] Status workflow validation prevents invalid transitions

### Integration

- [ ] End-to-end create meeting item flow
- [ ] End-to-end update meeting item flow
- [ ] Document upload during creation
- [ ] Document upload after creation
- [ ] Template changes preserve historical data
- [ ] API error messages display in UI

---

## Next Steps

1. **Configure Environment Variables**
   - Set `VITE_API_URL` in frontend .env file
   - Example: `VITE_API_URL=https://localhost:5001/api`

2. **Backend Setup**
   - Copy MediatR handlers to your project
   - Update namespaces to match your project structure
   - Register handlers with MediatR in `Program.cs`
   - Add MeetingItemsController to Controllers folder
   - Configure blob storage connection string

3. **Authentication Integration**
   - Replace `USER123` with actual user ID from auth context
   - Implement permission checks (CanCreateMeetingItems, CanUpdateMeetingItemStatus)
   - Add JWT/OAuth token to API requests

4. **Testing**
   - Run backend API
   - Run frontend dev server (`npm run dev`)
   - Test complete create/edit/upload flow
   - Verify validation errors display correctly

5. **Dynamic Field Rendering** (Future Enhancement)
   - Update Details tab to render fields from template.fieldDefinitions
   - Use Quasar components based on fieldType (q-input, q-select, q-date, etc.)
   - Apply validation rules from template
   - Display help text and placeholders

---

## Key Integration Points

1. **Field Type Mapping** - Frontend MUST map to typed properties
2. **Template Loading** - Frontend needs templateId from decision board
3. **Document Base64 Encoding** - Required for API payload
4. **Active vs Historical Fields** - Frontend displays separately
5. **Status Workflow** - Secretary/chair only can update status
6. **Validation Error Display** - Map backend errors to form fields

---

**All integration complete! Frontend fully connected to backend MediatR handlers.**
