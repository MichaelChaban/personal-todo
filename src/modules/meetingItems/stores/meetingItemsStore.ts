import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import {
  meetingItemsApiClient,
  type MeetingItemDto,
  type GetMeetingItemResponse,
  type CreateMeetingItemCommand,
  type UpdateMeetingItemCommand,
  type CreateMeetingItemResponse,
  type UpdateMeetingItemResponse
} from '@/api/generated/meetingItemsApi'

export const useMeetingItemsStore = defineStore('meetingItems', () => {
  // State
  const meetingItems = ref<MeetingItemDto[]>([])
  const currentMeetingItem = ref<GetMeetingItemResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const getMeetingItemsByStatus = computed(() => {
    return (status: string) => meetingItems.value.filter(item => item.status === status)
  })

  const draftMeetingItems = computed(() => {
    return meetingItems.value.filter(item => item.status === 'Draft')
  })

  const submittedMeetingItems = computed(() => {
    return meetingItems.value.filter(item => item.status === 'Submitted')
  })

  // Actions
  async function fetchMeetingItemsByDecisionBoard(decisionBoardId: string) {
    loading.value = true
    error.value = null

    try {
      const response = await meetingItemsApiClient.getMeetingItemsByDecisionBoard(decisionBoardId)
      meetingItems.value = response.meetingItems
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch meeting items'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchMeetingItem(id: string) {
    loading.value = true
    error.value = null

    try {
      const response = await meetingItemsApiClient.getMeetingItem(id)
      currentMeetingItem.value = response
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createMeetingItem(command: CreateMeetingItemCommand): Promise<CreateMeetingItemResponse> {
    loading.value = true
    error.value = null

    try {
      const response = await meetingItemsApiClient.createMeetingItem(command)

      // Refresh the list after creation
      if (command.decisionBoardId) {
        await fetchMeetingItemsByDecisionBoard(command.decisionBoardId)
      }

      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateMeetingItem(id: string, command: UpdateMeetingItemCommand): Promise<UpdateMeetingItemResponse> {
    loading.value = true
    error.value = null

    try {
      const response = await meetingItemsApiClient.updateMeetingItem(id, command)

      // Update the current item if it's loaded
      if (currentMeetingItem.value?.id === id) {
        await fetchMeetingItem(id)
      }

      // Update the list
      const index = meetingItems.value.findIndex(item => item.id === id)
      if (index !== -1) {
        const decisionBoardId = meetingItems.value[index]?.decisionBoardId
        if (decisionBoardId) {
          await fetchMeetingItemsByDecisionBoard(decisionBoardId)
        }
      }

      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update meeting item'
      throw err
    } finally {
      loading.value = false
    }
  }

  function clearCurrentMeetingItem() {
    currentMeetingItem.value = null
  }

  function clearError() {
    error.value = null
  }

  return {
    // State
    meetingItems,
    currentMeetingItem,
    loading,
    error,

    // Getters
    getMeetingItemsByStatus,
    draftMeetingItems,
    submittedMeetingItems,

    // Actions
    fetchMeetingItemsByDecisionBoard,
    fetchMeetingItem,
    createMeetingItem,
    updateMeetingItem,
    clearCurrentMeetingItem,
    clearError
  }
})
