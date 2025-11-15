<template>
  <div class="documents-section">
    <!-- Upload Area -->
    <div class="upload-area q-mb-md">
      <q-card flat bordered>
        <q-card-section>
          <div class="row items-center q-gutter-md">
            <div class="col">
              <q-file
                v-model="selectedFile"
                :label="$t('Select document to upload')"
                outlined
                dense
                counter
                max-file-size="10485760"
                accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx"
                @update:model-value="handleFileSelect"
              >
                <template v-slot:prepend>
                  <q-icon name="attach_file" />
                </template>
                <template v-slot:hint>
                  {{ $t('Max file size: 10MB. Allowed formats: PDF, Word, Excel, PowerPoint') }}
                </template>
              </q-file>
            </div>
            <div class="col-auto">
              <q-btn
                unelevated
                color="primary"
                icon="cloud_upload"
                :label="$t('Upload')"
                :disable="!selectedFile"
                :loading="uploading"
                @click="handleUpload"
              />
            </div>
          </div>
        </q-card-section>
      </q-card>
    </div>

    <!-- Documents List -->
    <div v-if="documents.length > 0" class="documents-list">
      <div class="list-header q-mb-sm">
        <span class="text-weight-medium">{{ $t('Uploaded Documents') }} ({{ documents.length }})</span>
      </div>

      <q-list bordered separator class="rounded-borders">
        <q-item
          v-for="doc in sortedDocuments"
          :key="doc.id"
          class="document-item"
        >
          <!-- Document Icon -->
          <q-item-section avatar>
            <q-avatar :color="getFileColor(doc.originalFileName)" text-color="white" size="md">
              <q-icon :name="getFileIcon(doc.originalFileName)" />
            </q-avatar>
          </q-item-section>

          <!-- Document Info -->
          <q-item-section>
            <q-item-label class="text-weight-medium">
              {{ doc.originalFileName }}
              <q-chip
                v-if="doc.version > 1"
                dense
                size="sm"
                color="info"
                text-color="white"
                class="q-ml-sm"
              >
                v{{ doc.version }}
              </q-chip>
              <q-chip
                v-if="doc.isLatestVersion"
                dense
                size="sm"
                color="positive"
                text-color="white"
                class="q-ml-xs"
              >
                {{ $t('Latest') }}
              </q-chip>
            </q-item-label>
            <q-item-label caption>
              <div class="doc-meta">
                <span>{{ doc.fileName }}</span>
                <q-separator vertical spaced />
                <span>{{ formatFileSize(doc.fileSize) }}</span>
                <q-separator vertical spaced />
                <span>{{ formatDate(doc.uploadDate) }}</span>
                <q-separator vertical spaced />
                <span>{{ $t('By') }}: {{ doc.uploadedBy }}</span>
              </div>
            </q-item-label>
          </q-item-section>

          <!-- Actions -->
          <q-item-section side>
            <div class="q-gutter-xs row items-center">
              <!-- Upload New Version -->
              <q-btn
                v-if="doc.isLatestVersion && meetingItemId"
                flat
                round
                dense
                icon="upload_file"
                color="info"
                size="sm"
                @click="handleUploadVersion(doc)"
              >
                <q-tooltip>{{ $t('Upload New Version') }}</q-tooltip>
              </q-btn>

              <!-- View All Versions -->
              <q-btn
                v-if="doc.version > 1 || hasMultipleVersions(doc)"
                flat
                round
                dense
                icon="history"
                color="grey-7"
                size="sm"
                @click="handleViewVersions(doc)"
              >
                <q-tooltip>{{ $t('View All Versions') }}</q-tooltip>
              </q-btn>

              <!-- Download -->
              <q-btn
                flat
                round
                dense
                icon="download"
                color="primary"
                size="sm"
                @click="handleDownload(doc)"
              >
                <q-tooltip>{{ $t('Download') }}</q-tooltip>
              </q-btn>

              <!-- Delete -->
              <q-btn
                flat
                round
                dense
                icon="delete"
                color="negative"
                size="sm"
                @click="handleDelete(doc)"
              >
                <q-tooltip>{{ $t('Delete') }}</q-tooltip>
              </q-btn>
            </div>
          </q-item-section>
        </q-item>
      </q-list>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <q-icon name="description" size="4rem" color="grey-5" />
      <p class="text-grey-7 q-mt-md">{{ $t('No documents uploaded yet') }}</p>
      <p class="text-grey-6 text-caption">{{ $t('Upload documents to support your request') }}</p>
    </div>

    <!-- Version Upload Dialog -->
    <q-dialog v-model="showVersionDialog" persistent>
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ $t('Upload New Version') }}</div>
          <div class="text-caption text-grey-7">
            {{ $t('Current file') }}: {{ currentBaseDocument?.originalFileName }}
          </div>
        </q-card-section>

        <q-card-section>
          <q-file
            v-model="versionFile"
            :label="$t('Select new version file')"
            outlined
            dense
            counter
            max-file-size="10485760"
            accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx"
          >
            <template v-slot:prepend>
              <q-icon name="attach_file" />
            </template>
          </q-file>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn
            flat
            :label="$t('Cancel')"
            color="grey-7"
            @click="closeVersionDialog"
          />
          <q-btn
            unelevated
            :label="$t('Upload Version')"
            color="primary"
            :disable="!versionFile"
            :loading="uploadingVersion"
            @click="handleUploadVersionConfirm"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Versions List Dialog -->
    <q-dialog v-model="showVersionsDialog">
      <q-card style="min-width: 600px">
        <q-card-section>
          <div class="text-h6">{{ $t('Document Version History') }}</div>
          <div class="text-caption text-grey-7">
            {{ currentVersionsDocument?.originalFileName }}
          </div>
        </q-card-section>

        <q-card-section>
          <q-list bordered separator>
            <q-item
              v-for="version in documentVersions"
              :key="version.id"
            >
              <q-item-section avatar>
                <q-avatar color="primary" text-color="white">
                  v{{ version.version }}
                </q-avatar>
              </q-item-section>

              <q-item-section>
                <q-item-label>
                  {{ version.originalFileName }}
                  <q-chip
                    v-if="version.isLatestVersion"
                    dense
                    size="sm"
                    color="positive"
                    text-color="white"
                    class="q-ml-sm"
                  >
                    {{ $t('Latest') }}
                  </q-chip>
                </q-item-label>
                <q-item-label caption>
                  {{ formatFileSize(version.fileSize) }} • {{ formatDate(version.uploadDate) }} • {{ version.uploadedBy }}
                </q-item-label>
              </q-item-section>

              <q-item-section side>
                <q-btn
                  flat
                  round
                  dense
                  icon="download"
                  color="primary"
                  size="sm"
                  @click="handleDownload(version)"
                >
                  <q-tooltip>{{ $t('Download') }}</q-tooltip>
                </q-btn>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn
            flat
            :label="$t('Close')"
            color="grey-7"
            @click="showVersionsDialog = false"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useQuasar } from 'quasar'
import type { DocumentDetail } from '../../../stores/meetingItemStore'

// Placeholder $t function
const $t = (key: string): string => key

const $q = useQuasar()

// Props
const props = defineProps<{
  documents: DocumentDetail[]
  meetingItemId?: string
}>()

// Emits
defineEmits<{
  'upload': [file: File]
  'upload-version': [baseDocumentId: string, file: File]
  'download': [document: DocumentDetail]
  'delete': [documentId: string]
}>()

const emit = defineEmits<{
  'upload': [file: File]
  'upload-version': [baseDocumentId: string, file: File]
  'download': [document: DocumentDetail]
  'delete': [documentId: string]
}>()

// State
const selectedFile = ref<File | null>(null)
const uploading = ref<boolean>(false)
const showVersionDialog = ref<boolean>(false)
const showVersionsDialog = ref<boolean>(false)
const versionFile = ref<File | null>(null)
const uploadingVersion = ref<boolean>(false)
const currentBaseDocument = ref<DocumentDetail | null>(null)
const currentVersionsDocument = ref<DocumentDetail | null>(null)
const documentVersions = ref<DocumentDetail[]>([])

// Computed
const sortedDocuments = computed(() => {
  return [...props.documents].sort((a, b) => {
    // Sort by original filename, then by version descending
    if (a.originalFileName === b.originalFileName) {
      return b.version - a.version
    }
    return a.originalFileName.localeCompare(b.originalFileName)
  })
})

// Methods
const handleFileSelect = (file: File | null): void => {
  if (file) {
    // Validate file size
    if (file.size > 10485760) {
      $q.notify({
        type: 'negative',
        message: $t('File size exceeds 10MB limit')
      })
      selectedFile.value = null
      return
    }
  }
}

const handleUpload = (): void => {
  if (!selectedFile.value) return

  emit('upload', selectedFile.value)
  selectedFile.value = null
}

const handleUploadVersion = (doc: DocumentDetail): void => {
  currentBaseDocument.value = doc
  showVersionDialog.value = true
}

const closeVersionDialog = (): void => {
  showVersionDialog.value = false
  versionFile.value = null
  currentBaseDocument.value = null
}

const handleUploadVersionConfirm = (): void => {
  if (!versionFile.value || !currentBaseDocument.value) return

  emit('upload-version', currentBaseDocument.value.id, versionFile.value)
  closeVersionDialog()
}

const handleViewVersions = (doc: DocumentDetail): void => {
  currentVersionsDocument.value = doc

  // Get all versions of this document
  // In real implementation, this would filter by baseDocumentId or originalFileName
  documentVersions.value = props.documents
    .filter(d => d.originalFileName === doc.originalFileName)
    .sort((a, b) => b.version - a.version)

  showVersionsDialog.value = true
}

const hasMultipleVersions = (doc: DocumentDetail): boolean => {
  return props.documents.filter(d => d.originalFileName === doc.originalFileName).length > 1
}

const handleDownload = (doc: DocumentDetail): void => {
  emit('download', doc)
}

const handleDelete = (doc: DocumentDetail): void => {
  $q.dialog({
    title: $t('Confirm Delete'),
    message: $t('Are you sure you want to delete "{fileName}" (v{version})?', {
      fileName: doc.originalFileName,
      version: doc.version.toString()
    }),
    cancel: true,
    persistent: true
  }).onOk((): void => {
    emit('delete', doc.id)
  })
}

const getFileIcon = (fileName: string): string => {
  const ext = fileName.split('.').pop()?.toLowerCase()
  const icons: Record<string, string> = {
    'pdf': 'picture_as_pdf',
    'doc': 'description',
    'docx': 'description',
    'xls': 'table_chart',
    'xlsx': 'table_chart',
    'ppt': 'slideshow',
    'pptx': 'slideshow'
  }
  return icons[ext || ''] || 'insert_drive_file'
}

const getFileColor = (fileName: string): string => {
  const ext = fileName.split('.').pop()?.toLowerCase()
  const colors: Record<string, string> = {
    'pdf': 'red-6',
    'doc': 'blue-6',
    'docx': 'blue-6',
    'xls': 'green-6',
    'xlsx': 'green-6',
    'ppt': 'orange-6',
    'pptx': 'orange-6'
  }
  return colors[ext || ''] || 'grey-6'
}

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}
</script>

<style scoped lang="scss">
.documents-section {
  padding: 0.5rem 0;
}

.upload-area {
  .q-card {
    border: 2px dashed #e0e0e0;
    transition: border-color 0.3s;

    &:hover {
      border-color: #1976d2;
    }
  }
}

.documents-list {
  .list-header {
    padding: 0.75rem 1rem;
    background: #f5f5f5;
    border-radius: 8px 8px 0 0;
    font-size: 0.9rem;
    color: #424242;
  }
}

.document-item {
  padding: 1rem;
  transition: background-color 0.2s;

  &:hover {
    background-color: #fafafa;
  }
}

.doc-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: #757575;
  margin-top: 0.25rem;
  flex-wrap: wrap;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
  background: #fafafa;
  border-radius: 8px;
  border: 2px dashed #e0e0e0;

  p {
    margin: 0;
  }
}
</style>
