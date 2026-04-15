<script setup lang="ts">
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <el-header class="app-header">
    <div class="header-container">
      <div class="logo" @click="router.push('/')">好買HowBuy</div>
      <div class="header-actions">
        <template v-if="authStore.isLoggedIn">
          <span class="member-name">{{ `Hello, ${authStore.memberInfo?.memberName}` }}</span>
          <el-button @click="handleLogout">登出</el-button>
        </template>
        <template v-else>
          <el-button @click="router.push('/login')">登入</el-button>
          <el-button type="primary" @click="router.push('/register')">註冊</el-button>
        </template>
      </div>
    </div>
  </el-header>
</template>

<style scoped>
.app-header {
  border-bottom: 1px solid #eee;
  padding: 0;
  height: 60px;
  background: #fff;
}
.header-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  height: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.logo {
  font-size: 20px;
  font-weight: bold;
  cursor: pointer;
}
.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}
.member-name {
  font-size: 14px;
  color: #606266;
}
</style>
