<template>
  <div class="form-section">
    <h6 class="section-title">System Information</h6>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6">
        <q-input
          v-model="localMeetingItem.submissionDate"
          label="Submission Date"
          outlined
          dense
          readonly
          hint="Auto-registered on submission"
        >
          <template v-slot:prepend>
            <q-icon name="event" />
          </template>
        </q-input>
      </div>

      <div class="col-12 col-md-6">
        <q-select
          v-model="localMeetingItem.status"
          label="Status"
          outlined
          dense
          :options="statusOptions"
          :disable="!canChangeStatus"
          hint="Current workflow status"
          @update:model-value="emitUpdate"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  meetingItem: {
    type: Object,
    required: true
  },
  canChangeStatus: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:meetingItem'])

// Local copy for v-model
const localMeetingItem = ref({ ...props.meetingItem })

// Watch for external changes
watch(() => props.meetingItem, (newVal) => {
  localMeetingItem.value = { ...newVal }
}, { deep: true })

// Emit updates
const emitUpdate = () => {
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
