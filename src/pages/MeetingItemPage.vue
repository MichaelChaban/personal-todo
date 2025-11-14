<template>
  <q-page class="q-pa-md">
    <div class="meeting-item-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">{{ isEditMode ? 'Edit Meeting Item' : 'Create Meeting Item' }}</h4>
          <p class="text-grey-7 q-ma-none">{{ isEditMode ? 'Update meeting item details' : 'Complete all required fields to submit your request' }}</p>
        </div>
        <q-chip v-if="meetingItem.status" :color="statusColor" text-color="white" class="status-chip">
          {{ meetingItem.status }}
        </q-chip>
      </div>

      <!-- Tabs -->
      <q-card flat bordered class="meeting-card">
        <q-tabs
          v-model="activeTab"
          dense
          class="text-grey-7"
          active-color="primary"
          indicator-color="primary"
          align="left"
          narrow-indicator
        >
          <q-tab name="general" label="General" icon="info" />
          <q-tab name="details" label="Details" icon="description" />
          <q-tab name="additional" label="Additional Details" icon="dashboard_customize" :badge="template?.fieldDefinitions?.length || undefined" />
          <q-tab name="documents" label="Documents" icon="attachment" :badge="documentCount || undefined" />
        </q-tabs>

        <q-separator />

        <q-tab-panels v-model="activeTab" animated>
          <!-- General Tab -->
          <q-tab-panel name="general" class="q-pa-lg">
            <GeneralInfoTab
              :meeting-item="meetingItem"
              @update:meeting-item="handleMeetingItemUpdate"
            />
          </q-tab-panel>

          <!-- Details Tab -->
          <q-tab-panel name="details" class="q-pa-lg">
            <DetailsTab
              :meeting-item="meetingItem"
              :can-change-status="canChangeStatus"
              @update:meeting-item="handleMeetingItemUpdate"
            />
          </q-tab-panel>

          <!-- Additional Details Tab (Dynamic Template Fields) -->
          <q-tab-panel name="additional" class="q-pa-lg">
            <AdditionalDetailsTab
              :field-values="meetingItem.fieldValues"
              :template="template"
              :historical-fields="historicalFields"
              :loading="loading"
              :decision-board-name="decisionBoardName"
              @update:field-values="handleFieldValuesUpdate"
            />
          </q-tab-panel>

          <!-- Documents Tab -->
          <q-tab-panel name="documents" class="q-pa-lg">
            <DocumentUpload
              :meeting-item-id="meetingItem.id"
              :decision-board-abbr="decisionBoardAbbr"
              :topic="meetingItem.topic"
              @documents-updated="handleDocumentsUpdate"
            />
          </q-tab-panel>
        </q-tab-panels>
      </q-card>

      <!-- Action Buttons -->
      <div class="actions-bar q-mt-lg">
        <q-btn
          flat
          label="Cancel"
          color="grey-7"
          @click="handleCancel"
        />
        <div class="q-gutter-sm">
          <q-btn
            outline
            label="Save Draft"
            color="primary"
            @click="handleSaveDraft"
            :loading="saving"
          />
          <q-btn
            unelevated
            label="Submit"
            color="primary"
            @click="handleSubmit"
            :loading="submitting"
            :disable="!isFormValid"
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
import DocumentUpload from '../components/DocumentUpload.vue'
import GeneralInfoTab from '../components/meetingItem/GeneralInfoTab.vue'
import DetailsTab from '../components/meetingItem/DetailsTab.vue'
import AdditionalDetailsTab from '../components/meetingItem/AdditionalDetailsTab.vue'
import { meetingItemsApi } from '../api/meetingItems'

const router = useRouter()
const $q = useQuasar()

// Props (if editing existing item)
const props = defineProps({
  id: String
})

// State
const activeTab = ref('general')
const saving = ref(false)
const submitting = ref(false)
const documentCount = ref(0)
const loading = ref(false)

const meetingItem = ref({
  id: null,
  decisionBoardId: 'board-123', // TODO: Get from context/route
  templateId: 'template-456', // TODO: Get from decision board
  topic: '',
  purpose: '',
  outcome: null,
  digitalProduct: '',
  duration: null,
  requestor: '',
  ownerPresenter: '',
  sponsor: '',
  status: 'Submitted',
  submissionDate: null,
  fieldValues: {}, // Dynamic fields from template
  documents: []
})

const template = ref(null)
const activeFields = ref([])
const historicalFields = ref([])

// Options
const outcomeOptions = ['Decision', 'Discussion', 'Information']
const statusOptions = ['Submitted', 'Proposed', 'Planned', 'Discussed', 'Denied']
const decisionBoardAbbr = ref('DB')
const decisionBoardName = ref('')

// Computed
const isEditMode = computed(() => !!props.id)

const canChangeStatus = computed(() => {
  // Only secretary/admin can change status
  // This should check user permissions from auth context
  return false
})

const statusColor = computed(() => {
  const colors = {
    'Submitted': 'orange',
    'Proposed': 'blue',
    'Planned': 'purple',
    'Discussed': 'green',
    'Denied': 'red'
  }
  return colors[meetingItem.value.status] || 'grey'
})

const isFormValid = computed(() => {
  return !!(
    meetingItem.value.topic &&
    meetingItem.value.purpose &&
    meetingItem.value.outcome &&
    meetingItem.value.digitalProduct &&
    meetingItem.value.duration > 0 &&
    meetingItem.value.requestor &&
    meetingItem.value.ownerPresenter
  )
})

// Lifecycle
onMounted(async () => {
  // Initialize requestor from current user
  meetingItem.value.requestor = 'USER123' // TODO: Get from auth context
  meetingItem.value.ownerPresenter = meetingItem.value.requestor

  // Load template for decision board
  await loadTemplate()

  if (isEditMode.value) {
    await loadMeetingItem(props.id)
  }
})

// Methods
const loadTemplate = async () => {
  try {
    loading.value = true

    // TODO: Replace with real API call
    // const response = await meetingItemsApi.getTemplateByDecisionBoard(meetingItem.value.decisionBoardId)

    // Mock template data for demonstration
    const response = {
      templateId: 'template-456',
      templateName: 'IT Governance Template',
      description: 'Standard template for IT Governance Board',
      fieldDefinitions: [
        {
          id: 'field-1',
          fieldName: 'projectDescription',
          label: 'Project Description',
          fieldType: 'Text',
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
          isRequired: false,
          isActive: true,
          isTextArea: false,
          placeholderText: '0',
          helpText: 'Number of people in the project team',
          options: null
        }
      ]
    }

    template.value = response
    meetingItem.value.templateId = response.templateId

    // Initialize field values from template
    response.fieldDefinitions.forEach(field => {
      if (field.isRequired || field.options?.some(opt => opt.isDefault)) {
        const defaultOption = field.options?.find(opt => opt.isDefault)
        meetingItem.value.fieldValues[field.fieldName] = defaultOption?.value || null
      }
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

const loadMeetingItem = async (id) => {
  try {
    loading.value = true
    const response = await meetingItemsApi.getMeetingItem(id)

    // Map response to form structure
    meetingItem.value.id = response.id
    meetingItem.value.decisionBoardId = response.decisionBoardId
    meetingItem.value.templateId = response.templateId
    meetingItem.value.topic = response.topic
    meetingItem.value.purpose = response.purpose
    meetingItem.value.outcome = response.outcome
    meetingItem.value.digitalProduct = response.digitalProduct
    meetingItem.value.duration = response.durationMinutes
    meetingItem.value.requestor = response.requestorUserId
    meetingItem.value.ownerPresenter = response.ownerPresenterUserId
    meetingItem.value.sponsor = response.sponsorUserId
    meetingItem.value.status = response.status
    meetingItem.value.submissionDate = response.submissionDate

    decisionBoardName.value = response.decisionBoardName

    // Map active fields
    activeFields.value = response.activeFields || []
    response.activeFields?.forEach(field => {
      meetingItem.value.fieldValues[field.fieldName] = getFieldValue(field)
    })

    // Store historical fields for display
    historicalFields.value = response.historicalFields || []

    // Documents
    meetingItem.value.documents = response.documents || []
    documentCount.value = meetingItem.value.documents.length
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load meeting item',
      caption: error.message
    })
  } finally {
    loading.value = false
  }
}

const getFieldValue = (field) => {
  // Extract value based on field type
  if (field.textValue !== null) return field.textValue
  if (field.numberValue !== null) return field.numberValue
  if (field.dateValue !== null) return field.dateValue
  if (field.booleanValue !== null) return field.booleanValue
  if (field.jsonValue !== null) return JSON.parse(field.jsonValue)
  return null
}

const mapFieldValuesToDto = () => {
  // Convert field values object to FieldValueDto array
  const fieldValues = []

  Object.entries(meetingItem.value.fieldValues).forEach(([fieldName, value]) => {
    if (value === null || value === undefined) return

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
      case 'Dropdown':
        dto.textValue = value
        break
      case 'Number':
        dto.numberValue = typeof value === 'number' ? value : parseFloat(value)
        break
      case 'Date':
        dto.dateValue = value instanceof Date ? value.toISOString() : value
        break
      case 'YesNo':
        dto.booleanValue = Boolean(value)
        break
      case 'MultipleChoice':
        dto.jsonValue = JSON.stringify(Array.isArray(value) ? value : [value])
        break
      default:
        dto.textValue = String(value)
    }

    fieldValues.push(dto)
  })

  return fieldValues
}

const handleSaveDraft = async () => {
  saving.value = true
  try {
    const payload = {
      decisionBoardId: meetingItem.value.decisionBoardId,
      templateId: meetingItem.value.templateId,
      topic: meetingItem.value.topic,
      purpose: meetingItem.value.purpose,
      outcome: meetingItem.value.outcome,
      digitalProduct: meetingItem.value.digitalProduct,
      durationMinutes: meetingItem.value.duration,
      ownerPresenterUserId: meetingItem.value.ownerPresenter,
      sponsorUserId: meetingItem.value.sponsor || null,
      fieldValues: mapFieldValuesToDto(),
      documents: []
    }

    if (isEditMode.value) {
      await meetingItemsApi.updateMeetingItem(meetingItem.value.id, payload)
    } else {
      const response = await meetingItemsApi.createMeetingItem(payload)
      meetingItem.value.id = response.meetingItemId
    }

    $q.notify({
      type: 'positive',
      message: 'Draft saved successfully'
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

const handleSubmit = async () => {
  if (!isFormValid.value) {
    $q.notify({
      type: 'warning',
      message: 'Please complete all required fields'
    })
    return
  }

  submitting.value = true
  try {
    const payload = {
      decisionBoardId: meetingItem.value.decisionBoardId,
      templateId: meetingItem.value.templateId,
      topic: meetingItem.value.topic,
      purpose: meetingItem.value.purpose,
      outcome: meetingItem.value.outcome,
      digitalProduct: meetingItem.value.digitalProduct,
      durationMinutes: meetingItem.value.duration,
      ownerPresenterUserId: meetingItem.value.ownerPresenter,
      sponsorUserId: meetingItem.value.sponsor || null,
      fieldValues: mapFieldValuesToDto(),
      documents: meetingItem.value.documents.map(doc => ({
        fileName: doc.fileName,
        contentType: doc.contentType,
        base64Content: doc.base64Content
      }))
    }

    if (isEditMode.value) {
      await meetingItemsApi.updateMeetingItem(meetingItem.value.id, payload)
      $q.notify({
        type: 'positive',
        message: 'Meeting item updated successfully'
      })
    } else {
      await meetingItemsApi.createMeetingItem(payload)
      $q.notify({
        type: 'positive',
        message: 'Meeting item submitted successfully'
      })
    }

    router.push('/meeting-items')
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to submit meeting item',
      caption: error.message
    })
  } finally {
    submitting.value = false
  }
}

const handleCancel = () => {
  router.push('/meeting-items')
}

const handleDocumentsUpdate = (documents) => {
  meetingItem.value.documents = documents
  documentCount.value = documents.length
}

const handleMeetingItemUpdate = (updatedItem) => {
  meetingItem.value = { ...meetingItem.value, ...updatedItem }
}

const handleFieldValuesUpdate = (updatedFieldValues) => {
  meetingItem.value.fieldValues = { ...updatedFieldValues }
}
</script>

<style scoped lang="scss">
.meeting-item-container {
  max-width: 1200px;
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

.status-chip {
  font-weight: 600;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.meeting-card {
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.actions-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
}

:deep(.q-tab) {
  padding: 1rem 1.5rem;
  font-weight: 500;
  text-transform: none;
  font-size: 0.9rem;
}
</style>
