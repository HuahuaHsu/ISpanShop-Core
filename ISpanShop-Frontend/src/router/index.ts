import { createRouter, createWebHistory } from 'vue-router';
import { routes } from './routes';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

// 全域路由守衛（Vue Router 4 新寫法：直接 return，不使用 next）
router.beforeEach((to, from) => {
  const authStore = useAuthStore();
  const isLoggedIn = authStore.isLoggedIn;

  // 1. 檢查頁面是否需要登入
  if (to.meta.requiresAuth && !isLoggedIn) {
    // 未登入且進入需要登入的頁面 -> 導向 /login
    return { name: 'login', query: { redirect: to.fullPath } };
  }

  // 2. 檢查賣家身分
  if (to.meta.requiresSeller && !authStore.memberInfo.isSeller) {
    // 雖然已登入但不是賣家，進入賣家頁面 -> 導向賣場申請/狀態檢查頁
    return { name: 'member-mystore' };
  }

  // 3. 檢查是否為已登入不應進入的頁面 (例如登入後進 /login)
  if (to.meta.hideForAuth && isLoggedIn) {
    // 已登入但進入 /login 或 /register -> 導向首頁
    return { name: 'home' };
  }

  // 正常跳轉
  return true;
});

export default router;
