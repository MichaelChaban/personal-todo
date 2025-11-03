import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'home',
    component: () => import('../pages/HomePage.vue')
  },
  {
    path: '/meeting-items',
    name: 'meeting-items',
    component: () => import('../pages/MeetingItemsListPage.vue')
  },
  {
    path: '/meeting-items/create',
    name: 'create-meeting-item',
    component: () => import('../pages/MeetingItemPage.vue')
  },
  {
    path: '/meeting-items/:id',
    name: 'edit-meeting-item',
    component: () => import('../pages/MeetingItemPage.vue'),
    props: true
  },
  {
    path: '/templates',
    name: 'templates',
    component: () => import('../pages/ComingSoonPage.vue')
  },
  {
    path: '/meetings',
    name: 'meetings',
    component: () => import('../pages/ComingSoonPage.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
