<template>
  <div class="details-section">
    <div class="row q-col-gutter-md">
      <!-- Submission Date -->
      <div class="col-12 col-md-6">
        <q-input
          :model-value="submissionDate ? formatDate(submissionDate) : ''"
          :label="$t('Submission Date')"
          :hint="$t('Auto-registered on submission')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="event" />
          </template>
        </q-input>
      </div>

      <!-- Status -->
      <div class="col-12 col-md-6">
        <q-select
          :model-value="status"
          @update:model-value="$emit('update:status', $event)"
          :label="$t('Status') + ' *'"
          :hint="$t('Current workflow status')"
          :options="statusOptions"
          outlined
          dense
          emit-value
          map-options
          :disable="!canChangeStatus"
          :rules="[(val: string) => !!val || $t('Status is required')]"
        >
          <template v-slot:prepend>
            <q-icon name="flag" />
          </template>
        </q-select>
      </div>

      <!-- Decision Board ID -->
      <div class="col-12 col-md-6">
        <q-input
          :model-value="decisionBoardId"
          :label="$t('Decision Board')"
          :hint="$t('Associated decision board')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="dashboard" />
          </template>
        </q-input>
      </div>

      <!-- Template ID -->
      <div class="col-12 col-md-6">
        <q-input
          :model-value="templateId || $t('No template')"
          :label="$t('Template')"
          :hint="$t('Template used for this item')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="description" />
          </template>
        </q-input>
      </div>

      <!-- Created At -->
      <div class="col-12 col-md-6">
        <q-input
          :model-value="createdAt ? formatDate(createdAt) : ''"
          :label="$t('Created At')"
          :hint="$t('Item creation timestamp')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="schedule" />
          </template>
        </q-input>
      </div>

      <!-- Created By -->
      <div class="col-12 col-md-6">
        <q-input
          :model-value="createdBy"
          :label="$t('Created By')"
          :hint="$t('User who created this item')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="person" />
          </template>
        </q-input>
      </div>

      <!-- Updated At (if available) -->
      <div v-if="updatedAt" class="col-12 col-md-6">
        <q-input
          :model-value="formatDate(updatedAt)"
          :label="$t('Last Updated At')"
          :hint="$t('Last update timestamp')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="update" />
          </template>
        </q-input>
      </div>

      <!-- Updated By (if available) -->
      <div v-if="updatedBy" class="col-12 col-md-6">
        <q-input
          :model-value="updatedBy"
          :label="$t('Last Updated By')"
          :hint="$t('User who last updated this item')"
          outlined
          dense
          readonly
        >
          <template v-slot:prepend>
            <q-icon name="person_outline" />
          </template>
        </q-input>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Props
defineProps<{
  submissionDate: string | null
  status: string
  decisionBoardId: string
  templateId: string | null
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  canChangeStatus?: boolean
}>()

// Emits
defineEmits<{
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

// Methods
const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped lang="scss">
.details-section {
  padding: 0.5rem 0;
}
</style>
