// Enums
export enum MeetingItemStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  UnderReview = 'UnderReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Deferred = 'Deferred'
}

export enum MeetingItemOutcome {
  Decision = 'Decision',
  Discussion = 'Discussion',
  Information = 'Information'
}

// Interfaces
export interface MeetingItem {
  id: string | null
  topic: string
  purpose: string
  outcome: MeetingItemOutcome
  duration: number
  digitalProduct: string
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  submissionDate: string
  status: MeetingItemStatus
  decisionBoardId: string
  templateId: string | null
  documents: Document[]
  createdAt: string
  createdBy: string
}

export interface Document {
  id: string
  fileName: string
  fileSize: number
  contentType: string
  uploadDate: string
  uploadedBy: string
}

// DTOs for API requests
export interface DocumentDto {
  fileName: string
  contentType: string
  fileSize: number
  base64Content: string
}

export interface CreateMeetingItemDto {
  decisionBoardId: string
  templateId: string | null
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  documents?: DocumentDto[]
}

export interface UpdateMeetingItemDto {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  ownerPresenter: string
  sponsor: string | null
  status: string
  newDocuments?: DocumentDto[]
  documentsToDelete?: string[]
}

export interface DocumentResponseDto {
  id: string
  fileName: string
  fileSize: number
  contentType: string
}

// Factory functions
export function createEmptyMeetingItem(): MeetingItem {
  return {
    id: null,
    topic: '',
    purpose: '',
    outcome: MeetingItemOutcome.Decision,
    duration: 30,
    digitalProduct: '',
    requestor: '',
    ownerPresenter: '',
    sponsor: null,
    submissionDate: new Date().toISOString(),
    status: MeetingItemStatus.Draft,
    decisionBoardId: '',
    templateId: null,
    documents: [],
    createdAt: new Date().toISOString(),
    createdBy: ''
  }
}
