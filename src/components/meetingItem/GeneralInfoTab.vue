<template>
  <div>
    <div class="form-section">
      <h6 class="section-title">{{ $t('Basic Information') }}</h6>

      <div class="row q-col-gutter-md">
        <div class="col-12">
          <q-input
            v-model="localMeetingItem.topic"
            :label="$t('Topic') + ' *'"
            :hint="$t('Short title for the discussion')"
            outlined
            dense
            :rules="[(val: string) => !!val || $t('Topic is required')]"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12">
          <q-input
            v-model="localMeetingItem.purpose"
            :label="$t('Purpose') + ' *'"
            :hint="$t('Full explanation of the discussion and requested decision')"
            outlined
            dense
            type="textarea"
            rows="4"
            :rules="[(val: string) => !!val || $t('Purpose is required')]"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12 col-md-6">
          <q-select
            v-model="localMeetingItem.outcome"
            :label="$t('Outcome') + ' *'"
            :hint="$t('Expected outcome type')"
            outlined
            dense
            :options="outcomeOptions"
            :rules="[(val: string) => !!val || $t('Outcome is required')]"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12 col-md-6">
          <q-input
            v-model.number="localMeetingItem.duration"
            :label="$t('Duration (minutes)') + ' *'"
            :hint="$t('Estimated discussion time')"
            outlined
            dense
            type="number"
            min="1"
            :rules="[
              (val: number) => !!val || $t('Duration is required'),
              (val: number) => val > 0 || $t('Duration must be positive')
            ]"
            @update:model-value="emitUpdate"
          />
        </div>

        <div class="col-12">
          <q-input
            v-model="localMeetingItem.digitalProduct"
            :label="$t('Digital Product') + ' *'"
            :hint="$t('Related digital product or service')"
            outlined
            dense
            :rules="[(val: string) => !!val || $t('Digital Product is required')]"
            @update:model-value="emitUpdate"
          />
        </div>
      </div>
    </div>

    <q-separator class="q-my-lg" />

    <div class="form-section">
      <h6 class="section-title">{{ $t('Participants') }}</h6>

      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-4">
          <q-input
            v-model="localMeetingItem.requestor"
            :label="$t('Requestor') + ' *'"
            :hint="$t('KBC ID (auto-filled)')"
            outlined
            dense
            readonly
            :rules="[(val: string) => !!val || $t('Requestor is required')]"
          >
            <template v-slot:prepend>
              <q-icon name="person" />
            </template>
          </q-input>
        </div>

        <div class="col-12 col-md-4">
          <q-input
            v-model="localMeetingItem.ownerPresenter"
            :label="$t('Owner/Presenter') + ' *'"
            :hint="$t('KBC ID (defaults to Requestor)')"
            outlined
            dense
            :rules="[(val: string) => !!val || $t('Owner/Presenter is required')]"
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
            :label="$t('Sponsor')"
            :hint="$t('KBC ID (optional)')"
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

<script setup lang="ts">
import { ref, watch } from 'vue'

// Placeholder $t function
const $t = (key: string): string => key

const props = defineProps<{
  meetingItem: any
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
