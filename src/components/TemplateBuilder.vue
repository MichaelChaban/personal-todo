<template>
  <div class="builder-layout row q-col-gutter-lg">
    <!-- Left Panel: Template Settings & Field Palette -->
    <div class="col-12 col-md-3">
      <!-- Template Settings -->
      <q-card flat bordered class="settings-panel">
        <q-card-section class="panel-header">
          <div class="text-weight-medium">{{ $t('Template Settings') }}</div>
        </q-card-section>

        <q-separator />

        <q-card-section>
          <q-input
            v-model="localTemplate.name"
            :label="$t('Template Name') + ' *'"
            outlined
            dense
            :hint="$t('E.g., IT Governance Template')"
            :rules="[val => !!val || $t('Required')]"
            class="q-mb-md"
            @update:model-value="emitTemplateUpdate"
          />

          <q-input
            v-model="localTemplate.description"
            :label="$t('Description')"
            outlined
            dense
            type="textarea"
            rows="3"
            :hint="$t('Brief description of this template')"
            class="q-mb-md"
            @update:model-value="emitTemplateUpdate"
          />

          <q-select
            v-model="localTemplate.decisionBoardId"
            :label="$t('Decision Board') + ' *'"
            outlined
            dense
            :options="decisionBoards"
            option-value="id"
            option-label="name"
            emit-value
            map-options
            :rules="[val => !!val || $t('Required')]"
            class="q-mb-md"
            @update:model-value="emitTemplateUpdate"
          />

          <q-toggle
            v-model="localTemplate.isActive"
            :label="$t('Active Template')"
            color="primary"
            class="q-mb-md"
            @update:model-value="emitTemplateUpdate"
          />

          <div class="stats-box">
            <div class="stat-item">
              <span class="stat-label">{{ $t('Total Fields') }}:</span>
              <span class="stat-value">{{ localTemplate.fields.length }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">{{ $t('Required') }}:</span>
              <span class="stat-value">{{ requiredFieldsCount }}</span>
            </div>
          </div>
        </q-card-section>
      </q-card>

      <!-- Field Types Palette -->
      <q-card flat bordered class="field-palette q-mt-md">
        <q-card-section class="panel-header">
          <div class="text-weight-medium">{{ $t('Field Types') }}</div>
          <div class="text-caption text-grey-7">
            {{ $t('Drag to canvas') }}
          </div>
        </q-card-section>

        <q-separator />

        <q-card-section>
          <draggable
            :list="fieldTypes"
            :group="{ name: 'fields', pull: 'clone', put: false }"
            :clone="cloneFieldType"
            :sort="false"
            item-key="type"
            class="field-types-list"
          >
            <template #item="{ element }">
              <div class="field-type-item">
                <q-icon :name="element.icon" size="20px" :color="element.color" />
                <span class="field-type-label">{{ $t(element.label) }}</span>
              </div>
            </template>
          </draggable>
        </q-card-section>
      </q-card>
    </div>

    <!-- Center Panel: Canvas (Drag & Drop Area) -->
    <div class="col-12 col-md-6">
      <q-card flat bordered class="canvas-panel">
        <q-card-section class="panel-header">
          <div class="text-weight-medium">{{ $t('Template Fields') }}</div>
          <div class="text-caption text-grey-7">
            {{ localTemplate.fields.length }} {{ $t('field(s)') }}
          </div>
        </q-card-section>

        <q-separator />

        <q-card-section class="canvas-area">
          <!-- Loading State -->
          <div v-if="loading" class="text-center q-py-xl">
            <q-spinner color="primary" size="3rem" />
            <p class="text-grey-7 q-mt-md">{{ $t('Loading template...') }}</p>
          </div>

          <!-- Drop Zone -->
          <div v-else class="drop-zone-container">
            <draggable
              v-model="localTemplate.fields"
              group="fields"
              item-key="id"
              handle=".drag-handle"
              :animation="200"
              class="drop-zone"
              :class="{ 'drop-zone-empty': localTemplate.fields.length === 0 }"
              @change="handleFieldsChange"
            >
              <template #item="{ element }">
                <div
                  class="field-item"
                  :class="{ 'field-item-selected': selectedField?.id === element.id }"
                  @click="selectField(element)"
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
                        {{ $t('Required') }}
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
                        @click.stop="editField(element)"
                      >
                        <q-tooltip>{{ $t('Edit Field') }}</q-tooltip>
                      </q-btn>
                      <q-btn
                        flat
                        dense
                        round
                        size="sm"
                        icon="content_copy"
                        color="grey-7"
                        @click.stop="duplicateField(element)"
                      >
                        <q-tooltip>{{ $t('Duplicate') }}</q-tooltip>
                      </q-btn>
                      <q-btn
                        flat
                        dense
                        round
                        size="sm"
                        icon="delete"
                        color="negative"
                        @click.stop="deleteField(element)"
                      >
                        <q-tooltip>{{ $t('Delete') }}</q-tooltip>
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
                <div v-if="localTemplate.fields.length === 0" class="empty-drop-zone">
                  <q-icon name="add_circle_outline" size="4rem" color="grey-5" />
                  <p class="text-grey-7 q-mt-md">{{ $t('Drag fields from the left panel to get started') }}</p>
                </div>
              </template>
            </draggable>
          </div>
        </q-card-section>
      </q-card>
    </div>

    <!-- Right Panel: Field Properties -->
    <div class="col-12 col-md-3">
      <q-card flat bordered class="properties-panel">
        <q-card-section class="panel-header">
          <div class="text-weight-medium">{{ $t('Field Properties') }}</div>
          <q-btn
            v-if="selectedField"
            flat
            dense
            round
            size="sm"
            icon="close"
            color="grey-7"
            @click="selectedField = null"
          />
        </q-card-section>

        <q-separator />

        <q-card-section v-if="selectedField" class="properties-form">
          <q-input
            v-model="selectedField.fieldName"
            :label="$t('Field Name (ID)') + ' *'"
            :hint="$t('Unique identifier (camelCase)')"
            outlined
            dense
            :rules="[
              val => !!val || $t('Required'),
              val => isValidFieldName(val) || $t('Must start with lowercase letter and contain only letters and numbers')
            ]"
            class="q-mb-md"
            @update:model-value="updateSelectedField"
          />

          <q-input
            v-model="selectedField.label"
            :label="$t('Label') + ' *'"
            :hint="$t('Display label for the field')"
            outlined
            dense
            :rules="[val => !!val || $t('Required')]"
            class="q-mb-md"
            @update:model-value="updateSelectedField"
          />

          <q-select
            v-model="selectedField.fieldType"
            :label="$t('Field Type') + ' *'"
            outlined
            dense
            :options="fieldTypeOptions"
            :rules="[val => !!val || $t('Required')]"
            class="q-mb-md"
            @update:model-value="handleFieldTypeChange"
          />

          <q-toggle
            v-model="selectedField.isRequired"
            :label="$t('Required Field')"
            color="primary"
            class="q-mb-md"
            @update:model-value="updateSelectedField"
          />

          <!-- Text-specific options -->
          <template v-if="selectedField.fieldType === 'Text'">
            <q-toggle
              v-model="selectedField.isTextArea"
              :label="$t('Multi-line Text')"
              color="primary"
              class="q-mb-md"
              @update:model-value="updateSelectedField"
            />
          </template>

          <q-input
            v-model="selectedField.placeholderText"
            :label="$t('Placeholder Text')"
            :hint="$t('Example text shown when field is empty')"
            outlined
            dense
            class="q-mb-md"
            @update:model-value="updateSelectedField"
          />

          <q-input
            v-model="selectedField.helpText"
            :label="$t('Help Text')"
            :hint="$t('Additional guidance for users')"
            outlined
            dense
            type="textarea"
            rows="2"
            class="q-mb-md"
            @update:model-value="updateSelectedField"
          />

          <!-- Options for Dropdown and MultipleChoice -->
          <template v-if="['Dropdown', 'MultipleChoice'].includes(selectedField.fieldType)">
            <q-separator class="q-my-md" />
            <div class="section-title q-mb-md">{{ $t('Options') }}</div>

            <draggable
              v-model="selectedField.options"
              item-key="value"
              handle=".option-drag-handle"
              class="options-list"
            >
              <template #item="{ element, index }">
                <div class="option-item q-mb-sm">
                  <q-icon name="drag_indicator" class="option-drag-handle" size="18px" color="grey-6" />
                  <q-input
                    v-model="element.value"
                    :placeholder="$t('Value')"
                    outlined
                    dense
                    class="option-input"
                    @update:model-value="updateSelectedField"
                  />
                  <q-input
                    v-model="element.label"
                    :placeholder="$t('Label')"
                    outlined
                    dense
                    class="option-input"
                    @update:model-value="updateSelectedField"
                  />
                  <q-checkbox
                    v-model="element.isDefault"
                    :label="$t('Default')"
                    dense
                    @update:model-value="updateSelectedField"
                  />
                  <q-btn
                    flat
                    dense
                    round
                    size="sm"
                    icon="delete"
                    color="negative"
                    @click="removeOption(index)"
                  />
                </div>
              </template>
            </draggable>

            <q-btn
              :label="$t('Add Option')"
              icon="add"
              color="primary"
              outline
              dense
              class="full-width q-mt-sm"
              @click="addOption"
            />
          </template>
        </q-card-section>

        <q-card-section v-else class="empty-properties">
          <q-icon name="settings" size="3rem" color="grey-5" />
          <p class="text-grey-7 q-mt-md">{{ $t('Select a field to edit its properties') }}</p>
        </q-card-section>
      </q-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { PropType } from 'vue'
import draggable from 'vuedraggable'
import { useQuasar } from 'quasar'
import type {
  Template,
  FieldDefinition,
  FieldType,
  FieldTypeDefinition,
  DecisionBoard,
  FieldOption
} from '../types/models'

const $q = useQuasar()

const props = defineProps({
  template: {
    type: Object as PropType<Template>,
    required: true
  },
  fieldTypes: {
    type: Array as PropType<FieldTypeDefinition[]>,
    required: true
  },
  decisionBoards: {
    type: Array as PropType<DecisionBoard[]>,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits<{
  'update:template': [value: Template]
  'fieldsChanged': []
}>()

// Local state
const localTemplate = ref<Template>({ ...props.template, fields: [...props.template.fieldDefinitions] })
const selectedField = ref<FieldDefinition | null>(null)

// Watch for external changes
watch(() => props.template, (newVal: Template) => {
  localTemplate.value = { ...newVal, fields: [...newVal.fieldDefinitions] }
}, { deep: true })

// Computed
const requiredFieldsCount = computed<number>(() => {
  return localTemplate.value.fields.filter(f => f.isRequired).length
})

const fieldTypeOptions = computed<FieldType[]>(() => {
  return props.fieldTypes.map(ft => ft.type)
})

// Methods
const emitTemplateUpdate = (): void => {
  emit('update:template', {
    ...localTemplate.value,
    fieldDefinitions: localTemplate.value.fields
  })
}

const cloneFieldType = (fieldType: FieldTypeDefinition): FieldDefinition => {
  const fieldsCount = localTemplate.value.fields.length
  const clonedField: FieldDefinition = {
    id: `field_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
    fieldName: `${fieldType.type.toLowerCase()}Field${fieldsCount + 1}`,
    label: `${fieldType.label} ${fieldsCount + 1}`,
    fieldType: fieldType.type,
    displayOrder: fieldsCount,
    isRequired: false,
    isActive: true,
    isTextArea: false,
    placeholderText: null,
    helpText: null,
    options: null
  }

  if (fieldType.type === 'Dropdown' || fieldType.type === 'MultipleChoice') {
    clonedField.options = [
      { value: 'option1', label: 'Option 1', isDefault: false, isActive: true },
      { value: 'option2', label: 'Option 2', isDefault: false, isActive: true }
    ]
  }

  return clonedField
}

const selectField = (field: FieldDefinition): void => {
  selectedField.value = { ...field }
}

const editField = (field: FieldDefinition): void => {
  selectedField.value = { ...field }
}

const duplicateField = (field: FieldDefinition): void => {
  const newField: FieldDefinition = {
    ...field,
    id: `field_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
    fieldName: `${field.fieldName}_copy`,
    label: `${field.label} (Copy)`,
    displayOrder: localTemplate.value.fields.length,
    options: field.options ? field.options.map(opt => ({ ...opt })) : null
  }

  localTemplate.value.fields.push(newField)
  emitTemplateUpdate()

  $q.notify({
    type: 'positive',
    message: $t('Field duplicated'),
    timeout: 1000
  })
}

const deleteField = (field: FieldDefinition): void => {
  $q.dialog({
    title: $t('Confirm'),
    message: $t('Are you sure you want to delete this field?'),
    cancel: true,
    persistent: true
  }).onOk(() => {
    const index = localTemplate.value.fields.findIndex(f => f.id === field.id)
    if (index !== -1) {
      localTemplate.value.fields.splice(index, 1)
      if (selectedField.value?.id === field.id) {
        selectedField.value = null
      }
      emitTemplateUpdate()

      $q.notify({
        type: 'positive',
        message: $t('Field deleted'),
        timeout: 1000
      })
    }
  })
}

const handleFieldsChange = (): void => {
  localTemplate.value.fields.forEach((field, index) => {
    field.displayOrder = index
  })
  emitTemplateUpdate()
  emit('fieldsChanged')
}

const updateSelectedField = (): void => {
  if (!selectedField.value) return

  const index = localTemplate.value.fields.findIndex(f => f.id === selectedField.value!.id)
  if (index !== -1) {
    localTemplate.value.fields[index] = { ...selectedField.value }
    localTemplate.value.fields = [...localTemplate.value.fields]
    emitTemplateUpdate()
  }
}

const handleFieldTypeChange = (): void => {
  if (!selectedField.value) return

  if (!['Dropdown', 'MultipleChoice'].includes(selectedField.value.fieldType)) {
    selectedField.value.options = null
  } else if (!selectedField.value.options) {
    selectedField.value.options = [
      { value: 'option1', label: 'Option 1', isDefault: false, isActive: true },
      { value: 'option2', label: 'Option 2', isDefault: false, isActive: true }
    ]
  }

  if (selectedField.value.fieldType !== 'Text') {
    selectedField.value.isTextArea = false
  }

  updateSelectedField()
}

const addOption = (): void => {
  if (!selectedField.value || !selectedField.value.options) return

  const newOption: FieldOption = {
    value: `option${selectedField.value.options.length + 1}`,
    label: `Option ${selectedField.value.options.length + 1}`,
    isDefault: false,
    isActive: true
  }

  selectedField.value.options.push(newOption)
  updateSelectedField()
}

const removeOption = (index: number): void => {
  if (!selectedField.value || !selectedField.value.options) return

  selectedField.value.options.splice(index, 1)
  updateSelectedField()
}

const isValidFieldName = (name: string): boolean => {
  return /^[a-z][a-zA-Z0-9]*$/.test(name)
}

const getFieldIcon = (fieldType: FieldType): string => {
  const field = props.fieldTypes.find(ft => ft.type === fieldType)
  return field?.icon || 'text_fields'
}

const getFieldColor = (fieldType: FieldType): string => {
  const field = props.fieldTypes.find(ft => ft.type === fieldType)
  return field?.color || 'grey-6'
}

const getFieldComponent = (fieldType: FieldType): string => {
  const components: Record<string, string> = {
    'Text': 'q-input',
    'Number': 'q-input',
    'Date': 'q-input',
    'YesNo': 'q-checkbox',
    'Dropdown': 'q-select',
    'MultipleChoice': 'q-select'
  }
  return components[fieldType] || 'q-input'
}

const getFieldProps = (field: FieldDefinition): Record<string, any> => {
  const baseProps: Record<string, any> = {
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

const getPreviewValue = (field: FieldDefinition): any => {
  if (field.fieldType === 'YesNo') return false
  if (field.fieldType === 'Dropdown' && field.options?.length) {
    const defaultOpt = field.options.find(opt => opt.isDefault)
    return defaultOpt?.label || field.options[0]?.label
  }
  if (field.fieldType === 'MultipleChoice') return []
  return ''
}

// Expose $t function for translations (placeholder)
const $t = (key: string): string => key
</script>

<style scoped lang="scss">
.builder-layout {
  margin-top: 2rem;
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

.settings-panel,
.properties-panel,
.field-palette,
.canvas-panel {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.stats-box {
  background: #f5f7fa;
  border-radius: 8px;
  padding: 1rem;
  margin-top: 1rem;

  .stat-item {
    display: flex;
    justify-content: space-between;
    padding: 0.5rem 0;
    border-bottom: 1px solid #e0e0e0;

    &:last-child {
      border-bottom: none;
    }

    .stat-label {
      font-size: 0.875rem;
      color: #666;
    }

    .stat-value {
      font-size: 0.875rem;
      font-weight: 600;
      color: #1976d2;
    }
  }
}

.field-types-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field-type-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: #fff;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  cursor: move;
  transition: all 0.2s;

  &:hover {
    border-color: #1976d2;
    background: #f0f7ff;
    transform: translateX(4px);
  }

  .field-type-label {
    font-size: 0.875rem;
    font-weight: 500;
    color: #424242;
  }
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

.properties-form {
  max-height: calc(100vh - 200px);
  overflow-y: auto;
}

.empty-properties {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
}

.section-title {
  font-weight: 600;
  color: #2c3e50;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.options-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.option-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  background: #f5f7fa;
  border-radius: 4px;
}

.option-drag-handle {
  cursor: move;
}

.option-input {
  flex: 1;
}
</style>
