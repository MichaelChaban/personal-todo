<template>
  <q-page class="q-pa-md">
    <div class="list-container">
      <!-- Header -->
      <div class="page-header q-mb-lg">
        <div>
          <h4 class="q-ma-none q-mb-xs">Meeting Items</h4>
          <p class="text-grey-7 q-ma-none">View and manage all meeting items</p>
        </div>
        <q-btn
          unelevated
          color="primary"
          icon="add"
          label="Create New"
          @click="$router.push('/meeting-items/create')"
        />
      </div>

      <!-- Filters -->
      <q-card flat bordered class="q-mb-md">
        <q-card-section>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input
                v-model="filters.search"
                outlined
                dense
                placeholder="Search by topic..."
                clearable
              >
                <template v-slot:prepend>
                  <q-icon name="search" />
                </template>
              </q-input>
            </div>
            <div class="col-12 col-md-3">
              <q-select
                v-model="filters.status"
                outlined
                dense
                placeholder="All Statuses"
                :options="statusOptions"
                clearable
              />
            </div>
            <div class="col-12 col-md-3">
              <q-select
                v-model="filters.outcome"
                outlined
                dense
                placeholder="All Outcomes"
                :options="outcomeOptions"
                clearable
              />
            </div>
            <div class="col-12 col-md-2">
              <q-btn
                outline
                color="primary"
                label="Clear Filters"
                class="full-width"
                @click="clearFilters"
              />
            </div>
          </div>
        </q-card-section>
      </q-card>

      <!-- Items Table -->
      <q-card flat bordered>
        <q-table
          :rows="filteredItems"
          :columns="columns"
          row-key="id"
          :pagination="pagination"
          flat
          class="meeting-items-table"
        >
          <template v-slot:body-cell-topic="props">
            <q-td :props="props">
              <div class="topic-cell">
                <div class="topic-title">{{ props.row.topic }}</div>
                <div class="topic-subtitle text-grey-7">{{ props.row.digitalProduct }}</div>
              </div>
            </q-td>
          </template>

          <template v-slot:body-cell-status="props">
            <q-td :props="props">
              <q-chip
                :color="getStatusColor(props.row.status)"
                text-color="white"
                size="sm"
              >
                {{ props.row.status }}
              </q-chip>
            </q-td>
          </template>

          <template v-slot:body-cell-outcome="props">
            <q-td :props="props">
              <q-badge :color="getOutcomeColor(props.row.outcome)">
                {{ props.row.outcome }}
              </q-badge>
            </q-td>
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn
                flat
                round
                dense
                icon="edit"
                size="sm"
                color="primary"
                @click="$router.push(`/meeting-items/${props.row.id}`)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-btn>
              <q-btn
                flat
                round
                dense
                icon="visibility"
                size="sm"
                color="grey-7"
              >
                <q-tooltip>View Details</q-tooltip>
              </q-btn>
            </q-td>
          </template>
        </q-table>
      </q-card>
    </div>
  </q-page>
</template>

<script setup>
import { ref, computed } from 'vue'

const filters = ref({
  search: '',
  status: null,
  outcome: null
})

const statusOptions = ['Submitted', 'Proposed', 'Planned', 'Discussed', 'Denied']
const outcomeOptions = ['Decision', 'Discussion', 'Information']

const pagination = ref({
  rowsPerPage: 10
})

const columns = [
  {
    name: 'topic',
    label: 'Topic',
    align: 'left',
    field: 'topic',
    sortable: true
  },
  {
    name: 'status',
    label: 'Status',
    align: 'center',
    field: 'status',
    sortable: true
  },
  {
    name: 'outcome',
    label: 'Outcome',
    align: 'center',
    field: 'outcome',
    sortable: true
  },
  {
    name: 'requestor',
    label: 'Requestor',
    align: 'left',
    field: 'requestor',
    sortable: true
  },
  {
    name: 'duration',
    label: 'Duration',
    align: 'center',
    field: 'duration',
    sortable: true,
    format: (val) => `${val} min`
  },
  {
    name: 'submissionDate',
    label: 'Submitted',
    align: 'left',
    field: 'submissionDate',
    sortable: true,
    format: (val) => new Date(val).toLocaleDateString()
  },
  {
    name: 'actions',
    label: 'Actions',
    align: 'center'
  }
]

// Mock data
const meetingItems = ref([
  {
    id: '1',
    topic: 'Cloud Migration Strategy',
    digitalProduct: 'Infrastructure Platform',
    status: 'Submitted',
    outcome: 'Decision',
    requestor: 'USER123',
    duration: 30,
    submissionDate: '2025-01-15T10:00:00'
  },
  {
    id: '2',
    topic: 'Q1 Security Audit Results',
    digitalProduct: 'Security Framework',
    status: 'Planned',
    outcome: 'Information',
    requestor: 'USER456',
    duration: 45,
    submissionDate: '2025-01-18T14:30:00'
  },
  {
    id: '3',
    topic: 'Mobile App Feature Approval',
    digitalProduct: 'Mobile Banking',
    status: 'Discussed',
    outcome: 'Decision',
    requestor: 'USER789',
    duration: 60,
    submissionDate: '2025-01-10T09:00:00'
  },
  {
    id: '4',
    topic: 'API Gateway Implementation',
    digitalProduct: 'Integration Layer',
    status: 'Proposed',
    outcome: 'Discussion',
    requestor: 'USER321',
    duration: 40,
    submissionDate: '2025-01-20T11:00:00'
  }
])

const filteredItems = computed(() => {
  return meetingItems.value.filter(item => {
    const matchesSearch = !filters.value.search ||
      item.topic.toLowerCase().includes(filters.value.search.toLowerCase()) ||
      item.digitalProduct.toLowerCase().includes(filters.value.search.toLowerCase())

    const matchesStatus = !filters.value.status || item.status === filters.value.status
    const matchesOutcome = !filters.value.outcome || item.outcome === filters.value.outcome

    return matchesSearch && matchesStatus && matchesOutcome
  })
})

const getStatusColor = (status) => {
  const colors = {
    'Submitted': 'orange',
    'Proposed': 'blue',
    'Planned': 'purple',
    'Discussed': 'green',
    'Denied': 'red'
  }
  return colors[status] || 'grey'
}

const getOutcomeColor = (outcome) => {
  const colors = {
    'Decision': 'primary',
    'Discussion': 'info',
    'Information': 'secondary'
  }
  return colors[outcome] || 'grey'
}

const clearFilters = () => {
  filters.value = {
    search: '',
    status: null,
    outcome: null
  }
}
</script>

<style scoped lang="scss">
.list-container {
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

.topic-cell {
  .topic-title {
    font-weight: 500;
    color: #1a1a1a;
    margin-bottom: 0.25rem;
  }

  .topic-subtitle {
    font-size: 0.75rem;
  }
}

:deep(.meeting-items-table) {
  .q-table__top {
    padding: 1rem;
  }
}
</style>
