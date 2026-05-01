<template>
  <router-link :to="`/product/${product.id}`" class="product-card-link">
    <div class="product-card">
      <div class="card-image-wrap">
        <el-image
          :src="getFullImageUrl(product.imageUrl) || fallbackImage"
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

        <!-- 活動標籤 -->
        <span v-if="product.promotion" class="card-promo-badge" :class="product.promotion.type">
          {{ product.promotion.typeLabel }}
        </span>
      </div>

      <div class="card-body">
        <p class="card-name">{{ product.name }}</p>

        <!-- 價格區域 -->
        <div class="card-price-area">
          <!-- 有打折活動（限時特賣/限量搶購） -->
          <template v-if="product.promotion && (product.promotion.type === 'flashSale' || product.promotion.type === 'limitedBuy') && product.promotion.discountPrice">
            <div class="card-price-row">
              <span class="card-promo-price">${{ formatPrice(product.promotion.discountPrice) }}</span>
              <span class="card-sold">{{ formatSoldCount(product.soldCount) }}</span>
            </div>
            <div class="card-original-row">
              <span class="card-original-price">${{ formatPrice(product.promotion.originalPrice) }}</span>
              <span class="card-discount-tag">{{ Math.floor(product.promotion.discountPercent! / 10) }}折</span>
            </div>
          </template>
          
          <!-- 滿額折扣活動 -->
          <template v-else-if="product.promotion && product.promotion.type === 'discount'">
            <div class="card-price-row">
              <span class="card-price">${{ formatPrice(product.price) }}</span>
              <span class="card-sold">{{ formatSoldCount(product.soldCount) }}</span>
            </div>
            <span v-if="product.promotion.rule && product.promotion.rule.discountType === 1" class="card-promo-hint">
              滿{{ formatPrice(product.promotion.rule.threshold) }}折{{ product.promotion.rule.discountValue }}
            </span>
          </template>
          
          <!-- 沒有活動 -->
          <template v-else>
            <div class="card-price-row">
              <span class="card-price">${{ formatPrice(product.price) }}</span>
              <span class="card-sold">{{ formatSoldCount(product.soldCount) }}</span>
            </div>
          </template>
        </div>
      </div>
    </div>
  </router-link>
</template>

<script setup lang="ts">
import { Picture } from '@element-plus/icons-vue'
import type { ProductListItem } from '@/types/product'
import { formatPrice, formatSoldCount, getFullImageUrl } from '@/utils/format'

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

/* 價格區域 */
.card-price-area {
  min-height: 44px;
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

/* 活動折扣價 */
.card-promo-price {
  font-size: 18px;
  font-weight: 700;
  color: #dc2626;
  flex-shrink: 0;
}

.card-original-row {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
}

.card-original-price {
  font-size: 13px;
  color: #999;
  text-decoration: line-through;
}

.card-discount-tag {
  display: inline-block;
  background: #fee2e2;
  color: #dc2626;
  padding: 1px 6px;
  border-radius: 3px;
  font-size: 11px;
  font-weight: 600;
}

/* 滿額折扣提示 */
.card-promo-hint {
  display: block;
  font-size: 12px;
  color: #ea580c;
  margin-top: 4px;
}

/* 活動標籤（圖片左上角） */
.card-promo-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  color: #fff;
  z-index: 2;
}
.card-promo-badge.flashSale {
  background: #ef4444;
}
.card-promo-badge.discount {
  background: #f97316;
}
.card-promo-badge.limitedBuy {
  background: #7c3aed;
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
