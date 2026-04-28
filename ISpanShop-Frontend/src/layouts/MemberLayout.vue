<!-- src/layouts/MemberLayout.vue -->
<template>
  <div class="layout">
    <div class="body">
      <div class="body-inner">
        <!-- 側邊欄 -->
        <aside class="sidebar">
          <!-- 會員資訊區塊 -->
          <div class="sidebar-user-card" @click="router.push('/member/profile')">
            <div class="avatar" :style="{ 
              background: `linear-gradient(135deg, ${levelStyle.color} 0%, ${levelStyle.darker} 100%)`,
              boxShadow: `0 4px 12px ${levelStyle.shadow}`
            }">
            <img v-if="authStore.memberInfo?.avatarUrl"
            :src="getFullImageUrl(authStore.memberInfo.avatarUrl)"
            style="width:100%; height:100%; border-radius:50%; object-fit:cover;"/>
            <span v-else>
              {{ authStore.memberInfo?.account?.charAt(0).toUpperCase() || 'U' }}
            </span>
          </div>
            <div class="user-details">
              <span class="username">{{ authStore.memberInfo?.account || '正在讀取...' }}</span>
              <span class="level-badge" :style="{ color: levelStyle.color }">
                ★ {{ authStore.memberInfo?.levelName || '一般會員' }}
              </span>
            </div>
          </div>

          <el-menu :default-active="activeMenu" :default-openeds="['my-account']" router>
            <el-menu-item index="/member">
              <el-icon><Menu /></el-icon>
              <span>會員中心</span>
            </el-menu-item>

            <!-- 我的帳戶 子選單 -->
            <el-sub-menu index="my-account">
              <template #title>
                <el-icon><User /></el-icon>
                <span>我的帳戶</span>
              </template>
              <el-menu-item index="/member/profile">個人資料</el-menu-item>
              <el-menu-item index="/member/address">地址</el-menu-item>
              <el-menu-item index="/member/password">更改密碼</el-menu-item>
              <el-menu-item index="/member/level">會員等級</el-menu-item>
            </el-sub-menu>

            <el-menu-item index="/member/orders">
              <el-icon><Document /></el-icon>
              <span>我的訂單</span>
            </el-menu-item>
            <el-menu-item index="/member/wallet">
              <el-icon><Wallet /></el-icon>
              <span>我的錢包</span>
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
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { User, Document, Wallet, Menu } from '@element-plus/icons-vue'
import { useAuthStore } from '../stores/auth'
import { getFullImageUrl } from '../utils/format'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const activeMenu = computed(() => route.path)

// 根據等級名稱決定顏色 (對應 LevelView.vue 的設定)
const levelStyle = computed(() => {
  const level = authStore.memberInfo?.levelName || '一般會員'
  
  // 金牌 / 黃金
  if (level.includes('金')) {
    return { 
      color: '#f59e0b', 
      darker: '#d97706', 
      shadow: 'rgba(245, 158, 11, 0.4)' 
    }
  } 
  // 銀牌
  else if (level.includes('銀')) {
    return { 
      color: '#64748b', 
      darker: '#475569', 
      shadow: 'rgba(100, 116, 139, 0.4)' 
    }
  }
  // 銅牌 / 一般會員 / 預設品牌橘
  return { 
    color: '#EE4D2D', 
    darker: '#BE3E24', 
    shadow: 'rgba(238, 77, 45, 0.3)' 
  }
})

onMounted(() => {
  authStore.fetchUserInfo()
})


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
  position: sticky;
  top: 30px;
  z-index: 100;
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
:deep(.el-sub-menu__icon-arrow) {
  display: none !important;
}
:deep(.el-sub-menu__title),
:deep(.el-menu-item) {
  height: 50px;
  line-height: 50px;
  margin: 4px 12px;
  border-radius: 8px;
  color: #64748b;
  font-weight: 500;
}
:deep(.el-sub-menu__title:hover),
:deep(.el-menu-item:hover) {
  color: #EE4D2D !important;
  background-color: #f8fafc !important;
}
:deep(.el-menu-item.is-active),
:deep(.el-sub-menu.is-active > .el-sub-menu__title) {
  color: #EE4D2D !important;
  background-color: #fef2f2 !important;
  font-weight: 700;
}
:deep(.el-sub-menu .el-menu-item) {
  margin-left: 20px;
  margin-right: 12px;
}
:deep(.el-menu-item .el-icon),
:deep(.el-sub-menu__title .el-icon) {
  font-size: 18px;
  margin-right: 12px;
}
</style>
