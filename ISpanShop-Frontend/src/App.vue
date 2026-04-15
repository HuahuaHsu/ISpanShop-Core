<template>
  <router-view />
</template>

<script setup lang="ts">
// App.vue 作為根容器，只負責渲染路由
// 實際的版型（header/footer）由各個 Layout 負責
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