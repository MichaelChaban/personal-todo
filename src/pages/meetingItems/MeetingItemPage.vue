<template>
  <q-page class="q-pa-md">
    <div class="meeting-item-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">
            {{ isEditMode ? $t('Edit Meeting Item') : $t('Create Meeting Item') }}
          </h4>
          <p class="text-grey-7 q-ma-none">
            {{
              isEditMode
                ? $t('Update meeting item details')
                : $t('Complete all required fields to submit your request')
            }}
          </p>
        </div>
        <q-chip
          v-if="formData.status && isEditMode"
          :color="getStatusColor(formData.status)"
          text-color="white"
          class="status-chip"
        >
          {{ formData.status }}
        </q-chip>
      </div>

      <!-- Loading State -->
      <div v-if="meetingItemStore.loading" class="text-center q-pa-xl">
        <q-spinner color="primary" size="50px" />
        <div class="q-mt-md text-grey-6">{{ $t('Loading meeting item...') }}</div>
      </div>

      <!-- Error State -->
      <q-banner
        v-else-if="meetingItemStore.error"
        class="bg-negative text-white q-mb-md"
      >
        <template #avatar>
          <q-icon name="error" color="white" />
        </template>
        {{ meetingItemStore.error }}
        <template #action>
          <q-btn
            flat
            color="white"
            :label="$t('Retry')"
            @click="loadMeetingItem"
          />
        </template>
      </q-banner>

      <!-- Form Card -->
      <Form
        v-else
        :validation-schema="meetingItemSchema"
        @submit="handleSubmit"
        v-slot="{ meta }"
      >
        <q-card flat bordered class="meeting-item-form">
          <!-- Tabs -->
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
            <q-tab
              name="documents"
              :label="$t('Documents')"
              icon="attachment"
              :badge="formData.documents.length || undefined"
            />
          </q-tabs>

          <q-separator />

          <!-- Tab Panels -->
          <q-tab-panels v-model="activeTab" animated>
            <!-- General Tab -->
            <q-tab-panel name="general" class="q-pa-lg">
              <GeneralSection
                v-model:topic="formData.topic"
                v-model:purpose="formData.purpose"
                v-model:outcome="formData.outcome"
                v-model:duration="formData.duration"
                v-model:digital-product="formData.digitalProduct"
                v-model:requestor="formData.requestor"
                v-model:owner-presenter="formData.ownerPresenter"
                v-model:sponsor="formData.sponsor"
              />
            </q-tab-panel>

            <!-- Details Tab -->
            <q-tab-panel name="details" class="q-pa-lg">
              <DetailsSection
                :submission-date="formData.submissionDate"
                v-model:status="formData.status"
                :decision-board-id="formData.decisionBoardId"
                :template-id="formData.templateId"
                :created-at="formData.createdAt"
                :created-by="formData.createdBy"
                :updated-at="formData.updatedAt"
                :updated-by="formData.updatedBy"
                :can-change-status="canChangeStatus"
              />
            </q-tab-panel>

            <!-- Documents Tab -->
            <q-tab-panel name="documents" class="q-pa-lg">
              <DocumentsSection
                :documents="formData.documents"
                :meeting-item-id="meetingItemId"
                @upload="handleDocumentUpload"
                @upload-version="handleDocumentVersion"
                @download="handleDocumentDownload"
                @delete="handleDocumentDelete"
              />
            </q-tab-panel>
          </q-tab-panels>

          <!-- Action Buttons -->
          <q-separator />
          <q-card-actions align="right" class="q-pa-md">
            <q-btn
              flat
              :label="$t('Cancel')"
              color="grey-7"
              @click="handleCancel"
            />
            <q-btn
              unelevated
              type="submit"
              :label="isEditMode ? $t('Update') : $t('Submit')"
              color="primary"
              :loading="submitting"
              :disable="!meta.valid"
            />
          </q-card-actions>
        </q-card>
      </Form>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useQuasar } from 'quasar'
import { Form } from 'vee-validate'
import GeneralSection from '../../components/meetingItem/sections/GeneralSection.vue'
import DetailsSection from '../../components/meetingItem/sections/DetailsSection.vue'
import DocumentsSection from '../../components/meetingItem/sections/DocumentsSection.vue'
import { useMeetingItemStore } from '../../stores/meetingItemStore'
import { meetingItemSchema } from '../../validation/meetingItemSchema'
import type {
  DocumentDetail,
  DocumentDto,
  DocumentVersionDto
} from '../../stores/meetingItemStore'

// Placeholder $t function
const $t = (key: string): string => key

const router = useRouter()
const route = useRoute()
const $q = useQuasar()
const meetingItemStore = useMeetingItemStore()

// State
const activeTab = ref<string>('general')
const submitting = ref<boolean>(false)
const documentsToUpload = ref<File[]>([])
const documentVersionsToUpload = ref<{ baseDocumentId: string; file: File }[]>([])
const documentsToDelete = ref<string[]>([])

// Form Data
interface FormData {
  topic: string
  purpose: string
  outcome: string
  duration: number | null
  digitalProduct: string
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  submissionDate: string | null
  status: string
  decisionBoardId: string
  templateId: string | null
  documents: DocumentDetail[]
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
}

const formData = ref<FormData>({
  topic: '',
  purpose: '',
  outcome: '',
  duration: null,
  digitalProduct: '',
  requestor: 'USER123', // TODO: Get from auth context
  ownerPresenter: 'USER123',
  sponsor: null,
  submissionDate: null,
  status: 'Draft',
  decisionBoardId: '',
  templateId: null,
  documents: [],
  createdAt: new Date().toISOString(),
  createdBy: 'USER123',
  updatedAt: null,
  updatedBy: null
})

// Computed
const isEditMode = computed(() => !!route.params.id)
const meetingItemId = computed(() => route.params.id as string | undefined)

const decisionBoardId = computed(() => {
  return (route.query.decisionBoardId as string) || 'board-123'
})

const canChangeStatus = computed(() => {
  // TODO: Check user permissions from auth context
  // Only secretary/admin can change status
  return false
})


// Watch for store changes
watch(
  () => meetingItemStore.currentMeetingItem,
  (newVal) => {
    if (newVal && isEditMode.value) {
      formData.value = {
        topic: newVal.topic,
        purpose: newVal.purpose,
        outcome: newVal.outcome,
        duration: newVal.duration,
        digitalProduct: newVal.digitalProduct,
        requestor: newVal.requestor,
        ownerPresenter: newVal.ownerPresenter,
        sponsor: newVal.sponsor,
        submissionDate: newVal.submissionDate,
        status: newVal.status,
        decisionBoardId: newVal.decisionBoardId,
        templateId: newVal.templateId,
        documents: newVal.documents,
        createdAt: newVal.createdAt,
        createdBy: newVal.createdBy,
        updatedAt: newVal.updatedAt,
        updatedBy: newVal.updatedBy
      }
    }
  },
  { immediate: true, deep: true }
)

// Lifecycle
onMounted(async (): Promise<void> => {
  // Set decision board ID
  formData.value.decisionBoardId = decisionBoardId.value

  if (isEditMode.value) {
    await loadMeetingItem()
  }
})

// Methods
const loadMeetingItem = async (): Promise<void> => {
  const id = route.params.id as string
  if (id) {
    try {
      await meetingItemStore.getMeetingItem(id)
    } catch (error) {
      console.error('Failed to load meeting item:', error)
    }
  }
}

const convertFileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = () => {
      const result = reader.result as string
      // Remove data URL prefix (e.g., "data:application/pdf;base64,")
      const base64 = result.split(',')[1]
      resolve(base64)
    }
    reader.onerror = (error) => reject(error)
  })
}

const handleDocumentUpload = async (file: File): Promise<void> => {
  try {
    if (isEditMode.value && meetingItemId.value) {
      // If editing, upload immediately
      const base64Content = await convertFileToBase64(file)

      const response = await meetingItemStore.uploadDocument({
        meetingItemId: meetingItemId.value,
        fileName: file.name,
        contentType: file.type,
        fileSize: file.size,
        base64Content
      })

      // Add to documents list
      formData.value.documents.push({
        id: response.documentId,
        fileName: response.fileName,
        originalFileName: file.name,
        storagePath: `meeting-items/${meetingItemId.value}/${response.fileName}`,
        fileSize: file.size,
        contentType: file.type,
        version: 1,
        uploadDate: response.uploadDate,
        uploadedBy: response.uploadedBy,
        isLatestVersion: true
      })

      $q.notify({
        type: 'positive',
        message: $t('Document uploaded successfully')
      })
    } else {
      // If creating, queue for upload on submit
      documentsToUpload.value.push(file)

      // Add to preview list
      formData.value.documents.push({
        id: `temp-${Date.now()}`,
        fileName: file.name,
        originalFileName: file.name,
        storagePath: '',
        fileSize: file.size,
        contentType: file.type,
        version: 1,
        uploadDate: new Date().toISOString(),
        uploadedBy: formData.value.requestor,
        isLatestVersion: true
      })

      $q.notify({
        type: 'info',
        message: $t('Document will be uploaded on submit')
      })
    }
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to upload document'),
      caption: error.message
    })
  }
}

const handleDocumentVersion = (baseDocumentId: string, file: File): void => {
  // Queue version upload
  documentVersionsToUpload.value.push({ baseDocumentId, file })

  // Add to preview (simplified - would need proper version calculation)
  const baseDoc = formData.value.documents.find((d) => d.id === baseDocumentId)
  if (baseDoc) {
    formData.value.documents.push({
      id: `temp-v-${Date.now()}`,
      fileName: file.name,
      originalFileName: baseDoc.originalFileName,
      storagePath: '',
      fileSize: file.size,
      contentType: file.type,
      version: baseDoc.version + 1,
      uploadDate: new Date().toISOString(),
      uploadedBy: formData.value.requestor,
      isLatestVersion: true
    })

    // Mark old version as not latest
    baseDoc.isLatestVersion = false
  }

  $q.notify({
    type: 'info',
    message: $t('New version will be uploaded on submit')
  })
}

const handleDocumentDownload = async (doc: DocumentDetail): Promise<void> => {
  try {
    const blob = await meetingItemStore.downloadDocument(doc.id)

    // Create download link
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = doc.originalFileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    $q.notify({
      type: 'positive',
      message: $t('Downloaded {fileName}', { fileName: doc.originalFileName })
    })
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to download document'),
      caption: error.message
    })
  }
}

const handleDocumentDelete = (documentId: string): void => {
  if (documentId.startsWith('temp-')) {
    // Remove from preview
    const index = formData.value.documents.findIndex((d) => d.id === documentId)
    if (index !== -1) {
      formData.value.documents.splice(index, 1)

      // Remove from upload queue if exists
      if (documentId.startsWith('temp-v-')) {
        const vIndex = documentVersionsToUpload.value.findIndex((v) =>
          documentId.includes(Date.now().toString())
        )
        if (vIndex !== -1) {
          documentVersionsToUpload.value.splice(vIndex, 1)
        }
      } else {
        documentsToUpload.value.splice(index, 1)
      }
    }
  } else {
    // Queue for deletion
    documentsToDelete.value.push(documentId)

    // Remove from display
    const index = formData.value.documents.findIndex((d) => d.id === documentId)
    if (index !== -1) {
      formData.value.documents.splice(index, 1)
    }
  }

  $q.notify({
    type: 'positive',
    message: $t('Document removed')
  })
}

const handleSubmit = async (): Promise<void> => {

  submitting.value = true
  try {
    if (isEditMode.value && meetingItemId.value) {
      // Update existing meeting item
      const newDocumentDtos: DocumentDto[] = []
      for (const file of documentsToUpload.value) {
        const base64Content = await convertFileToBase64(file)
        newDocumentDtos.push({
          fileName: file.name,
          contentType: file.type,
          fileSize: file.size,
          base64Content
        })
      }

      const documentVersionDtos: DocumentVersionDto[] = []
      for (const versionData of documentVersionsToUpload.value) {
        const base64Content = await convertFileToBase64(versionData.file)
        documentVersionDtos.push({
          baseDocumentId: versionData.baseDocumentId,
          document: {
            fileName: versionData.file.name,
            contentType: versionData.file.type,
            fileSize: versionData.file.size,
            base64Content
          }
        })
      }

      await meetingItemStore.updateMeetingItem(meetingItemId.value, {
        id: meetingItemId.value,
        topic: formData.value.topic,
        purpose: formData.value.purpose,
        outcome: formData.value.outcome,
        digitalProduct: formData.value.digitalProduct,
        duration: formData.value.duration!,
        ownerPresenter: formData.value.ownerPresenter,
        sponsor: formData.value.sponsor,
        status: formData.value.status,
        newDocuments: newDocumentDtos,
        documentVersions: documentVersionDtos,
        documentsToDelete: documentsToDelete.value
      })

      $q.notify({
        type: 'positive',
        message: $t('Meeting item updated successfully')
      })
    } else {
      // Create new meeting item
      const documentDtos: DocumentDto[] = []
      for (const file of documentsToUpload.value) {
        const base64Content = await convertFileToBase64(file)
        documentDtos.push({
          fileName: file.name,
          contentType: file.type,
          fileSize: file.size,
          base64Content
        })
      }

      await meetingItemStore.createMeetingItem({
        decisionBoardId: formData.value.decisionBoardId,
        topic: formData.value.topic,
        purpose: formData.value.purpose,
        outcome: formData.value.outcome,
        digitalProduct: formData.value.digitalProduct,
        duration: formData.value.duration!,
        requestor: formData.value.requestor,
        ownerPresenter: formData.value.ownerPresenter,
        sponsor: formData.value.sponsor,
        documents: documentDtos
      })

      $q.notify({
        type: 'positive',
        message: $t('Meeting item submitted successfully')
      })
    }

    await router.push({
      name: 'meeting-items-list',
      query: { decisionBoardId: decisionBoardId.value }
    })
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
  router.push({
    name: 'meeting-items-list',
    query: { decisionBoardId: decisionBoardId.value }
  })
}

const getStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    Draft: 'grey',
    Submitted: 'blue',
    UnderReview: 'orange',
    Approved: 'green',
    Rejected: 'red',
    Deferred: 'purple'
  }
  return colors[status] || 'grey'
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
  gap: 1rem;

  h4 {
    font-size: 1.5rem;
    font-weight: 600;
    color: #1a1a1a;
  }

  p {
    font-size: 0.9rem;
  }

  .status-chip {
    font-weight: 600;
    font-size: 0.85rem;
  }
}

.meeting-item-form {
  max-width: 1000px;
  margin: 0 auto;
}

:deep(.q-tab) {
  padding: 1rem 1.5rem;
  font-weight: 500;
  text-transform: none;
  font-size: 0.9rem;
}
</style>
