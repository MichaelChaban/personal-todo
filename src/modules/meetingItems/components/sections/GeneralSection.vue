<template>
  <div class="general-section">
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-input
          :model-value="topic"
          @update:model-value="$emit('update:topic', $event)"
          :label="$t('Topic') + ' *'"
          :hint="$t('Short title for the discussion')"
          outlined
          dense
          :rules="[(val: string) => !!val || $t('Topic is required'), (val: string) => val.length <= 200 || $t('Topic must not exceed 200 characters')]"
        />
      </div>

      <div class="col-12">
        <q-input
          :model-value="purpose"
          @update:model-value="$emit('update:purpose', $event)"
          :label="$t('Purpose') + ' *'"
          :hint="$t('Detailed explanation of the meeting item purpose')"
          type="textarea"
          outlined
          dense
          rows="4"
          :rules="[
            (val: string) => !!val || $t('Purpose is required'),
            (val: string) => val.length >= 10 || $t('Purpose must be at least 10 characters'),
            (val: string) => val.length <= 2000 || $t('Purpose must not exceed 2000 characters')
          ]"
        />
      </div>

      <div class="col-12 col-md-6">
        <q-select
          :model-value="outcome"
          @update:model-value="$emit('update:outcome', $event)"
          :label="$t('Outcome') + ' *'"
          :hint="$t('Expected outcome of the discussion')"
          :options="outcomeOptions"
          outlined
          dense
          emit-value
          map-options
          :rules="[(val: string) => !!val || $t('Outcome is required')]"
        />
      </div>

      <div class="col-12 col-md-6">
        <q-input
          :model-value="duration"
          @update:model-value="$emit('update:duration', $event as number)"
          :label="$t('Duration (minutes)') + ' *'"
          :hint="$t('Estimated duration in minutes')"
          type="number"
          outlined
          dense
          :rules="[
            (val: number) => !!val || $t('Duration is required'),
            (val: number) => val > 0 || $t('Duration must be greater than 0'),
            (val: number) => val <= 480 || $t('Duration must not exceed 480 minutes')
          ]"
        />
      </div>

      <div class="col-12">
        <q-input
          :model-value="digitalProduct"
          @update:model-value="$emit('update:digitalProduct', $event)"
          :label="$t('Digital Product') + ' *'"
          :hint="$t('The product or system being discussed')"
          outlined
          dense
          :rules="[(val: string) => !!val || $t('Digital Product is required'), (val: string) => val.length <= 200 || $t('Digital Product must not exceed 200 characters')]"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Props
defineProps<{
  topic: string
  purpose: string
  outcome: string
  duration: number
  digitalProduct: string
}>()

// Emits
defineEmits<{
  'update:topic': [value: string]
  'update:purpose': [value: string]
  'update:outcome': [value: string]
  'update:duration': [value: number]
  'update:digitalProduct': [value: string]
}>()

// Placeholder $t function
const $t = (key: string): string => key

// Options
const outcomeOptions = [
  { label: $t('Decision'), value: 'Decision' },
  { label: $t('Discussion'), value: 'Discussion' },
  { label: $t('Information'), value: 'Information' }
]
</script>
