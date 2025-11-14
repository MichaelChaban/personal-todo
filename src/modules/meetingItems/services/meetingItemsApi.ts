import type {
  MeetingItem,
  CreateMeetingItemDto,
  UpdateMeetingItemDto,
  DocumentResponseDto
} from '../models/meetingItem'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'

class MeetingItemsApiService {
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

  async getMeetingItem(id: string): Promise<MeetingItem> {
    return this.request<MeetingItem>(`/meeting-items/${id}`)
  }

  async createMeetingItem(dto: CreateMeetingItemDto): Promise<{
    meetingItemId: string
    uploadedDocuments: DocumentResponseDto[]
  }> {
    return this.request<{
      meetingItemId: string
      uploadedDocuments: DocumentResponseDto[]
    }>('/meeting-items', {
      method: 'POST',
      body: JSON.stringify(dto),
    })
  }

  async updateMeetingItem(id: string, dto: UpdateMeetingItemDto): Promise<{
    meetingItemId: string
    newlyUploadedDocuments: DocumentResponseDto[]
    deletedDocumentIds: string[]
  }> {
    return this.request<{
      meetingItemId: string
      newlyUploadedDocuments: DocumentResponseDto[]
      deletedDocumentIds: string[]
    }>(`/meeting-items/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    })
  }
}

export const meetingItemsApi = new MeetingItemsApiService()
