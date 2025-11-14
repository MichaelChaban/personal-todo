<template>
  <div class="details-section">
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6">
        <q-input
          :model-value="requestor"
          @update:model-value="$emit('update:requestor', $event)"
          :label="$t('Requestor') + ' *'"
          :hint="$t('Person requesting the meeting item')"
          outlined
          dense
          :readonly="!canEditRequestor"
          :rules="[(val: string) => !!val || $t('Requestor is required'), (val: string) => val.length <= 50 || $t('Requestor must not exceed 50 characters')]"
        />
      </div>

      <div class="col-12 col-md-6">
        <q-input
          :model-value="ownerPresenter"
          @update:model-value="$emit('update:ownerPresenter', $event)"
          :label="$t('Owner/Presenter') + ' *'"
          :hint="$t('Person responsible for presenting')"
          outlined
          dense
          :rules="[(val: string) => !!val || $t('Owner/Presenter is required'), (val: string) => val.length <= 50 || $t('Owner/Presenter must not exceed 50 characters')]"
        />
      </div>

      <div class="col-12 col-md-6">
        <q-input
          :model-value="sponsor"
          @update:model-value="$emit('update:sponsor', $event)"
          :label="$t('Sponsor')"
          :hint="$t('Optional sponsor for the meeting item')"
          outlined
          dense
          clearable
          :rules="[(val: string) => !val || val.length <= 50 || $t('Sponsor must not exceed 50 characters')]"
        />
      </div>

      <div v-if="canChangeStatus" class="col-12 col-md-6">
        <q-select
          :model-value="status"
          @update:model-value="$emit('update:status', $event)"
          :label="$t('Status') + ' *'"
          :hint="$t('Current status of the meeting item')"
          :options="statusOptions"
          outlined
          dense
          emit-value
          map-options
          :rules="[(val: string) => !!val || $t('Status is required')]"
        />
      </div>

      <div v-else class="col-12 col-md-6">
        <q-input
          :model-value="statusLabel"
          :label="$t('Status')"
          outlined
          dense
          readonly
        >
          <template #prepend>
            <q-icon :name="statusIcon" :color="statusColor" />
          </template>
        </q-input>
      </div>

      <div class="col-12">
        <q-input
          :model-value="submissionDate"
          :label="$t('Submission Date')"
          outlined
          dense
          readonly
        >
          <template #prepend>
            <q-icon name="event" />
          </template>
        </q-input>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props
const props = defineProps<{
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  status: string
  submissionDate: string
  canEditRequestor?: boolean
  canChangeStatus?: boolean
}>()

// Emits
defineEmits<{
  'update:requestor': [value: string]
  'update:ownerPresenter': [value: string]
  'update:sponsor': [value: string | null]
  'update:status': [value: string]
}>()

// Placeholder $t function
const $t = (key: string): string => key

// Options
const statusOptions = [
  { label: $t('Draft'), value: 'Draft' },
  { label: $t('Submitted'), value: 'Submitted' },
  { label: $t('Under Review'), value: 'UnderReview' },
  { label: $t('Approved'), value: 'Approved' },
  { label: $t('Rejected'), value: 'Rejected' },
  { label: $t('Deferred'), value: 'Deferred' }
]

// Computed
const statusLabel = computed(() => {
  const option = statusOptions.find(opt => opt.value === props.status)
  return option ? option.label : props.status
})

const statusIcon = computed(() => {
  const icons: Record<string, string> = {
    'Draft': 'edit',
    'Submitted': 'send',
    'UnderReview': 'rate_review',
    'Approved': 'check_circle',
    'Rejected': 'cancel',
    'Deferred': 'schedule'
  }
  return icons[props.status] || 'info'
})

const statusColor = computed(() => {
  const colors: Record<string, string> = {
    'Draft': 'grey',
    'Submitted': 'blue',
    'UnderReview': 'orange',
    'Approved': 'green',
    'Rejected': 'red',
    'Deferred': 'purple'
  }
  return colors[props.status] || 'grey'
})
</script>
