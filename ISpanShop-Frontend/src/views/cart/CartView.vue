<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, Plus, Minus } from '@element-plus/icons-vue'
import { useCartStore } from '@/stores/cart'

const router = useRouter()
const cartStore = useCartStore()

const isEmpty = computed(() => cartStore.items.length === 0)

function increment(productId: number, variantId: number | null, current: number): void {
  cartStore.updateQty(productId, variantId, current + 1)
}

function decrement(productId: number, variantId: number | null, current: number): void {
  if (current <= 1) {
    void confirmRemove(productId, variantId)
  } else {
    cartStore.updateQty(productId, variantId, current - 1)
  }
}

async function confirmRemove(productId: number, variantId: number | null): Promise<void> {
  try {
    await ElMessageBox.confirm('確定要移除此商品？', '移除商品', {
      confirmButtonText: '確定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    cartStore.removeItem(productId, variantId)
    ElMessage.success('已移除商品')
  } catch {
    // 取消，不做任何事
  }
}

function formatPrice(price: number): string {
  return price.toLocaleString('zh-TW')
}

function handleCheckout(): void {
  router.push('/checkout')
}
</script>

<template>
  <div class="cart-page">
    <div class="cart-container">
      <!-- Header -->
      <div class="cart-header">
        <el-button circle @click="router.back()">
          <el-icon><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 18l-6-6 6-6"/></svg></el-icon>
        </el-button>
        <h1 class="cart-title">購物車</h1>
        <span class="cart-count">（{{ cartStore.totalQuantity }} 件）</span>
      </div>

      <!-- 空購物車 -->
      <div v-if="isEmpty" class="cart-empty">
        <el-empty description="購物車是空的" :image-size="120">
          <el-button type="primary" @click="router.push('/')">去逛逛</el-button>
        </el-empty>
      </div>

      <!-- 購物車清單 -->
      <template v-else>
        <!-- 全選列 -->
        <div class="cart-selection-header">
          <el-checkbox v-model="cartStore.isAllSelected" class="select-all-checkbox">
            全選
          </el-checkbox>
        </div>

        <div class="cart-list">
          <div
            v-for="item in cartStore.items"
            :key="`${item.productId}-${item.variantId}`"
            class="cart-item"
          >
            <!-- 勾選框 -->
            <el-checkbox v-model="item.selected" class="item-checkbox" />

            <!-- 商品圖片 -->
            <el-image
              :src="item.image"
              fit="cover"
              class="item-image"
              @click="router.push(`/product/${item.productId}`)"
            >
              <template #error>
                <div class="image-fallback">🖼️</div>
              </template>
            </el-image>

            <!-- 商品資訊 -->
            <div class="item-info">
              <div
                class="item-name"
                @click="router.push(`/product/${item.productId}`)"
              >{{ item.name }}</div>
              <div v-if="item.specLabel" class="item-spec">{{ item.specLabel }}</div>
              <div class="item-price">NT$ {{ formatPrice(item.price) }}</div>
            </div>

            <!-- 數量控制 -->
            <div class="item-qty">
              <el-button
                circle
                size="small"
                :icon="Minus"
                @click="decrement(item.productId, item.variantId, item.quantity)"
              />
              <span class="qty-num">{{ item.quantity }}</span>
              <el-button
                circle
                size="small"
                :icon="Plus"
                @click="increment(item.productId, item.variantId, item.quantity)"
              />
            </div>

            <!-- 小計 -->
            <div class="item-subtotal">
              NT$ {{ formatPrice(item.price * item.quantity) }}
            </div>

            <!-- 刪除 -->
            <el-button
              circle
              size="small"
              type="danger"
              plain
              :icon="Delete"
              @click="confirmRemove(item.productId, item.variantId)"
            />
          </div>
        </div>

        <!-- 底部結帳列 -->
        <div class="cart-footer">
          <div class="footer-right">
            <span class="total-label">合計 ({{ cartStore.selectedQuantity }} 件)：</span>
            <span class="total-price">NT$ {{ formatPrice(cartStore.selectedPrice) }}</span>
            <el-button type="primary" size="large" @click="handleCheckout" :disabled="cartStore.selectedQuantity === 0">
              結帳
            </el-button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.cart-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px 0 60px;
}
.cart-container {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 16px;
}
.cart-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
  padding: 16px 0 12px;
}
.cart-title {
  font-size: 22px;
  font-weight: 700;
  margin: 0;
}
.cart-count {
  color: #909399;
  font-size: 15px;
}
.cart-selection-header {
  background: white;
  padding: 12px 20px;
  border-radius: 8px;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
}
.cart-empty {
  background: white;
  border-radius: 8px;
  padding: 60px 20px;
}
.cart-list {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 16px;
}
.cart-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.2s;
}
.cart-item:last-child { border-bottom: none; }
.cart-item:hover { background: #fafafa; }
.item-checkbox {
  margin-right: 4px;
}
.item-image {
  width: 80px;
  height: 80px;
  border-radius: 6px;
  flex-shrink: 0;
  cursor: pointer;
  border: 1px solid #eee;
}
.image-fallback {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  background: #f5f5f5;
}
.item-info {
  flex: 1;
  min-width: 0;
}
.item-name {
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  margin-bottom: 4px;
  color: #303133;
}
.item-name:hover { color: #EE4D2D; }
.item-spec {
  font-size: 12px;
  color: #909399;
  margin-bottom: 6px;
}
.item-price {
  font-size: 13px;
  color: #606266;
}
.item-qty {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}
.qty-num {
  min-width: 28px;
  text-align: center;
  font-size: 15px;
  font-weight: 500;
}
.item-subtotal {
  min-width: 90px;
  text-align: right;
  font-size: 15px;
  font-weight: 600;
  color: #EE4D2D;
  flex-shrink: 0;
}
.cart-footer {
  position: sticky;
  bottom: 20px; /* Leave some space from bottom or set to 0 for full width */
  z-index: 100;
  background: white;
  border-radius: 8px;
  padding: 16px 20px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  box-shadow: 0 -4px 20px rgba(0,0,0,0.1);
  margin-top: 20px;
}
.footer-right {
  display: flex;
  align-items: center;
  gap: 16px;
}
.total-label {
  font-size: 15px;
  color: #606266;
}
.total-price {
  font-size: 22px;
  font-weight: 700;
  color: #EE4D2D;
}
</style>
