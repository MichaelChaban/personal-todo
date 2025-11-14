<template>
  <q-card flat bordered class="settings-panel">
    <q-card-section class="panel-header">
      <div class="text-weight-medium">Template Settings</div>
    </q-card-section>

    <q-separator />

    <q-card-section>
      <q-input
        v-model="localTemplate.name"
        label="Template Name *"
        outlined
        dense
        hint="E.g., IT Governance Template"
        :rules="[val => !!val || 'Required']"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-input
        v-model="localTemplate.description"
        label="Description"
        outlined
        dense
        type="textarea"
        rows="3"
        hint="Brief description of this template"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-select
        v-model="localTemplate.decisionBoardId"
        label="Decision Board *"
        outlined
        dense
        :options="decisionBoards"
        option-value="id"
        option-label="name"
        emit-value
        map-options
        :rules="[val => !!val || 'Required']"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <q-toggle
        v-model="localTemplate.isActive"
        label="Active Template"
        color="primary"
        class="q-mb-md"
        @update:model-value="emitUpdate"
      />

      <div class="stats-box">
        <div class="stat-item">
          <span class="stat-label">Total Fields:</span>
          <span class="stat-value">{{ totalFields }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Required:</span>
          <span class="stat-value">{{ requiredFields }}</span>
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  template: {
    type: Object,
    required: true
  },
  decisionBoards: {
    type: Array,
    required: true
  },
  totalFields: {
    type: Number,
    default: 0
  },
  requiredFields: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits(['update:template'])

// Local copy for v-model
const localTemplate = ref({ ...props.template })

// Watch for external changes
watch(() => props.template, (newVal) => {
  localTemplate.value = { ...newVal }
}, { deep: true })

// Emit updates
const emitUpdate = () => {
  emit('update:template', localTemplate.value)
}
</script>

<style scoped lang="scss">
.settings-panel {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.panel-header {
  background: #f8f9fa;
  padding: 0.75rem 1rem;
  font-weight: 600;
  color: #2c3e50;
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
</style>
