<template>
  <div class="todo-page">
    <el-empty
      description="此功能開發中，敬請期待"
      :image-size="120"
    >
      <template #image>
        <el-icon :size="80" class="wip-icon"><Tools /></el-icon>
      </template>
      <p class="route-name">目前頁面：{{ routeName }}</p>
      <el-button type="primary" @click="router.push('/seller')">
        返回賣家中心首頁
      </el-button>
    </el-empty>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Tools } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()

const routeName = computed<string>(() => String(route.name ?? route.path))
</script>

<style scoped>
.todo-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
}
.wip-icon {
  color: #ee4d2d;
  opacity: 0.6;
}
.route-name {
  color: #94a3b8;
  font-size: 13px;
  margin-bottom: 16px;
}
</style>
