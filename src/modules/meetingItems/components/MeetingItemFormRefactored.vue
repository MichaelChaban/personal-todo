<template>
  <div class="meeting-item-form">
    <q-card flat bordered class="meeting-card">
      <q-tabs
        v-model="activeTab"
        dense
        class="text-grey"
        active-color="primary"
        indicator-color="primary"
        align="left"
      >
        <q-tab name="general" :label="$t('General')" icon="info" />
        <q-tab name="details" :label="$t('Details')" icon="description" />
        <q-tab
          name="documents"
          :label="$t('Documents')"
          icon="attachment"
          :badge="formData.documents.length || undefined"
          badge-color="primary"
        />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="activeTab" animated>
        <!-- General Tab -->
        <q-tab-panel name="general">
          <GeneralSection
            v-model:topic="formData.topic"
            v-model:purpose="formData.purpose"
            v-model:outcome="formData.outcome"
            v-model:duration="formData.duration"
            v-model:digital-product="formData.digitalProduct"
          />
        </q-tab-panel>

        <!-- Details Tab -->
        <q-tab-panel name="details">
          <DetailsSection
            v-model:requestor="formData.requestor"
            v-model:owner-presenter="formData.ownerPresenter"
            v-model:sponsor="formData.sponsor"
            v-model:status="formData.status"
            :submission-date="formData.submissionDate"
            :can-edit-requestor="isNewItem"
            :can-change-status="canChangeStatus"
          />
        </q-tab-panel>

        <!-- Documents Tab -->
        <q-tab-panel name="documents">
          <DocumentsSection
            :documents="formData.documents"
            :readonly="readonly"
            @upload="handleDocumentUpload"
            @delete="handleDocumentDelete"
          />
        </q-tab-panel>
      </q-tab-panels>

      <q-separator />

      <q-card-actions align="right" class="q-pa-md">
        <q-btn
          flat
          :label="$t('Cancel')"
          color="grey"
          @click="$emit('cancel')"
        />
        <q-btn
          unelevated
          :label="isNewItem ? $t('Create') : $t('Save')"
          color="primary"
          :loading="saving"
          @click="handleSubmit"
        />
      </q-card-actions>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import GeneralSection from './sections/GeneralSection.vue'
import DetailsSection from './sections/DetailsSection.vue'
import DocumentsSection from './sections/DocumentsSection.vue'
import { fileToDocumentDto } from '../utils/fileHelpers'
import type { GetMeetingItemResponse, DocumentDto } from '@/api/generated/meetingItemsApi'

// Props
const props = defineProps<{
  meetingItem?: GetMeetingItemResponse | null
  decisionBoardId: string
  canChangeStatus?: boolean
  readonly?: boolean
}>()

// Emits
const emit = defineEmits<{
  'submit': [data: FormData]
  'cancel': []
}>()

// Placeholder $t function
const $t = (key: string): string => key

// State
const activeTab = ref<string>('general')
const saving = ref(false)
const documentsToUpload = ref<File[]>([])
const documentsToDelete = ref<string[]>([])

// Form data
interface FormData {
  topic: string
  purpose: string
  outcome: string
  duration: number
  digitalProduct: string
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  status: string
  submissionDate: string
  documents: Array<{
    id: string
    fileName: string
    fileSize: number
    contentType: string
    uploadDate: string
    uploadedBy: string
  }>
}

const formData = ref<FormData>({
  topic: props.meetingItem?.topic || '',
  purpose: props.meetingItem?.purpose || '',
  outcome: props.meetingItem?.outcome || 'Decision',
  duration: props.meetingItem?.duration || 30,
  digitalProduct: props.meetingItem?.digitalProduct || '',
  requestor: props.meetingItem?.requestor || '',
  ownerPresenter: props.meetingItem?.ownerPresenter || '',
  sponsor: props.meetingItem?.sponsor || null,
  status: props.meetingItem?.status || 'Draft',
  submissionDate: props.meetingItem?.submissionDate || new Date().toISOString(),
  documents: props.meetingItem?.documents || []
})

// Computed
const isNewItem = computed(() => !props.meetingItem)

// Methods
async function handleDocumentUpload(file: File) {
  documentsToUpload.value.push(file)

  // Add to UI immediately for preview
  formData.value.documents.push({
    id: `temp-${Date.now()}`,
    fileName: file.name,
    fileSize: file.size,
    contentType: file.type,
    uploadDate: new Date().toISOString(),
    uploadedBy: 'current-user'
  })
}

function handleDocumentDelete(documentId: string) {
  // If it's a temporary document (not yet uploaded), just remove from list
  if (documentId.startsWith('temp-')) {
    const index = formData.value.documents.findIndex(d => d.id === documentId)
    if (index !== -1) {
      formData.value.documents.splice(index, 1)
      documentsToUpload.value.splice(index, 1)
    }
  } else {
    // Mark for deletion
    documentsToDelete.value.push(documentId)
    // Remove from UI
    const index = formData.value.documents.findIndex(d => d.id === documentId)
    if (index !== -1) {
      formData.value.documents.splice(index, 1)
    }
  }
}

async function handleSubmit() {
  saving.value = true

  try {
    // Convert uploaded files to DocumentDto
    const newDocuments: DocumentDto[] = []
    for (const file of documentsToUpload.value) {
      const dto = await fileToDocumentDto(file)
      newDocuments.push(dto)
    }

    const submitData = {
      ...formData.value,
      decisionBoardId: props.decisionBoardId,
      newDocuments: newDocuments.length > 0 ? newDocuments : undefined,
      documentsToDelete: documentsToDelete.value.length > 0 ? documentsToDelete.value : undefined
    }

    emit('submit', submitData as any)
  } catch (error) {
    console.error('Failed to submit:', error)
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.meeting-card {
  max-width: 1000px;
  margin: 0 auto;
}
</style>
