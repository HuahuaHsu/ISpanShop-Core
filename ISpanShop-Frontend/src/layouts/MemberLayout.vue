<!-- src/layouts/MemberLayout.vue -->
<template>
  <div class="layout">
    <!-- 全域導覽 Header (同步首頁配色) -->
    <header class="global-header">
      <div class="header-inner">
        <div class="logo" @click="router.push('/')">
          <span class="logo-icon">🛍️</span>
          <span class="logo-text">HowBuy</span>
        </div>
        <div class="header-right">
          <el-button text class="nav-link" @click="router.push('/')">回首頁</el-button>
          <span class="divider-text">|</span>
          <el-button text class="nav-link" @click="logout">登出</el-button>
        </div>
      </div>
    </header>

    <div class="body">
      <div class="body-inner">
        <!-- 側邊欄 -->
        <aside class="sidebar">
          <!-- 會員資訊區塊 -->
          <div class="sidebar-user-card" @click="router.push('/member/profile')">
            <div class="avatar">
              {{ authStore.memberInfo?.account?.charAt(0).toUpperCase() || 'U' }}
            </div>
            <div class="user-details">
              <span class="username">{{ authStore.memberInfo?.account || '正在讀取...' }}</span>
              <span class="level-badge">
                ★ {{ authStore.memberInfo?.levelName || '一般會員' }}
              </span>
            </div>
          </div>

          <el-menu :default-active="activeMenu" router>
            <el-menu-item index="/member">
              <el-icon><Menu /></el-icon>
              <span>會員中心</span>
            </el-menu-item>
            <el-menu-item index="/member/profile">
              <el-icon><User /></el-icon>
              <span>個人資料</span>
            </el-menu-item>
            <el-menu-item index="/member/orders">
              <el-icon><Document /></el-icon>
              <span>我的訂單</span>
            </el-menu-item>
            <el-menu-item index="/member/wallet">
              <el-icon><Wallet /></el-icon>
              <span>我的錢包</span>
            </el-menu-item>
            <el-menu-item index="/member/settings">
              <el-icon><Setting /></el-icon>
              <span>帳號設定</span>
            </el-menu-item>
          </el-menu>
        </aside>

        <!-- 內容區 -->
        <main class="content-wrapper">
          <router-view />
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { User, Document, Wallet, Setting, Menu } from '@element-plus/icons-vue'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const activeMenu = computed(() => route.path)

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.layout {
  min-height: 100vh;
  background: linear-gradient(180deg, #f1f5f9 0%, #e2e8f0 100%);
}

/* 深色 Header (同步首頁 DefaultLayout) */
.global-header {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 50%, #1e1b4b 100%);
  padding: 18px 0;
  color: white;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}
.header-inner {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
  padding: 0 20px;
}

/* Logo 樣式同步首頁 */
.logo {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: transform 0.3s;
}
.logo:hover {
  transform: scale(1.05);
}
.logo-icon {
  font-size: 24px;
}
.logo-text {
  font-size: 24px;
  font-weight: 800;
  background: linear-gradient(135deg, #EE4D2D 0%, #F3826C 50%, #F7A696 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  text-shadow: 0 0 20px rgba(238, 77, 45, 0.2);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 0;
}
.nav-link {
  color: #cbd5e1 !important;
  font-size: 14px;
  padding: 0 12px;
  transition: color 0.2s;
}
.nav-link:hover {
  color: #EE4D2D !important;
}
.divider-text {
  color: rgba(255, 255, 255, 0.2);
  font-size: 12px;
}

/* 側邊欄與佈局 */
.body-inner {
  max-width: 1200px;
  margin: 30px auto;
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 30px;
  padding: 0 20px;
}

.sidebar {
  background: white;
  border-radius: 12px;
  height: fit-content;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

/* 會員資訊卡片 */
.sidebar-user-card {
  padding: 24px 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  border-bottom: 1px solid #f1f5f9;
  cursor: pointer;
  background: linear-gradient(to bottom right, #ffffff, #f8fafc);
  transition: background 0.2s;
}
.sidebar-user-card:hover {
  background: #f1f5f9;
}
.avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #EE4D2D 0%, #BE3E24 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 18px;
  box-shadow: 0 4px 12px rgba(238, 77, 45, 0.3);
  flex-shrink: 0;
}
.user-details {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.username {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
}
.level-badge {
  font-size: 12px;
  font-weight: 600;
  color: #EE4D2D;
  margin-top: 4px;
}

.content-wrapper {
  min-height: 600px;
}

/* Menu 樣式微調 */
:deep(.el-menu) {
  border-right: none;
  padding: 10px 0;
}
:deep(.el-menu-item) {
  height: 50px;
  line-height: 50px;
  margin: 4px 12px;
  border-radius: 8px;
  color: #64748b;
  font-weight: 500;
}
:deep(.el-menu-item:hover) {
  color: #EE4D2D;
  background-color: #f8fafc;
}
:deep(.el-menu-item.is-active) {
  color: #EE4D2D !important;
  background-color: #fef2f2 !important;
  font-weight: 700;
}
:deep(.el-menu-item .el-icon) {
  font-size: 18px;
  margin-right: 12px;
}
</style>