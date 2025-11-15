<template>
  <div class="document-upload">
    <!-- Upload Area -->
    <q-file
      v-model="selectedFile"
      :label="$t('Upload Supporting Documents')"
      outlined
      dense
      counter
      max-file-size="10485760"
      accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx"
      @update:model-value="handleFileSelect"
      class="q-mb-md"
    >
      <template v-slot:prepend>
        <q-icon name="cloud_upload" />
      </template>
      <template v-slot:hint>
        {{ $t('Max file size: 10MB. Allowed formats: PDF, Word, Excel, PowerPoint') }}
      </template>
      <template v-slot:after>
        <q-btn
          round
          dense
          flat
          icon="add"
          color="primary"
          @click="triggerFileInput"
          :disable="!selectedFile"
        />
      </template>
    </q-file>

    <!-- Document List -->
    <div v-if="documents.length > 0" class="documents-list">
      <div class="list-header">
        <span class="text-weight-medium">{{ $t('Uploaded Documents') }} ({{ documents.length }})</span>
      </div>

      <q-list bordered separator class="rounded-borders">
        <q-item
          v-for="doc in sortedDocuments"
          :key="doc.id"
          class="document-item"
        >
          <q-item-section avatar>
            <q-avatar :color="getFileColor(doc.fileName)" text-color="white" size="md">
              <q-icon :name="getFileIcon(doc.fileName)" />
            </q-avatar>
          </q-item-section>

          <q-item-section>
            <q-item-label class="text-weight-medium">{{ doc.originalFileName }}</q-item-label>
            <q-item-label caption>
              <div class="doc-meta">
                <span>{{ doc.storedFileName }}</span>
                <q-separator vertical spaced />
                <span>v{{ doc.version }}</span>
                <q-separator vertical spaced />
                <span>{{ formatDate(doc.uploadDate) }}</span>
                <q-separator vertical spaced />
                <span>{{ formatFileSize(doc.fileSize) }}</span>
              </div>
            </q-item-label>
          </q-item-section>

          <q-item-section side>
            <div class="q-gutter-xs">
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { meetingItemsApi } from '../api/meetingItems'

// Placeholder $t function
const $t = (key: string): string => key

const props = defineProps<{
  meetingItemId?: string
  decisionBoardAbbr: string
  topic?: string
  initialDocuments?: any[]
}>()

const emit = defineEmits<{
  'documents-updated': [value: any[]]
}>()

const $q = useQuasar()

// State
const selectedFile = ref<File | null>(null)
const documents = ref<any[]>([])
const uploading = ref<boolean>(false)

// Computed
const sortedDocuments = computed(() => {
  return [...documents.value].sort((a, b) => {
    if (a.originalFileName === b.originalFileName) {
      return parseInt(b.version) - parseInt(a.version)
    }
    return a.originalFileName.localeCompare(b.originalFileName)
  })
})

// Lifecycle
onMounted(() => {
  if (props.initialDocuments && props.initialDocuments.length > 0) {
    documents.value = props.initialDocuments
  }
})

// Methods
const triggerFileInput = (): void => {
  // The file is already selected, proceed with upload
  if (selectedFile.value) {
    handleUpload()
  }
}

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

const convertFileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = error => reject(error)
  })
}

const handleUpload = async (): Promise<void> => {
  if (!selectedFile.value) return

  const file = selectedFile.value
  uploading.value = true

  try {
    // Convert file to base64
    const base64Content = await convertFileToBase64(file)

    if (props.meetingItemId) {
      // Upload via API if meeting item already exists
      const response = await meetingItemsApi.uploadDocument(props.meetingItemId, {
        fileName: file.name,
        contentType: file.type,
        base64Content: base64Content
      })

      const newDoc = {
        id: response.documentId,
        originalFileName: file.name,
        fileName: file.name,
        storedFileName: response.storedFileName,
        version: response.versionNumber,
        uploadDate: new Date().toISOString(),
        fileSize: file.size,
        contentType: file.type
      }

      documents.value.push(newDoc)

      $q.notify({
        type: 'positive',
        message: $t('Document uploaded successfully'),
        caption: response.storedFileName
      })
    } else {
      // Store locally until meeting item is created
      const newDoc = {
        id: Date.now().toString(),
        fileName: file.name,
        originalFileName: file.name,
        contentType: file.type,
        base64Content: base64Content,
        fileSize: file.size,
        uploadDate: new Date().toISOString()
      }

      documents.value.push(newDoc)

      $q.notify({
        type: 'positive',
        message: $t('Document added (will be uploaded on submit)'),
        caption: file.name
      })
    }

    // Emit updated documents array
    emit('documents-updated', documents.value)
    selectedFile.value = null

  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to upload document'),
      caption: error.message
    })
  } finally {
    uploading.value = false
  }
}

const handleDownload = async (doc: any): Promise<void> => {
  try {
    const blob = await meetingItemsApi.downloadDocument(doc.id)

    // Create download link
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = doc.originalFileName || doc.fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    $q.notify({
      type: 'positive',
      message: $t('Downloaded {fileName}', { fileName: doc.originalFileName || doc.fileName })
    })
  } catch (error: any) {
    $q.notify({
      type: 'negative',
      message: $t('Failed to download document'),
      caption: error.message
    })
  }
}

const handleDelete = (doc: any): void => {
  $q.dialog({
    title: $t('Confirm Delete'),
    message: $t('Are you sure you want to delete "{fileName}"{version}?', {
      fileName: doc.originalFileName || doc.fileName,
      version: doc.version ? ` (v${doc.version})` : ''
    }),
    cancel: true,
    persistent: true
  }).onOk(async (): Promise<void> => {
    try {
      if (props.meetingItemId && doc.id) {
        // Delete via API if document is already uploaded
        await meetingItemsApi.deleteDocument(doc.id)
      }

      // Remove from local array
      documents.value = documents.value.filter(d => d.id !== doc.id)

      $q.notify({
        type: 'positive',
        message: $t('Document deleted successfully')
      })

      emit('documents-updated', documents.value)
    } catch (error: any) {
      $q.notify({
        type: 'negative',
        message: $t('Failed to delete document'),
        caption: error.message
      })
    }
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
  return date.toLocaleDateString('en-GB', {
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
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}
</script>

<style scoped lang="scss">
.document-upload {
  min-height: 300px;
}

.documents-list {
  .list-header {
    padding: 0.75rem 1rem;
    background: #f5f5f5;
    border-radius: 8px 8px 0 0;
    margin-bottom: 0.5rem;
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
  margin-top: 1rem;

  p {
    margin: 0;
  }
}

:deep(.q-field__control) {
  border-radius: 8px;
}
</style>
