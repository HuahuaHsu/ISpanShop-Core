import type { RouteRecordRaw } from 'vue-router';
import DefaultLayout from '../layouts/DefaultLayout.vue';
import MemberLayout from '../layouts/MemberLayout.vue';

export const routes: RouteRecordRaw[] = [
  // ── 前台主佈局（Header + Footer） ──
  {
    path: '/',
    component: DefaultLayout,
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('../views/HomeView.vue'),
        meta: { requiresAuth: false }
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('../views/ProductsView.vue'),
        meta: { requiresAuth: false }
      },
      {
        path: 'product/:id',
        name: 'ProductDetail',
        component: () => import('../views/ProductDetailView.vue'),
        meta: { requiresAuth: false }
      },
      {
        path: 'cart',
        name: 'cart',
        component: () => import('../views/cart/CartView.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'coupons',
        name: 'coupons',
        component: () => import('../views/CouponsView.vue'),
        meta: { requiresAuth: false }
      },
      // ── 會員中心嵌套佈局（在 DefaultLayout 內再包一層側邊欄） ──
      {
        path: 'member',
        component: MemberLayout,
        meta: { requiresAuth: true },
        children: [
          {
            path: '',
            name: 'member-center',
            component: () => import('../views/member/MemberCenterView.vue'),
          },
          {
            path: 'profile',
            name: 'member-profile',
            component: () => import('../views/member/ProfileView.vue'),
          },
          {
            path: 'orders',
            name: 'member-orders',
            component: () => import('../views/member/OrdersView.vue'),
          },
          {
            path: 'orders/:id',
            name: 'member-order-detail',
            component: () => import('../views/member/OrderDetailView.vue'),
          },
          {
            path: 'settings',
            name: 'member-settings',
            component: () => import('../views/member/SettingsView.vue'),
          },
          {
            path: 'wallet',
            name: 'member-wallet',
            component: () => import('../views/member/WalletView.vue'),
          },
          {
            path: 'coupons',
            name: 'member-coupons',
            component: () => import('../views/member/MemberCouponsView.vue'),
          },
          {
            path: 'mystore',
            name: 'member-mystore',
            component: () => import('../views/member/MyStoreView.vue'),
          }
        ]
      }
    ]
  },

  // ── 獨立頁面（無 Header/Footer） ──
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/auth/LoginView.vue'),
    meta: { requiresAuth: false, hideForAuth: true }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/auth/RegisterView.vue'),
    meta: { requiresAuth: false, hideForAuth: true }
  },

  // ── 賣家中心（獨立 Layout，與前台完全分離） ──
  {
    path: '/seller',
    component: () => import('../layouts/SellerLayout.vue'),
    meta: { requiresAuth: true, requiresSeller: true },
    children: [
      {
        path: '',
        name: 'SellerDashboard',
        component: () => import('../views/seller/DashboardView.vue'),
      },
      {
        path: 'products',
        name: 'SellerProducts',
        component: () => import('../views/seller/ProductListView.vue'),
      },
      {
        path: 'products/new',
        name: 'SellerProductNew',
        component: () => import('../views/seller/ProductEditView.vue'),
      },
      {
        path: 'products/:id/edit',
        name: 'SellerProductEdit',
        component: () => import('../views/seller/ProductEditView.vue'),
      },
      // ── TODO 佔位路由 ──
      { path: 'orders',             name: 'SellerOrders',     component: () => import('../views/seller/TodoView.vue') },
      { path: 'orders/batch',       name: 'SellerOrdersBatch', component: () => import('../views/seller/TodoView.vue') },
      { path: 'returns',            name: 'SellerReturns',    component: () => import('../views/seller/TodoView.vue') },
      { path: 'promotions',         name: 'SellerPromotions', component: () => import('../views/seller/TodoView.vue') },
      { path: 'coupons',            name: 'SellerCoupons',    component: () => import('../views/seller/TodoView.vue') },
      { path: 'analytics/sales',    name: 'SellerSales',      component: () => import('../views/seller/TodoView.vue') },
      { path: 'analytics/traffic',  name: 'SellerTraffic',    component: () => import('../views/seller/TodoView.vue') },
      { path: 'chat',               name: 'SellerChat',       component: () => import('../views/seller/TodoView.vue') },
    ],
  },

  // ── 錯誤/其他頁面 ──
  {
    path: '/checkout',
    name: 'checkout',
    component: () => import('../views/cart/CheckoutView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/wip',
    name: 'wip',
    component: () => import('../views/error/WipView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('../views/error/NotFoundView.vue')
  }
];
