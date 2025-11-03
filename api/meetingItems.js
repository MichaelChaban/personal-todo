// API service for meeting items
// Replace with your actual API client (axios, fetch, etc.)

const API_BASE_URL = process.env.VUE_APP_API_URL || '/api'

export const meetingItemsApi = {
  /**
   * Get a single meeting item by ID
   */
  async getMeetingItem(id) {
    const response = await fetch(`${API_BASE_URL}/meeting-items/${id}`)
    if (!response.ok) throw new Error('Failed to fetch meeting item')
    return response.json()
  },

  /**
   * Create a new meeting item
   */
  async createMeetingItem(data) {
    const response = await fetch(`${API_BASE_URL}/meeting-items`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
    if (!response.ok) throw new Error('Failed to create meeting item')
    return response.json()
  },

  /**
   * Update an existing meeting item
   */
  async updateMeetingItem(id, data) {
    const response = await fetch(`${API_BASE_URL}/meeting-items/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
    if (!response.ok) throw new Error('Failed to update meeting item')
    return response.json()
  },

  /**
   * Update meeting item status
   */
  async updateStatus(id, status) {
    const response = await fetch(`${API_BASE_URL}/meeting-items/${id}/status`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status })
    })
    if (!response.ok) throw new Error('Failed to update status')
    return response.json()
  },

  /**
   * Upload a document for a meeting item
   */
  async uploadDocument(meetingItemId, formData) {
    const response = await fetch(`${API_BASE_URL}/meeting-items/${meetingItemId}/documents`, {
      method: 'POST',
      body: formData
    })
    if (!response.ok) throw new Error('Failed to upload document')
    return response.json()
  },

  /**
   * Get documents for a meeting item
   */
  async getDocuments(meetingItemId) {
    const response = await fetch(`${API_BASE_URL}/meeting-items/${meetingItemId}/documents`)
    if (!response.ok) throw new Error('Failed to fetch documents')
    return response.json()
  },

  /**
   * Download a document
   */
  async downloadDocument(documentId) {
    const response = await fetch(`${API_BASE_URL}/documents/${documentId}/download`)
    if (!response.ok) throw new Error('Failed to download document')
    const blob = await response.blob()
    return blob
  },

  /**
   * Delete a document
   */
  async deleteDocument(documentId) {
    const response = await fetch(`${API_BASE_URL}/documents/${documentId}`, {
      method: 'DELETE'
    })
    if (!response.ok) throw new Error('Failed to delete document')
    return response.json()
  },

  /**
   * Get field definitions (for dynamic form rendering)
   */
  async getFieldDefinitions(templateId = null) {
    const url = templateId
      ? `${API_BASE_URL}/field-definitions?templateId=${templateId}`
      : `${API_BASE_URL}/field-definitions`

    const response = await fetch(url)
    if (!response.ok) throw new Error('Failed to fetch field definitions')
    return response.json()
  }
}
