<template>
  <q-page class="q-pa-md">
    <div class="templates-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">Template Management</h4>
          <p class="text-grey-7 q-ma-none">
            Create and manage meeting item templates for decision boards
          </p>
        </div>
        <q-btn
          unelevated
          color="primary"
          icon="add"
          label="Create Template"
          @click="$router.push('/templates/create')"
        />
      </div>

      <!-- Filters -->
      <q-card flat bordered class="q-mb-md">
        <q-card-section class="row q-col-gutter-md">
          <div class="col-12 col-md-4">
            <q-input
              v-model="search"
              outlined
              dense
              placeholder="Search templates..."
              clearable
            >
              <template v-slot:prepend>
                <q-icon name="search" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-md-3">
            <q-select
              v-model="filterBoard"
              outlined
              dense
              placeholder="All Decision Boards"
              :options="decisionBoards"
              option-value="id"
              option-label="name"
              clearable
            />
          </div>
          <div class="col-12 col-md-2">
            <q-select
              v-model="filterStatus"
              outlined
              dense
              placeholder="All Status"
              :options="['Active', 'Inactive']"
              clearable
            />
          </div>
        </q-card-section>
      </q-card>

      <!-- Templates Grid -->
      <div class="row q-col-gutter-md">
        <div
          v-for="template in filteredTemplates"
          :key="template.id"
          class="col-12 col-md-6 col-lg-4"
        >
          <q-card flat bordered class="template-card">
            <q-card-section class="card-header">
              <div class="template-header">
                <div>
                  <div class="template-name">{{ template.name }}</div>
                  <div class="template-board text-caption text-grey-7">
                    {{ template.decisionBoardName }}
                  </div>
                </div>
                <q-chip
                  :color="template.isActive ? 'green' : 'grey'"
                  text-color="white"
                  size="sm"
                  dense
                >
                  {{ template.isActive ? 'Active' : 'Inactive' }}
                </q-chip>
              </div>
            </q-card-section>

            <q-separator />

            <q-card-section>
              <div class="template-description text-grey-8">
                {{ template.description || 'No description provided' }}
              </div>

              <div class="template-stats row q-col-gutter-sm q-mt-md">
                <div class="col-6">
                  <div class="stat-box">
                    <q-icon name="view_list" size="20px" color="blue-6" />
                    <div class="stat-content">
                      <div class="stat-value">{{ template.fieldCount }}</div>
                      <div class="stat-label">Fields</div>
                    </div>
                  </div>
                </div>
                <div class="col-6">
                  <div class="stat-box">
                    <q-icon name="category" size="20px" color="purple-6" />
                    <div class="stat-content">
                      <div class="stat-value">{{ template.categoryCount }}</div>
                      <div class="stat-label">Categories</div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="template-meta q-mt-md">
                <div class="meta-item">
                  <q-icon name="event" size="16px" color="grey-6" />
                  <span class="text-caption text-grey-7">
                    Created {{ formatDate(template.createdDate) }}
                  </span>
                </div>
                <div class="meta-item">
                  <q-icon name="update" size="16px" color="grey-6" />
                  <span class="text-caption text-grey-7">
                    Updated {{ formatDate(template.updatedDate) }}
                  </span>
                </div>
              </div>
            </q-card-section>

            <q-separator />

            <q-card-actions align="right">
              <q-btn
                flat
                dense
                label="View"
                color="grey-7"
                icon="visibility"
                @click="viewTemplate(template)"
              />
              <q-btn
                flat
                dense
                label="Edit"
                color="primary"
                icon="edit"
                @click="editTemplate(template)"
              />
              <q-btn
                flat
                dense
                label="Duplicate"
                color="grey-7"
                icon="content_copy"
                @click="duplicateTemplate(template)"
              />
              <q-btn
                flat
                dense
                :label="template.isActive ? 'Deactivate' : 'Activate'"
                :color="template.isActive ? 'negative' : 'positive'"
                :icon="template.isActive ? 'block' : 'check_circle'"
                @click="toggleTemplateStatus(template)"
              />
            </q-card-actions>
          </q-card>
        </div>

        <!-- Empty State -->
        <div v-if="filteredTemplates.length === 0" class="col-12">
          <q-card flat bordered class="empty-state">
            <q-card-section class="text-center">
              <q-icon name="description" size="5rem" color="grey-5" />
              <div class="text-h6 text-grey-7 q-mt-md">No templates found</div>
              <p class="text-grey-6 q-mb-lg">
                {{ search || filterBoard || filterStatus
                  ? 'Try adjusting your filters'
                  : 'Get started by creating your first template'
                }}
              </p>
              <q-btn
                v-if="!search && !filterBoard && !filterStatus"
                unelevated
                color="primary"
                icon="add"
                label="Create Template"
                @click="$router.push('/templates/create')"
              />
            </q-card-section>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'

const router = useRouter()
const $q = useQuasar()

// State
const search = ref('')
const filterBoard = ref(null)
const filterStatus = ref(null)

// Mock data - replace with API call
const decisionBoards = ref([
  { id: 'board-1', name: 'IT Governance' },
  { id: 'board-2', name: 'Architecture Board' },
  { id: 'board-3', name: 'Security Council' }
])

const templates = ref([
  {
    id: 'template-1',
    name: 'IT Governance Standard',
    description: 'Standard template for IT Governance Board meetings covering architecture, security, and infrastructure decisions',
    decisionBoardId: 'board-1',
    decisionBoardName: 'IT Governance',
    isActive: true,
    fieldCount: 12,
    categoryCount: 3,
    createdDate: '2025-01-10T10:00:00Z',
    updatedDate: '2025-01-12T14:30:00Z'
  },
  {
    id: 'template-2',
    name: 'Architecture Review',
    description: 'Comprehensive architecture review template with technical assessment fields',
    decisionBoardId: 'board-2',
    decisionBoardName: 'Architecture Board',
    isActive: true,
    fieldCount: 15,
    categoryCount: 4,
    createdDate: '2025-01-08T09:00:00Z',
    updatedDate: '2025-01-11T16:00:00Z'
  },
  {
    id: 'template-3',
    name: 'Security Assessment',
    description: 'Security-focused template for evaluating risks and compliance requirements',
    decisionBoardId: 'board-3',
    decisionBoardName: 'Security Council',
    isActive: true,
    fieldCount: 10,
    categoryCount: 2,
    createdDate: '2025-01-05T11:00:00Z',
    updatedDate: '2025-01-09T13:00:00Z'
  },
  {
    id: 'template-4',
    name: 'Legacy IT Governance',
    description: 'Previous version of IT Governance template - deprecated',
    decisionBoardId: 'board-1',
    decisionBoardName: 'IT Governance',
    isActive: false,
    fieldCount: 8,
    categoryCount: 2,
    createdDate: '2024-12-01T10:00:00Z',
    updatedDate: '2025-01-05T10:00:00Z'
  }
])

// Computed
const filteredTemplates = computed(() => {
  let filtered = templates.value

  if (search.value) {
    const searchLower = search.value.toLowerCase()
    filtered = filtered.filter(t =>
      t.name.toLowerCase().includes(searchLower) ||
      t.description?.toLowerCase().includes(searchLower) ||
      t.decisionBoardName.toLowerCase().includes(searchLower)
    )
  }

  if (filterBoard.value) {
    filtered = filtered.filter(t => t.decisionBoardId === filterBoard.value.id)
  }

  if (filterStatus.value) {
    const isActive = filterStatus.value === 'Active'
    filtered = filtered.filter(t => t.isActive === isActive)
  }

  return filtered
})

// Methods
const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now - date
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))

  if (diffDays === 0) return 'Today'
  if (diffDays === 1) return 'Yesterday'
  if (diffDays < 7) return `${diffDays} days ago`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`

  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}

const viewTemplate = (template) => {
  $q.notify({
    type: 'info',
    message: `Viewing template: ${template.name}`,
    timeout: 1000
  })
  // TODO: Open view-only mode or preview
}

const editTemplate = (template) => {
  router.push(`/templates/${template.id}`)
}

const duplicateTemplate = (template) => {
  $q.dialog({
    title: 'Duplicate Template',
    message: `Create a copy of "${template.name}"?`,
    cancel: true,
    persistent: true
  }).onOk(() => {
    // TODO: API call to duplicate
    $q.notify({
      type: 'positive',
      message: 'Template duplicated successfully'
    })
  })
}

const toggleTemplateStatus = (template) => {
  const action = template.isActive ? 'deactivate' : 'activate'

  $q.dialog({
    title: `${action.charAt(0).toUpperCase() + action.slice(1)} Template`,
    message: `Are you sure you want to ${action} "${template.name}"?`,
    cancel: true,
    persistent: true
  }).onOk(() => {
    template.isActive = !template.isActive

    $q.notify({
      type: 'positive',
      message: `Template ${action}d successfully`
    })
  })
}
</script>

<style scoped lang="scss">
.templates-container {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;

  h4 {
    font-size: 1.75rem;
    font-weight: 600;
    color: #1a1a1a;
  }

  p {
    font-size: 0.875rem;
  }
}

.template-card {
  height: 100%;
  display: flex;
  flex-direction: column;
  border-radius: 12px;
  transition: all 0.2s;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
    transform: translateY(-2px);
  }
}

.card-header {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.template-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 0.25rem;
}

.template-board {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.template-description {
  font-size: 0.875rem;
  line-height: 1.5;
  min-height: 3rem;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.template-stats {
  margin-top: 1rem;
}

.stat-box {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: #f5f7fa;
  border-radius: 8px;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c3e50;
  line-height: 1;
}

.stat-label {
  font-size: 0.75rem;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.template-meta {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.empty-state {
  padding: 3rem 1rem;
  border-radius: 12px;
}

:deep(.q-card__actions) {
  padding: 0.75rem;
}
</style>
