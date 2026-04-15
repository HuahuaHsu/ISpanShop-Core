<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router';
const route = useRoute();
const router = useRouter();
const title = (route.query.title as string) || '功能開發中';
</script>

<template>
  <div class="page-container">
    <div class="header">
      <el-button @click="router.back()" circle icon="ArrowLeft" />
      <h2>{{ title }}</h2>
    </div>
    <div class="content">
      <div class="wip-container">
        <div class="wip-emoji">🚧</div>
        <h3>{{ title }} 正在努力開發中</h3>
        <p>此功能即將上線，敬請期待您的體驗</p>
        <el-button type="primary" @click="router.push('/member')">返回會員中心</el-button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px;
}
.header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 30px;
}
.wip-container {
  text-align: center;
  margin-top: 100px;
}
.wip-emoji {
  font-size: 64px;
}
</style>
