<template>
  <q-page padding>
    <div class="q-pa-md">
      <div class="row justify-between items-center q-mb-md">
        <h4 class="q-ma-none">{{ $t('Meeting Items') }}</h4>
        <q-btn
          color="primary"
          :label="$t('New Meeting Item')"
          icon="add"
          @click="showCreateDialog = true"
        />
      </div>

      <!-- Loading State -->
      <div v-if="meetingItemsStore.loading" class="text-center q-pa-lg">
        <q-spinner color="primary" size="50px" />
        <div class="q-mt-md text-grey-6">{{ $t('Loading meeting items...') }}</div>
      </div>

      <!-- Error State -->
      <q-banner v-else-if="meetingItemsStore.error" class="bg-negative text-white">
        <template #avatar>
          <q-icon name="error" color="white" />
        </template>
        {{ meetingItemsStore.error }}
        <template #action>
          <q-btn
            flat
            color="white"
            :label="$t('Retry')"
            @click="loadMeetingItems"
          />
        </template>
      </q-banner>

      <!-- Meeting Items List -->
      <div v-else>
        <q-tabs
          v-model="statusFilter"
          dense
          class="text-grey q-mb-md"
          active-color="primary"
          indicator-color="primary"
          align="left"
        >
          <q-tab name="all" :label="$t('All') + ` (${meetingItemsStore.meetingItems.length})`" />
          <q-tab name="draft" :label="$t('Draft') + ` (${meetingItemsStore.draftMeetingItems.length})`" />
          <q-tab name="submitted" :label="$t('Submitted') + ` (${meetingItemsStore.submittedMeetingItems.length})`" />
        </q-tabs>

        <q-list bordered separator>
          <q-item
            v-for="item in filteredItems"
            :key="item.id"
            clickable
            @click="editMeetingItem(item.id)"
          >
            <q-item-section>
              <q-item-label>{{ item.topic }}</q-item-label>
              <q-item-label caption lines="2">{{ item.purpose }}</q-item-label>
            </q-item-section>

            <q-item-section side>
              <div class="column items-end">
                <q-chip
                  :color="getStatusColor(item.status)"
                  text-color="white"
                  dense
                  size="sm"
                >
                  {{ item.status }}
                </q-chip>
                <div class="text-caption text-grey-6 q-mt-xs">
                  {{ item.duration }} {{ $t('minutes') }} • {{ item.documentCount }} {{ $t('docs') }}
                </div>
              </div>
            </q-item-section>
          </q-item>
        </q-list>

        <div v-if="filteredItems.length === 0" class="text-center text-grey-6 q-pa-xl">
          <q-icon name="inbox" size="64px" />
          <div class="q-mt-md">{{ $t('No meeting items found') }}</div>
        </div>
      </div>
    </div>

    <!-- Create Dialog -->
    <q-dialog v-model="showCreateDialog" persistent>
      <MeetingItemFormRefactored
        :decision-board-id="decisionBoardId"
        @submit="handleCreate"
        @cancel="showCreateDialog = false"
      />
    </q-dialog>

    <!-- Edit Dialog -->
    <q-dialog v-model="showEditDialog" persistent>
      <MeetingItemFormRefactored
        v-if="meetingItemsStore.currentMeetingItem"
        :meeting-item="meetingItemsStore.currentMeetingItem"
        :decision-board-id="decisionBoardId"
        :can-change-status="true"
        @submit="handleUpdate"
        @cancel="closeEditDialog"
      />
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useMeetingItemsStore } from '../stores/meetingItemsStore'
import MeetingItemFormRefactored from '../components/MeetingItemFormRefactored.vue'
import type { CreateMeetingItemCommand, UpdateMeetingItemCommand } from '@/api/generated/meetingItemsApi'

// Props
const props = defineProps<{
  decisionBoardId: string
}>()

// Placeholder $t function
const $t = (key: string): string => key

// Store
const meetingItemsStore = useMeetingItemsStore()

// State
const statusFilter = ref<string>('all')
const showCreateDialog = ref(false)
const showEditDialog = ref(false)

// Computed
const filteredItems = computed(() => {
  switch (statusFilter.value) {
    case 'draft':
      return meetingItemsStore.draftMeetingItems
    case 'submitted':
      return meetingItemsStore.submittedMeetingItems
    default:
      return meetingItemsStore.meetingItems
  }
})

// Methods
async function loadMeetingItems() {
  try {
    await meetingItemsStore.fetchMeetingItemsByDecisionBoard(props.decisionBoardId)
  } catch (error) {
    console.error('Failed to load meeting items:', error)
  }
}

async function handleCreate(data: CreateMeetingItemCommand) {
  try {
    await meetingItemsStore.createMeetingItem(data)
    showCreateDialog.value = false
  } catch (error) {
    console.error('Failed to create meeting item:', error)
  }
}

async function editMeetingItem(id: string) {
  try {
    await meetingItemsStore.fetchMeetingItem(id)
    showEditDialog.value = true
  } catch (error) {
    console.error('Failed to load meeting item:', error)
  }
}

async function handleUpdate(data: UpdateMeetingItemCommand) {
  try {
    if (meetingItemsStore.currentMeetingItem?.id) {
      await meetingItemsStore.updateMeetingItem(meetingItemsStore.currentMeetingItem.id, data)
      showEditDialog.value = false
    }
  } catch (error) {
    console.error('Failed to update meeting item:', error)
  }
}

function closeEditDialog(): void {
  showEditDialog.value = false
  meetingItemsStore.clearCurrentMeetingItem()
}

function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    'Draft': 'grey',
    'Submitted': 'blue',
    'UnderReview': 'orange',
    'Approved': 'green',
    'Rejected': 'red',
    'Deferred': 'purple'
  }
  return colors[status] || 'grey'
}

// Lifecycle
onMounted(() => {
  loadMeetingItems()
})
</script>
