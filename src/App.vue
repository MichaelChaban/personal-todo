<template>
  <q-layout view="hHh lpR fFf">
    <!-- Header -->
    <q-header elevated class="bg-white text-grey-9">
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          @click="leftDrawerOpen = !leftDrawerOpen"
        />

        <q-toolbar-title class="text-weight-bold">
          Meeting Items Management
        </q-toolbar-title>

        <q-btn flat round dense icon="account_circle" />
      </q-toolbar>
    </q-header>

    <!-- Drawer -->
    <q-drawer
      v-model="leftDrawerOpen"
      show-if-above
      :width="250"
      :breakpoint="500"
      bordered
      class="bg-grey-1"
    >
      <q-scroll-area class="fit">
        <q-list padding>
          <q-item-label header class="text-weight-bold text-grey-8">
            Navigation
          </q-item-label>

          <q-item
            clickable
            v-ripple
            :active="$route.name === 'home'"
            active-class="bg-primary text-white"
            @click="$router.push('/')"
          >
            <q-item-section avatar>
              <q-icon name="home" />
            </q-item-section>
            <q-item-section>
              <q-item-label>Dashboard</q-item-label>
            </q-item-section>
          </q-item>

          <q-item
            clickable
            v-ripple
            :active="$route.name === 'meeting-items'"
            active-class="bg-primary text-white"
            @click="$router.push('/meeting-items')"
          >
            <q-item-section avatar>
              <q-icon name="list" />
            </q-item-section>
            <q-item-section>
              <q-item-label>Meeting Items</q-item-label>
            </q-item-section>
          </q-item>

          <q-item
            clickable
            v-ripple
            :active="$route.name === 'create-meeting-item'"
            active-class="bg-primary text-white"
            @click="$router.push('/meeting-items/create')"
          >
            <q-item-section avatar>
              <q-icon name="add_circle" />
            </q-item-section>
            <q-item-section>
              <q-item-label>Create Meeting Item</q-item-label>
            </q-item-section>
          </q-item>

          <q-separator class="q-my-md" />

          <q-item-label header class="text-weight-bold text-grey-8">
            Administration
          </q-item-label>

          <q-item
            clickable
            v-ripple
            :active="$route.name === 'templates'"
            active-class="bg-primary text-white"
            @click="$router.push('/templates')"
          >
            <q-item-section avatar>
              <q-icon name="dashboard_customize" />
            </q-item-section>
            <q-item-section>
              <q-item-label>Templates</q-item-label>
              <q-item-label caption>Coming Soon</q-item-label>
            </q-item-section>
          </q-item>

          <q-item
            clickable
            v-ripple
            :active="$route.name === 'meetings'"
            active-class="bg-primary text-white"
            @click="$router.push('/meetings')"
          >
            <q-item-section avatar>
              <q-icon name="event" />
            </q-item-section>
            <q-item-section>
              <q-item-label>Meetings</q-item-label>
              <q-item-label caption>Coming Soon</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <!-- Page Content -->
    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const leftDrawerOpen = ref(false)
</script>

<style>
/* Global styles */
body {
  font-family: 'Roboto', sans-serif;
}
</style>
