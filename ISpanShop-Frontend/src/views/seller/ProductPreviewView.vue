<template>
  <div class="preview-wrapper">
    <!-- 預覽提示橫條 -->
    <div class="preview-banner">
      <el-icon><View /></el-icon>
      <span>這是商品預覽畫面，僅您本人可見。審核通過上架後，買家才能看到此商品。</span>
      <el-button size="small" plain @click="goBack">← 回到我的商品</el-button>
    </div>

    <!-- 套用前台商品詳情頁完整版面 -->
    <ProductDetailView :preview-id="productId" :is-preview="true" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { View } from '@element-plus/icons-vue'
import ProductDetailView from '@/views/ProductDetailView.vue'

const route = useRoute()
const router = useRouter()

const productId = computed(() => Number(route.params.id))

function goBack(): void {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/seller/products')
  }
}
</script>

<style scoped>
.preview-wrapper {
  min-height: 100vh;
  background: #f5f5f5;
}

.preview-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 24px;
  background: #fef0e6;
  border-bottom: 2px solid #ff6b35;
  color: #ff6b35;
  font-size: 14px;
  position: sticky;
  top: 0;
  z-index: 100;
}

.preview-banner :deep(.el-icon) {
  font-size: 18px;
}

.preview-banner span {
  flex: 1;
}
</style>