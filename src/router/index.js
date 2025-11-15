import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'home',
    component: () => import('../pages/HomePage.vue')
  },
  {
    path: '/meeting-items',
    name: 'meeting-items-list',
    component: () => import('../pages/meetingItems/MeetingItemsListPage.vue')
  },
  {
    path: '/meeting-items/create',
    name: 'meeting-item-create',
    component: () => import('../pages/meetingItems/MeetingItemPage.vue')
  },
  {
    path: '/meeting-items/:id',
    name: 'meeting-item-edit',
    component: () => import('../pages/meetingItems/MeetingItemPage.vue'),
    props: true
  },
  {
    path: '/templates',
    name: 'templates',
    component: () => import('../pages/TemplatesListPage.vue')
  },
  {
    path: '/templates/create',
    name: 'create-template',
    component: () => import('../pages/TemplateBuilderPage.vue')
  },
  {
    path: '/templates/:id',
    name: 'edit-template',
    component: () => import('../pages/TemplateBuilderPage.vue'),
    props: true
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
