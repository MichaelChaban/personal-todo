<template>
  <div class="documents-section">
    <div class="q-mb-md">
      <q-btn
        :label="$t('Upload Document')"
        icon="cloud_upload"
        color="primary"
        @click="showUploadDialog = true"
        :disable="readonly"
      />
    </div>

    <q-list bordered separator v-if="documents.length > 0">
      <q-item v-for="doc in documents" :key="doc.id">
        <q-item-section avatar>
          <q-icon :name="getFileIcon(doc.fileName)" color="primary" size="md" />
        </q-item-section>

        <q-item-section>
          <q-item-label>{{ doc.fileName }}</q-item-label>
          <q-item-label caption>
            {{ formatFileSize(doc.fileSize) }} • {{ $t('Uploaded') }} {{ formatDate(doc.uploadDate) }}
          </q-item-label>
        </q-item-section>

        <q-item-section side v-if="!readonly">
          <q-btn
            flat
            round
            dense
            icon="delete"
            color="negative"
            @click="confirmDelete(doc.id)"
          >
            <q-tooltip>{{ $t('Delete document') }}</q-tooltip>
          </q-btn>
        </q-item-section>
      </q-item>
    </q-list>

    <div v-else class="text-center text-grey-6 q-pa-md">
      <q-icon name="description" size="48px" />
      <div class="q-mt-sm">{{ $t('No documents uploaded') }}</div>
    </div>

    <!-- Upload Dialog -->
    <q-dialog v-model="showUploadDialog">
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ $t('Upload Document') }}</div>
        </q-card-section>

        <q-card-section>
          <q-file
            v-model="fileToUpload"
            :label="$t('Select file')"
            outlined
            dense
            :max-file-size="10485760"
            @rejected="onFileRejected"
          >
            <template #prepend>
              <q-icon name="attach_file" />
            </template>
          </q-file>

          <div v-if="fileToUpload" class="q-mt-sm text-caption text-grey-7">
            {{ $t('File size') }}: {{ formatFileSize(fileToUpload.size) }}
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="$t('Cancel')" color="grey" v-close-popup />
          <q-btn
            :label="$t('Upload')"
            color="primary"
            @click="uploadFile"
            :disable="!fileToUpload"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Delete Confirmation Dialog -->
    <q-dialog v-model="showDeleteDialog">
      <q-card>
        <q-card-section>
          <div class="text-h6">{{ $t('Confirm Delete') }}</div>
        </q-card-section>

        <q-card-section>
          {{ $t('Are you sure you want to delete this document?') }}
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="$t('Cancel')" color="grey" v-close-popup />
          <q-btn
            flat
            :label="$t('Delete')"
            color="negative"
            @click="deleteDocument"
            v-close-popup
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { formatFileSize, getFileExtension } from '../../utils/fileHelpers'
import type { GetMeetingItemDocumentDto } from '@/api/generated/meetingItemsApi'
import { QFile } from 'quasar'

// Props
defineProps<{
  documents: GetMeetingItemDocumentDto[]
  readonly?: boolean
}>()

// Emits
const emit = defineEmits<{
  'upload': [file: File]
  'delete': [documentId: string]
}>()

// Placeholder $t function
const $t = (key: string): string => key

// State
const showUploadDialog = ref(false)
const showDeleteDialog = ref(false)
const fileToUpload = ref<File | null>(null)
const documentToDelete = ref<string | null>(null)

// Methods
function getFileIcon(fileName: string): string {
  const extension = getFileExtension(fileName).toLowerCase()
  const iconMap: Record<string, string> = {
    'pdf': 'picture_as_pdf',
    'doc': 'description',
    'docx': 'description',
    'xls': 'table_chart',
    'xlsx': 'table_chart',
    'ppt': 'slideshow',
    'pptx': 'slideshow',
    'txt': 'text_snippet',
    'jpg': 'image',
    'jpeg': 'image',
    'png': 'image',
    'gif': 'image'
  }
  return iconMap[extension] || 'insert_drive_file'
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString()
}

function onFileRejected() {
  // Handle file rejection (size too large, etc.)
  alert($t('File rejected. Please ensure the file is less than 10 MB.'))
}

function uploadFile() {
  if (fileToUpload.value) {
    emit('upload', fileToUpload.value)
    fileToUpload.value = null
    showUploadDialog.value = false
  }
}

function confirmDelete(documentId: string) {
  documentToDelete.value = documentId
  showDeleteDialog.value = true
}

function deleteDocument() {
  if (documentToDelete.value) {
    emit('delete', documentToDelete.value)
    documentToDelete.value = null
  }
}
</script>
