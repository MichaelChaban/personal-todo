import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

// ==================== INTERFACES ====================

export interface DocumentDto {
  fileName: string
  contentType: string
  fileSize: number
  base64Content: string
}

export interface DocumentResponseDto {
  id: string
  fileName: string
  originalFileName: string
  fileSize: number
  contentType: string
  version: number
}

export interface DocumentDetail {
  id: string
  fileName: string
  originalFileName: string
  storagePath: string
  fileSize: number
  contentType: string
  version: number
  uploadDate: string
  uploadedBy: string
  isLatestVersion: boolean
}

export interface DocumentVersionDto {
  baseDocumentId: string
  document: DocumentDto
}

export interface CreateMeetingItemCommand {
  decisionBoardId: string
  templateId?: string | null
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor?: string | null
  documents?: DocumentDto[]
}

export interface UpdateMeetingItemCommand {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  ownerPresenter: string
  sponsor?: string | null
  status: string
  newDocuments?: DocumentDto[]
  documentVersions?: DocumentVersionDto[]
  documentsToDelete?: string[]
}

export interface CreateMeetingItemResponse {
  meetingItemId: string
  uploadedDocuments: DocumentResponseDto[]
}

export interface UpdateMeetingItemResponse {
  meetingItemId: string
  newlyUploadedDocuments: DocumentResponseDto[]
  versionedDocuments: DocumentResponseDto[]
  deletedDocumentIds: string[]
}

export interface MeetingItemListItem {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  submissionDate: string
  status: string
  templateId: string | null
  documentCount: number
  createdAt: string
  createdBy: string
}

export interface MeetingItemDetail {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  submissionDate: string
  status: string
  decisionBoardId: string
  templateId: string | null
  documents: DocumentDetail[]
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
}

export interface UploadDocumentCommand {
  meetingItemId: string
  fileName: string
  contentType: string
  fileSize: number
  base64Content: string
}

export interface UploadDocumentResponse {
  documentId: string
  fileName: string
  fileSize: number
  contentType: string
  uploadDate: string
  uploadedBy: string
}

// ==================== MOCK DATA ====================

const mockMeetingItems: MeetingItemListItem[] = [
  {
    id: '1',
    topic: 'Digital Transformation Strategy',
    purpose: 'Discuss the roadmap for digital transformation across all departments',
    outcome: 'Decision',
    digitalProduct: 'Enterprise Platform',
    duration: 60,
    requestor: 'user-001',
    ownerPresenter: 'user-001',
    sponsor: 'user-002',
    submissionDate: '2024-11-10T10:00:00Z',
    status: 'Submitted',
    templateId: 'template-001',
    documentCount: 3,
    createdAt: '2024-11-10T09:00:00Z',
    createdBy: 'user-001'
  },
  {
    id: '2',
    topic: 'Q4 Budget Review',
    purpose: 'Review and approve the Q4 budget allocations for all teams',
    outcome: 'Discussion',
    digitalProduct: 'Finance System',
    duration: 45,
    requestor: 'user-003',
    ownerPresenter: 'user-003',
    sponsor: null,
    submissionDate: '2024-11-12T14:00:00Z',
    status: 'UnderReview',
    templateId: null,
    documentCount: 1,
    createdAt: '2024-11-12T13:00:00Z',
    createdBy: 'user-003'
  },
  {
    id: '3',
    topic: 'Security Compliance Update',
    purpose: 'Information session on new security compliance requirements',
    outcome: 'Information',
    digitalProduct: 'Security Platform',
    duration: 30,
    requestor: 'user-004',
    ownerPresenter: 'user-004',
    sponsor: 'user-002',
    submissionDate: '2024-11-14T11:00:00Z',
    status: 'Draft',
    templateId: 'template-002',
    documentCount: 0,
    createdAt: '2024-11-14T10:30:00Z',
    createdBy: 'user-004'
  }
]

// ==================== STORE ====================

export const useMeetingItemStore = defineStore('meetingItem', () => {
  // State
  const meetingItems = ref<MeetingItemListItem[]>([])
  const currentMeetingItem = ref<MeetingItemDetail | null>(null)
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)

  // Computed
  const draftMeetingItems = computed(() =>
    meetingItems.value.filter((item) => item.status === 'Draft')
  )

  const submittedMeetingItems = computed(() =>
    meetingItems.value.filter((item) => item.status === 'Submitted')
  )

  const underReviewMeetingItems = computed(() =>
    meetingItems.value.filter((item) => item.status === 'UnderReview')
  )

  const approvedMeetingItems = computed(() =>
    meetingItems.value.filter((item) => item.status === 'Approved')
  )

  // Actions
  const getMeetingItemsByDecisionBoard = async (
    decisionBoardId: string
  ): Promise<MeetingItemListItem[]> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 500))

      // Filter mock data by decision board (for now return all)
      meetingItems.value = mockMeetingItems
      return meetingItems.value
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch meeting items'
      throw err
    } finally {
      loading.value = false
    }
  }

  const getMeetingItem = async (id: string): Promise<MeetingItemDetail> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 300))

      // Find from mock data and expand with detail
      const listItem = mockMeetingItems.find((item) => item.id === id)
      if (!listItem) {
        throw new Error('Meeting item not found')
      }

      // Create detailed version with mock documents
      const detail: MeetingItemDetail = {
        id: listItem.id,
        topic: listItem.topic,
        purpose: listItem.purpose,
        outcome: listItem.outcome,
        digitalProduct: listItem.digitalProduct,
        duration: listItem.duration,
        requestor: listItem.requestor,
        ownerPresenter: listItem.ownerPresenter,
        sponsor: listItem.sponsor,
        submissionDate: listItem.submissionDate,
        status: listItem.status,
        decisionBoardId: 'board-123',
        templateId: listItem.templateId,
        documents: [
          {
            id: 'doc-1',
            fileName: 'proposal_20241114113045_a1b2c3d4.pdf',
            originalFileName: 'Project Proposal.pdf',
            storagePath: 'meeting-items/1/proposal_20241114113045_a1b2c3d4.pdf',
            fileSize: 1024000,
            contentType: 'application/pdf',
            version: 1,
            uploadDate: '2024-11-14T11:30:00Z',
            uploadedBy: 'user-001',
            isLatestVersion: true
          },
          {
            id: 'doc-2',
            fileName: 'budget_20241114113100_b2c3d4e5.xlsx',
            originalFileName: 'Budget Analysis.xlsx',
            storagePath: 'meeting-items/1/budget_20241114113100_b2c3d4e5.xlsx',
            fileSize: 512000,
            contentType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            version: 1,
            uploadDate: '2024-11-14T11:31:00Z',
            uploadedBy: 'user-001',
            isLatestVersion: true
          }
        ],
        createdAt: listItem.createdAt,
        createdBy: listItem.createdBy,
        updatedAt: null,
        updatedBy: null
      }

      currentMeetingItem.value = detail
      return detail
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  const createMeetingItem = async (
    command: CreateMeetingItemCommand
  ): Promise<CreateMeetingItemResponse> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 800))

      const newId = `${Date.now()}`
      const now = new Date().toISOString()

      // Create mock response
      const uploadedDocs: DocumentResponseDto[] =
        command.documents?.map((doc, index) => ({
          id: `doc-${newId}-${index}`,
          fileName: `${doc.fileName.split('.')[0]}_${Date.now()}_${Math.random().toString(36).substring(7)}.${doc.fileName.split('.').pop()}`,
          originalFileName: doc.fileName,
          fileSize: doc.fileSize,
          contentType: doc.contentType,
          version: 1
        })) || []

      // Add to local state
      const newItem: MeetingItemListItem = {
        id: newId,
        topic: command.topic,
        purpose: command.purpose,
        outcome: command.outcome,
        digitalProduct: command.digitalProduct,
        duration: command.duration,
        requestor: command.requestor,
        ownerPresenter: command.ownerPresenter,
        sponsor: command.sponsor || null,
        submissionDate: now,
        status: 'Draft',
        templateId: command.templateId || null,
        documentCount: uploadedDocs.length,
        createdAt: now,
        createdBy: command.requestor
      }

      meetingItems.value.push(newItem)

      return {
        meetingItemId: newId,
        uploadedDocuments: uploadedDocs
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to create meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateMeetingItem = async (
    id: string,
    command: UpdateMeetingItemCommand
  ): Promise<UpdateMeetingItemResponse> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 800))

      // Find and update in local state
      const index = meetingItems.value.findIndex((item) => item.id === id)
      if (index === -1) {
        throw new Error('Meeting item not found')
      }

      // Process new documents
      const newDocs: DocumentResponseDto[] =
        command.newDocuments?.map((doc, i) => ({
          id: `doc-new-${id}-${Date.now()}-${i}`,
          fileName: `${doc.fileName.split('.')[0]}_${Date.now()}_${Math.random().toString(36).substring(7)}.${doc.fileName.split('.').pop()}`,
          originalFileName: doc.fileName,
          fileSize: doc.fileSize,
          contentType: doc.contentType,
          version: 1
        })) || []

      // Process versioned documents
      const versionedDocs: DocumentResponseDto[] =
        command.documentVersions?.map((docVer, i) => ({
          id: `doc-ver-${id}-${Date.now()}-${i}`,
          fileName: `${docVer.document.fileName.split('.')[0]}_${Date.now()}_${Math.random().toString(36).substring(7)}.${docVer.document.fileName.split('.').pop()}`,
          originalFileName: docVer.document.fileName,
          fileSize: docVer.document.fileSize,
          contentType: docVer.document.contentType,
          version: 2 // Simplified - would calculate actual version
        })) || []

      // Update item
      meetingItems.value[index] = {
        ...meetingItems.value[index],
        topic: command.topic,
        purpose: command.purpose,
        outcome: command.outcome,
        digitalProduct: command.digitalProduct,
        duration: command.duration,
        ownerPresenter: command.ownerPresenter,
        sponsor: command.sponsor || null,
        status: command.status,
        documentCount:
          meetingItems.value[index].documentCount +
          newDocs.length +
          versionedDocs.length -
          (command.documentsToDelete?.length || 0)
      }

      return {
        meetingItemId: id,
        newlyUploadedDocuments: newDocs,
        versionedDocuments: versionedDocs,
        deletedDocumentIds: command.documentsToDelete || []
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to update meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  const uploadDocument = async (
    command: UploadDocumentCommand
  ): Promise<UploadDocumentResponse> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 500))

      const now = new Date().toISOString()
      const docId = `doc-${Date.now()}`

      // Update document count in list
      const index = meetingItems.value.findIndex(
        (item) => item.id === command.meetingItemId
      )
      if (index !== -1) {
        meetingItems.value[index].documentCount++
      }

      return {
        documentId: docId,
        fileName: `${command.fileName.split('.')[0]}_${Date.now()}_${Math.random().toString(36).substring(7)}.${command.fileName.split('.').pop()}`,
        fileSize: command.fileSize,
        contentType: command.contentType,
        uploadDate: now,
        uploadedBy: 'current-user'
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to upload document'
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteDocument = async (documentId: string): Promise<void> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 300))

      // Would update document count in real implementation
      // For mock, just simulate success
    } catch (err: any) {
      error.value = err.message || 'Failed to delete document'
      throw err
    } finally {
      loading.value = false
    }
  }

  const downloadDocument = async (documentId: string): Promise<Blob> => {
    loading.value = true
    error.value = null
    try {
      // Mock API call - simulate delay
      await new Promise((resolve) => setTimeout(resolve, 500))

      // Create a mock blob
      const mockContent = 'Mock document content'
      return new Blob([mockContent], { type: 'application/pdf' })
    } catch (err: any) {
      error.value = err.message || 'Failed to download document'
      throw err
    } finally {
      loading.value = false
    }
  }

  const clearCurrentMeetingItem = (): void => {
    currentMeetingItem.value = null
  }

  const clearError = (): void => {
    error.value = null
  }

  return {
    // State
    meetingItems,
    currentMeetingItem,
    loading,
    error,

    // Computed
    draftMeetingItems,
    submittedMeetingItems,
    underReviewMeetingItems,
    approvedMeetingItems,

    // Actions
    getMeetingItemsByDecisionBoard,
    getMeetingItem,
    createMeetingItem,
    updateMeetingItem,
    uploadDocument,
    deleteDocument,
    downloadDocument,
    clearCurrentMeetingItem,
    clearError
  }
})
