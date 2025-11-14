# Template Builder - User Guide

Complete drag-and-drop template builder for creating custom meeting item templates.

---

## Overview

The Template Builder allows administrators to create and manage custom templates for meeting items. Each decision board can have multiple templates with unique field configurations.

**Key Features:**
- Drag-and-drop field designer
- 7 field types (Text, Number, Email, Date, Boolean, Dropdown, MultiSelect)
- Category-based organization
- Live field preview
- Validation rule configuration
- Dropdown/MultiSelect options management
- Field reordering and duplication
- Save draft and publish workflow

---

## Pages Created

### 1. TemplatesListPage.vue
**Route:** `/templates`

Template management dashboard with:
- Grid view of all templates
- Search and filter by decision board/status
- Template statistics (field count, category count)
- Quick actions (View, Edit, Duplicate, Activate/Deactivate)
- Empty state with call-to-action

**Screenshot Layout:**
```
┌─────────────────────────────────────────────────────────┐
│ Template Management                    [Create Template]│
├─────────────────────────────────────────────────────────┤
│ [Search...] [Decision Board ▼] [Status ▼]              │
├─────────────────────────────────────────────────────────┤
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐    │
│ │IT Governance │ │Architecture  │ │Security      │    │
│ │Standard      │ │Review        │ │Assessment    │    │
│ │              │ │              │ │              │    │
│ │12 Fields     │ │15 Fields     │ │10 Fields     │    │
│ │3 Categories  │ │4 Categories  │ │2 Categories  │    │
│ │              │ │              │ │              │    │
│ │[View][Edit]  │ │[View][Edit]  │ │[View][Edit]  │    │
│ └──────────────┘ └──────────────┘ └──────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 2. TemplateBuilderPage.vue
**Routes:**
- `/templates/create` - Create new template
- `/templates/:id` - Edit existing template

Full-featured drag-and-drop builder with 3-panel layout:

**Left Panel: Template Settings & Field Palette**
- Template name, description, decision board
- Template statistics
- Draggable field type palette

**Center Panel: Canvas (Drop Zone)**
- Category tabs
- Drag-and-drop area for fields
- Live field preview
- Field actions (Edit, Duplicate, Delete)

**Right Panel: Field Properties**
- Field configuration form
- Validation rules
- Dropdown/MultiSelect options editor

---

## Field Types

### 1. Text Input
**Icon:** text_fields
**Color:** blue-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle

**Validation Rules:**
- Min Length
- Max Length
- Regex Pattern
- Pattern Error Message

**Use Cases:**
- Short text answers
- Names, titles, descriptions
- Custom identifiers

---

### 2. Number
**Icon:** pin
**Color:** green-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle

**Validation Rules:**
- Minimum Value
- Maximum Value

**Use Cases:**
- Budget amounts
- Duration estimates
- Numeric ratings
- Quantities

---

### 3. Email
**Icon:** email
**Color:** purple-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle

**Validation Rules:**
- Min Length
- Max Length
- Email format (automatic)

**Use Cases:**
- Contact emails
- Stakeholder addresses
- Notification recipients

---

### 4. Date
**Icon:** event
**Color:** orange-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle

**Use Cases:**
- Due dates
- Milestone dates
- Review dates
- Go-live dates

---

### 5. Boolean (Checkbox)
**Icon:** check_box
**Color:** teal-6

**Properties:**
- Field Name (ID)
- Label
- Help Text
- Required toggle

**Use Cases:**
- Yes/No questions
- Acknowledgements
- Feature flags
- Compliance checkboxes

---

### 6. Dropdown
**Icon:** arrow_drop_down_circle
**Color:** indigo-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle
- Options (configurable)

**Options Configuration:**
- Value (internal ID)
- Label (display text)
- Display Order (drag to reorder)
- Is Default (checkbox)

**Use Cases:**
- Priority levels (Low, Medium, High)
- Status selections
- Department choices
- Single-choice categories

**Example:**
```javascript
{
  fieldName: 'priority',
  fieldType: 'Dropdown',
  options: [
    { value: 'low', label: 'Low', displayOrder: 0, isDefault: false },
    { value: 'medium', label: 'Medium', displayOrder: 1, isDefault: true },
    { value: 'high', label: 'High', displayOrder: 2, isDefault: false }
  ]
}
```

---

### 7. MultiSelect
**Icon:** checklist
**Color:** pink-6

**Properties:**
- Field Name (ID)
- Label
- Placeholder
- Help Text
- Required toggle
- Options (configurable)

**Options Configuration:**
- Value (internal ID)
- Label (display text)
- Display Order (drag to reorder)
- Is Default (checkbox)

**Use Cases:**
- Multiple stakeholders
- Affected systems
- Technology stack
- Multiple impacts

**Example:**
```javascript
{
  fieldName: 'affectedSystems',
  fieldType: 'MultiSelect',
  options: [
    { value: 'crm', label: 'CRM', displayOrder: 0, isDefault: false },
    { value: 'erp', label: 'ERP', displayOrder: 1, isDefault: false },
    { value: 'portal', label: 'Customer Portal', displayOrder: 2, isDefault: false }
  ]
}
```

---

## Workflow: Creating a Template

### Step 1: Navigate to Template Builder
1. Go to `/templates`
2. Click **"Create Template"** button
3. You'll be taken to `/templates/create`

### Step 2: Configure Template Settings (Left Panel)

Fill in basic information:
```
Template Name: IT Governance Standard Template
Description: Comprehensive template for IT Governance Board
Decision Board: IT Governance
Active Template: ✓ (enabled)
```

### Step 3: Add Categories

Categories help organize fields logically.

**Default Categories:**
- General
- Technical
- Business

**To Add a Category:**
1. Click **"Add Category"** button in canvas tabs
2. Enter category name (e.g., "Security Assessment")
3. Click **"Add"**
4. New tab appears in canvas

### Step 4: Add Fields (Drag & Drop)

**From Left Panel Field Palette:**
1. Drag a field type (e.g., "Text Input") from palette
2. Drop it onto the canvas drop zone
3. Field appears with default configuration

**Field appears as:**
```
┌─────────────────────────────────────────────┐
│ ⋮⋮ 📝 Text Input 1            [Required]   │
│ ────────────────────────────────────────── │
│ [Input field preview - disabled]            │
│ ℹ️  Additional guidance text (if configured)│
│                           [Edit][Copy][Del] │
└─────────────────────────────────────────────┘
```

### Step 5: Configure Field Properties (Right Panel)

Click on a field to select it. Right panel shows properties:

**Basic Properties:**
```
Field Name (ID): architectureImpact
Label: Architecture Impact Assessment
Field Type: Dropdown
Placeholder: Select impact level
Help Text: Describe the architectural changes required
Display Order: 0
Required Field: ✓
```

**Validation Rules (for Text fields):**
```
Min Length: 10
Max Length: 500
Regex Pattern: (optional)
Pattern Error Message: (optional)
```

**Validation Rules (for Number fields):**
```
Minimum Value: 0
Maximum Value: 1000000
```

**Options (for Dropdown/MultiSelect):**
```
┌─────────────────────────────────────────┐
│ ⋮⋮ [low      ▼] [Low       ▼] ☐ [×]   │
│ ⋮⋮ [medium   ▼] [Medium    ▼] ✓ [×]   │
│ ⋮⋮ [high     ▼] [High      ▼] ☐ [×]   │
│ ⋮⋮ [critical ▼] [Critical  ▼] ☐ [×]   │
│ [+ Add Option]                          │
└─────────────────────────────────────────┘
```

### Step 6: Reorder Fields

**Drag Handle (⋮⋮):**
- Click and hold the drag handle on the left of each field
- Drag up or down to reorder
- Display Order updates automatically

**Between Categories:**
- Drag a field to a different category tab
- Field's category updates automatically

### Step 7: Duplicate Fields

For similar fields with slight variations:
1. Click **[Copy]** button on field
2. New field appears with "_copy" suffix
3. Modify the duplicated field as needed

**Example Use Case:**
- Create "Primary Contact Email"
- Duplicate to "Secondary Contact Email"
- Update label and help text

### Step 8: Save Draft

Click **"Save Draft"** button to save work in progress.

**Draft behavior:**
- Template not visible to users
- Can continue editing later
- Not assigned to decision board yet

### Step 9: Publish Template

Click **"Publish Template"** button when ready.

**Validation:**
- Template name required
- Decision board required
- At least 1 field required

**After Publishing:**
- Template becomes active
- Available for creating meeting items
- Can still edit (creates new version)

---

## Advanced Features

### Category Management

**Add Category:**
```
1. Click "Add Category" button in canvas tabs
2. Enter name: "Risk Assessment"
3. Category tab appears
4. Drag fields into new category
```

**Organize by Category:**
- General: Basic information fields
- Technical: Architecture, technology stack
- Business: Budget, timeline, stakeholders
- Security: Compliance, risk assessment
- Custom: Domain-specific categories

### Field Validation Examples

**Email with Length Constraints:**
```javascript
{
  fieldName: 'primaryEmail',
  fieldType: 'Email',
  validationRules: {
    minLength: 5,
    maxLength: 100
  }
}
```

**Text with Regex Pattern:**
```javascript
{
  fieldName: 'projectCode',
  fieldType: 'Text',
  validationRules: {
    pattern: '^[A-Z]{3}[0-9]{4}$',
    patternMessage: 'Format: ABC1234 (3 letters + 4 digits)'
  }
}
```

**Number with Range:**
```javascript
{
  fieldName: 'budget',
  fieldType: 'Number',
  validationRules: {
    min: 0,
    max: 10000000
  }
}
```

### Dropdown Options Management

**Creating Options:**
1. Select dropdown/multiselect field
2. Scroll to "Options" section in right panel
3. Click "Add Option"
4. Configure:
   - **Value**: Internal identifier (e.g., "high")
   - **Label**: Display text (e.g., "High Priority")
   - **Default**: Check to pre-select
5. Drag ⋮⋮ to reorder

**Best Practices:**
- Use descriptive labels
- Keep values lowercase and simple
- Set sensible defaults
- Limit options to 10-15 for usability

### Field Reordering Strategy

**Display Order determines:**
- Order within category
- Tab order for keyboard navigation
- Form rendering sequence

**Recommended Order:**
1. Required fields first
2. Logical grouping (related fields together)
3. Optional fields last
4. Help text for complex fields

---

## UI Components Breakdown

### Left Panel: Field Palette

**Field Type Items:**
```html
<div class="field-type-item">
  <q-icon name="text_fields" color="blue-6" />
  <span>Text Input</span>
</div>
```

**Hover Effect:**
- Border changes to primary color
- Slight translate animation
- Background lightens

**Drag Behavior:**
- Pull: Clone (original stays in palette)
- Creates new field instance
- Assigns unique ID automatically

### Center Panel: Canvas

**Drop Zone States:**

**Empty:**
```
┌──────────────────────────────────────┐
│                                      │
│         ⊕ (large icon)               │
│                                      │
│  Drag fields from the left panel    │
│       to get started                 │
│                                      │
└──────────────────────────────────────┘
```

**With Fields:**
```
┌──────────────────────────────────────┐
│ General | Technical | Business       │
├──────────────────────────────────────┤
│ ┌────────────────────────────────┐  │
│ │ ⋮⋮ 📝 Project Name    [Req]    │  │
│ │ [input preview]         [···]  │  │
│ └────────────────────────────────┘  │
│ ┌────────────────────────────────┐  │
│ │ ⋮⋮ 📧 Contact Email   [Req]    │  │
│ │ [input preview]         [···]  │  │
│ └────────────────────────────────┘  │
└──────────────────────────────────────┘
```

**Field Selected State:**
- Blue border
- Light blue background
- Shadow effect

### Right Panel: Properties

**States:**

**No Selection:**
```
┌──────────────────────────┐
│ Field Properties      [×]│
├──────────────────────────┤
│                          │
│     ⚙️ (large icon)      │
│                          │
│  Select a field to edit  │
│    its properties        │
│                          │
└──────────────────────────┘
```

**Field Selected:**
```
┌──────────────────────────┐
│ Field Properties      [×]│
├──────────────────────────┤
│ Field Name (ID) *        │
│ [architectureImpact   ]  │
│                          │
│ Label *                  │
│ [Architecture Impact  ]  │
│                          │
│ Field Type *             │
│ [Dropdown            ▼]  │
│                          │
│ ☑️ Required Field        │
│                          │
│ ───── Options ─────      │
│ ⋮⋮ [low][Low     ] ☐ [×]│
│ ⋮⋮ [med][Medium  ] ✓ [×]│
│ [+ Add Option]           │
└──────────────────────────┘
```

---

## Data Structure

### Template Object

```javascript
{
  id: 'template-123',
  name: 'IT Governance Standard',
  description: 'Comprehensive template for IT Governance Board',
  decisionBoardId: 'board-456',
  isActive: true,
  fields: [
    {
      id: 'field_1705145234567_abc123',
      fieldName: 'projectName',
      label: 'Project Name',
      fieldType: 'Text',
      category: 'General',
      displayOrder: 0,
      isRequired: true,
      isActive: true,
      placeholderText: 'Enter project name',
      helpText: 'Official name of the project or initiative',
      validationRules: {
        minLength: 3,
        maxLength: 100,
        min: null,
        max: null,
        pattern: null,
        patternMessage: null
      },
      options: null
    },
    {
      id: 'field_1705145234568_def456',
      fieldName: 'architectureImpact',
      label: 'Architecture Impact',
      fieldType: 'Dropdown',
      category: 'Technical',
      displayOrder: 0,
      isRequired: true,
      isActive: true,
      placeholderText: 'Select impact level',
      helpText: 'Level of architectural change required',
      validationRules: {
        minLength: null,
        maxLength: null,
        min: null,
        max: null,
        pattern: null,
        patternMessage: null
      },
      options: [
        {
          value: 'low',
          label: 'Low',
          displayOrder: 0,
          isDefault: false,
          isActive: true
        },
        {
          value: 'medium',
          label: 'Medium',
          displayOrder: 1,
          isDefault: true,
          isActive: true
        },
        {
          value: 'high',
          label: 'High',
          displayOrder: 2,
          isDefault: false,
          isActive: true
        }
      ]
    }
  ]
}
```

### Field ID Generation

```javascript
const uniqueId = `field_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
// Example: field_1705145234567_abc123def
```

**Benefits:**
- Guaranteed uniqueness
- Timestamp for debugging
- Random suffix prevents collisions

---

## API Integration (Future)

### Create Template
```javascript
POST /api/templates

{
  name: "IT Governance Standard",
  description: "...",
  decisionBoardId: "board-123",
  isActive: true,
  fieldDefinitions: [
    {
      fieldName: "projectName",
      label: "Project Name",
      fieldType: "Text",
      category: "General",
      displayOrder: 0,
      isRequired: true,
      placeholderText: "Enter project name",
      helpText: "...",
      validationRulesJson: JSON.stringify({ minLength: 3, maxLength: 100 }),
      options: null
    }
  ]
}
```

### Update Template
```javascript
PUT /api/templates/{id}

// Same structure as create
// Existing fields updated, new fields added, removed fields deactivated
```

### Get Template
```javascript
GET /api/templates/{id}

Response:
{
  templateId: "template-123",
  templateName: "IT Governance Standard",
  description: "...",
  fieldDefinitions: [
    {
      id: "field-def-789",
      fieldName: "projectName",
      label: "Project Name",
      fieldType: "Text",
      isRequired: true,
      category: "General",
      displayOrder: 0,
      helpText: "...",
      placeholderText: "...",
      validationRules: { minLength: 3, maxLength: 100 },
      options: null
    }
  ]
}
```

---

## Best Practices

### Template Design

1. **Start with MVP:**
   - Include only essential fields
   - Test with real users
   - Iterate based on feedback

2. **Logical Grouping:**
   - Use categories for related fields
   - General → Technical → Business flow
   - Keep categories balanced (3-5 fields each)

3. **Clear Labels:**
   - Use descriptive, unambiguous labels
   - Avoid jargon unless domain-specific
   - Add help text for complex fields

4. **Required vs Optional:**
   - Mark truly essential fields as required
   - Allow flexibility where possible
   - Use validation rules to guide users

### Field Naming Conventions

**Good Field Names:**
- `projectName` (camelCase)
- `estimatedBudget`
- `primaryContactEmail`
- `architectureImpact`

**Bad Field Names:**
- `project-name` (kebab-case not allowed)
- `ProjectName` (PascalCase incorrect)
- `name` (too generic)
- `field1` (not descriptive)

### Validation Rules

**When to Use:**
- Text length: Ensure quality responses
- Number range: Prevent unrealistic values
- Email: Auto-validated by field type
- Regex: Enforce specific formats (IDs, codes)

**When NOT to Use:**
- Over-constraining creativity
- Business rules better handled in backend
- Complex conditional validation

### Dropdown Options

**Guidelines:**
- 3-10 options ideal
- Use "Other" option for flexibility
- Alphabetical order (unless priority matters)
- Consistent capitalization
- Avoid duplicates

---

## Keyboard Shortcuts (Future Enhancement)

- `Ctrl+S` - Save draft
- `Ctrl+Enter` - Publish template
- `Delete` - Delete selected field
- `Ctrl+D` - Duplicate selected field
- `Ctrl+Z` - Undo last action
- `Ctrl+Shift+Z` - Redo
- `↑/↓` - Navigate fields
- `Tab` - Next property input

---

## Troubleshooting

### Field Not Dragging

**Symptoms:** Field type item doesn't drag from palette

**Solutions:**
1. Check browser console for errors
2. Ensure vuedraggable installed: `npm install vuedraggable@next`
3. Verify drag handle not disabled

### Field Properties Not Updating

**Symptoms:** Changes in right panel don't reflect in canvas

**Solutions:**
1. Ensure `@update:model-value="updateField"` present
2. Check reactivity: `template.value.fields = [...template.value.fields]`
3. Verify selectedField is reactive ref

### Options Not Saving

**Symptoms:** Dropdown options disappear after adding

**Solutions:**
1. Check options array initialization
2. Ensure `updateField()` called after changes
3. Verify options structure matches expected format

---

## Future Enhancements

### Planned Features

1. **Conditional Logic:**
   - Show field if another field has specific value
   - Enable/disable based on conditions

2. **Field Dependencies:**
   - Calculated fields (e.g., total = field1 + field2)
   - Cascading dropdowns

3. **Templates from Templates:**
   - Clone existing template as starting point
   - Template marketplace

4. **Version History:**
   - Track template changes over time
   - Rollback to previous version

5. **Preview Mode:**
   - See exactly how form will appear to users
   - Test validation rules

6. **Import/Export:**
   - Export template as JSON
   - Import from file or other boards

7. **Field Library:**
   - Save custom field configurations
   - Reuse across templates

---

**Template Builder is production-ready for V8-V9 evolution phase!**
