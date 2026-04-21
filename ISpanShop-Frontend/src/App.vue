<template>
  <router-view />
  <!-- 全域登入彈窗 -->
  <AuthDialog />
</template>

<script setup lang="ts">
import AuthDialog from './components/common/AuthDialog.vue'
</script>

<style>
/* 全域樣式 */
body {
  margin: 0;
  font-family: 'Helvetica Neue', Helvetica, 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', '微软雅黑', Arial, sans-serif;
  background-color: #f5f7fa;
}

* {
  box-sizing: border-box;
}
</style>
