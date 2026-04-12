import { createRouter, createWebHistory } from 'vue-router'
import DefaultLayout from '@/layouts/DefaultLayout.vue'
import MemberLayout from '@/layouts/MemberLayout.vue'
import BlankLayout from '@/layouts/BlankLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // 前台
    {
      path: '/',
      component: DefaultLayout,
      children: [
        { path: '', name: 'home', component: () => import('@/views/HomeView.vue') },
        // 之後加:商品列表、商品詳情、購物車等
      ],
    },
    // 會員中心
    {
      path: '/member',
      component: MemberLayout,
      children: [
        { path: 'profile', component: () => import('@/views/HomeView.vue') },
        // 之後加各種會員頁
      ],
    },
    // 登入註冊
    {
      path: '/auth',
      component: BlankLayout,
      children: [
        { path: 'login', component: () => import('@/views/HomeView.vue') },
        { path: 'register', component: () => import('@/views/HomeView.vue') },
      ],
    },
  ],
})

export default router
