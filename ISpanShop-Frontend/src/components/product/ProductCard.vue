<template>
  <router-link :to="`/product/${product.id}`" class="product-card-link">
    <div class="product-card">
      <div class="card-image-wrap">
        <el-image
          :src="product.imageUrl || fallbackImage"
          :alt="product.name"
          lazy
          fit="cover"
          class="card-image"
        >
          <template #error>
            <div class="image-error">
              <el-icon :size="40"><Picture /></el-icon>
            </div>
          </template>
        </el-image>
        <div v-if="product.totalStock === 0" class="sold-out-overlay">
          <span class="sold-out-text">已售完</span>
        </div>
      </div>

      <div class="card-body">
        <p class="card-name">{{ product.name }}</p>
        <div class="card-price-row">
          <span class="card-price">${{ formatPrice(product.price) }}</span>
          <span class="card-sold">{{ formatSoldCount(product.soldCount) }}</span>
        </div>
      </div>
    </div>
  </router-link>
</template>

<script setup lang="ts">
import { Picture } from '@element-plus/icons-vue'
import type { ProductListItem } from '@/types/product'
import { formatPrice, formatSoldCount } from '@/utils/format'

const props = defineProps<{
  product: ProductListItem
}>()

// 圖片載入失敗時的佔位圖（data URI 1x1 透明 PNG）
const fallbackImage = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII='

// 避免 props 未使用的 lint 警告
void props
</script>

<style scoped>
.product-card-link {
  display: block;
  text-decoration: none;
  color: inherit;
}

.product-card {
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #f1f5f9;
  background: #fff;
  cursor: pointer;
  transition:
    transform 0.25s ease,
    box-shadow 0.25s ease;
}

.product-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.12);
  border-color: #EE4D2D;
}

/* 正方形圖片區 */
.card-image-wrap {
  position: relative;
  width: 100%;
  padding-top: 100%; /* 1:1 aspect ratio */
  background: #f8fafc;
  overflow: hidden;
}

.card-image {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
}

.image-error {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #cbd5e1;
  background: #f8fafc;
}

/* 文字資訊 */
.card-body {
  padding: 10px 12px 12px;
}

.card-name {
  font-size: 13px;
  color: #334155;
  line-height: 1.4;
  margin: 0 0 8px;
  /* 兩行截斷 */
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  height: 36px;
}

.card-price-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 8px;
}

.card-price {
  font-size: 18px;
  font-weight: 700;
  color: #EE4D2D;
  flex-shrink: 0;
}

.card-sold {
  font-size: 12px;
  color: #999;
  white-space: nowrap;
  flex-shrink: 0;
}

.sold-out-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1;
}

.sold-out-text {
  color: #fff;
  font-size: 18px;
  font-weight: bold;
  background: rgba(0, 0, 0, 0.7);
  padding: 6px 16px;
  border-radius: 4px;
}
</style>
