import { createRouter, createWebHistory } from 'vue-router';
import { routes } from './routes';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

// 全域路由守衛
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const isLoggedIn = authStore.isLoggedIn;

  // 1. 檢查頁面是否需要登入
  if (to.meta.requiresAuth && !isLoggedIn) {
    // 未登入且進入需要登入的頁面 -> 導向 /login
    return next({ name: 'login', query: { redirect: to.fullPath } });
  }

  // 2. 檢查是否為已登入不應進入的頁面 (例如登入後進 /login)
  if (to.meta.hideForAuth && isLoggedIn) {
    // 已登入但進入 /login 或 /register -> 導向首頁
    return next({ name: 'home' });
  }

  // 正常跳轉
  next();
});

export default router;
