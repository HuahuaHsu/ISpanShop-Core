import { createRouter, createWebHistory } from 'vue-router';
import { routes } from './routes';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

// ✅ 停權帳號「允許」造訪的路由名稱白名單
const BLACKLISTED_ALLOWED_ROUTES = new Set([
  'home',
  'products',
  'ProductDetail',
  'StorePage',
  'member-center',      // 必開：顯示停權通知
  'member-orders',      // 必開：查看歷史訂單
  'member-order-detail',// 必開：查看訂單詳情
  'member-order-refund',
  'member-order-refund-detail',
  'member-support-tickets', // 必開：申訴客服管道
  'login',
  'register',
  'not-found',
  'About'
]);

// 全域路由守衛
router.beforeEach(async (to, from) => {
  const authStore = useAuthStore();
  const isLoggedIn = authStore.isLoggedIn;

  // 0. 即時同步身分狀態 (當下更新)
  if (isLoggedIn) {
    try {
      await authStore.fetchUserInfo();
    } catch {
      // 靜默失敗，不影響導航
    }
  }

  const isBlacklisted = authStore.isBlacklisted;

  // 1. 檢查頁面是否需要登入
  if (to.meta.requiresAuth && !isLoggedIn) {
    return { name: 'login', query: { redirect: to.fullPath } };
  }

  // 2. 停權帳號攔截邏輯
  if (isLoggedIn && isBlacklisted) {
    // 如果進入的路由不在白名單內，強制導向會員中心
    if (!BLACKLISTED_ALLOWED_ROUTES.has(to.name as string)) {
      console.warn(`[Router Guard] 停權帳號試圖進入受限頁面: ${to.path}，已攔截並導向會員中心`);
      return { name: 'member-center' };
    }
  }

  // 3. 檢查賣家權限 (進入 /seller 等 meta 標記為 requiresSeller 的頁面)
  if (to.meta.requiresSeller) {
    if (!isLoggedIn) {
      return { name: 'login', query: { redirect: to.fullPath } };
    }
    // 注意：這裡交由 SellerLayout 的 checkStoreStatus 處理更精細的賣場狀態
  }

  // 4. 檢查是否為已登入不應進入的頁面 (例如登入後進 /login)
  if (to.meta.hideForAuth && isLoggedIn) {
    return { name: 'home' };
  }

  return true;
});

export default router;
