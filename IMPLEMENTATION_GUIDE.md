# Implementation Guide - Meeting Items Management System

## 📊 Project Overview

This document outlines what has been implemented and the next steps needed to complete the Meeting Items Management System for KBC's decision board.

---

## ✅ What's Already Done

### 1. **Frontend Application (Complete)**

#### **Technology Stack**
- ✅ Vue 3 (Composition API)
- ✅ Vite (Build tool & dev server)
- ✅ Quasar Framework (UI components)
- ✅ Vue Router (Navigation)

#### **Pages Implemented**

| Page | File | Status | Description |
|------|------|--------|-------------|
| Dashboard | `src/pages/HomePage.vue` | ✅ Complete | Statistics overview, quick actions, recent activity |
| Meeting Items List | `src/pages/MeetingItemsListPage.vue` | ✅ Complete | Searchable table with filters, status badges |
| Create/Edit Form | `src/pages/MeetingItemPage.vue` | ✅ Complete | Main form with 3 tabs (General, Details, Documents) |
| Coming Soon | `src/pages/ComingSoonPage.vue` | ✅ Complete | Placeholder for future features |

#### **Components**

| Component | File | Status | Features |
|-----------|------|--------|----------|
| Document Upload | `src/components/DocumentUpload.vue` | ✅ Complete | File upload, versioning, download, delete |
| Main Layout | `src/App.vue` | ✅ Complete | Responsive drawer, navigation, header |

#### **Core Features Implemented**

**Meeting Item Form (Main Feature):**
- ✅ Tabbed interface (General, Details, Documents)
- ✅ All mandatory fields with validation:
  - Topic (short title)
  - Purpose (full explanation)
  - Outcome (Decision/Discussion/Information)
  - Digital Product
  - Duration (minutes)
  - Requestor (auto-filled from auth)
  - Owner/Presenter (defaults to Requestor)
  - Sponsor (optional)
- ✅ Real-time validation with error messages
- ✅ Status workflow display with color-coded chips
- ✅ Form state management (draft/submit)

**Document Management:**
- ✅ File upload component
- ✅ Automatic versioning: `<ABBR><YYYYMMDD><TOPIC>.v<XX>`
- ✅ Version increment logic
- ✅ File type validation (PDF, Word, Excel, PowerPoint)
- ✅ File size validation (10MB max)
- ✅ Visual file indicators (icons, colors)
- ✅ Download/delete actions

**Status Workflow:**
- ✅ Status states defined: Submitted → Proposed → Planned → Discussed (+ Denied)
- ✅ Status display with color coding
- ✅ Permission-based status changes (prepared for backend)

**UI/UX:**
- ✅ Modern, clean design
- ✅ Responsive layout (desktop, tablet, mobile)
- ✅ Consistent spacing and typography
- ✅ Loading states and notifications
- ✅ Empty states and helpful hints

#### **Architecture (Future-Proof)**

**API Layer:**
- ✅ Service abstraction in `api/meetingItems.js`
- ✅ All CRUD endpoints defined
- ✅ Document upload/download/delete endpoints
- ✅ Field definitions endpoint (for templates)

**Composables (Future Expansion):**
- ✅ `composables/useDynamicFields.js` - Ready for template system
- ✅ Field validation logic
- ✅ Serialization/deserialization
- ✅ Category grouping

**Configuration:**
- ✅ Vite configuration with path aliases
- ✅ Router setup with lazy loading
- ✅ Quasar plugin configuration
- ✅ Git repository initialized

---

## 🚧 What Needs to Be Done

### **Phase 1: Backend Integration (Priority: HIGH)**

#### 1.1 Create .NET Backend API

**Recommended Tech Stack:**
- ASP.NET Core Web API (.NET 6 or 8)
- Entity Framework Core
- SQL Server or PostgreSQL
- Azure Blob Storage (for documents)

**Database Schema:**

```csharp
// Models to create

public class MeetingItem
{
    public Guid Id { get; set; }
    public string Topic { get; set; }
    public string Purpose { get; set; }
    public string Outcome { get; set; }
    public string DigitalProduct { get; set; }
    public int Duration { get; set; }
    public string Requestor { get; set; }
    public string OwnerPresenter { get; set; }
    public string? Sponsor { get; set; }
    public MeetingItemStatus Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public Guid? DecisionBoardId { get; set; }
    public Guid? TemplateId { get; set; } // For future templates

    // Navigation
    public virtual ICollection<Document> Documents { get; set; }
    public virtual ICollection<MeetingItemField> Fields { get; set; }
}

public class Document
{
    public Guid Id { get; set; }
    public Guid MeetingItemId { get; set; }
    public string OriginalFileName { get; set; }
    public string StoredFileName { get; set; }
    public string BlobUrl { get; set; }
    public string VersionNumber { get; set; }
    public DateTime UploadDate { get; set; }
    public long FileSize { get; set; }
    public string UploadedBy { get; set; }
}

public class FieldDefinition
{
    public Guid Id { get; set; }
    public Guid? TemplateId { get; set; }
    public string FieldName { get; set; }
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public string Category { get; set; }
    public int DisplayOrder { get; set; }
    public string? ConfigurationJson { get; set; } // JSON for options, validation
}

public class MeetingItemField
{
    public Guid Id { get; set; }
    public Guid MeetingItemId { get; set; }
    public Guid FieldDefinitionId { get; set; }
    public string Value { get; set; } // JSON or text
}

public enum MeetingItemStatus
{
    Submitted,
    Proposed,
    Planned,
    Discussed,
    Denied
}

public enum FieldType
{
    Text,
    MultipleChoice,
    YesNo,
    Selection,
    FileUpload
}
```

**API Endpoints to Implement:**

```
POST   /api/meeting-items              # Create meeting item
GET    /api/meeting-items/{id}         # Get single item
GET    /api/meeting-items              # List with filters
PUT    /api/meeting-items/{id}         # Update item
PATCH  /api/meeting-items/{id}/status  # Update status
DELETE /api/meeting-items/{id}         # Soft delete

POST   /api/meeting-items/{id}/documents    # Upload document
GET    /api/meeting-items/{id}/documents    # List documents
GET    /api/documents/{id}/download         # Download document
DELETE /api/documents/{id}                  # Delete document

GET    /api/field-definitions               # Get form schema
GET    /api/field-definitions?templateId={} # Get by template
```

**Controllers Example:**

```csharp
[ApiController]
[Route("api/meeting-items")]
public class MeetingItemsController : ControllerBase
{
    private readonly IMeetingItemService _service;

    [HttpPost]
    public async Task<ActionResult<MeetingItemDto>> Create(
        [FromBody] CreateMeetingItemRequest request)
    {
        // Validate
        // Create entity
        // Save to database
        // Return DTO
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeetingItemDto>> GetById(Guid id)
    {
        // Fetch from database
        // Include documents and fields
        // Return DTO
    }

    // ... other endpoints
}
```

#### 1.2 Update Frontend API Configuration

**In `api/meetingItems.js`:**

```javascript
// Change from:
const API_BASE_URL = process.env.VUE_APP_API_URL || '/api'

// To:
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://your-backend.azurewebsites.net/api'
```

**Create `.env.local` file:**

```env
VITE_API_URL=https://localhost:7001/api
```

#### 1.3 Connect Frontend to Backend

**In `src/pages/MeetingItemPage.vue`:**

Remove all `// TODO: API call` comments and uncomment API calls:

```javascript
import { meetingItemsApi } from '@/api/meetingItems'

// In loadMeetingItem()
const response = await meetingItemsApi.getMeetingItem(props.id)
meetingItem.value = response.data

// In handleSubmit()
const response = await meetingItemsApi.createMeetingItem(meetingItem.value)

// In handleSaveDraft()
await meetingItemsApi.updateMeetingItem(meetingItem.value.id, meetingItem.value)
```

**In `src/components/DocumentUpload.vue`:**

```javascript
// In handleUpload()
const formData = new FormData()
formData.append('file', file)
formData.append('storedFileName', storedFileName)
const response = await meetingItemsApi.uploadDocument(props.meetingItemId, formData)

// In handleDownload()
const blob = await meetingItemsApi.downloadDocument(doc.id)
const url = window.URL.createObjectURL(blob)
const a = document.createElement('a')
a.href = url
a.download = doc.originalFileName
a.click()

// In handleDelete()
await meetingItemsApi.deleteDocument(doc.id)
```

---

### **Phase 2: Authentication & Authorization (Priority: HIGH)**

#### 2.1 Implement Authentication

**Options:**
- Azure AD (recommended for KBC)
- IdentityServer
- Auth0
- JWT tokens

**Frontend Changes:**

```bash
npm install @azure/msal-vue
# or
npm install @auth0/auth0-vue
```

**In `src/main.js`:**

```javascript
import { createApp } from 'vue'
import { msalPlugin } from '@azure/msal-vue'

const msalConfig = {
  auth: {
    clientId: 'your-client-id',
    authority: 'https://login.microsoftonline.com/your-tenant-id'
  }
}

app.use(msalPlugin, msalConfig)
```

**In `src/router/index.js`:**

```javascript
router.beforeEach(async (to, from, next) => {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)

  if (requiresAuth && !isAuthenticated()) {
    next({ name: 'login' })
  } else {
    next()
  }
})
```

#### 2.2 Implement Authorization

**Role-based permissions:**
- Requestor: Can create, view own items
- Secretary/Chair: Can change status, assign to meetings
- Viewer: Can only view

**In components:**

```javascript
const canChangeStatus = computed(() => {
  return user.value.roles.includes('secretary') ||
         user.value.roles.includes('chair')
})
```

---

### **Phase 3: File Storage (Priority: HIGH)**

#### 3.1 Implement Azure Blob Storage

**Backend (.NET):**

```bash
dotnet add package Azure.Storage.Blobs
```

```csharp
public class DocumentService
{
    private readonly BlobContainerClient _containerClient;

    public async Task<string> UploadDocumentAsync(
        IFormFile file,
        string storedFileName)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        await blobClient.UploadAsync(file.OpenReadStream());
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadDocumentAsync(string storedFileName)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }
}
```

#### 3.2 Document Naming Logic

Implement the naming convention:

```csharp
public class DocumentNamingService
{
    public string GenerateStoredFileName(
        string decisionBoardAbbr,
        string topic,
        string originalFileName,
        int versionNumber)
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var topicSlug = SanitizeFileName(topic);
        var extension = Path.GetExtension(originalFileName);
        var version = versionNumber.ToString("D2");

        return $"{decisionBoardAbbr}{date}{topicSlug}.v{version}{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid))
                     .Substring(0, Math.Min(20, fileName.Length));
    }
}
```

---

### **Phase 4: Template System (Priority: MEDIUM)**

**This is for future expansion - already architected in the frontend!**

#### 4.1 Create Template Management UI

**New pages to add:**
- `src/pages/TemplatesListPage.vue`
- `src/pages/TemplateEditorPage.vue`
- `src/pages/FieldDefinitionEditor.vue`

#### 4.2 Backend Template Endpoints

```
GET    /api/templates                    # List templates
POST   /api/templates                    # Create template
PUT    /api/templates/{id}               # Update template
DELETE /api/templates/{id}               # Delete template

POST   /api/templates/{id}/fields        # Add field definition
PUT    /api/field-definitions/{id}       # Update field
DELETE /api/field-definitions/{id}       # Delete field
```

#### 4.3 Dynamic Form Rendering

**Already prepared in `composables/useDynamicFields.js`!**

Just need to create field components:

```
src/components/fields/
├── TextField.vue
├── MultipleChoiceField.vue
├── YesNoField.vue
├── SelectionField.vue
└── FileUploadField.vue
```

**Example:**

```vue
<!-- src/components/fields/TextField.vue -->
<template>
  <q-input
    v-model="modelValue"
    :label="field.fieldName"
    :hint="field.configuration?.hint"
    :rules="validationRules"
    outlined
    dense
  />
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: String,
  field: Object
})

const emit = defineEmits(['update:modelValue'])

const validationRules = computed(() => {
  const rules = []
  if (props.field.isRequired) {
    rules.push(val => !!val || `${props.field.fieldName} is required`)
  }
  return rules
})
</script>
```

**Then use in Details tab:**

```vue
<template v-for="field in fieldDefinitions" :key="field.id">
  <component
    :is="getFieldComponent(field.fieldType)"
    v-model="fieldValues[field.fieldName]"
    :field="field"
  />
</template>
```

---

### **Phase 5: Meeting Scheduling (Priority: MEDIUM)**

#### 5.1 Meeting Management

**New features needed:**
- Meeting instance creation
- Assign meeting items to meetings
- Set discussion time slots
- Meeting agenda generation

**Database additions:**

```csharp
public class Meeting
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; }
    public ICollection<MeetingItemAssignment> Assignments { get; set; }
}

public class MeetingItemAssignment
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid MeetingItemId { get; set; }
    public int Order { get; set; }
    public int AllocatedMinutes { get; set; }
}
```

---

### **Phase 6: Additional Features (Priority: LOW)**

#### 6.1 Notifications
- Email notifications on status changes
- Reminders for pending reviews
- Meeting invitations

#### 6.2 Reporting
- Meeting item statistics
- Status workflow reports
- Export to Excel/PDF

#### 6.3 Search & Filtering
- Full-text search
- Advanced filters
- Saved searches

#### 6.4 Audit Trail
- Track all changes to meeting items
- User activity logs
- Change history view

---

## 🛠️ Development Setup

### **Prerequisites**
- Node.js 16+ (Frontend)
- .NET 6 or 8 SDK (Backend)
- SQL Server or PostgreSQL (Database)
- Azure subscription (for Blob Storage)
- Git

### **Running the Frontend**

```bash
cd VueProject
npm install
npm run dev
# Access at http://localhost:5173
```

### **Creating the Backend**

```bash
dotnet new webapi -n MeetingItemsApi
cd MeetingItemsApi
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Azure.Storage.Blobs
dotnet run
# API runs at https://localhost:7001
```

---

## 📦 Deployment

### **Frontend (Azure Static Web Apps)**

```bash
npm run build
# Deploy dist/ folder to Azure Static Web Apps
```

### **Backend (Azure App Service)**

```bash
dotnet publish -c Release
# Deploy to Azure App Service
```

### **Database (Azure SQL)**

```bash
# Run migrations
dotnet ef database update
```

---

## 📋 Testing Checklist

### **Frontend Tests**
- [ ] Create new meeting item
- [ ] Edit existing meeting item
- [ ] Save as draft
- [ ] Submit meeting item
- [ ] Upload document
- [ ] Download document
- [ ] Delete document
- [ ] Version increment on duplicate upload
- [ ] Field validation (all required fields)
- [ ] Status transitions
- [ ] Responsive layout (mobile, tablet)
- [ ] Navigation between pages
- [ ] Filter and search in list view

### **Backend Tests**
- [ ] API authentication
- [ ] CRUD operations for meeting items
- [ ] Document upload to blob storage
- [ ] Document download
- [ ] Status workflow validation
- [ ] Role-based permissions
- [ ] File size and type validation
- [ ] Concurrent edits handling

### **Integration Tests**
- [ ] Frontend → Backend → Database flow
- [ ] File upload → Storage → Download flow
- [ ] User authentication → Authorization
- [ ] Status change notifications

---

## 🔐 Security Considerations

### **Already Implemented**
- ✅ Input validation on frontend
- ✅ File type validation
- ✅ File size limits

### **To Implement**
- [ ] HTTPS only
- [ ] CORS configuration
- [ ] SQL injection prevention (use EF Core parameterized queries)
- [ ] XSS prevention (Vue escapes by default)
- [ ] CSRF tokens
- [ ] Rate limiting
- [ ] Blob storage access control
- [ ] Sensitive data encryption
- [ ] Audit logging

---

## 📚 Documentation

### **User Documentation Needed**
- [ ] User manual for creating meeting items
- [ ] Admin guide for template management
- [ ] Secretary guide for meeting scheduling

### **Developer Documentation**
- [x] README.md (Basic setup)
- [x] IMPLEMENTATION_GUIDE.md (This file)
- [ ] API documentation (Swagger/OpenAPI)
- [ ] Database schema documentation
- [ ] Deployment guide

---

## 👥 Team Responsibilities

| Role | Responsibilities |
|------|------------------|
| **Frontend Developer** | Connect frontend to API, refine UI/UX, add features |
| **Backend Developer** | Build .NET API, database schema, business logic |
| **DevOps** | Set up CI/CD, Azure resources, monitoring |
| **Secretary/Chair** | Define field requirements, test workflow |
| **Security Team** | Review security implementation, approve for production |

---

## 📞 Support & Questions

For questions about the frontend implementation:
- Review component files (well-commented)
- Check `README.md` for setup instructions
- Review this guide for architecture decisions

For questions about requirements:
- Refer to original specification document
- Consult with secretary/chair for workflow clarification

---

## 🎯 Quick Win Milestones

**Week 1-2:**
- [ ] Backend API scaffolding
- [ ] Database schema implementation
- [ ] Authentication setup

**Week 3-4:**
- [ ] Connect frontend to backend
- [ ] Document storage implementation
- [ ] Basic testing

**Week 5-6:**
- [ ] Role-based permissions
- [ ] Status workflow validation
- [ ] User acceptance testing

**Week 7-8:**
- [ ] Bug fixes
- [ ] Performance optimization
- [ ] Production deployment

---

**Last Updated:** 2025-11-03
**Version:** 1.0
**Status:** Frontend Complete, Backend Pending
