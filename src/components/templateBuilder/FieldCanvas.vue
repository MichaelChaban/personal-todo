<template>
  <q-card flat bordered class="canvas-panel">
    <q-card-section class="panel-header">
      <div class="text-weight-medium">Template Fields</div>
      <div class="text-caption text-grey-7">{{ fields.length }} field(s)</div>
    </q-card-section>

    <q-separator />

    <q-card-section class="canvas-area">
      <!-- Loading State -->
      <div v-if="loading" class="text-center q-py-xl">
        <q-spinner color="primary" size="3rem" />
        <p class="text-grey-7 q-mt-md">Loading template...</p>
      </div>

      <!-- Drop Zone -->
      <div v-else class="drop-zone-container">
        <draggable
          :model-value="fields"
          @update:model-value="handleFieldsUpdate"
          group="fields"
          item-key="id"
          handle=".drag-handle"
          :animation="200"
          class="drop-zone"
          :class="{ 'drop-zone-empty': fields.length === 0 }"
          @change="handleFieldsChange"
        >
          <template #item="{ element }">
            <div
              class="field-item"
              :class="{ 'field-item-selected': selectedFieldId === element.id }"
              @click="$emit('select-field', element)"
            >
              <div class="field-item-header">
                <div class="field-item-left">
                  <q-icon name="drag_indicator" class="drag-handle" size="20px" color="grey-6" />
                  <q-icon :name="getFieldIcon(element.fieldType)" size="20px" :color="getFieldColor(element.fieldType)" class="q-ml-xs" />
                  <span class="field-item-label">{{ element.label || element.fieldName }}</span>
                  <q-chip
                    v-if="element.isRequired"
                    dense
                    size="sm"
                    color="orange"
                    text-color="white"
                    class="q-ml-sm"
                  >
                    Required
                  </q-chip>
                </div>
                <div class="field-item-actions">
                  <q-btn
                    flat
                    dense
                    round
                    size="sm"
                    icon="edit"
                    color="primary"
                    @click.stop="$emit('edit-field', element)"
                  >
                    <q-tooltip>Edit Field</q-tooltip>
                  </q-btn>
                  <q-btn
                    flat
                    dense
                    round
                    size="sm"
                    icon="content_copy"
                    color="grey-7"
                    @click.stop="$emit('duplicate-field', element)"
                  >
                    <q-tooltip>Duplicate</q-tooltip>
                  </q-btn>
                  <q-btn
                    flat
                    dense
                    round
                    size="sm"
                    icon="delete"
                    color="negative"
                    @click.stop="$emit('delete-field', element)"
                  >
                    <q-tooltip>Delete</q-tooltip>
                  </q-btn>
                </div>
              </div>

              <div class="field-item-preview">
                <component
                  :is="getFieldComponent(element.fieldType)"
                  v-bind="getFieldProps(element)"
                  :model-value="getPreviewValue(element)"
                  outlined
                  dense
                  readonly
                  disable
                />
              </div>

              <div v-if="element.helpText" class="field-item-help">
                <q-icon name="info" size="14px" color="grey-6" />
                <span class="text-caption text-grey-7">{{ element.helpText }}</span>
              </div>
            </div>
          </template>

          <template #footer>
            <div v-if="fields.length === 0" class="empty-drop-zone">
              <q-icon name="add_circle_outline" size="4rem" color="grey-5" />
              <p class="text-grey-7 q-mt-md">Drag fields from the left panel to get started</p>
            </div>
          </template>
        </draggable>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import draggable from 'vuedraggable'

const props = defineProps({
  fields: {
    type: Array,
    required: true
  },
  selectedFieldId: {
    type: String,
    default: null
  },
  fieldTypes: {
    type: Array,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:fields', 'fields-changed', 'select-field', 'edit-field', 'duplicate-field', 'delete-field'])

const handleFieldsUpdate = (newFields) => {
  emit('update:fields', newFields)
}

const handleFieldsChange = () => {
  emit('fields-changed')
}

const getFieldIcon = (fieldType) => {
  const field = props.fieldTypes.find(ft => ft.type === fieldType)
  return field?.icon || 'text_fields'
}

const getFieldColor = (fieldType) => {
  const field = props.fieldTypes.find(ft => ft.type === fieldType)
  return field?.color || 'grey-6'
}

const getFieldComponent = (fieldType) => {
  const components = {
    'Text': 'q-input',
    'Number': 'q-input',
    'Date': 'q-input',
    'YesNo': 'q-checkbox',
    'Dropdown': 'q-select',
    'MultipleChoice': 'q-select'
  }
  return components[fieldType] || 'q-input'
}

const getFieldProps = (field) => {
  const baseProps = {
    label: field.label,
    hint: field.helpText,
    placeholder: field.placeholderText
  }

  if (field.fieldType === 'Number') {
    baseProps.type = 'number'
  } else if (field.fieldType === 'Date') {
    baseProps.type = 'date'
  } else if (field.fieldType === 'Text' && field.isTextArea) {
    baseProps.type = 'textarea'
    baseProps.rows = 3
  } else if (field.fieldType === 'Dropdown' || field.fieldType === 'MultipleChoice') {
    baseProps.options = field.options?.map(opt => opt.label) || []
    if (field.fieldType === 'MultipleChoice') {
      baseProps.multiple = true
    }
  }

  return baseProps
}

const getPreviewValue = (field) => {
  if (field.fieldType === 'YesNo') return false
  if (field.fieldType === 'Dropdown' && field.options?.length) {
    const defaultOpt = field.options.find(opt => opt.isDefault)
    return defaultOpt?.label || field.options[0]?.label
  }
  if (field.fieldType === 'MultipleChoice') return []
  return ''
}
</script>

<style scoped lang="scss">
.canvas-panel {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8f9fa;
  padding: 0.75rem 1rem;
  font-weight: 600;
  color: #2c3e50;
}

.canvas-area {
  min-height: 600px;
  max-height: calc(100vh - 300px);
  overflow-y: auto;
}

.drop-zone-container {
  min-height: 400px;
}

.drop-zone {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  min-height: 400px;
  padding: 1rem;
  background: #fafafa;
  border: 2px dashed #e0e0e0;
  border-radius: 8px;

  &.drop-zone-empty {
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

.empty-drop-zone {
  text-align: center;
  padding: 3rem 1rem;
}

.field-item {
  background: #fff;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  padding: 1rem;
  transition: all 0.2s;
  cursor: pointer;

  &:hover {
    border-color: #1976d2;
    box-shadow: 0 2px 8px rgba(25, 118, 210, 0.15);
  }

  &.field-item-selected {
    border-color: #1976d2;
    background: #f0f7ff;
    box-shadow: 0 2px 8px rgba(25, 118, 210, 0.2);
  }
}

.field-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.field-item-left {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.drag-handle {
  cursor: move;
}

.field-item-label {
  font-weight: 600;
  color: #2c3e50;
  font-size: 0.95rem;
}

.field-item-actions {
  display: flex;
  gap: 0.25rem;
}

.field-item-preview {
  pointer-events: none;
  opacity: 0.8;
}

.field-item-help {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  margin-top: 0.75rem;
  padding: 0.5rem;
  background: #f5f7fa;
  border-radius: 4px;
}
</style>
