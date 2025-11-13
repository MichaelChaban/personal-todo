// API service for meeting items
// Matches the backend MediatR handlers and controller endpoints

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

/**
 * Create fetch wrapper with error handling
 */
async function fetchWithErrorHandling(url, options = {}) {
  const defaultHeaders = {
    'Content-Type': 'application/json'
  }

  const config = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers
    }
  }

  const response = await fetch(url, config)

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Request failed' }))
    throw new Error(error.message || `HTTP ${response.status}`)
  }

  return response.json()
}

export const meetingItemsApi = {
  /**
   * Get template by decision board ID
   * GET /api/MeetingItems/template/{decisionBoardId}
   */
  async getTemplateByDecisionBoard(decisionBoardId) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems/template/${decisionBoardId}`
    )
  },

  /**
   * Get meeting item by ID
   * GET /api/MeetingItems/{id}
   * Returns: { activeFields, historicalFields, documents, ...staticFields }
   */
  async getMeetingItem(id) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems/${id}`
    )
  },

  /**
   * Create meeting item
   * POST /api/MeetingItems
   */
  async createMeetingItem(data) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems`,
      {
        method: 'POST',
        body: JSON.stringify({
          decisionBoardId: data.decisionBoardId,
          templateId: data.templateId,
          topic: data.topic,
          purpose: data.purpose,
          outcome: data.outcome,
          digitalProduct: data.digitalProduct,
          durationMinutes: data.durationMinutes,
          ownerPresenterUserId: data.ownerPresenterUserId,
          sponsorUserId: data.sponsorUserId,
          fieldValues: data.fieldValues?.map(fv => ({
            fieldName: fv.fieldName,
            textValue: fv.textValue || null,
            numberValue: fv.numberValue || null,
            dateValue: fv.dateValue || null,
            booleanValue: fv.booleanValue || null,
            jsonValue: fv.jsonValue || null
          })),
          documents: data.documents?.map(doc => ({
            fileName: doc.fileName,
            contentType: doc.contentType,
            base64Content: doc.base64Content
          }))
        })
      }
    )
  },

  /**
   * Update meeting item
   * PUT /api/MeetingItems/{id}
   */
  async updateMeetingItem(id, data) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems/${id}`,
      {
        method: 'PUT',
        body: JSON.stringify({
          meetingItemId: id,
          topic: data.topic,
          purpose: data.purpose,
          outcome: data.outcome,
          digitalProduct: data.digitalProduct,
          durationMinutes: data.durationMinutes,
          ownerPresenterUserId: data.ownerPresenterUserId,
          sponsorUserId: data.sponsorUserId,
          fieldValues: data.fieldValues?.map(fv => ({
            fieldName: fv.fieldName,
            textValue: fv.textValue || null,
            numberValue: fv.numberValue || null,
            dateValue: fv.dateValue || null,
            booleanValue: fv.booleanValue || null,
            jsonValue: fv.jsonValue || null
          }))
        })
      }
    )
  },

  /**
   * Update meeting item status
   * PATCH /api/MeetingItems/{id}/status
   */
  async updateStatus(id, newStatus, comment = null, denialReason = null) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems/${id}/status`,
      {
        method: 'PATCH',
        body: JSON.stringify({
          meetingItemId: id,
          newStatus,
          comment,
          denialReason
        })
      }
    )
  },

  /**
   * Upload document
   * POST /api/MeetingItems/{id}/documents
   */
  async uploadDocument(meetingItemId, document) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/MeetingItems/${meetingItemId}/documents`,
      {
        method: 'POST',
        body: JSON.stringify({
          meetingItemId,
          document: {
            fileName: document.fileName,
            contentType: document.contentType,
            base64Content: document.base64Content
          }
        })
      }
    )
  },

  /**
   * Download document (blob)
   */
  async downloadDocument(documentId) {
    const response = await fetch(`${API_BASE_URL}/Documents/${documentId}/download`)
    if (!response.ok) throw new Error('Failed to download document')
    return response.blob()
  },

  /**
   * Delete document
   */
  async deleteDocument(documentId) {
    return fetchWithErrorHandling(
      `${API_BASE_URL}/Documents/${documentId}`,
      {
        method: 'DELETE'
      }
    )
  }
}
