<template>
  <div>
    <q-card flat bordered class="meeting-card">
      <q-tabs
        v-model="activeTab"
        dense
        class="text-grey-7"
        active-color="primary"
        indicator-color="primary"
        align="left"
        narrow-indicator
      >
        <q-tab name="general" :label="$t('General')" icon="info" />
        <q-tab name="details" :label="$t('Details')" icon="description" />
        <q-tab
          name="additional"
          :label="$t('Additional Details')"
          icon="dashboard_customize"
          :badge="template?.fieldDefinitions?.length || undefined"
        />
        <q-tab
          name="documents"
          :label="$t('Documents')"
          icon="attachment"
          :badge="documentCount || undefined"
        />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="activeTab" animated>
        <!-- General Tab -->
        <q-tab-panel name="general" class="q-pa-lg">
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
                  :rules="[val => !!val || $t('Topic is required')]"
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
                  :rules="[val => !!val || $t('Purpose is required')]"
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
                  :rules="[val => !!val || $t('Outcome is required')]"
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
                    val => !!val || $t('Duration is required'),
                    val => val > 0 || $t('Duration must be positive')
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
                  :rules="[val => !!val || $t('Digital Product is required')]"
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
                  :rules="[val => !!val || $t('Requestor is required')]"
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
                  :rules="[val => !!val || $t('Owner/Presenter is required')]"
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
        </q-tab-panel>

        <!-- Details Tab -->
        <q-tab-panel name="details" class="q-pa-lg">
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
                  type="date"
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
                  :hint="$t('Current status of the meeting item')"
                  outlined
                  dense
                  :options="statusOptions"
                  :disable="!canChangeStatus"
                  @update:model-value="emitUpdate"
                >
                  <template v-slot:prepend>
                    <q-icon name="flag" />
                  </template>
                </q-select>
              </div>
            </div>
          </div>
        </q-tab-panel>

        <!-- Additional Details Tab -->
        <q-tab-panel name="additional" class="q-pa-lg">
          <div v-if="loading" class="text-center q-py-xl">
            <q-spinner color="primary" size="3rem" />
            <p class="text-grey-7 q-mt-md">{{ $t('Loading template fields...') }}</p>
          </div>

          <div v-else-if="template && template.fieldDefinitions && template.fieldDefinitions.length > 0">
            <q-banner class="bg-blue-1 text-blue-9 q-mb-md" rounded>
              <template v-slot:avatar>
                <q-icon name="dashboard_customize" color="blue-9" />
              </template>
              {{ $t('Template') }}: <strong>{{ template.name }}</strong>
              <div class="text-caption">{{ decisionBoardName }}</div>
            </q-banner>

            <div class="row q-col-gutter-md">
              <template v-for="field in template.fieldDefinitions" :key="field.fieldName">
                <!-- Text Field -->
                <div v-if="field.fieldType === 'Text'" class="col-12">
                  <q-input
                    v-model="localFieldValues[field.fieldName]"
                    :label="field.label + (field.isRequired ? ' *' : '')"
                    :hint="field.helpText || undefined"
                    :placeholder="field.placeholderText || undefined"
                    :type="field.isTextArea ? 'textarea' : 'text'"
                    :rows="field.isTextArea ? 3 : undefined"
                    outlined
                    dense
                    :rules="field.isRequired ? [val => !!val || $t(field.label + ' is required')] : []"
                    @update:model-value="emitFieldValuesUpdate"
                  />
                </div>

                <!-- Number Field -->
                <div v-if="field.fieldType === 'Number'" class="col-12 col-md-6">
                  <q-input
                    v-model.number="localFieldValues[field.fieldName]"
                    :label="field.label + (field.isRequired ? ' *' : '')"
                    :hint="field.helpText || undefined"
                    :placeholder="field.placeholderText || undefined"
                    type="number"
                    outlined
                    dense
                    :rules="field.isRequired ? [val => val !== null && val !== undefined && val !== '' || $t(field.label + ' is required')] : []"
                    @update:model-value="emitFieldValuesUpdate"
                  />
                </div>

                <!-- Date Field -->
                <div v-if="field.fieldType === 'Date'" class="col-12 col-md-6">
                  <q-input
                    v-model="localFieldValues[field.fieldName]"
                    :label="field.label + (field.isRequired ? ' *' : '')"
                    :hint="field.helpText || undefined"
                    type="date"
                    outlined
                    dense
                    :rules="field.isRequired ? [val => !!val || $t(field.label + ' is required')] : []"
                    @update:model-value="emitFieldValuesUpdate"
                  />
                </div>

                <!-- YesNo Field -->
                <div v-if="field.fieldType === 'YesNo'" class="col-12 col-md-6">
                  <q-checkbox
                    v-model="localFieldValues[field.fieldName]"
                    :label="field.label + (field.isRequired ? ' *' : '')"
                    @update:model-value="emitFieldValuesUpdate"
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
                    :hint="field.helpText || undefined"
                    :placeholder="field.placeholderText || undefined"
                    :options="getDropdownOptions(field)"
                    emit-value
                    map-options
                    outlined
                    dense
                    :rules="field.isRequired ? [val => !!val || $t(field.label + ' is required')] : []"
                    @update:model-value="emitFieldValuesUpdate"
                  />
                </div>

                <!-- MultipleChoice Field -->
                <div v-if="field.fieldType === 'MultipleChoice'" class="col-12">
                  <q-select
                    v-model="localFieldValues[field.fieldName]"
                    :label="field.label + (field.isRequired ? ' *' : '')"
                    :hint="field.helpText || undefined"
                    :placeholder="field.placeholderText || undefined"
                    :options="getDropdownOptions(field)"
                    emit-value
                    map-options
                    multiple
                    use-chips
                    outlined
                    dense
                    :rules="field.isRequired ? [val => (val && val.length > 0) || $t(field.label + ' is required')] : []"
                    @update:model-value="emitFieldValuesUpdate"
                  />
                </div>
              </template>
            </div>

            <!-- Historical Fields -->
            <div v-if="historicalFields && historicalFields.length > 0" class="q-mt-xl">
              <q-separator class="q-mb-md" />
              <h6 class="section-title">{{ $t('Historical Fields') }}</h6>
              <q-banner class="bg-orange-1 text-orange-9" rounded>
                <template v-slot:avatar>
                  <q-icon name="history" color="orange-9" />
                </template>
                {{ $t('These fields were part of a previous template version and are shown for reference only.') }}
              </q-banner>

              <div class="row q-col-gutter-md q-mt-sm">
                <div v-for="hField in historicalFields" :key="hField.fieldName" class="col-12 col-md-6">
                  <q-input
                    :model-value="formatHistoricalValue(hField)"
                    :label="hField.label"
                    outlined
                    dense
                    readonly
                    disable
                  >
                    <template v-slot:prepend>
                      <q-icon name="history" color="grey-6" />
                    </template>
                  </q-input>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="empty-state">
            <q-icon name="dashboard_customize" size="4rem" color="grey-5" />
            <p class="text-grey-7 q-mt-md">{{ $t('No custom template is configured for this decision board') }}</p>
          </div>
        </q-tab-panel>

        <!-- Documents Tab -->
        <q-tab-panel name="documents" class="q-pa-lg">
          <slot name="documents"></slot>
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { PropType } from 'vue'
import type {
  MeetingItem,
  Template,
  FieldValues,
  HistoricalField,
  FieldDefinition,
  MeetingItemOutcome,
  MeetingItemStatus
} from '../types/models'

const props = defineProps({
  meetingItem: {
    type: Object as PropType<MeetingItem>,
    required: true
  },
  template: {
    type: Object as PropType<Template | null>,
    default: null
  },
  historicalFields: {
    type: Array as PropType<HistoricalField[]>,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  canChangeStatus: {
    type: Boolean,
    default: false
  },
  decisionBoardName: {
    type: String,
    default: ''
  },
  documentCount: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits<{
  'update:meetingItem': [value: MeetingItem]
  'update:fieldValues': [value: FieldValues]
}>()

// Local state
const activeTab = ref<string>('general')
const localMeetingItem = ref<MeetingItem>({ ...props.meetingItem })
const localFieldValues = ref<FieldValues>({ ...props.meetingItem.fieldValues })

// Watch for external changes
watch(() => props.meetingItem, (newVal: MeetingItem) => {
  localMeetingItem.value = { ...newVal }
  localFieldValues.value = { ...newVal.fieldValues }
}, { deep: true })

// Options
const outcomeOptions = computed<MeetingItemOutcome[]>(() => [
  'Decision' as MeetingItemOutcome,
  'Discussion' as MeetingItemOutcome,
  'Information' as MeetingItemOutcome
])

const statusOptions = computed<MeetingItemStatus[]>(() => [
  'Draft' as MeetingItemStatus,
  'Submitted' as MeetingItemStatus,
  'UnderReview' as MeetingItemStatus,
  'Approved' as MeetingItemStatus,
  'Rejected' as MeetingItemStatus,
  'Deferred' as MeetingItemStatus
])

// Methods
const emitUpdate = (): void => {
  emit('update:meetingItem', localMeetingItem.value)
}

const emitFieldValuesUpdate = (): void => {
  emit('update:fieldValues', localFieldValues.value)
}

const getDropdownOptions = (field: FieldDefinition): Array<{ label: string; value: string }> => {
  if (!field.options) return []
  return field.options
    .filter(opt => opt.isActive)
    .map(opt => ({
      label: opt.label,
      value: opt.value
    }))
}

const formatHistoricalValue = (field: HistoricalField): string => {
  if (field.fieldType === 'YesNo') {
    return field.value ? 'Yes' : 'No'
  }
  if (field.fieldType === 'MultipleChoice' && Array.isArray(field.value)) {
    return field.value.join(', ')
  }
  return String(field.value)
}

// Expose $t function for translations (placeholder)
const $t = (key: string): string => key
</script>

<style scoped lang="scss">
.meeting-card {
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

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

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
}

:deep(.q-tab) {
  padding: 1rem 1.5rem;
  font-weight: 500;
  text-transform: none;
  font-size: 0.9rem;
}
</style>
