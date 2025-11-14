<template>
  <div class="meeting-item-form">
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
        <q-tab name="documents" :label="$t('Documents')" icon="attachment" :badge="meetingItem.documents.length || undefined" />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="activeTab" animated>
        <!-- General Tab -->
        <q-tab-panel name="general" class="q-pa-lg">
          <div class="form-section">
            <h6 class="section-title">{{ $t('Basic Information') }}</h6>

            <div class="row q-col-gutter-md">
              <div class="col-12">
                <q-input
                  v-model="meetingItem.topic"
                  :label="$t('Topic') + ' *'"
                  :hint="$t('Short title for the discussion')"
                  outlined
                  dense
                  :rules="[(val: string) => !!val || $t('Topic is required')]"
                />
              </div>

              <div class="col-12">
                <q-input
                  v-model="meetingItem.purpose"
                  :label="$t('Purpose') + ' *'"
                  :hint="$t('Full explanation of the discussion and requested decision')"
                  outlined
                  dense
                  type="textarea"
                  rows="4"
                  :rules="[(val: string) => !!val || $t('Purpose is required')]"
                />
              </div>

              <div class="col-12 col-md-6">
                <q-select
                  v-model="meetingItem.outcome"
                  :label="$t('Outcome') + ' *'"
                  :hint="$t('Expected outcome type')"
                  outlined
                  dense
                  :options="outcomeOptions"
                  :rules="[(val: string) => !!val || $t('Outcome is required')]"
                />
              </div>

              <div class="col-12 col-md-6">
                <q-input
                  v-model.number="meetingItem.duration"
                  :label="$t('Duration (minutes)') + ' *'"
                  :hint="$t('Estimated discussion time')"
                  outlined
                  dense
                  type="number"
                  min="1"
                  :rules="[
                    (val: number) => !!val || $t('Duration is required'),
                    (val: number) => val > 0 || $t('Duration must be positive')
                  ]"
                />
              </div>

              <div class="col-12">
                <q-input
                  v-model="meetingItem.digitalProduct"
                  :label="$t('Digital Product') + ' *'"
                  :hint="$t('Related digital product or service')"
                  outlined
                  dense
                  :rules="[(val: string) => !!val || $t('Digital Product is required')]"
                />
              </div>
            </div>
          </div>

          <q-separator class="q-my-lg" />

          <div class="form-section">
            <h6 class="section-title">{{ $t('Participants') }}</h6>

            <div class="row q-col-gutter-md">
              <div class="col-12 col-md-4">
                <q-input
                  v-model="meetingItem.requestor"
                  :label="$t('Requestor') + ' *'"
                  :hint="$t('KBC ID (auto-filled)')"
                  outlined
                  dense
                  readonly
                  :rules="[(val: string) => !!val || $t('Requestor is required')]"
                >
                  <template v-slot:prepend>
                    <q-icon name="person" />
                  </template>
                </q-input>
              </div>

              <div class="col-12 col-md-4">
                <q-input
                  v-model="meetingItem.ownerPresenter"
                  :label="$t('Owner/Presenter') + ' *'"
                  :hint="$t('KBC ID (defaults to Requestor)')"
                  outlined
                  dense
                  :rules="[(val: string) => !!val || $t('Owner/Presenter is required')]"
                >
                  <template v-slot:prepend>
                    <q-icon name="record_voice_over" />
                  </template>
                </q-input>
              </div>

              <div class="col-12 col-md-4">
                <q-input
                  v-model="meetingItem.sponsor"
                  :label="$t('Sponsor')"
                  :hint="$t('KBC ID (optional)')"
                  outlined
                  dense
                >
                  <template v-slot:prepend>
                    <q-icon name="supervisor_account" />
                  </template>
                </q-input>
              </div>
            </div>
          </div>
        </q-tab-panel>

        <!-- Details Tab -->
        <q-tab-panel name="details" class="q-pa-lg">
          <div class="form-section">
            <h6 class="section-title">{{ $t('System Information') }}</h6>

            <div class="row q-col-gutter-md">
              <div class="col-12 col-md-6">
                <q-input
                  v-model="formattedSubmissionDate"
                  :label="$t('Submission Date')"
                  outlined
                  dense
                  readonly
                  type="date"
                >
                  <template v-slot:prepend>
                    <q-icon name="event" />
                  </template>
                </q-input>
              </div>

              <div class="col-12 col-md-6">
                <q-select
                  v-model="meetingItem.status"
                  :label="$t('Status')"
                  :hint="$t('Current status of the meeting item')"
                  outlined
                  dense
                  :options="statusOptions"
                  :disable="!canChangeStatus"
                >
                  <template v-slot:prepend>
                    <q-icon name="flag" />
                  </template>
                </q-select>
              </div>
            </div>
          </div>
        </q-tab-panel>

        <!-- Documents Tab -->
        <q-tab-panel name="documents" class="q-pa-lg">
          <div class="documents-section">
            <div class="documents-header">
              <h6 class="section-title">{{ $t('Documents') }}</h6>
              <q-btn
                :label="$t('Upload Document')"
                icon="cloud_upload"
                color="primary"
                outline
                dense
                @click="showUploadDialog"
              />
            </div>

            <div v-if="meetingItem.documents.length === 0" class="empty-state">
              <q-icon name="description" size="4rem" color="grey-5" />
              <p class="text-grey-7 q-mt-md">{{ $t('No documents uploaded yet') }}</p>
            </div>

            <q-list v-else bordered separator class="documents-list">
              <q-item v-for="doc in meetingItem.documents" :key="doc.id" class="document-item">
                <q-item-section avatar>
                  <q-icon :name="getFileIcon(doc.fileName)" size="2rem" color="primary" />
                </q-item-section>

                <q-item-section>
                  <q-item-label>{{ doc.fileName }}</q-item-label>
                  <q-item-label caption>
                    {{ formatFileSize(doc.fileSize) }} • {{ $t('Uploaded') }} {{ formatDate(doc.uploadDate) }} {{ $t('by') }} {{ doc.uploadedBy }}
                  </q-item-label>
                </q-item-section>

                <q-item-section side>
                  <div class="document-actions">
                    <q-btn
                      flat
                      dense
                      round
                      icon="download"
                      color="primary"
                      @click="downloadDocument(doc)"
                    >
                      <q-tooltip>{{ $t('Download') }}</q-tooltip>
                    </q-btn>
                    <q-btn
                      flat
                      dense
                      round
                      icon="delete"
                      color="negative"
                      @click="confirmDeleteDocument(doc.id)"
                    >
                      <q-tooltip>{{ $t('Delete') }}</q-tooltip>
                    </q-btn>
                  </div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </q-tab-panel>
      </q-tab-panels>
    </q-card>

    <!-- Upload Dialog -->
    <q-dialog v-model="uploadDialogVisible">
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ $t('Upload Document') }}</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-file
            v-model="selectedFile"
            :label="$t('Select file')"
            outlined
            dense
            :max-file-size="10485760"
            @rejected="onFileRejected"
          >
            <template v-slot:prepend>
              <q-icon name="attach_file" />
            </template>
          </q-file>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="$t('Cancel')" color="primary" v-close-popup />
          <q-btn
            :label="$t('Upload')"
            color="primary"
            :disable="!selectedFile"
            :loading="uploading"
            @click="handleUpload"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useQuasar } from 'quasar'
import type {
  MeetingItem,
  MeetingItemOutcome,
  MeetingItemStatus,
  Document
} from '../models/meetingItem'
import { meetingItemsApi } from '../services/meetingItemsApi'

const $q = useQuasar()

const props = defineProps<{
  meetingItem: MeetingItem
  canChangeStatus?: boolean
}>()

const emit = defineEmits<{
  'update:meetingItem': [value: MeetingItem]
  'documentUploaded': [document: Document]
  'documentDeleted': [documentId: string]
}>()

// State
const activeTab = ref<string>('general')
const uploadDialogVisible = ref<boolean>(false)
const selectedFile = ref<File | null>(null)
const uploading = ref<boolean>(false)

// Computed
const formattedSubmissionDate = computed<string>(() => {
  return new Date(props.meetingItem.submissionDate).toISOString().split('T')[0]
})

const outcomeOptions = computed<MeetingItemOutcome[]>(() => [
  'Decision' as MeetingItemOutcome,
  'Discussion' as MeetingItemOutcome,
  'Information' as MeetingItemOutcome
])

const statusOptions = computed<MeetingItemStatus[]>(() => [
  'Draft' as MeetingItemStatus,
  'Submitted' as MeetingItemStatus,
  'UnderReview' as MeetingItemStatus,
  'Approved' as MeetingItemStatus,
  'Rejected' as MeetingItemStatus,
  'Deferred' as MeetingItemStatus
])

// Methods
const showUploadDialog = (): void => {
  uploadDialogVisible.value = true
  selectedFile.value = null
}

const handleUpload = async (): Promise<void> => {
  if (!selectedFile.value || !props.meetingItem.id) return

  uploading.value = true

  try {
    const reader = new FileReader()
    reader.onload = async (e) => {
      const base64Content = e.target?.result as string

      const document = await meetingItemsApi.uploadDocument({
        meetingItemId: props.meetingItem.id!,
        fileName: selectedFile.value!.name,
        contentType: selectedFile.value!.type,
        fileSize: selectedFile.value!.size,
        base64Content
      })

      emit('documentUploaded', document)

      $q.notify({
        type: 'positive',
        message: $t('Document uploaded successfully'),
        timeout: 2000
      })

      uploadDialogVisible.value = false
    }

    reader.readAsDataURL(selectedFile.value)
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to upload document'),
      caption: error instanceof Error ? error.message : String(error),
      timeout: 3000
    })
  } finally {
    uploading.value = false
  }
}

const onFileRejected = (): void => {
  $q.notify({
    type: 'negative',
    message: $t('File rejected. Maximum file size is 10 MB'),
    timeout: 3000
  })
}

const confirmDeleteDocument = (documentId: string): void => {
  $q.dialog({
    title: $t('Confirm'),
    message: $t('Are you sure you want to delete this document?'),
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await meetingItemsApi.deleteDocument(props.meetingItem.id!, documentId)
      emit('documentDeleted', documentId)

      $q.notify({
        type: 'positive',
        message: $t('Document deleted successfully'),
        timeout: 2000
      })
    } catch (error) {
      $q.notify({
        type: 'negative',
        message: $t('Failed to delete document'),
        caption: error instanceof Error ? error.message : String(error),
        timeout: 3000
      })
    }
  })
}

const downloadDocument = async (doc: Document): Promise<void> => {
  $q.notify({
    type: 'info',
    message: $t('Download functionality to be implemented'),
    timeout: 2000
  })
}

const getFileIcon = (fileName: string): string => {
  const extension = fileName.split('.').pop()?.toLowerCase()

  const iconMap: Record<string, string> = {
    pdf: 'picture_as_pdf',
    doc: 'description',
    docx: 'description',
    xls: 'table_chart',
    xlsx: 'table_chart',
    ppt: 'slideshow',
    pptx: 'slideshow',
    jpg: 'image',
    jpeg: 'image',
    png: 'image',
    gif: 'image',
    zip: 'folder_zip',
    txt: 'text_snippet'
  }

  return iconMap[extension || ''] || 'insert_drive_file'
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'

  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString()
}

// Placeholder $t function (replace with actual i18n)
const $t = (key: string): string => key
</script>

<style scoped lang="scss">
.meeting-item-form {
  max-width: 1200px;
  margin: 0 auto;
}

.meeting-card {
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.form-section {
  .section-title {
    font-size: 1rem;
    font-weight: 600;
    color: #2c3e50;
    margin: 0 0 1.25rem 0;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid #e0e0e0;
  }
}

.documents-section {
  .documents-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;

    .section-title {
      margin: 0;
      border: none;
      padding: 0;
    }
  }
}

.documents-list {
  border-radius: 8px;

  .document-item {
    padding: 1rem;
    transition: background-color 0.2s;

    &:hover {
      background-color: #f5f7fa;
    }
  }
}

.document-actions {
  display: flex;
  gap: 0.25rem;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
}

:deep(.q-tab) {
  padding: 1rem 1.5rem;
  font-weight: 500;
  text-transform: none;
  font-size: 0.9rem;
}
</style>
