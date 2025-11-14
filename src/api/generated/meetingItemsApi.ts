// This file is auto-generated from Swagger/OpenAPI specification
// Do not edit manually

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'

// DTOs
export interface DocumentDto {
  fileName: string
  contentType: string
  fileSize: number
  base64Content: string
}

export interface DocumentResponseDto {
  id: string
  fileName: string
  fileSize: number
  contentType: string
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

export interface CreateMeetingItemResponse {
  meetingItemId: string
  uploadedDocuments: DocumentResponseDto[]
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
  documentsToDelete?: string[]
}

export interface UpdateMeetingItemResponse {
  meetingItemId: string
  newlyUploadedDocuments: DocumentResponseDto[]
  deletedDocumentIds: string[]
}

export interface GetMeetingItemDocumentDto {
  id: string
  fileName: string
  fileSize: number
  contentType: string
  uploadDate: string
  uploadedBy: string
}

export interface GetMeetingItemResponse {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor?: string | null
  submissionDate: string
  status: string
  decisionBoardId: string
  templateId?: string | null
  documents: GetMeetingItemDocumentDto[]
  createdAt: string
  createdBy: string
}

export interface MeetingItemDto {
  id: string
  topic: string
  purpose: string
  outcome: string
  digitalProduct: string
  duration: number
  requestor: string
  ownerPresenter: string
  sponsor?: string | null
  submissionDate: string
  status: string
  templateId?: string | null
  documentCount: number
  createdAt: string
  createdBy: string
}

export interface GetMeetingItemsByDecisionBoardResponse {
  meetingItems: MeetingItemDto[]
}

export interface ErrorResponse {
  message: string
}

// API Client
export class MeetingItemsApiClient {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`

    const defaultHeaders: HeadersInit = {
      'Content-Type': 'application/json',
    }

    const config: RequestInit = {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    }

    try {
      const response = await fetch(url, config)

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'An error occurred' }))
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error('API request failed:', error)
      throw error
    }
  }

  /**
   * GET /api/meeting-items/decision-board/{decisionBoardId}
   * Get meeting items by decision board ID
   */
  async getMeetingItemsByDecisionBoard(decisionBoardId: string): Promise<GetMeetingItemsByDecisionBoardResponse> {
    return this.request<GetMeetingItemsByDecisionBoardResponse>(`/meeting-items/decision-board/${decisionBoardId}`)
  }

  /**
   * GET /api/meeting-items/{id}
   * Get a meeting item by ID
   */
  async getMeetingItem(id: string): Promise<GetMeetingItemResponse> {
    return this.request<GetMeetingItemResponse>(`/meeting-items/${id}`)
  }

  /**
   * POST /api/meeting-items
   * Create a new meeting item
   */
  async createMeetingItem(command: CreateMeetingItemCommand): Promise<CreateMeetingItemResponse> {
    return this.request<CreateMeetingItemResponse>('/meeting-items', {
      method: 'POST',
      body: JSON.stringify(command),
    })
  }

  /**
   * PUT /api/meeting-items/{id}
   * Update an existing meeting item (including document upload/deletion)
   */
  async updateMeetingItem(id: string, command: UpdateMeetingItemCommand): Promise<UpdateMeetingItemResponse> {
    return this.request<UpdateMeetingItemResponse>(`/meeting-items/${id}`, {
      method: 'PUT',
      body: JSON.stringify(command),
    })
  }
}

// Export singleton instance
export const meetingItemsApiClient = new MeetingItemsApiClient()
