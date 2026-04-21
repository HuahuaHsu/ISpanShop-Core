<template>
  <div class="page-container">
    <div class="maintenance-box">
      <div class="icon-wrap">
        <el-icon :size="60"><Location /></el-icon>
      </div>
      <h2>地址管理功能維護中</h2>
      <p>我們正在優化地址管理流程，此功能將於近期開放，敬請期待。</p>
      <div class="progress-bar">
        <el-progress :percentage="60" status="warning" />
      </div>
      <el-button type="primary" plain @click="$router.push('/member')">返回會員中心</el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Location } from '@element-plus/icons-vue'
</script>

<style scoped>
.page-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 500px;
}
.maintenance-box {
  text-align: center;
  background: white;
  padding: 60px 40px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  max-width: 500px;
}
.icon-wrap {
  color: #ee4d2d;
  margin-bottom: 24px;
}
h2 {
  font-size: 24px;
  color: #1e293b;
  margin-bottom: 12px;
}
p {
  color: #64748b;
  line-height: 1.6;
  margin-bottom: 30px;
}
.progress-bar {
  margin: 0 auto 30px;
  max-width: 300px;
}
</style>
