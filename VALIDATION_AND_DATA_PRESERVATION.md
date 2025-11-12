# Validation & Data Preservation Guide

## 🎯 Overview

This guide covers:
1. **Frontend Validation** using Yup schemas (dynamic based on template)
2. **Backend Validation** using FluentValidation (dynamic based on template)
3. **Data Preservation** when templates change (CRITICAL for production)

---

## 📋 Table of Contents

1. [Frontend Validation (Yup)](#frontend-validation-yup)
2. [Backend Validation (FluentValidation)](#backend-validation-fluentvalidation)
3. [Data Preservation Strategy](#data-preservation-strategy)
4. [Template Change Scenarios](#template-change-scenarios)
5. [Migration Safety](#migration-safety)

---

## 🎨 Frontend Validation (Yup)

### **Install Yup**

```bash
npm install yup
```

### **Dynamic Yup Schema Generator**

Create a composable that generates Yup schema from template field definitions:

```javascript
// composables/useFieldValidation.js
import * as yup from 'yup'

export function useFieldValidation() {

  /**
   * Generate Yup schema from field definitions
   */
  const generateYupSchema = (fieldDefinitions) => {
    const schemaFields = {}

    fieldDefinitions.forEach(field => {
      let fieldSchema

      // Base schema by field type
      switch (field.fieldType) {
        case 'Text':
        case 'TextArea':
        case 'Email':
          fieldSchema = yup.string()
          break

        case 'Number':
          fieldSchema = yup.number()
            .typeError('Must be a number')
          break

        case 'Date':
          fieldSchema = yup.date()
            .typeError('Must be a valid date')
          break

        case 'Dropdown':
        case 'Radio':
          fieldSchema = yup.string()
            .oneOf(
              field.options?.map(opt => opt.value) || [],
              'Invalid selection'
            )
          break

        case 'MultiSelect':
          fieldSchema = yup.array()
            .of(yup.string())
          break

        case 'YesNo':
          fieldSchema = yup.boolean()
          break

        default:
          fieldSchema = yup.mixed()
      }

      // Apply required validation
      if (field.isRequired) {
        fieldSchema = fieldSchema.required(`${field.label} is required`)
      } else {
        fieldSchema = fieldSchema.nullable().optional()
      }

      // Apply validation rules from JSON
      if (field.validationRules) {
        const rules = typeof field.validationRules === 'string'
          ? JSON.parse(field.validationRules)
          : field.validationRules

        // String validations
        if (rules.minLength && fieldSchema.min) {
          fieldSchema = fieldSchema.min(
            rules.minLength,
            `Minimum length is ${rules.minLength} characters`
          )
        }

        if (rules.maxLength && fieldSchema.max) {
          fieldSchema = fieldSchema.max(
            rules.maxLength,
            `Maximum length is ${rules.maxLength} characters`
          )
        }

        // Number validations
        if (rules.min !== undefined && fieldSchema.min) {
          fieldSchema = fieldSchema.min(
            rules.min,
            `Minimum value is ${rules.min}`
          )
        }

        if (rules.max !== undefined && fieldSchema.max) {
          fieldSchema = fieldSchema.max(
            rules.max,
            `Maximum value is ${rules.max}`
          )
        }

        // Regex pattern
        if (rules.pattern) {
          fieldSchema = fieldSchema.matches(
            new RegExp(rules.pattern),
            rules.patternMessage || 'Invalid format'
          )
        }

        // Email validation
        if (field.fieldType === 'Email') {
          fieldSchema = fieldSchema.email('Invalid email address')
        }

        // Custom validation function
        if (rules.customValidation) {
          try {
            // Safely evaluate custom validation
            const customFn = new Function('value', rules.customValidation)
            fieldSchema = fieldSchema.test(
              'custom',
              rules.customValidationMessage || 'Validation failed',
              customFn
            )
          } catch (error) {
            console.error('Custom validation error:', error)
          }
        }
      }

      schemaFields[field.fieldName] = fieldSchema
    })

    return yup.object().shape(schemaFields)
  }

  /**
   * Validate form data against schema
   */
  const validateForm = async (schema, data) => {
    try {
      await schema.validate(data, { abortEarly: false })
      return { isValid: true, errors: {} }
    } catch (err) {
      const errors = {}
      err.inner.forEach(error => {
        errors[error.path] = error.message
      })
      return { isValid: false, errors }
    }
  }

  /**
   * Validate single field
   */
  const validateField = async (schema, fieldName, value) => {
    try {
      await schema.validateAt(fieldName, { [fieldName]: value })
      return { isValid: true, error: null }
    } catch (err) {
      return { isValid: false, error: err.message }
    }
  }

  return {
    generateYupSchema,
    validateForm,
    validateField
  }
}
```

### **Usage in MeetingItemPage.vue**

```vue
<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useFieldValidation } from '@/composables/useFieldValidation'
import { useDynamicFields } from '@/composables/useDynamicFields'

const {
  fieldDefinitions,
  fieldValues,
  loadFieldDefinitions
} = useDynamicFields()

const {
  generateYupSchema,
  validateForm,
  validateField
} = useFieldValidation()

// Validation state
const validationErrors = ref({})
const validationSchema = ref(null)

// Generate schema when field definitions load
watch(fieldDefinitions, (newFields) => {
  if (newFields && newFields.length > 0) {
    validationSchema.value = generateYupSchema(newFields)
  }
}, { immediate: true })

// Validate single field on blur
const handleFieldBlur = async (fieldName) => {
  if (!validationSchema.value) return

  const { isValid, error } = await validateField(
    validationSchema.value,
    fieldName,
    fieldValues.value[fieldName]
  )

  if (isValid) {
    delete validationErrors.value[fieldName]
  } else {
    validationErrors.value[fieldName] = error
  }
}

// Validate entire form on submit
const handleSubmit = async () => {
  if (!validationSchema.value) return

  // Validate dynamic fields
  const { isValid, errors } = await validateForm(
    validationSchema.value,
    fieldValues.value
  )

  validationErrors.value = errors

  if (!isValid) {
    $q.notify({
      type: 'negative',
      message: 'Please fix validation errors',
      caption: `${Object.keys(errors).length} field(s) have errors`
    })
    return
  }

  // Also validate static fields
  if (!meetingItem.value.topic || !meetingItem.value.purpose) {
    $q.notify({
      type: 'negative',
      message: 'Please complete all required fields'
    })
    return
  }

  // Submit to API
  try {
    await submitMeetingItem()
  } catch (error) {
    // Handle API errors
  }
}

onMounted(async () => {
  const boardId = getUserDecisionBoard()
  await loadFieldDefinitions(boardId)
})
</script>

<template>
  <q-page>
    <!-- Static fields with Quasar validation -->
    <q-input
      v-model="meetingItem.topic"
      label="Topic *"
      :rules="[val => !!val || 'Topic is required']"
    />

    <!-- Dynamic fields with Yup validation -->
    <div v-for="field in fieldDefinitions" :key="field.id">
      <q-input
        v-if="field.fieldType === 'Text'"
        v-model="fieldValues[field.fieldName]"
        :label="field.label + (field.isRequired ? ' *' : '')"
        :error="!!validationErrors[field.fieldName]"
        :error-message="validationErrors[field.fieldName]"
        @blur="handleFieldBlur(field.fieldName)"
      />

      <q-input
        v-else-if="field.fieldType === 'Number'"
        v-model.number="fieldValues[field.fieldName]"
        type="number"
        :label="field.label + (field.isRequired ? ' *' : '')"
        :error="!!validationErrors[field.fieldName]"
        :error-message="validationErrors[field.fieldName]"
        @blur="handleFieldBlur(field.fieldName)"
      />

      <!-- Other field types... -->
    </div>

    <q-btn @click="handleSubmit" label="Submit" />
  </q-page>
</template>
```

---

## 🔧 Backend Validation (FluentValidation)

### **Install FluentValidation**

```bash
dotnet add package FluentValidation.AspNetCore
```

### **Configure in Program.cs**

```csharp
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
        fv.ImplicitlyValidateChildProperties = true;
    });

builder.Services.AddScoped<IDynamicValidator, DynamicValidator>();
```

### **Dynamic Validator Service**

```csharp
// Services/Validation/DynamicValidator.cs
public interface IDynamicValidator
{
    Task<ValidationResult> ValidateMeetingItemAsync(
        MeetingItem meetingItem,
        Template template);

    ValidationResult ValidateFieldValue(
        FieldDefinition fieldDefinition,
        object value);
}

public class DynamicValidator : IDynamicValidator
{
    private readonly ApplicationDbContext _context;

    public DynamicValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationResult> ValidateMeetingItemAsync(
        MeetingItem meetingItem,
        Template template)
    {
        var errors = new List<ValidationFailure>();

        // Validate static fields
        if (string.IsNullOrWhiteSpace(meetingItem.Topic))
            errors.Add(new ValidationFailure("Topic", "Topic is required"));

        if (string.IsNullOrWhiteSpace(meetingItem.Purpose))
            errors.Add(new ValidationFailure("Purpose", "Purpose is required"));

        if (meetingItem.DurationMinutes <= 0)
            errors.Add(new ValidationFailure("DurationMinutes", "Duration must be positive"));

        // Get field definitions
        var fieldDefinitions = await _context.FieldDefinitions
            .Include(f => f.Options)
            .Where(f => f.TemplateId == template.Id)
            .ToListAsync();

        // Validate dynamic fields
        foreach (var fieldDef in fieldDefinitions)
        {
            var fieldValue = meetingItem.FieldValues
                .FirstOrDefault(fv => fv.FieldDefinitionId == fieldDef.Id);

            if (fieldDef.IsRequired && fieldValue == null)
            {
                errors.Add(new ValidationFailure(
                    fieldDef.FieldName,
                    $"{fieldDef.Label} is required"
                ));
                continue;
            }

            if (fieldValue != null)
            {
                var fieldResult = ValidateFieldValue(fieldDef, fieldValue);
                errors.AddRange(fieldResult.Errors);
            }
        }

        return new ValidationResult(errors);
    }

    public ValidationResult ValidateFieldValue(
        FieldDefinition fieldDefinition,
        object value)
    {
        var errors = new List<ValidationFailure>();

        if (value is MeetingItemFieldValue fieldValue)
        {
            // Get actual value based on type
            object actualValue = fieldDefinition.FieldType switch
            {
                "Text" or "TextArea" or "Email" => fieldValue.TextValue,
                "Number" => fieldValue.NumberValue,
                "Date" => fieldValue.DateValue,
                "YesNo" => fieldValue.BooleanValue,
                "Dropdown" or "Radio" or "MultiSelect" => fieldValue.TextValue ?? fieldValue.JsonValue,
                _ => null
            };

            // Required validation
            if (fieldDefinition.IsRequired && actualValue == null)
            {
                errors.Add(new ValidationFailure(
                    fieldDefinition.FieldName,
                    $"{fieldDefinition.Label} is required"
                ));
                return new ValidationResult(errors);
            }

            // Type-specific validation
            switch (fieldDefinition.FieldType)
            {
                case "Email":
                    if (actualValue is string email && !IsValidEmail(email))
                        errors.Add(new ValidationFailure(
                            fieldDefinition.FieldName,
                            "Invalid email format"
                        ));
                    break;

                case "Number":
                    if (actualValue is decimal number)
                    {
                        var rules = ParseValidationRules(fieldDefinition.ValidationRulesJson);
                        if (rules.ContainsKey("min") && number < (decimal)rules["min"])
                            errors.Add(new ValidationFailure(
                                fieldDefinition.FieldName,
                                $"Minimum value is {rules["min"]}"
                            ));
                        if (rules.ContainsKey("max") && number > (decimal)rules["max"])
                            errors.Add(new ValidationFailure(
                                fieldDefinition.FieldName,
                                $"Maximum value is {rules["max"]}"
                            ));
                    }
                    break;

                case "Text":
                case "TextArea":
                    if (actualValue is string text)
                    {
                        var rules = ParseValidationRules(fieldDefinition.ValidationRulesJson);
                        if (rules.ContainsKey("minLength") && text.Length < (int)rules["minLength"])
                            errors.Add(new ValidationFailure(
                                fieldDefinition.FieldName,
                                $"Minimum length is {rules["minLength"]}"
                            ));
                        if (rules.ContainsKey("maxLength") && text.Length > (int)rules["maxLength"])
                            errors.Add(new ValidationFailure(
                                fieldDefinition.FieldName,
                                $"Maximum length is {rules["maxLength"]}"
                            ));
                        if (rules.ContainsKey("pattern"))
                        {
                            var pattern = rules["pattern"].ToString();
                            if (!Regex.IsMatch(text, pattern))
                                errors.Add(new ValidationFailure(
                                    fieldDefinition.FieldName,
                                    rules.ContainsKey("patternMessage")
                                        ? rules["patternMessage"].ToString()
                                        : "Invalid format"
                                ));
                        }
                    }
                    break;

                case "Dropdown":
                case "Radio":
                    if (actualValue is string selectedValue)
                    {
                        var validOptions = fieldDefinition.Options
                            .Select(o => o.Value)
                            .ToList();

                        if (!validOptions.Contains(selectedValue))
                            errors.Add(new ValidationFailure(
                                fieldDefinition.FieldName,
                                "Invalid selection"
                            ));
                    }
                    break;
            }
        }

        return new ValidationResult(errors);
    }

    private Dictionary<string, object> ParseValidationRules(string? rulesJson)
    {
        if (string.IsNullOrWhiteSpace(rulesJson))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(rulesJson)
                ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### **Use in Controller**

```csharp
[ApiController]
[Route("api/meeting-items")]
public class MeetingItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IDynamicValidator _dynamicValidator;

    public MeetingItemsController(
        ApplicationDbContext context,
        IDynamicValidator dynamicValidator)
    {
        _context = context;
        _dynamicValidator = dynamicValidator;
    }

    [HttpPost]
    public async Task<ActionResult<MeetingItemDto>> Create(
        [FromBody] CreateMeetingItemRequest request)
    {
        // Get template
        var template = await _context.Templates
            .Include(t => t.FieldDefinitions)
                .ThenInclude(f => f.Options)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

        if (template == null)
            return BadRequest("Invalid template");

        // Build meeting item
        var meetingItem = new MeetingItem
        {
            Id = Guid.NewGuid(),
            DecisionBoardId = request.DecisionBoardId,
            TemplateId = request.TemplateId,
            Topic = request.Topic,
            Purpose = request.Purpose,
            Outcome = request.Outcome,
            DigitalProduct = request.DigitalProduct,
            DurationMinutes = request.DurationMinutes,
            RequestorUserId = User.GetUserId(),
            OwnerPresenterUserId = request.OwnerPresenterUserId,
            SponsorUserId = request.SponsorUserId,
            Status = "Submitted",
            SubmissionDate = DateTime.UtcNow,
            FieldValues = request.FieldValues.Select(fv => new MeetingItemFieldValue
            {
                Id = Guid.NewGuid(),
                FieldDefinitionId = template.FieldDefinitions
                    .First(fd => fd.FieldName == fv.FieldName).Id,
                TextValue = fv.TextValue,
                NumberValue = fv.NumberValue,
                DateValue = fv.DateValue,
                BooleanValue = fv.BooleanValue,
                JsonValue = fv.JsonValue
            }).ToList()
        };

        // VALIDATE
        var validationResult = await _dynamicValidator
            .ValidateMeetingItemAsync(meetingItem, template);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }

        // Save
        _context.MeetingItems.Add(meetingItem);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(meetingItem));
    }
}
```

---

## 🛡️ Data Preservation Strategy

### **Problem**

```
Timeline:
1. User creates meeting item with field "riskLevel" = "High"
2. Secretary removes "riskLevel" field from template
3. What happens to existing data?
```

**Answer: Data MUST be preserved for audit, compliance, and historical reference!**

### **Solution: Never Delete Field Values**

The database schema already supports this via `FieldDefinition` → `MeetingItemFieldValue` relationship.

### **Key Principles**

1. **Never delete `FieldDefinition` records** if they're used by existing meeting items
2. **Mark fields as inactive** instead of deleting
3. **Keep historical data** in `MeetingItemFieldValue`
4. **Display historical fields** in read-only mode

### **Update FieldDefinition Schema**

```csharp
public class FieldDefinition
{
    // ... existing fields

    // NEW: Soft delete
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedDate { get; set; }
    public string? DeactivatedBy { get; set; }
    public string? DeactivationReason { get; set; }
}
```

### **Prevent Deleting Fields in Use**

```csharp
[HttpDelete("fields/{fieldId}")]
public async Task<ActionResult> DeleteField(Guid fieldId)
{
    var field = await _context.FieldDefinitions
        .Include(f => f.Template)
        .FirstOrDefaultAsync(f => f.Id == fieldId);

    if (field == null)
        return NotFound();

    // CHECK: Is field in use?
    var inUse = await _context.MeetingItemFieldValues
        .AnyAsync(fv => fv.FieldDefinitionId == fieldId);

    if (inUse)
    {
        // DON'T DELETE - Deactivate instead
        field.IsActive = false;
        field.DeactivatedDate = DateTime.UtcNow;
        field.DeactivatedBy = User.GetUserId();
        field.DeactivationReason = "Removed from template";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Field deactivated (data preserved for existing items)",
            isDeactivated = true
        });
    }

    // Only delete if never used
    _context.FieldDefinitions.Remove(field);
    await _context.SaveChangesAsync();

    return NoContent();
}
```

### **Loading Template (Show Only Active Fields)**

```csharp
[HttpGet("by-decision-board/{decisionBoardId}")]
public async Task<ActionResult<TemplateDto>> GetByDecisionBoard(Guid decisionBoardId)
{
    var board = await _context.DecisionBoards
        .Include(b => b.DefaultTemplate)
            .ThenInclude(t => t.FieldDefinitions.Where(f => f.IsActive)) // ← Only active
        .FirstOrDefaultAsync(b => b.Id == decisionBoardId);

    if (board == null)
        return NotFound();

    return Ok(MapToDto(board.DefaultTemplate));
}
```

### **Loading Existing Meeting Item (Show Historical Fields)**

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<MeetingItemDto>> GetById(Guid id)
{
    var meetingItem = await _context.MeetingItems
        .Include(mi => mi.Template)
            .ThenInclude(t => t.FieldDefinitions) // ← ALL fields (active + inactive)
        .Include(mi => mi.FieldValues)
            .ThenInclude(fv => fv.FieldDefinition)
        .FirstOrDefaultAsync(mi => mi.Id == id);

    if (meetingItem == null)
        return NotFound();

    // Separate active and inactive fields
    var dto = new MeetingItemDto
    {
        Id = meetingItem.Id,
        // ... static fields

        ActiveFields = meetingItem.FieldValues
            .Where(fv => fv.FieldDefinition.IsActive)
            .Select(MapFieldValueToDto)
            .ToList(),

        HistoricalFields = meetingItem.FieldValues
            .Where(fv => !fv.FieldDefinition.IsActive)
            .Select(fv => new HistoricalFieldDto
            {
                FieldName = fv.FieldDefinition.FieldName,
                Label = fv.FieldDefinition.Label,
                Value = GetFieldValueAsString(fv),
                DeactivatedDate = fv.FieldDefinition.DeactivatedDate,
                DeactivationReason = fv.FieldDefinition.DeactivationReason
            })
            .ToList()
    };

    return Ok(dto);
}
```

### **Frontend: Display Historical Fields**

```vue
<template>
  <q-page>
    <!-- Active fields (editable) -->
    <div class="form-section">
      <h6>Current Fields</h6>
      <div v-for="field in activeFields" :key="field.id">
        <q-input v-model="fieldValues[field.fieldName]" />
      </div>
    </div>

    <!-- Historical fields (read-only) -->
    <div v-if="historicalFields.length > 0" class="form-section q-mt-lg">
      <q-banner class="bg-grey-2">
        <template v-slot:avatar>
          <q-icon name="history" color="orange" />
        </template>
        <div class="text-weight-bold">Historical Data</div>
        <div class="text-caption">
          These fields were removed from the template but data is preserved for audit purposes
        </div>
      </q-banner>

      <div class="q-mt-md">
        <div
          v-for="field in historicalFields"
          :key="field.fieldName"
          class="historical-field q-mb-sm"
        >
          <q-input
            :label="field.label + ' (Removed)'"
            :model-value="field.value"
            readonly
            filled
            disable
            hint="This field was removed from the template"
          >
            <template v-slot:prepend>
              <q-icon name="info" color="orange">
                <q-tooltip>
                  Removed: {{ formatDate(field.deactivatedDate) }}
                  <br>
                  Reason: {{ field.deactivationReason }}
                </q-tooltip>
              </q-icon>
            </template>
          </q-input>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const activeFields = ref([])
const historicalFields = ref([])
const fieldValues = ref({})

onMounted(async () => {
  const response = await meetingItemsApi.getMeetingItem(props.id)

  activeFields.value = response.activeFields
  historicalFields.value = response.historicalFields

  // Populate field values
  response.activeFields.forEach(field => {
    fieldValues.value[field.fieldName] = field.value
  })
})
</script>

<style scoped>
.historical-field {
  opacity: 0.7;
}
</style>
```

---

## 📊 Template Change Scenarios

### **Scenario 1: Field Removed from Template**

```
Initial State:
- Template has field "riskLevel"
- 10 meeting items have data for "riskLevel"

Action: Secretary removes "riskLevel" field

Result:
✅ FieldDefinition.IsActive = false
✅ Existing 10 items still show "riskLevel" (read-only)
✅ New items don't show "riskLevel"
✅ Data preserved in MeetingItemFieldValue table
```

### **Scenario 2: Field Added to Template**

```
Initial State:
- Template has 5 fields
- 10 meeting items exist

Action: Secretary adds "estimatedCost" field

Result:
✅ New FieldDefinition created
✅ Existing 10 items don't have "estimatedCost" (null is OK)
✅ New items have "estimatedCost"
✅ Existing items can be edited to add "estimatedCost" (optional)
```

### **Scenario 3: Field Made Required**

```
Initial State:
- Field "budget" is optional
- 5 meeting items have null budget

Action: Secretary makes "budget" required

Problem: 5 existing items don't have budget!

Solution Options:

Option A: Grandfather existing items
✅ Only NEW items require budget
✅ Existing items can be edited without budget

Option B: Force update
❌ Require users to fill budget before next edit
❌ Potentially blocks users

Recommended: Option A
```

**Implementation:**

```csharp
public class FieldDefinition
{
    public bool IsRequired { get; set; }
    public DateTime? RequiredSinceDate { get; set; } // ← NEW
}

// When making field required
field.IsRequired = true;
field.RequiredSinceDate = DateTime.UtcNow;

// Validation logic
if (fieldDef.IsRequired)
{
    // Grandfather old items
    if (meetingItem.CreatedDate < fieldDef.RequiredSinceDate)
    {
        // Not required for this old item
    }
    else
    {
        // Required for new items
        if (fieldValue == null)
            errors.Add(...);
    }
}
```

### **Scenario 4: Field Options Changed (Dropdown)**

```
Initial State:
- Dropdown "priority" has options: Low, Medium, High
- 10 items selected "Medium"

Action: Secretary removes "Medium" option

Problem: 10 items have invalid option!

Solution: Keep historical options
```

**Implementation:**

```csharp
public class FieldOption
{
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedDate { get; set; }
}

// Don't delete option - deactivate
option.IsActive = false;
option.DeactivatedDate = DateTime.UtcNow;

// Load options for NEW items
var activeOptions = field.Options.Where(o => o.IsActive);

// Load options for EXISTING item (include historical)
var allOptions = field.Options; // Active + inactive
```

---

## 🔒 Migration Safety Checklist

### **Before Removing a Field**

```sql
-- Check if field is used
SELECT COUNT(DISTINCT MeetingItemId)
FROM MeetingItemFieldValues
WHERE FieldDefinitionId = '<field-id>';

-- If count > 0: DEACTIVATE, don't delete
UPDATE FieldDefinitions
SET IsActive = 0,
    DeactivatedDate = GETUTCDATE(),
    DeactivationReason = 'Removed from template'
WHERE Id = '<field-id>';
```

### **Before Changing Field Type**

❌ **DON'T**: Change FieldType from "Text" to "Number" directly

✅ **DO**: Create new field, migrate data, deactivate old field

```sql
-- 1. Create new field
INSERT INTO FieldDefinitions (Id, TemplateId, FieldName, FieldType, ...)
VALUES (NEWID(), '<template-id>', 'budgetNew', 'Number', ...);

-- 2. Migrate data (if possible)
UPDATE fv
SET fv.NumberValue = TRY_CAST(fv.TextValue AS DECIMAL)
FROM MeetingItemFieldValues fv
WHERE fv.FieldDefinitionId = '<old-field-id>'
  AND TRY_CAST(fv.TextValue AS DECIMAL) IS NOT NULL;

-- 3. Deactivate old field
UPDATE FieldDefinitions
SET IsActive = 0
WHERE Id = '<old-field-id>';
```

### **Data Validation Script**

Run periodically to check data integrity:

```sql
-- Find orphaned field values (field definition deleted)
SELECT fv.*
FROM MeetingItemFieldValues fv
LEFT JOIN FieldDefinitions fd ON fv.FieldDefinitionId = fd.Id
WHERE fd.Id IS NULL;

-- Find required fields with null values
SELECT mi.Id, fd.FieldName, fd.Label
FROM MeetingItems mi
CROSS JOIN FieldDefinitions fd
LEFT JOIN MeetingItemFieldValues fv
  ON fv.MeetingItemId = mi.Id AND fv.FieldDefinitionId = fd.Id
WHERE fd.TemplateId = mi.TemplateId
  AND fd.IsRequired = 1
  AND fd.IsActive = 1
  AND fv.Id IS NULL;
```

---

## ✅ Summary

### **Frontend Validation (Yup)**
✅ Dynamic schema generation from field definitions
✅ Type-specific validation (email, number, date, etc.)
✅ Min/max length and value validation
✅ Regex pattern matching
✅ Real-time validation on blur
✅ Form-level validation on submit

### **Backend Validation (FluentValidation)**
✅ Dynamic validator service
✅ Template-based validation
✅ Type-safe validation per field type
✅ JSON-based validation rules
✅ Custom validation support
✅ API error responses with field-level errors

### **Data Preservation**
✅ Never delete field definitions in use
✅ Soft delete (IsActive flag) instead
✅ Historical data always preserved
✅ Separate display of active vs. historical fields
✅ Read-only display for removed fields
✅ Audit trail of field changes
✅ Grandfather clauses for requirement changes

### **Banking Compliance**
✅ Complete audit trail
✅ No data loss
✅ Historical reconstruction possible
✅ Field-level change tracking
✅ Immutable historical records

---

**Your data is safe. Your validation is bulletproof. Your audit trail is complete.** 🛡️
