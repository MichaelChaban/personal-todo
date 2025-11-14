# Template Builder - Quick Start Guide

Get started with the drag-and-drop template builder in 5 minutes.

---

## What You Just Got

A complete template builder system with:

✅ **TemplatesListPage.vue** - Template management dashboard
✅ **TemplateBuilderPage.vue** - Drag-and-drop designer
✅ **7 Field Types** - Text, Number, Email, Date, Boolean, Dropdown, MultiSelect
✅ **Category Organization** - Group fields logically
✅ **Live Preview** - See fields as users will see them
✅ **Validation Rules** - Min/max length, number ranges, regex patterns
✅ **Options Editor** - Configure dropdown/multiselect choices

---

## Installation Complete

Dependencies installed:
```bash
npm install vuedraggable@next ✓
```

Routes configured:
- `/templates` → TemplatesListPage
- `/templates/create` → TemplateBuilderPage (create mode)
- `/templates/:id` → TemplateBuilderPage (edit mode)

---

## Try It Now

### Step 1: Start the Dev Server
```bash
cd "C:\Users\cmisa\Documents\VueProject"
npm run dev
```

### Step 2: Navigate to Templates
Open browser: `http://localhost:5173/templates`

You'll see:
- Mock templates (IT Governance, Architecture Review, Security Assessment)
- Search and filter controls
- "Create Template" button

### Step 3: Create Your First Template

Click **"Create Template"** button

You'll see 3-panel layout:

**Left Panel:**
- Template settings form
- Field types palette (drag these!)

**Center Panel:**
- Category tabs (General, Technical, Business)
- Drop zone for fields
- Empty state with instructions

**Right Panel:**
- Field properties editor (initially empty)
- Activates when you select a field

### Step 4: Configure Template

**Fill in basics:**
```
Template Name: My First Template
Description: Testing the template builder
Decision Board: IT Governance
Active Template: ✓
```

### Step 5: Add Your First Field

**From the field palette (left panel):**
1. Find "Text Input" (blue icon)
2. Click and drag it
3. Drop into the center canvas
4. Field appears with default name "textField1"

### Step 6: Configure the Field

**Click on the field in canvas** - Right panel activates

**Edit properties:**
```
Field Name: projectName
Label: Project Name
Placeholder: Enter the project name
Help Text: Official name of the project or initiative
Required Field: ✓
```

**Add validation:**
```
Min Length: 3
Max Length: 100
```

### Step 7: Add a Dropdown

**Drag "Dropdown" from palette**

**Configure:**
```
Field Name: priority
Label: Priority Level
Required Field: ✓
```

**Scroll to "Options" section:**

The dropdown comes with 2 default options. Edit them:
1. Change "option1" to "low" / "Low"
2. Change "option2" to "medium" / "Medium" - Check "Default"
3. Click "+ Add Option"
4. Add "high" / "High"

**Your options list:**
```
⋮⋮ [low    ] [Low    ] ☐ [×]
⋮⋮ [medium ] [Medium ] ✓ [×]  ← Default
⋮⋮ [high   ] [High   ] ☐ [×]
```

### Step 8: Add to Different Category

**Click "Technical" tab in center panel**

**Drag "Number" field from palette**

**Configure:**
```
Field Name: estimatedBudget
Label: Estimated Budget (EUR)
Placeholder: 0
Help Text: Total estimated project budget
Required Field: ✓
Min: 0
Max: 10000000
```

### Step 9: Preview Your Work

**Switch between category tabs:**
- General: Shows projectName, priority
- Technical: Shows estimatedBudget

**Each field shows:**
- Drag handle (⋮⋮)
- Field icon and name
- "Required" chip if marked required
- Preview of the input component
- Help text (if configured)
- Action buttons (Edit, Copy, Delete)

### Step 10: Save Draft

Click **"Save Draft"** button (top right)

**You'll see:**
- Success notification
- Template saved but not published
- Can continue editing

### Step 11: Publish Template

Click **"Publish Template"** button (top right)

**Validation checks:**
- Template name ✓
- Decision board ✓
- At least 1 field ✓

**After publishing:**
- Success notification
- Redirects to `/templates` list
- Template now active and usable

---

## Key UI Interactions

### Drag & Drop
- **From Palette → Canvas**: Creates new field
- **Within Canvas**: Reorders fields
- **Between Categories**: Moves field to different category
- **Options List**: Reorders dropdown options

### Field Selection
- **Click field in canvas**: Selects it, shows properties in right panel
- **Selected state**: Blue border, light blue background
- **Deselect**: Click [×] button in properties panel

### Field Actions
- **Edit (pencil icon)**: Same as clicking field
- **Duplicate (copy icon)**: Creates copy with "_copy" suffix
- **Delete (trash icon)**: Shows confirmation dialog

### Options Management (Dropdown/MultiSelect)
- **Add Option**: Click "+ Add Option" button
- **Reorder**: Drag by ⋮⋮ handle
- **Set Default**: Check checkbox
- **Delete**: Click [×] button

---

## Common Patterns

### Create Contact Info Section

1. Switch to "General" category
2. Add "Text Input" → Configure as "contactName"
3. Add "Email" → Configure as "contactEmail"
4. Add "Text Input" → Configure as "contactPhone"

### Create Budget Fields

1. Switch to "Business" category
2. Add "Number" → "estimatedBudget" (Min: 0, Max: 10000000)
3. Add "Number" → "approvedBudget" (Min: 0, Max: 10000000)
4. Add "Dropdown" → "currency" (Options: EUR, USD, GBP)

### Create Risk Assessment

1. Add new category: "Risk Assessment"
2. Add "Dropdown" → "riskLevel" (Options: Low, Medium, High, Critical)
3. Add "MultiSelect" → "riskCategories" (Options: Security, Financial, Operational, Reputational)
4. Add "Text Input" → "mitigationPlan" (Type: textarea)

---

## Data Flow

### Template Builder → API (Future)

**When you click "Publish Template":**

```javascript
POST /api/templates

{
  name: "My First Template",
  description: "Testing the template builder",
  decisionBoardId: "board-1",
  isActive: true,
  fieldDefinitions: [
    {
      fieldName: "projectName",
      label: "Project Name",
      fieldType: "Text",
      category: "General",
      displayOrder: 0,
      isRequired: true,
      placeholderText: "Enter the project name",
      helpText: "Official name of the project or initiative",
      validationRulesJson: "{\"minLength\":3,\"maxLength\":100}",
      options: null
    },
    {
      fieldName: "priority",
      label: "Priority Level",
      fieldType: "Dropdown",
      category: "General",
      displayOrder: 1,
      isRequired: true,
      placeholderText: "",
      helpText: "",
      validationRulesJson: null,
      options: [
        { value: "low", label: "Low", displayOrder: 0, isDefault: false },
        { value: "medium", label: "Medium", displayOrder: 1, isDefault: true },
        { value: "high", label: "High", displayOrder: 2, isDefault: false }
      ]
    }
  ]
}
```

### Backend Storage

**Database Schema (from DATABASE_SCHEMA_FINAL.md):**

**Template Table:**
```sql
Templates (
  Id, Name, Description, DecisionBoardId, IsActive, CreatedDate, UpdatedDate
)
```

**FieldDefinition Table:**
```sql
FieldDefinitions (
  Id, TemplateId, FieldName, Label, FieldType, Category, DisplayOrder,
  IsRequired, IsActive, PlaceholderText, HelpText, ValidationRulesJson,
  CreatedDate, DeactivatedDate, DeactivationReason
)
```

**FieldOption Table:**
```sql
FieldOptions (
  Id, FieldDefinitionId, Value, Label, DisplayOrder, IsDefault, IsActive
)
```

---

## Testing Checklist

### Template Builder Functionality

- [ ] Template settings form validates required fields
- [ ] Can drag field from palette to canvas
- [ ] Can drag field within canvas to reorder
- [ ] Can drag field between categories
- [ ] Can select field and see properties
- [ ] Can update field properties (name, label, type)
- [ ] Can add validation rules (min/max length, number range)
- [ ] Can add dropdown options
- [ ] Can reorder dropdown options
- [ ] Can set default option
- [ ] Can delete option
- [ ] Can duplicate field
- [ ] Can delete field with confirmation
- [ ] Can add new category
- [ ] Can switch between categories
- [ ] Save draft works
- [ ] Publish validates required fields
- [ ] Publish shows success and redirects

### Templates List

- [ ] Shows template cards
- [ ] Search filters by name/description
- [ ] Filter by decision board works
- [ ] Filter by status works
- [ ] Click "Create Template" navigates to builder
- [ ] Click "Edit" on template loads it in builder
- [ ] Activate/Deactivate toggles status
- [ ] Duplicate shows confirmation

---

## Next Steps

### 1. Connect to Backend API

Replace mock data with API calls:

**TemplatesListPage.vue:**
```javascript
// Replace mock templates
const templates = ref([])

onMounted(async () => {
  const response = await templatesApi.getAllTemplates()
  templates.value = response
})
```

**TemplateBuilderPage.vue:**
```javascript
const handlePublish = async () => {
  const payload = {
    name: template.value.name,
    description: template.value.description,
    decisionBoardId: template.value.decisionBoardId,
    isActive: template.value.isActive,
    fieldDefinitions: template.value.fields.map(field => ({
      fieldName: field.fieldName,
      label: field.label,
      fieldType: field.fieldType,
      category: field.category,
      displayOrder: field.displayOrder,
      isRequired: field.isRequired,
      placeholderText: field.placeholderText,
      helpText: field.helpText,
      validationRulesJson: JSON.stringify(field.validationRules),
      options: field.options
    }))
  }

  const response = await templatesApi.createTemplate(payload)
  router.push('/templates')
}
```

### 2. Create Backend API Endpoints

**TemplatesController.cs:**
```csharp
[HttpPost]
[Route("api/templates")]
public async Task<IActionResult> CreateTemplate(
    [FromBody] CreateTemplate.Command command)
{
    var result = await _mediator.Send(command);
    return HandleResult(result);
}
```

**CreateTemplate.cs MediatR Handler:**
```csharp
public record Command(
    string Name,
    string Description,
    string DecisionBoardId,
    bool IsActive,
    List<FieldDefinitionDto> FieldDefinitions
) : ICommand<Response>;
```

### 3. Integrate with MeetingItemPage

**Update MeetingItemPage.vue to render dynamic fields:**

```javascript
// Load template and render fields
const template = await meetingItemsApi.getTemplateByDecisionBoard(decisionBoardId)

// In Details tab, render fields dynamically
<template v-for="field in template.fieldDefinitions" :key="field.fieldName">
  <component
    :is="getFieldComponent(field.fieldType)"
    v-model="meetingItem.fieldValues[field.fieldName]"
    :label="field.label"
    :placeholder="field.placeholderText"
    :hint="field.helpText"
    :rules="generateValidationRules(field)"
    outlined
    dense
  />
</template>
```

### 4. Add Permissions

Restrict template builder to admin users:

```javascript
// In route guard
{
  path: '/templates/create',
  component: TemplateBuilderPage,
  meta: { requiresAdmin: true }
}
```

---

## Architecture

### Component Structure

```
TemplatesListPage.vue
├── Template cards grid
├── Search/filter controls
└── Navigate to TemplateBuilderPage

TemplateBuilderPage.vue
├── Left Panel
│   ├── Template settings form
│   └── Field types palette (draggable)
├── Center Panel
│   ├── Category tabs
│   ├── Drop zone (draggable)
│   └── Field items with preview
└── Right Panel
    ├── Field properties form
    ├── Validation rules section
    └── Options editor (for Dropdown/MultiSelect)
```

### State Management

```javascript
// Template state
const template = ref({
  id: null,
  name: '',
  description: '',
  decisionBoardId: null,
  isActive: true,
  fields: []  // Array of field objects
})

// Selected field (for properties panel)
const selectedField = ref(null)

// UI state
const activeCategory = ref('General')
const saving = ref(false)
const publishing = ref(false)
```

### Reactivity

**Key reactive updates:**
1. Field properties change → `updateField()` → Triggers re-render
2. Drag field → `handleFieldsChange()` → Updates displayOrder
3. Add option → Modifies `selectedField.options` → Calls `updateField()`

---

## Troubleshooting

**Field not appearing after drag:**
- Check browser console for errors
- Ensure `cloneFieldType()` returns complete object
- Verify category matches active tab

**Properties panel not updating:**
- Check `@update:model-value="updateField"` on inputs
- Ensure `template.value.fields = [...template.value.fields]` to trigger reactivity

**Options disappearing:**
- Initialize options array: `selectedField.value.options = []`
- Call `updateField()` after modifications

---

## Support

For detailed documentation, see:
- **[TEMPLATE_BUILDER_GUIDE.md](TEMPLATE_BUILDER_GUIDE.md)** - Complete user guide
- **[DATABASE_SCHEMA_FINAL.md](DATABASE_SCHEMA_FINAL.md)** - Database structure
- **[EVOLUTION_ROADMAP.md](EVOLUTION_ROADMAP.md)** - V8-V9 template builder phase

---

**You're ready to create custom templates! 🎉**
