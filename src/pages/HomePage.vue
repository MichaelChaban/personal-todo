<template>
  <q-page class="q-pa-md">
    <div class="dashboard-container">
      <!-- Welcome Section -->
      <div class="welcome-section q-mb-xl">
        <h4 class="q-ma-none q-mb-sm">Welcome to Meeting Items Management</h4>
        <p class="text-grey-7">Manage your decision board meeting items efficiently</p>
      </div>

      <!-- Stats Cards -->
      <div class="row q-col-gutter-md q-mb-xl">
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="stat-card">
                <q-icon name="description" size="3rem" color="blue" />
                <div class="stat-info">
                  <div class="stat-value">{{ stats.total }}</div>
                  <div class="stat-label">Total Items</div>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="stat-card">
                <q-icon name="pending" size="3rem" color="orange" />
                <div class="stat-info">
                  <div class="stat-value">{{ stats.submitted }}</div>
                  <div class="stat-label">Submitted</div>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="stat-card">
                <q-icon name="event" size="3rem" color="purple" />
                <div class="stat-info">
                  <div class="stat-value">{{ stats.planned }}</div>
                  <div class="stat-label">Planned</div>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="stat-card">
                <q-icon name="check_circle" size="3rem" color="green" />
                <div class="stat-info">
                  <div class="stat-value">{{ stats.discussed }}</div>
                  <div class="stat-label">Discussed</div>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-6">
          <q-card flat bordered>
            <q-card-section>
              <h6 class="q-ma-none q-mb-md">Quick Actions</h6>
              <div class="q-gutter-sm">
                <q-btn
                  unelevated
                  color="primary"
                  icon="add"
                  label="Create Meeting Item"
                  class="full-width"
                  @click="$router.push('/meeting-items/create')"
                />
                <q-btn
                  outline
                  color="primary"
                  icon="list"
                  label="View All Items"
                  class="full-width"
                  @click="$router.push('/meeting-items')"
                />
              </div>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-md-6">
          <q-card flat bordered>
            <q-card-section>
              <h6 class="q-ma-none q-mb-md">Recent Activity</h6>
              <q-list separator>
                <q-item v-for="activity in recentActivity" :key="activity.id">
                  <q-item-section avatar>
                    <q-avatar :color="activity.color" text-color="white" size="sm">
                      <q-icon :name="activity.icon" />
                    </q-avatar>
                  </q-item-section>
                  <q-item-section>
                    <q-item-label>{{ activity.title }}</q-item-label>
                    <q-item-label caption>{{ activity.date }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref } from 'vue'

const stats = ref({
  total: 24,
  submitted: 8,
  planned: 12,
  discussed: 4
})

const recentActivity = ref([
  {
    id: 1,
    title: 'New meeting item submitted',
    date: '2 hours ago',
    icon: 'add_circle',
    color: 'blue'
  },
  {
    id: 2,
    title: 'Meeting item approved',
    date: '5 hours ago',
    icon: 'check_circle',
    color: 'green'
  },
  {
    id: 3,
    title: 'Document uploaded',
    date: '1 day ago',
    icon: 'upload_file',
    color: 'orange'
  }
])
</script>

<style scoped lang="scss">
.dashboard-container {
  max-width: 1400px;
  margin: 0 auto;
}

.welcome-section {
  h4 {
    font-size: 2rem;
    font-weight: 600;
    color: #1a1a1a;
  }

  p {
    font-size: 1rem;
  }
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1rem;

  .stat-info {
    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: #1a1a1a;
      line-height: 1;
    }

    .stat-label {
      font-size: 0.875rem;
      color: #757575;
      margin-top: 0.25rem;
    }
  }
}
</style>
