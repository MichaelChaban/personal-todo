<template>
  <q-page padding>
    <div class="q-pa-md">
      <!-- Header -->
      <div class="row justify-between items-center q-mb-md">
        <h4 class="q-ma-none">{{ $t('Meeting Items') }}</h4>
        <q-btn
          unelevated
          color="primary"
          :label="$t('New Meeting Item')"
          icon="add"
          @click="createMeetingItem"
        />
      </div>

      <!-- Loading State -->
      <div v-if="meetingItemStore.loading" class="text-center q-pa-lg">
        <q-spinner color="primary" size="50px" />
        <div class="q-mt-md text-grey-6">{{ $t('Loading meeting items...') }}</div>
      </div>

      <!-- Error State -->
      <q-banner
        v-else-if="meetingItemStore.error"
        class="bg-negative text-white"
      >
        <template #avatar>
          <q-icon name="error" color="white" />
        </template>
        {{ meetingItemStore.error }}
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
        <!-- Status Filter Tabs -->
        <q-tabs
          v-model="statusFilter"
          dense
          class="text-grey q-mb-md"
          active-color="primary"
          indicator-color="primary"
          align="left"
        >
          <q-tab
            name="all"
            :label="$t('All') + ` (${meetingItemStore.meetingItems.length})`"
          />
          <q-tab
            name="draft"
            :label="$t('Draft') + ` (${meetingItemStore.draftMeetingItems.length})`"
          />
          <q-tab
            name="submitted"
            :label="
              $t('Submitted') + ` (${meetingItemStore.submittedMeetingItems.length})`
            "
          />
          <q-tab
            name="underReview"
            :label="
              $t('Under Review') +
              ` (${meetingItemStore.underReviewMeetingItems.length})`
            "
          />
          <q-tab
            name="approved"
            :label="
              $t('Approved') + ` (${meetingItemStore.approvedMeetingItems.length})`
            "
          />
        </q-tabs>

        <!-- Items List -->
        <q-list bordered separator>
          <q-item
            v-for="item in filteredItems"
            :key="item.id"
            clickable
            @click="editMeetingItem(item.id)"
            class="meeting-item"
          >
            <!-- Main Content -->
            <q-item-section>
              <q-item-label class="text-weight-medium">
                {{ item.topic }}
              </q-item-label>
              <q-item-label caption lines="2">
                {{ item.purpose }}
              </q-item-label>
              <q-item-label caption class="q-mt-xs">
                <div class="row items-center q-gutter-sm">
                  <q-chip dense size="sm" outline color="primary">
                    <q-icon name="category" size="xs" class="q-mr-xs" />
                    {{ item.outcome }}
                  </q-chip>
                  <q-chip dense size="sm" outline color="info">
                    <q-icon name="devices" size="xs" class="q-mr-xs" />
                    {{ item.digitalProduct }}
                  </q-chip>
                </div>
              </q-item-label>
            </q-item-section>

            <!-- Side Info -->
            <q-item-section side class="items-end">
              <div class="column items-end q-gutter-sm">
                <!-- Status -->
                <q-chip
                  :color="getStatusColor(item.status)"
                  text-color="white"
                  dense
                  size="sm"
                >
                  {{ item.status }}
                </q-chip>

                <!-- Meta Info -->
                <div class="text-caption text-grey-6">
                  <div>
                    <q-icon name="schedule" size="xs" />
                    {{ item.duration }} {{ $t('minutes') }}
                  </div>
                  <div>
                    <q-icon name="attachment" size="xs" />
                    {{ item.documentCount }} {{ $t('documents') }}
                  </div>
                  <div>
                    <q-icon name="event" size="xs" />
                    {{ formatDate(item.submissionDate) }}
                  </div>
                </div>

                <!-- Participants -->
                <div class="text-caption text-grey-6">
                  <div>
                    <q-icon name="person" size="xs" />
                    {{ $t('Owner') }}: {{ item.ownerPresenter }}
                  </div>
                  <div v-if="item.sponsor">
                    <q-icon name="star" size="xs" />
                    {{ $t('Sponsor') }}: {{ item.sponsor }}
                  </div>
                </div>
              </div>
            </q-item-section>
          </q-item>
        </q-list>

        <!-- Empty State -->
        <div
          v-if="filteredItems.length === 0"
          class="text-center text-grey-6 q-pa-xl"
        >
          <q-icon name="inbox" size="64px" />
          <div class="q-mt-md">{{ $t('No meeting items found') }}</div>
          <div class="text-caption q-mt-sm">
            {{ $t('Click "New Meeting Item" to create one') }}
          </div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useMeetingItemStore } from '../../stores/meetingItemStore'

// Placeholder $t function
const $t = (key: string): string => key

const router = useRouter()
const route = useRoute()
const meetingItemStore = useMeetingItemStore()

// State
const statusFilter = ref<string>('all')

// Computed
const decisionBoardId = computed(() => {
  return (route.query.decisionBoardId as string) || 'board-123'
})

const filteredItems = computed(() => {
  switch (statusFilter.value) {
    case 'draft':
      return meetingItemStore.draftMeetingItems
    case 'submitted':
      return meetingItemStore.submittedMeetingItems
    case 'underReview':
      return meetingItemStore.underReviewMeetingItems
    case 'approved':
      return meetingItemStore.approvedMeetingItems
    default:
      return meetingItemStore.meetingItems
  }
})

// Lifecycle
onMounted((): void => {
  loadMeetingItems()
})

// Methods
const loadMeetingItems = async (): Promise<void> => {
  try {
    await meetingItemStore.getMeetingItemsByDecisionBoard(decisionBoardId.value)
  } catch (error) {
    console.error('Failed to load meeting items:', error)
  }
}

const createMeetingItem = (): void => {
  router.push({
    name: 'meeting-item-create',
    query: { decisionBoardId: decisionBoardId.value }
  })
}

const editMeetingItem = (id: string): void => {
  router.push({
    name: 'meeting-item-edit',
    params: { id },
    query: { decisionBoardId: decisionBoardId.value }
  })
}

const getStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    Draft: 'grey',
    Submitted: 'blue',
    UnderReview: 'orange',
    Approved: 'green',
    Rejected: 'red',
    Deferred: 'purple'
  }
  return colors[status] || 'grey'
}

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}
</script>

<style scoped lang="scss">
.meeting-item {
  padding: 1.25rem;
  transition: background-color 0.2s;

  &:hover {
    background-color: #f5f5f5;
  }
}

h4 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1a1a1a;
}
</style>
