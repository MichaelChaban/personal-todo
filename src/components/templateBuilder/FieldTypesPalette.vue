<template>
  <q-card flat bordered class="field-palette">
    <q-card-section class="panel-header">
      <div class="text-weight-medium">Field Types</div>
      <p class="text-caption text-grey-7 q-ma-none q-mt-xs">
        Drag fields to the canvas
      </p>
    </q-card-section>

    <q-separator />

    <q-card-section class="field-types">
      <draggable
        :list="fieldTypes"
        :group="{ name: 'fields', pull: 'clone', put: false }"
        :clone="handleClone"
        :sort="false"
        item-key="type"
        class="field-types-list"
      >
        <template #item="{ element }">
          <div class="field-type-item">
            <q-icon :name="element.icon" size="20px" :color="element.color" />
            <span class="field-type-label">{{ element.label }}</span>
          </div>
        </template>
      </draggable>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { ref } from 'vue'
import draggable from 'vuedraggable'

const props = defineProps({
  fieldTypes: {
    type: Array,
    required: true
  },
  fieldsCount: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits(['clone-field'])

const handleClone = (fieldType) => {
  const clonedField = {
    id: `field_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
    fieldName: `${fieldType.type.toLowerCase()}Field${props.fieldsCount + 1}`,
    label: `${fieldType.label} ${props.fieldsCount + 1}`,
    fieldType: fieldType.type,
    displayOrder: props.fieldsCount,
    isRequired: false,
    isActive: true,
    placeholderText: '',
    helpText: '',
    isTextArea: false,
    options: null
  }

  // Initialize options for Dropdown/MultipleChoice
  if (fieldType.type === 'Dropdown' || fieldType.type === 'MultipleChoice') {
    clonedField.options = [
      { value: 'option1', label: 'Option 1', isDefault: false, isActive: true },
      { value: 'option2', label: 'Option 2', isDefault: false, isActive: true }
    ]
  }

  emit('clone-field', clonedField)
  return clonedField
}
</script>

<style scoped lang="scss">
.field-palette {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.panel-header {
  background: #f8f9fa;
  padding: 0.75rem 1rem;
  font-weight: 600;
  color: #2c3e50;
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
</style>
