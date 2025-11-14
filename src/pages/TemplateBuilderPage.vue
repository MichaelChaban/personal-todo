<template>
  <q-page class="q-pa-md">
    <div class="template-builder-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">
            {{ isEditMode ? 'Edit Template' : 'Create New Template' }}
          </h4>
          <p class="text-grey-7 q-ma-none">
            Design your custom meeting item template by adding and configuring fields
          </p>
        </div>
        <div class="q-gutter-sm">
          <q-btn
            flat
            label="Cancel"
            color="grey-7"
            @click="handleCancel"
          />
          <q-btn
            outline
            label="Save Draft"
            color="primary"
            icon="save"
            @click="handleSaveDraft"
            :loading="saving"
          />
          <q-btn
            unelevated
            label="Publish Template"
            color="primary"
            icon="publish"
            @click="handlePublish"
            :loading="publishing"
            :disable="!isTemplateValid"
          />
        </div>
      </div>

      <div class="builder-layout row q-col-gutter-lg">
        <!-- Left Panel: Template Settings & Field Palette -->
        <div class="col-12 col-md-3">
          <TemplateSettingsPanel
            :template="template"
            :decision-boards="decisionBoards"
            :total-fields="template.fields.length"
            :required-fields="requiredFieldsCount"
            @update:template="handleTemplateUpdate"
          />

          <!-- Field Types Palette -->
          <div class="q-mt-md">
            <FieldTypesPalette
              :field-types="fieldTypes"
              :fields-count="template.fields.length"
              @clone-field="handleCloneField"
            />
          </div>
        </div>

        <!-- Center Panel: Canvas (Drag & Drop Area) -->
        <div class="col-12 col-md-6">
          <FieldCanvas
            v-model:fields="template.fields"
            :selected-field-id="selectedField?.id"
            :field-types="fieldTypes"
            :loading="loading"
            @fields-changed="handleFieldsChange"
            @select-field="selectField"
            @edit-field="editField"
            @duplicate-field="duplicateField"
            @delete-field="deleteField"
          />
        </div>

        <!-- Right Panel: Field Properties -->
        <div class="col-12 col-md-3">
          <FieldPropertiesPanel
            :selected-field="selectedField"
            :field-type-options="fieldTypeOptions"
            @update:field="updateField"
            @deselect-field="selectedField = null"
          />
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import TemplateSettingsPanel from '../components/templateBuilder/TemplateSettingsPanel.vue'
import FieldTypesPalette from '../components/templateBuilder/FieldTypesPalette.vue'
import FieldCanvas from '../components/templateBuilder/FieldCanvas.vue'
import FieldPropertiesPanel from '../components/templateBuilder/FieldPropertiesPanel.vue'

const router = useRouter()
const $q = useQuasar()

// Props
const props = defineProps({
  id: String
})

// State
const template = ref({
  id: null,
  name: '',
  description: '',
  decisionBoardId: null,
  isActive: true,
  fields: []
})

const selectedField = ref(null)
const saving = ref(false)
const publishing = ref(false)
const loading = ref(false)

// Mock data - replace with API calls
const decisionBoards = ref([
  { id: 'board-1', name: 'IT Governance' },
  { id: 'board-2', name: 'Architecture Board' },
  { id: 'board-3', name: 'Security Council' }
])

// Simplified Field Types (6 types)
const fieldTypes = ref([
  { type: 'Text', label: 'Text', icon: 'text_fields', color: 'blue-6' },
  { type: 'Number', label: 'Number', icon: 'pin', color: 'green-6' },
  { type: 'Date', label: 'Date', icon: 'event', color: 'orange-6' },
  { type: 'YesNo', label: 'Yes/No', icon: 'check_box', color: 'teal-6' },
  { type: 'Dropdown', label: 'Dropdown', icon: 'arrow_drop_down_circle', color: 'indigo-6' },
  { type: 'MultipleChoice', label: 'Multiple Choice', icon: 'checklist', color: 'pink-6' }
])

const fieldTypeOptions = fieldTypes.value.map(ft => ft.type)

// Computed
const isEditMode = computed(() => !!props.id)

const isTemplateValid = computed(() => {
  return !!(
    template.value.name &&
    template.value.decisionBoardId &&
    template.value.fields.length > 0
  )
})

const requiredFieldsCount = computed(() => {
  return template.value.fields.filter(f => f.isRequired).length
})

// Lifecycle
onMounted(async () => {
  if (isEditMode.value) {
    await loadTemplate(props.id)
  }
})

// Methods
const loadTemplate = async (templateId) => {
  try {
    loading.value = true

    // TODO: Replace with real API call
    // const response = await templatesApi.getTemplate(templateId)

    // Mock template data for demonstration
    const response = {
      id: templateId,
      name: 'IT Governance Template',
      description: 'Standard template for IT Governance Board',
      decisionBoardId: 'board-1',
      isActive: true,
      fieldDefinitions: [
        {
          id: 'field-1',
          fieldName: 'projectDescription',
          label: 'Project Description',
          fieldType: 'Text',
          displayOrder: 0,
          isRequired: true,
          isActive: true,
          isTextArea: true,
          placeholderText: 'Describe the project in detail',
          helpText: 'Provide a comprehensive overview of the project scope and objectives',
          options: null
        },
        {
          id: 'field-2',
          fieldName: 'estimatedBudget',
          label: 'Estimated Budget (EUR)',
          fieldType: 'Number',
          displayOrder: 1,
          isRequired: true,
          isActive: true,
          isTextArea: false,
          placeholderText: '0',
          helpText: 'Total estimated budget in euros',
          options: null
        },
        {
          id: 'field-3',
          fieldName: 'priority',
          label: 'Priority Level',
          fieldType: 'Dropdown',
          displayOrder: 2,
          isRequired: true,
          isActive: true,
          isTextArea: false,
          placeholderText: 'Select priority',
          helpText: 'Business priority of this initiative',
          options: [
            { value: 'low', label: 'Low', isDefault: false, isActive: true },
            { value: 'medium', label: 'Medium', isDefault: true, isActive: true },
            { value: 'high', label: 'High', isDefault: false, isActive: true },
            { value: 'critical', label: 'Critical', isDefault: false, isActive: true }
          ]
        },
        {
          id: 'field-4',
          fieldName: 'expectedGoLiveDate',
          label: 'Expected Go-Live Date',
          fieldType: 'Date',
          displayOrder: 3,
          isRequired: false,
          isActive: true,
          isTextArea: false,
          placeholderText: '',
          helpText: 'Target date for production deployment',
          options: null
        },
        {
          id: 'field-5',
          fieldName: 'requiresArchitectureReview',
          label: 'Requires Architecture Review',
          fieldType: 'YesNo',
          displayOrder: 4,
          isRequired: false,
          isActive: true,
          isTextArea: false,
          placeholderText: '',
          helpText: 'Check if this project requires architecture board review',
          options: null
        },
        {
          id: 'field-6',
          fieldName: 'affectedSystems',
          label: 'Affected Systems',
          fieldType: 'MultipleChoice',
          displayOrder: 5,
          isRequired: false,
          isActive: true,
          isTextArea: false,
          placeholderText: 'Select all that apply',
          helpText: 'Select all systems that will be impacted by this change',
          options: [
            { value: 'crm', label: 'CRM', isDefault: false, isActive: true },
            { value: 'erp', label: 'ERP', isDefault: false, isActive: true },
            { value: 'portal', label: 'Customer Portal', isDefault: false, isActive: true },
            { value: 'mobile', label: 'Mobile App', isDefault: false, isActive: true },
            { value: 'api', label: 'API Gateway', isDefault: false, isActive: true }
          ]
        },
        {
          id: 'field-7',
          fieldName: 'businessJustification',
          label: 'Business Justification',
          fieldType: 'Text',
          displayOrder: 6,
          isRequired: true,
          isActive: true,
          isTextArea: true,
          placeholderText: 'Explain the business value',
          helpText: 'Describe the business benefits and ROI',
          options: null
        },
        {
          id: 'field-8',
          fieldName: 'teamSize',
          label: 'Team Size',
          fieldType: 'Number',
          displayOrder: 7,
          isRequired: false,
          isActive: true,
          isTextArea: false,
          placeholderText: '0',
          helpText: 'Number of people in the project team',
          options: null
        }
      ]
    }

    // Map response to template structure
    template.value = {
      id: response.id,
      name: response.name,
      description: response.description,
      decisionBoardId: response.decisionBoardId,
      isActive: response.isActive,
      fields: response.fieldDefinitions || []
    }

    $q.notify({
      type: 'positive',
      message: 'Template loaded successfully',
      timeout: 1000
    })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load template',
      caption: error.message
    })
  } finally {
    loading.value = false
  }
}

// Field action methods
const selectField = (field) => {
  selectedField.value = field
}

const editField = (field) => {
  selectedField.value = field
}

const duplicateField = (field) => {
  const duplicated = {
    ...JSON.parse(JSON.stringify(field)),
    id: `field_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
    fieldName: `${field.fieldName}_copy`,
    label: `${field.label} (Copy)`,
    displayOrder: template.value.fields.length
  }
  template.value.fields.push(duplicated)

  $q.notify({
    type: 'positive',
    message: 'Field duplicated',
    timeout: 1000
  })
}

const deleteField = (field) => {
  $q.dialog({
    title: 'Confirm Delete',
    message: `Are you sure you want to delete "${field.label}"?`,
    cancel: true,
    persistent: true
  }).onOk(() => {
    template.value.fields = template.value.fields.filter(f => f.id !== field.id)
    if (selectedField.value?.id === field.id) {
      selectedField.value = null
    }

    $q.notify({
      type: 'positive',
      message: 'Field deleted',
      timeout: 1000
    })
  })
}

const handleFieldsChange = () => {
  // Update display order based on position
  template.value.fields.forEach((field, index) => {
    field.displayOrder = index
  })
}

const handleSaveDraft = async () => {
  saving.value = true
  try {
    // TODO: API call to save template
    await new Promise(resolve => setTimeout(resolve, 1000))

    $q.notify({
      type: 'positive',
      message: 'Template draft saved successfully'
    })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to save draft',
      caption: error.message
    })
  } finally {
    saving.value = false
  }
}

const handlePublish = async () => {
  if (!isTemplateValid.value) {
    $q.notify({
      type: 'warning',
      message: 'Please complete all required fields and add at least one field'
    })
    return
  }

  publishing.value = true
  try {
    // TODO: API call to publish template
    await new Promise(resolve => setTimeout(resolve, 1000))

    $q.notify({
      type: 'positive',
      message: 'Template published successfully'
    })

    router.push('/templates')
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to publish template',
      caption: error.message
    })
  } finally {
    publishing.value = false
  }
}

const handleCancel = () => {
  router.push('/templates')
}

// Handler methods for component events
const handleTemplateUpdate = (updatedTemplate) => {
  template.value = { ...template.value, ...updatedTemplate }
}

const handleCloneField = (clonedField) => {
  // This is handled by the drag-and-drop mechanism
  // The field is automatically added to template.fields
}

const updateField = (updatedField) => {
  const index = template.value.fields.findIndex(f => f.id === updatedField.id)
  if (index !== -1) {
    template.value.fields[index] = { ...updatedField }
    // Trigger reactivity
    template.value.fields = [...template.value.fields]
  }
}
</script>

<style scoped lang="scss">
.template-builder-container {
  max-width: 1800px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;

  h4 {
    font-size: 1.75rem;
    font-weight: 600;
    color: #1a1a1a;
  }

  p {
    font-size: 0.875rem;
  }
}

.builder-layout {
  margin-top: 2rem;
}
</style>
