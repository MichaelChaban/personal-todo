<template>
  <div class="document-upload">
    <!-- Upload Area -->
    <q-file
      v-model="selectedFile"
      label="Upload Supporting Documents"
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
        Max file size: 10MB. Allowed formats: PDF, Word, Excel, PowerPoint
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
        <span class="text-weight-medium">Uploaded Documents ({{ documents.length }})</span>
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
                <q-tooltip>Download</q-tooltip>
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
                <q-tooltip>Delete</q-tooltip>
              </q-btn>
            </div>
          </q-item-section>
        </q-item>
      </q-list>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <q-icon name="description" size="4rem" color="grey-5" />
      <p class="text-grey-7 q-mt-md">No documents uploaded yet</p>
      <p class="text-grey-6 text-caption">Upload documents to support your request</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useQuasar } from 'quasar'

const props = defineProps({
  meetingItemId: String,
  decisionBoardAbbr: {
    type: String,
    required: true
  },
  topic: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['documents-updated'])

const $q = useQuasar()

// State
const selectedFile = ref(null)
const documents = ref([
  // Mock data - remove when connected to API
  // {
  //   id: '1',
  //   originalFileName: 'Business_Case.pdf',
  //   storedFileName: 'DB20250115TOPIC.v01',
  //   version: '01',
  //   uploadDate: new Date().toISOString(),
  //   fileSize: 245678
  // }
])

// Computed
const sortedDocuments = computed(() => {
  return [...documents.value].sort((a, b) => {
    if (a.originalFileName === b.originalFileName) {
      return b.version - a.version
    }
    return a.originalFileName.localeCompare(b.originalFileName)
  })
})

// Methods
const triggerFileInput = () => {
  // The file is already selected, proceed with upload
  if (selectedFile.value) {
    handleUpload()
  }
}

const handleFileSelect = (file) => {
  if (file) {
    // Validate file size
    if (file.size > 10485760) {
      $q.notify({
        type: 'negative',
        message: 'File size exceeds 10MB limit'
      })
      selectedFile.value = null
      return
    }
  }
}

const handleUpload = async () => {
  if (!selectedFile.value) return

  const file = selectedFile.value

  try {
    // Calculate version number
    const existingVersions = documents.value
      .filter(d => d.originalFileName === file.name)
      .map(d => parseInt(d.version))
    const nextVersion = existingVersions.length > 0
      ? Math.max(...existingVersions) + 1
      : 1

    // Generate stored filename
    const today = new Date()
    const dateStr = today.toISOString().split('T')[0].replace(/-/g, '')
    const topicSlug = (props.topic || 'TOPIC').replace(/\s+/g, '_').substring(0, 20)
    const versionStr = nextVersion.toString().padStart(2, '0')
    const fileExt = file.name.split('.').pop()
    const storedFileName = `${props.decisionBoardAbbr}${dateStr}${topicSlug}.v${versionStr}.${fileExt}`

    // TODO: Upload to API
    // const formData = new FormData()
    // formData.append('file', file)
    // formData.append('storedFileName', storedFileName)
    // const response = await api.uploadDocument(props.meetingItemId, formData)

    // Mock: Add to local array
    const newDoc = {
      id: Date.now().toString(),
      originalFileName: file.name,
      storedFileName,
      version: versionStr,
      uploadDate: new Date().toISOString(),
      fileSize: file.size
    }

    documents.value.push(newDoc)

    $q.notify({
      type: 'positive',
      message: 'Document uploaded successfully',
      caption: storedFileName
    })

    emit('documents-updated', documents.value.length)
    selectedFile.value = null

  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to upload document',
      caption: error.message
    })
  }
}

const handleDownload = async (doc) => {
  try {
    // TODO: Download from API
    // await api.downloadDocument(doc.id)
    $q.notify({
      type: 'info',
      message: `Downloading ${doc.originalFileName}...`
    })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to download document'
    })
  }
}

const handleDelete = (doc) => {
  $q.dialog({
    title: 'Confirm Delete',
    message: `Are you sure you want to delete "${doc.originalFileName}" (v${doc.version})?`,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      // TODO: Delete via API
      // await api.deleteDocument(doc.id)

      documents.value = documents.value.filter(d => d.id !== doc.id)

      $q.notify({
        type: 'positive',
        message: 'Document deleted successfully'
      })

      emit('documents-updated', documents.value.length)
    } catch (error) {
      $q.notify({
        type: 'negative',
        message: 'Failed to delete document'
      })
    }
  })
}

const getFileIcon = (fileName) => {
  const ext = fileName.split('.').pop()?.toLowerCase()
  const icons = {
    'pdf': 'picture_as_pdf',
    'doc': 'description',
    'docx': 'description',
    'xls': 'table_chart',
    'xlsx': 'table_chart',
    'ppt': 'slideshow',
    'pptx': 'slideshow'
  }
  return icons[ext] || 'insert_drive_file'
}

const getFileColor = (fileName) => {
  const ext = fileName.split('.').pop()?.toLowerCase()
  const colors = {
    'pdf': 'red-6',
    'doc': 'blue-6',
    'docx': 'blue-6',
    'xls': 'green-6',
    'xlsx': 'green-6',
    'ppt': 'orange-6',
    'pptx': 'orange-6'
  }
  return colors[ext] || 'grey-6'
}

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFileSize = (bytes) => {
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
