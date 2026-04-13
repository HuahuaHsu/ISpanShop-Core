import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/cart',
      name: 'Cart',
      component: () => import('@/views/cart/CartView.vue'),
      meta: {
        layout: 'DefaultLayout', // 確保使用前台版型 [cite: 124, 185]
        title: '購物車'
      }
    },
    {
      path: '/member/order/:id',
      name: 'OrderDetail',
      component: () => import('@/views/member/OrderDetailView.vue'),
      meta: {
        title: '訂單詳情'
      }
    },
    {
      path: '/member/orders',
      name: 'OrderList',
      component: () => import('@/views/member/OrderListView.vue'),
      meta: {
        title: '我的訂單'
      }
    },
    {
      path: '/member/store-dashboard',
      name: 'StoreDashboard',
      component: () => import('@/views/member/StoreDashboardView.vue'),
      meta: {
        title: '賣場數據概覽'
      }
    },
  ],
})

export default router
