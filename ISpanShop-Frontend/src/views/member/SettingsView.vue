<script setup lang="ts">
import { useRouter } from 'vue-router';
const router = useRouter();
</script>

<template>
  <div class="page-container">
    <div class="header">
      <el-button @click="router.back()" circle icon="ArrowLeft" />
      <h2>帳號設定</h2>
    </div>
    <div class="content">
      <div class="wip-container">
        <div class="wip-emoji">⚙️</div>
        <h3>設定功能開發中</h3>
        <p>此功能即將上線，敬請期待</p>
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
