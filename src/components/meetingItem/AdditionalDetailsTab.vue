<template>
  <div>
    <!-- Dynamic Template Fields -->
    <div v-if="template && template.fieldDefinitions && template.fieldDefinitions.length > 0" class="form-section">
      <h6 class="section-title">
        {{ template.templateName || 'Custom Fields' }}
        <q-chip dense size="sm" color="primary" text-color="white" class="q-ml-sm">
          {{ template.fieldDefinitions.length }} field(s)
        </q-chip>
      </h6>
      <p class="text-grey-7 text-caption q-mb-lg">
        Complete the custom fields required by {{ decisionBoardName || 'this decision board' }}
      </p>

      <div class="row q-col-gutter-md">
        <template v-for="field in template.fieldDefinitions" :key="field.fieldName">
          <!-- Text Field -->
          <div v-if="field.fieldType === 'Text'" class="col-12">
            <q-input
              v-model="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              :placeholder="field.placeholderText"
              :hint="field.helpText"
              outlined
              dense
              :type="field.isTextArea ? 'textarea' : 'text'"
              :rows="field.isTextArea ? 4 : undefined"
              :rules="field.isRequired ? [val => !!val || 'Required'] : []"
              @update:model-value="emitUpdate"
            />
          </div>

          <!-- Number Field -->
          <div v-if="field.fieldType === 'Number'" class="col-12 col-md-6">
            <q-input
              v-model.number="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              :placeholder="field.placeholderText"
              :hint="field.helpText"
              outlined
              dense
              type="number"
              :rules="field.isRequired ? [val => val !== null && val !== undefined && val !== '' || 'Required'] : []"
              @update:model-value="emitUpdate"
            />
          </div>

          <!-- Date Field -->
          <div v-if="field.fieldType === 'Date'" class="col-12 col-md-6">
            <q-input
              v-model="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              :placeholder="field.placeholderText"
              :hint="field.helpText"
              outlined
              dense
              type="date"
              :rules="field.isRequired ? [val => !!val || 'Required'] : []"
              @update:model-value="emitUpdate"
            >
              <template v-slot:prepend>
                <q-icon name="event" class="cursor-pointer" />
              </template>
            </q-input>
          </div>

          <!-- Yes/No Field -->
          <div v-if="field.fieldType === 'YesNo'" class="col-12">
            <q-checkbox
              v-model="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              color="primary"
              class="q-mt-sm"
              @update:model-value="emitUpdate"
            />
            <div v-if="field.helpText" class="text-caption text-grey-7 q-ml-lg">
              {{ field.helpText }}
            </div>
          </div>

          <!-- Dropdown Field -->
          <div v-if="field.fieldType === 'Dropdown'" class="col-12 col-md-6">
            <q-select
              v-model="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              :placeholder="field.placeholderText"
              :hint="field.helpText"
              outlined
              dense
              :options="field.options?.map(opt => ({ label: opt.label, value: opt.value })) || []"
              option-label="label"
              option-value="value"
              emit-value
              map-options
              :rules="field.isRequired ? [val => !!val || 'Required'] : []"
              @update:model-value="emitUpdate"
            />
          </div>

          <!-- Multiple Choice Field -->
          <div v-if="field.fieldType === 'MultipleChoice'" class="col-12">
            <q-select
              v-model="localFieldValues[field.fieldName]"
              :label="field.label + (field.isRequired ? ' *' : '')"
              :placeholder="field.placeholderText"
              :hint="field.helpText"
              outlined
              dense
              multiple
              :options="field.options?.map(opt => ({ label: opt.label, value: opt.value })) || []"
              option-label="label"
              option-value="value"
              emit-value
              map-options
              use-chips
              :rules="field.isRequired ? [val => (val && val.length > 0) || 'Required'] : []"
              @update:model-value="emitUpdate"
            />
          </div>
        </template>
      </div>

      <!-- Historical Fields (Read-only) -->
      <template v-if="historicalFields.length > 0">
        <q-separator class="q-my-lg" />

        <div class="historical-fields-section">
          <h6 class="section-title text-grey-7">
            Historical Fields
            <q-chip dense size="sm" color="grey-6" text-color="white" class="q-ml-sm">
              {{ historicalFields.length }} removed
            </q-chip>
          </h6>

          <q-banner rounded class="bg-amber-1 text-amber-9 q-mb-md">
            <template v-slot:avatar>
              <q-icon name="history" color="amber" />
            </template>
            These fields were removed from the template but your data is preserved for reference.
          </q-banner>

          <div class="row q-col-gutter-md">
            <div v-for="field in historicalFields" :key="field.fieldName" class="col-12 col-md-6">
              <q-input
                :model-value="field.value"
                :label="field.label + ' (Removed)'"
                outlined
                dense
                readonly
                disable
                class="historical-field"
              >
                <template v-slot:append>
                  <q-icon name="lock" color="grey-6" />
                </template>
              </q-input>
              <div v-if="field.deactivationReason" class="text-caption text-grey-6 q-mt-xs q-ml-sm">
                Removed: {{ field.deactivationReason }}
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>

    <!-- No Template Message -->
    <div v-else-if="!loading" class="form-section">
      <q-banner rounded class="bg-blue-1 text-blue-9">
        <template v-slot:avatar>
          <q-icon name="info" color="blue" />
        </template>
        No custom template configured for this decision board.
      </q-banner>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="form-section text-center q-py-lg">
      <q-spinner color="primary" size="3rem" />
      <p class="text-grey-7 q-mt-md">Loading template...</p>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  fieldValues: {
    type: Object,
    required: true
  },
  template: {
    type: Object,
    default: null
  },
  historicalFields: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  decisionBoardName: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:fieldValues'])

// Local copy for v-model
const localFieldValues = ref({ ...props.fieldValues })

// Watch for external changes
watch(() => props.fieldValues, (newVal) => {
  localFieldValues.value = { ...newVal }
}, { deep: true })

// Emit updates
const emitUpdate = () => {
  emit('update:fieldValues', localFieldValues.value)
}
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

.historical-fields-section {
  margin-top: 2rem;
}

.historical-field {
  :deep(.q-field__control) {
    background: #fafafa;
  }
}
</style>
