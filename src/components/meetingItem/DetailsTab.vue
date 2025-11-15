<template>
  <div class="form-section">
    <h6 class="section-title">{{ $t('System Information') }}</h6>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6">
        <q-input
          v-model="localMeetingItem.submissionDate"
          :label="$t('Submission Date')"
          outlined
          dense
          readonly
          :hint="$t('Auto-registered on submission')"
        >
          <template v-slot:prepend>
            <q-icon name="event" />
          </template>
        </q-input>
      </div>

      <div class="col-12 col-md-6">
        <q-select
          v-model="localMeetingItem.status"
          :label="$t('Status')"
          outlined
          dense
          :options="statusOptions"
          :disable="!canChangeStatus"
          :hint="$t('Current workflow status')"
          @update:model-value="emitUpdate"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

// Placeholder $t function
const $t = (key: string): string => key

const props = defineProps<{
  meetingItem: any
  canChangeStatus?: boolean
}>()

const emit = defineEmits<{
  'update:meetingItem': [value: any]
}>()

// Local copy for v-model
const localMeetingItem = ref({ ...props.meetingItem })

// Watch for external changes
watch(() => props.meetingItem, (newVal: any) => {
  localMeetingItem.value = { ...newVal }
}, { deep: true })

// Emit updates
const emitUpdate = (): void => {
  emit('update:meetingItem', localMeetingItem.value)
}

// Options
const statusOptions = ['Submitted', 'Proposed', 'Planned', 'Discussed', 'Denied']
</script>

<style scoped lang="scss">
.form-section {
  .section-title {
    font-size: 1rem;
    font-weight: 600;
    color: #2c3e50;
    margin: 0 0 1.25rem 0;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid #e0e0e0;
  }
}
</style>
