import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const routes = [
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard' },
      { path: 'dashboard', component: () => import('../views/DashboardView.vue') },
      { path: 'exercises', component: () => import('../views/ExercisesView.vue') },
      { path: 'pain', component: () => import('../views/PainView.vue') },
      { path: 'activity-log', component: () => import('../views/ActivityLogView.vue') },
      { path: 'appointments', component: () => import('../views/AppointmentsView.vue') },
    ],
  },
  {
    path: '/auth',
    component: () => import('../layouts/AuthLayout.vue'),
    children: [
      { path: 'login', component: () => import('../views/LoginView.vue') },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from) => {
  const auth = useAuthStore();
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { path: '/auth/login', query: { redirect: to.fullPath } };
  }
  if (to.path.startsWith('/auth') && auth.isAuthenticated) {
    return { path: '/dashboard' };
  }
});

export default router;
