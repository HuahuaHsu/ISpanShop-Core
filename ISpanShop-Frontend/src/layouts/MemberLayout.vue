<!-- src/layouts/MemberLayout.vue -->
<template>
  <div class="layout">
    <!-- 共用簡化 Header -->
    <header class="header">
      <div class="header-inner">
        <div class="logo" @click="$router.push('/')">HowBuy</div>
        <div class="user-info">
          <el-icon><User /></el-icon>
          <span>Hi, 王小明</span>
          <el-button text @click="logout">登出</el-button>
        </div>
      </div>
    </header>

    <div class="body">
      <div class="body-inner">
        <!-- 側邊欄 -->
        <aside class="sidebar">
          <div class="user-card">
            <el-avatar :size="60" />
            <div class="user-name">王小明</div>
          </div>
          <el-menu :default-active="activeMenu" router>
            <el-menu-item index="/member/profile">
              <el-icon><User /></el-icon>個人資料
            </el-menu-item>
            <el-menu-item index="/member/orders">
              <el-icon><Document /></el-icon>我的訂單
            </el-menu-item>
            <el-menu-item index="/member/favorites">
              <el-icon><Star /></el-icon>我的收藏
            </el-menu-item>
            <el-menu-item index="/member/address">
              <el-icon><Location /></el-icon>收件地址
            </el-menu-item>
            <el-menu-item index="/member/password">
              <el-icon><Lock /></el-icon>修改密碼
            </el-menu-item>
          </el-menu>
        </aside>

        <!-- 內容區 -->
        <main class="content">
          <router-view />
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { User, Document, Star, Location, Lock } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const activeMenu = computed(() => route.path)

function logout() {
  router.push('/login')
}
</script>

<style scoped>
.layout { min-height: 100vh; background: #f1f5f9; }
.header { background: #1e293b; padding: 15px 0; }
.header-inner {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 30px;
}
.logo {
  color: #EE4D2D;
  font-size: 26px;
  font-weight: bold;
  cursor: pointer;
}
.user-info {
  color: white;
  display: flex;
  align-items: center;
  gap: 10px;
}
.user-info .el-button { color: white; }

.body-inner {
  max-width: 1400px;
  margin: 20px auto;
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 20px;
  padding: 0 30px;
}
.sidebar {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
.user-card {
  padding: 20px;
  text-align: center;
  border-bottom: 1px solid #e5e7eb;
  background: #f8fafc;
}
.user-name { margin-top: 10px; font-weight: bold; color: #1e293b; }
.content {
  background: white;
  border-radius: 8px;
  padding: 30px;
  min-height: 500px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

/* 讓側邊欄選單 active 時是綠色 */
:deep(.el-menu-item.is-active) {
  color: #EE4D2D !important;
  background: #FDEDEA !important;
}
</style>
