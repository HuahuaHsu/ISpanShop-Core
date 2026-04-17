<template>
  <div class="seller-layout">
    <!-- 頂部導覽列 -->
    <header class="seller-header">
      <div class="header-left">
        <div class="seller-logo" @click="router.push('/seller')">
          <span class="logo-icon">🏪</span>
          <span class="logo-text">HowBuy <span class="logo-sub">賣家中心</span></span>
        </div>
      </div>
      <div class="header-right">
        <!-- 帳號下拉 -->
        <el-dropdown trigger="click" @command="handleCommand">
          <span class="account-trigger">
            <el-avatar :size="32" class="account-avatar">
              {{ authStore.memberInfo.account?.charAt(0).toUpperCase() ?? 'S' }}
            </el-avatar>
            <span class="account-name">{{ authStore.memberInfo.account ?? '賣家' }}</span>
            <el-icon class="account-arrow"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="storefront">
                <el-icon><House /></el-icon> 回到前台
              </el-dropdown-item>
              <el-dropdown-item command="logout" divided>
                <el-icon><SwitchButton /></el-icon> 登出
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </header>

    <div class="seller-body">
      <!-- 左側選單 -->
      <aside class="seller-sidebar" :class="{ collapsed: isCollapsed }">
        <el-menu
          :default-active="activeMenu"
          :collapse="isCollapsed"
          :collapse-transition="true"
          router
          class="sidebar-menu"
        >
          <!-- 首頁 -->
          <el-menu-item index="/seller">
            <el-icon><DataAnalysis /></el-icon>
            <template #title>首頁</template>
          </el-menu-item>

          <!-- 商品管理 -->
          <el-sub-menu index="products">
            <template #title>
              <el-icon><Box /></el-icon>
              <span>商品管理</span>
            </template>
            <el-menu-item index="/seller/products">
              <el-icon><List /></el-icon>
              <template #title>我的商品</template>
            </el-menu-item>
            <el-menu-item index="/seller/products/new">
              <el-icon><Plus /></el-icon>
              <template #title>新增商品</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 訂單管理 -->
          <el-sub-menu index="orders">
            <template #title>
              <el-icon><Document /></el-icon>
              <span>訂單管理</span>
            </template>
            <el-menu-item index="/seller/orders">
              <el-icon><List /></el-icon>
              <template #title>我的訂單</template>
            </el-menu-item>
            <el-menu-item index="/seller/returns">
              <el-icon><RefreshLeft /></el-icon>
              <template #title>退貨 / 退款</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 促銷管理 -->
          <el-sub-menu index="promotions">
            <template #title>
              <el-icon><PriceTag /></el-icon>
              <span>促銷管理</span>
            </template>
            <el-menu-item index="/seller/promotions">
              <el-icon><StarFilled /></el-icon>
              <template #title>行銷活動</template>
            </el-menu-item>
            <el-menu-item index="/seller/coupons">
              <el-icon><Ticket /></el-icon>
              <template #title>優惠券</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 數據中心 -->
          <el-sub-menu index="analytics">
            <template #title>
              <el-icon><TrendCharts /></el-icon>
              <span>數據中心</span>
            </template>
            <el-menu-item index="/seller/analytics/sales">
              <el-icon><Histogram /></el-icon>
              <template #title>銷售報表</template>
            </el-menu-item>
            <el-menu-item index="/seller/analytics/traffic">
              <el-icon><DataLine /></el-icon>
              <template #title>流量分析</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 聊聊管理 -->
          <el-menu-item index="/seller/chat">
            <el-icon><ChatDotRound /></el-icon>
            <template #title>聊聊管理</template>
          </el-menu-item>
        </el-menu>

        <!-- 收合按鈕 -->
        <div class="collapse-btn" @click="toggleCollapse">
          <el-icon>
            <component :is="isCollapsed ? DArrowRight : DArrowLeft" />
          </el-icon>
          <span v-if="!isCollapsed" class="collapse-label">收合選單</span>
        </div>
      </aside>

      <!-- 主內容 -->
      <main class="seller-main" :style="{ marginLeft: isCollapsed ? '64px' : '220px' }">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  ArrowDown, House, SwitchButton,
  DataAnalysis, Box, List, Plus, Document, RefreshLeft,
  PriceTag, StarFilled, Ticket, TrendCharts, Histogram, DataLine,
  ChatDotRound, DArrowLeft, DArrowRight
} from '@element-plus/icons-vue'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const isCollapsed = ref<boolean>(false)
const activeMenu = computed<string>(() => route.path)

function toggleCollapse(): void {
  isCollapsed.value = !isCollapsed.value
}

function handleCommand(command: string): void {
  switch (command) {
    case 'storefront':
      void router.push('/')
      break
    case 'logout':
      authStore.logout()
      ElMessage.success('已登出')
      void router.push('/login')
      break
  }
}
</script>

<style scoped>
.seller-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f5f6fa;
}

/* ── 頂部導覽列 ── */
.seller-header {
  height: 56px;
  background: #1a1a2e;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.seller-logo {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}
.logo-icon { font-size: 22px; }
.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: white;
  letter-spacing: 0.5px;
}
.logo-sub {
  font-size: 14px;
  font-weight: 400;
  color: #ee4d2d;
  margin-left: 6px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.account-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #cbd5e1;
  transition: color 0.2s;
  outline: none;
}
.account-trigger:hover { color: white; }
.account-avatar {
  background: #ee4d2d;
  color: white;
  font-weight: 700;
  font-size: 14px;
}
.account-name { font-size: 14px; }
.account-arrow { font-size: 12px; }

/* ── 主體（側邊 + 內容） ── */
.seller-body {
  display: flex;
  flex: 1;
  margin-top: 56px;
}

/* ── 側邊選單 ── */
.seller-sidebar {
  width: 220px;
  min-height: calc(100vh - 56px);
  background: white;
  position: fixed;
  left: 0;
  top: 56px;
  bottom: 0;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #e8eaf0;
  transition: width 0.3s ease;
  z-index: 900;
  overflow: hidden;
}
.seller-sidebar.collapsed {
  width: 64px;
}

.sidebar-menu {
  flex: 1;
  border-right: none !important;
  overflow-y: auto;
  overflow-x: hidden;
}

/* 選中項目：左邊框橘色 + 淡橘底 */
:deep(.el-menu-item.is-active) {
  color: #ee4d2d !important;
  background-color: #fff7ed !important;
  border-left: 3px solid #ee4d2d;
}
:deep(.el-menu-item:hover) {
  background-color: #fff7ed !important;
  color: #ee4d2d !important;
}
:deep(.el-sub-menu__title:hover) {
  background-color: #fff7ed !important;
  color: #ee4d2d !important;
}
:deep(.el-sub-menu.is-active > .el-sub-menu__title) {
  color: #ee4d2d !important;
}
:deep(.el-menu-item) {
  height: 48px;
  line-height: 48px;
  font-size: 14px;
}
:deep(.el-sub-menu__title) {
  height: 48px;
  line-height: 48px;
  font-size: 14px;
}

/* 收合按鈕 */
.collapse-btn {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  color: #64748b;
  font-size: 13px;
  border-top: 1px solid #e8eaf0;
  transition: color 0.2s, background 0.2s;
  padding: 0 16px;
}
.collapse-btn:hover {
  color: #ee4d2d;
  background: #fff7ed;
}
.collapse-label {
  white-space: nowrap;
  overflow: hidden;
}

/* ── 右側主內容 ── */
.seller-main {
  flex: 1;
  min-height: calc(100vh - 56px);
  padding: 24px;
  transition: margin-left 0.3s ease;
  overflow-x: hidden;
}
</style>
