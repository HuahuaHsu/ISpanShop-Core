<script setup lang="ts">
import { useAuthStore } from '../../stores/auth';
const authStore = useAuthStore();
</script>

<template>
  <div class="p-8">
    <h1>會員中心</h1>
    <el-descriptions title="個人資料" border>
      <el-descriptions-item label="會員 ID">{{ authStore.memberInfo.memberId }}</el-descriptions-item>
      <el-descriptions-item label="姓名">{{ authStore.memberInfo.memberName }}</el-descriptions-item>
      <el-descriptions-item label="Email">{{ authStore.memberInfo.email }}</el-descriptions-item>
    </el-descriptions>
    <div class="mt-4">
      <el-button type="danger" @click="authStore.logout()">登出</el-button>
    </div>
  </div>
</template>

<style scoped>
.p-8 { padding: 32px; }
.mt-4 { margin-top: 16px; }
</style>
