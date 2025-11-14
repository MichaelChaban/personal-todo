<template>
  <div>
    <div class="form-section">
      <h6 class="section-title">Basic Information</h6>

      <div class="row q-col-gutter-md">
        <div class="col-12">
          <q-input
            v-model="localMeetingItem.topic"
            label="Topic *"
            hint="Short title for the discussion"
            outlined
            dense
            :rules="[val => !!val || 'Topic is required']"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12">
          <q-input
            v-model="localMeetingItem.purpose"
            label="Purpose *"
            hint="Full explanation of the discussion and requested decision"
            outlined
            dense
            type="textarea"
            rows="4"
            :rules="[val => !!val || 'Purpose is required']"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12 col-md-6">
          <q-select
            v-model="localMeetingItem.outcome"
            label="Outcome *"
            hint="Expected outcome type"
            outlined
            dense
            :options="outcomeOptions"
            :rules="[val => !!val || 'Outcome is required']"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12 col-md-6">
          <q-input
            v-model.number="localMeetingItem.duration"
            label="Duration (minutes) *"
            hint="Estimated discussion time"
            outlined
            dense
            type="number"
            min="1"
            :rules="[
              val => !!val || 'Duration is required',
              val => val > 0 || 'Duration must be positive'
            ]"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12">
          <q-input
            v-model="localMeetingItem.digitalProduct"
            label="Digital Product *"
            hint="Related digital product or service"
            outlined
            dense
            :rules="[val => !!val || 'Digital Product is required']"
            @update:model-value="emitUpdate"
          />
        </div>
      </div>
    </div>

    <q-separator class="q-my-lg" />

    <div class="form-section">
      <h6 class="section-title">Participants</h6>

      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-4">
          <q-input
            v-model="localMeetingItem.requestor"
            label="Requestor *"
            hint="KBC ID (auto-filled)"
            outlined
            dense
            readonly
            :rules="[val => !!val || 'Requestor is required']"
          >
            <template v-slot:prepend>
              <q-icon name="person" />
            </template>
          </q-input>
        </div>

        <div class="col-12 col-md-4">
          <q-input
            v-model="localMeetingItem.ownerPresenter"
            label="Owner/Presenter *"
            hint="KBC ID (defaults to Requestor)"
            outlined
            dense
            :rules="[val => !!val || 'Owner/Presenter is required']"
            @update:model-value="emitUpdate"
          >
            <template v-slot:prepend>
              <q-icon name="record_voice_over" />
            </template>
          </q-input>
        </div>

        <div class="col-12 col-md-4">
          <q-input
            v-model="localMeetingItem.sponsor"
            label="Sponsor"
            hint="KBC ID (optional)"
            outlined
            dense
            @update:model-value="emitUpdate"
          >
            <template v-slot:prepend>
              <q-icon name="supervisor_account" />
            </template>
          </q-input>
        </div>
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
const outcomeOptions = ['Decision', 'Discussion', 'Information']
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
