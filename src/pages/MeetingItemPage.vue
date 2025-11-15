<template>
  <q-page class="q-pa-md">
    <div class="meeting-item-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">{{ isEditMode ? $t('Edit Meeting Item') : $t('Create Meeting Item') }}</h4>
          <p class="text-grey-7 q-ma-none">{{ isEditMode ? $t('Update meeting item details') : $t('Complete all required fields to submit your request') }}</p>
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
          <q-tab name="general" :label="$t('General')" icon="info" />
          <q-tab name="details" :label="$t('Details')" icon="description" />
          <q-tab name="documents" :label="$t('Documents')" icon="attachment" :badge="documentCount || undefined" />
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
          :label="$t('Cancel')"
          color="grey-7"
          @click="handleCancel"
        />
        <div class="q-gutter-sm">
          <q-btn
            outline
            :label="$t('Save Draft')"
            color="primary"
            @click="handleSaveDraft"
            :loading="saving"
          />
          <q-btn
            unelevated
            :label="$t('Submit')"
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

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import DocumentUpload from '../components/DocumentUpload.vue'
import GeneralInfoTab from '../components/meetingItem/GeneralInfoTab.vue'
import DetailsTab from '../components/meetingItem/DetailsTab.vue'
import { meetingItemsApi } from '../api/meetingItems'

// Placeholder $t function
const $t = (key: string): string => key

const router = useRouter()
const $q = useQuasar()

// Props (if editing existing item)
const props = defineProps<{
  id?: string
}>()

// State
const activeTab = ref<string>('general')
const saving = ref<boolean>(false)
const submitting = ref<boolean>(false)
const documentCount = ref<number>(0)
const loading = ref<boolean>(false)

const meetingItem = ref({
  id: null,
  decisionBoardId: 'board-123', // TODO: Get from context/route
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
  documents: []
})

const decisionBoardAbbr = ref<string>('DB')

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
onMounted(async (): Promise<void> => {
  // Initialize requestor from current user
  meetingItem.value.requestor = 'USER123' // TODO: Get from auth context
  meetingItem.value.ownerPresenter = meetingItem.value.requestor

  if (isEditMode.value) {
    await loadMeetingItem(props.id)
  }
})

// Methods
const loadMeetingItem = async (id?: string): Promise<void> => {
  if (!id) return

  try {
    loading.value = true
    const response = await meetingItemsApi.getMeetingItem(id)

    // Map response to form structure
    meetingItem.value.id = response.id
    meetingItem.value.decisionBoardId = response.decisionBoardId
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

    // Documents
    meetingItem.value.documents = response.documents || []
    documentCount.value = meetingItem.value.documents.length
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to load meeting item'),
      caption: error.message
    })
  } finally {
    loading.value = false
  }
}

const handleSaveDraft = async (): Promise<void> => {
  saving.value = true
  try {
    const payload = {
      decisionBoardId: meetingItem.value.decisionBoardId,
      topic: meetingItem.value.topic,
      purpose: meetingItem.value.purpose,
      outcome: meetingItem.value.outcome,
      digitalProduct: meetingItem.value.digitalProduct,
      durationMinutes: meetingItem.value.duration,
      ownerPresenterUserId: meetingItem.value.ownerPresenter,
      sponsorUserId: meetingItem.value.sponsor || null,
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
      message: $t('Draft saved successfully')
    })
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to save draft'),
      caption: error.message
    })
  } finally {
    saving.value = false
  }
}

const handleSubmit = async (): Promise<void> => {
  if (!isFormValid.value) {
    $q.notify({
      type: 'warning',
      message: $t('Please complete all required fields')
    })
    return
  }

  submitting.value = true
  try {
    const payload = {
      decisionBoardId: meetingItem.value.decisionBoardId,
      topic: meetingItem.value.topic,
      purpose: meetingItem.value.purpose,
      outcome: meetingItem.value.outcome,
      digitalProduct: meetingItem.value.digitalProduct,
      durationMinutes: meetingItem.value.duration,
      ownerPresenterUserId: meetingItem.value.ownerPresenter,
      sponsorUserId: meetingItem.value.sponsor || null,
      documents: meetingItem.value.documents.map((doc: any) => ({
        fileName: doc.fileName,
        contentType: doc.contentType,
        base64Content: doc.base64Content
      }))
    }

    if (isEditMode.value) {
      await meetingItemsApi.updateMeetingItem(meetingItem.value.id, payload)
      $q.notify({
        type: 'positive',
        message: $t('Meeting item updated successfully')
      })
    } else {
      await meetingItemsApi.createMeetingItem(payload)
      $q.notify({
        type: 'positive',
        message: $t('Meeting item submitted successfully')
      })
    }

    router.push('/meeting-items')
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to submit meeting item'),
      caption: error.message
    })
  } finally {
    submitting.value = false
  }
}

const handleCancel = (): void => {
  router.push('/meeting-items')
}

const handleDocumentsUpdate = (documents: any): void => {
  meetingItem.value.documents = documents
  documentCount.value = documents.length
}

const handleMeetingItemUpdate = (updatedItem: any): void => {
  meetingItem.value = { ...meetingItem.value, ...updatedItem }
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
