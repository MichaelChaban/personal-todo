<template>
  <q-card flat bordered class="properties-panel" :class="{ 'panel-disabled': !selectedField }">
    <q-card-section class="panel-header">
      <div class="text-weight-medium">Field Properties</div>
      <q-btn
        v-if="selectedField"
        flat
        dense
        round
        size="sm"
        icon="close"
        color="grey-7"
        @click="$emit('deselect-field')"
      />
    </q-card-section>

    <q-separator />

    <q-card-section v-if="selectedField" class="properties-form">
      <q-input
        v-model="localField.fieldName"
        label="Field Name (ID) *"
        outlined
        dense
        hint="Unique identifier (camelCase)"
        :rules="[val => !!val || 'Required', val => isValidFieldName(val) || 'Invalid format']"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-input
        v-model="localField.label"
        label="Label *"
        outlined
        dense
        hint="Display label for users"
        :rules="[val => !!val || 'Required']"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-select
        v-model="localField.fieldType"
        label="Field Type *"
        outlined
        dense
        :options="fieldTypeOptions"
        class="q-mb-md"
        @update:model-value="handleFieldTypeChange"
      />

      <q-input
        v-model="localField.placeholderText"
        label="Placeholder"
        outlined
        dense
        hint="Helper text in input"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-input
        v-model="localField.helpText"
        label="Help Text"
        outlined
        dense
        type="textarea"
        rows="2"
        hint="Additional guidance"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-toggle
        v-model="localField.isRequired"
        label="Required Field"
        color="primary"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <!-- Text Area Option for Text Fields -->
      <template v-if="localField.fieldType === 'Text'">
        <q-separator class="q-my-md" />
        <q-toggle
          v-model="localField.isTextArea"
          label="Multi-line Text (Textarea)"
          color="primary"
          class="q-mb-md"
          @update:model-value="emitUpdate"
        />
      </template>

      <!-- Options for Dropdown/MultipleChoice -->
      <template v-if="['Dropdown', 'MultipleChoice'].includes(localField.fieldType)">
        <q-separator class="q-my-md" />

        <div class="section-title q-mb-sm">Options</div>

        <div class="options-list q-mb-md">
          <draggable
            v-model="localField.options"
            item-key="value"
            handle=".option-drag-handle"
            :animation="200"
            @change="emitUpdate"
          >
            <template #item="{ element, index }">
              <div class="option-item">
                <q-icon name="drag_indicator" class="option-drag-handle" size="16px" color="grey-6" />
                <q-input
                  v-model="element.value"
                  dense
                  outlined
                  placeholder="Value"
                  class="option-input"
                  @update:model-value="emitUpdate"
                />
                <q-input
                  v-model="element.label"
                  dense
                  outlined
                  placeholder="Label"
                  class="option-input q-ml-xs"
                  @update:model-value="emitUpdate"
                />
                <q-checkbox
                  v-model="element.isDefault"
                  dense
                  @update:model-value="emitUpdate"
                >
                  <q-tooltip>Default</q-tooltip>
                </q-checkbox>
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
        </div>

        <q-btn
          outline
          dense
          icon="add"
          label="Add Option"
          color="primary"
          size="sm"
          @click="addOption"
          class="full-width"
        />
      </template>
    </q-card-section>

    <q-card-section v-else class="empty-properties">
      <q-icon name="settings" size="3rem" color="grey-5" />
      <p class="text-grey-7 q-mt-md">Select a field to edit its properties</p>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { ref, watch } from 'vue'
import draggable from 'vuedraggable'

const props = defineProps({
  selectedField: {
    type: Object,
    default: null
  },
  fieldTypeOptions: {
    type: Array,
    required: true
  }
})

const emit = defineEmits(['update:field', 'deselect-field'])

// Local copy for v-model
const localField = ref(props.selectedField ? { ...props.selectedField } : null)

// Watch for external changes
watch(() => props.selectedField, (newVal) => {
  localField.value = newVal ? { ...newVal } : null
}, { deep: true })

// Emit updates
const emitUpdate = () => {
  if (localField.value) {
    emit('update:field', localField.value)
  }
}

const isValidFieldName = (name) => {
  // camelCase validation
  return /^[a-z][a-zA-Z0-9]*$/.test(name)
}

const handleFieldTypeChange = () => {
  // Clear options if changing from Dropdown/MultipleChoice to other type
  if (!['Dropdown', 'MultipleChoice'].includes(localField.value.fieldType)) {
    localField.value.options = null
  } else if (!localField.value.options) {
    // Initialize options if changing to Dropdown/MultipleChoice
    localField.value.options = [
      { value: 'option1', label: 'Option 1', isDefault: false, isActive: true },
      { value: 'option2', label: 'Option 2', isDefault: false, isActive: true }
    ]
  }

  // Clear isTextArea if not Text field
  if (localField.value.fieldType !== 'Text') {
    localField.value.isTextArea = false
  }

  emitUpdate()
}

const addOption = () => {
  if (!localField.value.options) {
    localField.value.options = []
  }

  const nextIndex = localField.value.options.length
  localField.value.options.push({
    value: `option${nextIndex + 1}`,
    label: `Option ${nextIndex + 1}`,
    isDefault: false,
    isActive: true
  })

  emitUpdate()
}

const removeOption = (index) => {
  localField.value.options.splice(index, 1)
  emitUpdate()
}
</script>

<style scoped lang="scss">
.properties-panel {
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

.properties-form {
  max-height: calc(100vh - 200px);
  overflow-y: auto;
}

.panel-disabled {
  opacity: 0.6;
  pointer-events: none;
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
