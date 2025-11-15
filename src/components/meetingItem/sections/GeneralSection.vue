<template>
  <div class="general-section">
    <div class="row q-col-gutter-md">
      <!-- Topic -->
      <div class="col-12">
        <Field name="topic" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="topic"
            @update:model-value="$emit('update:topic', $event)"
            :label="$t('Topic') + ' *'"
            :hint="$t('Short title for the discussion')"
            outlined
            dense
            counter
            maxlength="200"
            :error="!!errorMessage"
            :error-message="errorMessage"
          />
        </Field>
      </div>

      <!-- Purpose -->
      <div class="col-12">
        <Field name="purpose" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="purpose"
            @update:model-value="$emit('update:purpose', $event)"
            :label="$t('Purpose') + ' *'"
            :hint="$t('Detailed explanation of the meeting item purpose')"
            type="textarea"
            outlined
            dense
            counter
            rows="4"
            maxlength="2000"
            :error="!!errorMessage"
            :error-message="errorMessage"
          />
        </Field>
      </div>

      <!-- Outcome -->
      <div class="col-12 col-md-6">
        <Field name="outcome" v-slot="{ field, errorMessage }">
          <q-select
            v-bind="field"
            :model-value="outcome"
            @update:model-value="$emit('update:outcome', $event)"
            :label="$t('Outcome') + ' *'"
            :hint="$t('Expected outcome of the discussion')"
            :options="outcomeOptions"
            outlined
            dense
            emit-value
            map-options
            :error="!!errorMessage"
            :error-message="errorMessage"
          />
        </Field>
      </div>

      <!-- Duration -->
      <div class="col-12 col-md-6">
        <Field name="duration" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="duration"
            @update:model-value="$emit('update:duration', $event !== null ? Number($event) : null)"
            :label="$t('Duration (minutes)') + ' *'"
            :hint="$t('Estimated duration in minutes')"
            type="number"
            outlined
            dense
            min="1"
            max="480"
            :error="!!errorMessage"
            :error-message="errorMessage"
          />
        </Field>
      </div>

      <!-- Digital Product -->
      <div class="col-12">
        <Field name="digitalProduct" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="digitalProduct"
            @update:model-value="$emit('update:digitalProduct', $event)"
            :label="$t('Digital Product') + ' *'"
            :hint="$t('The product or system being discussed')"
            outlined
            dense
            counter
            maxlength="200"
            :error="!!errorMessage"
            :error-message="errorMessage"
          />
        </Field>
      </div>

      <!-- Requestor -->
      <div class="col-12 col-md-6">
        <Field name="requestor" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="requestor"
            @update:model-value="$emit('update:requestor', $event)"
            :label="$t('Requestor') + ' *'"
            :hint="$t('Person requesting this meeting item')"
            outlined
            dense
            readonly
            :error="!!errorMessage"
            :error-message="errorMessage"
          >
            <template v-slot:prepend>
              <q-icon name="person" />
            </template>
          </q-input>
        </Field>
      </div>

      <!-- Owner/Presenter -->
      <div class="col-12 col-md-6">
        <Field name="ownerPresenter" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="ownerPresenter"
            @update:model-value="$emit('update:ownerPresenter', $event)"
            :label="$t('Owner/Presenter') + ' *'"
            :hint="$t('Person who will present this item')"
            outlined
            dense
            maxlength="50"
            :error="!!errorMessage"
            :error-message="errorMessage"
          >
            <template v-slot:prepend>
              <q-icon name="account_circle" />
            </template>
          </q-input>
        </Field>
      </div>

      <!-- Sponsor -->
      <div class="col-12 col-md-6">
        <Field name="sponsor" v-slot="{ field, errorMessage }">
          <q-input
            v-bind="field"
            :model-value="sponsor"
            @update:model-value="$emit('update:sponsor', $event)"
            :label="$t('Sponsor')"
            :hint="$t('Optional sponsor for this item')"
            outlined
            dense
            maxlength="50"
            clearable
            :error="!!errorMessage"
            :error-message="errorMessage"
          >
            <template v-slot:prepend>
              <q-icon name="star" />
            </template>
          </q-input>
        </Field>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Field } from 'vee-validate'

// Props
defineProps<{
  topic: string
  purpose: string
  outcome: string
  duration: number | null
  digitalProduct: string
  requestor: string
  ownerPresenter: string
  sponsor: string | null
}>()

// Emits
defineEmits<{
  'update:topic': [value: string]
  'update:purpose': [value: string]
  'update:outcome': [value: string]
  'update:duration': [value: number | null]
  'update:digitalProduct': [value: string]
  'update:requestor': [value: string]
  'update:ownerPresenter': [value: string]
  'update:sponsor': [value: string | null]
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

<style scoped lang="scss">
.general-section {
  padding: 0.5rem 0;
}
</style>
