<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, Plus, Minus } from '@element-plus/icons-vue'
import { useCartStore } from '@/stores/cart'

const router = useRouter()
const cartStore = useCartStore()

const isEmpty = computed(() => cartStore.items.length === 0)

const groupedItems = computed(() => {
  const groups: Record<number, { 
    id: number, 
    name: string, 
    status: number, 
    items: any[], 
    storePromotions: any[] 
  }> = {}
  
  cartStore.items.forEach(item => {
    if (!groups[item.storeId]) {
      groups[item.storeId] = {
        id: item.storeId,
        name: item.storeName,
        status: item.storeStatus,
        items: [],
        storePromotions: []
      }
    }
    groups[item.storeId].items.push(item)
  })

  // 計算每個賣場的活動進度與折扣
  Object.values(groups).forEach(group => {
    const promoMap: Record<number, any> = {}
    group.items.forEach(item => {
      const currentPrice = item.promoPrice ?? item.price
      item.promotions.forEach((p: any) => {
        if (!promoMap[p.promotionId]) {
          promoMap[p.promotionId] = { ...p, currentTotal: 0, appliedDiscount: 0 }
        }
        if (item.selected) {
          promoMap[p.promotionId].currentTotal += currentPrice * item.quantity
        }
      })
    })
    
    // 計算滿額折扣 (Type 2)
    group.storePromotions = Object.values(promoMap).map(promo => {
      if (promo.promotionType === 2 && promo.currentTotal >= promo.threshold) {
        if (promo.discountType === 1) { // 固定金額
          promo.appliedDiscount = promo.discountValue
        } else if (promo.discountType === 2) { // 百分比
          promo.appliedDiscount = Math.round(promo.currentTotal * (promo.discountValue / 100), 0)
        }
      }
      return promo
    })
  })

  return Object.values(groups)
})

// 計算整台購物車的最終折扣金額 (滿額折扣總和)
const totalStoreDiscount = computed(() => {
  return groupedItems.value.reduce((total, group) => {
    return total + group.storePromotions.reduce((sum, promo) => sum + (promo.appliedDiscount || 0), 0)
  }, 0)
})

const finalTotal = computed(() => {
  return cartStore.selectedPrice - totalStoreDiscount.value
})

/** 檢查已勾選項目中是否有休假中賣場的商品 */
const hasVacationItems = computed(() => {
  return cartStore.items.filter(item => item.selected).some(item => item.storeStatus === 2)
})

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
  if (hasVacationItems.value) {
    ElMessage.error('購物車包含休假中賣場的商品，請先移除後再結帳')
    return
  }
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
        <div v-if="hasVacationItems" class="vacation-warning">
          <el-alert
            title="部分賣場休假中"
            type="warning"
            description="購物車內有商品所屬賣場正在休假，請移除該商品後再進行結帳。"
            show-icon
            :closable="false"
          />
        </div>

        <div class="cart-groups">
          <div v-for="group in groupedItems" :key="group.id" class="store-group">
            <div class="store-header">
              <el-icon class="store-icon"><svg viewBox="0 0 1024 1024" width="16" height="16"><path d="M912 216H112c-17.7 0-32 14.3-32 32v560c0 17.7 14.3 32 32 32h800c17.7 0 32-14.3 32-32V248c0-17.7-14.3-32-32-32zM216 752H144V280h72v472z m664 0h-72V280h72v472z m-136 0H288V280h356v472z" fill="currentColor"></path></svg></el-icon>
              <span class="store-name">{{ group.name }}</span>
            </div>

            <!-- 活動提示區 -->
            <div v-if="group.storePromotions.length > 0" class="promotion-alerts">
              <div v-for="promo in group.storePromotions" :key="promo.promotionId" class="promo-alert">
                <el-tag size="small" type="danger" effect="plain" class="promo-tag">
                  {{ promo.promotionTypeText }}
                </el-tag>
                <span class="promo-text">
                  <span class="promo-name">{{ promo.name }}</span>
                  <template v-if="promo.promotionType === 2">
                    <span v-if="promo.currentTotal >= promo.threshold" class="promo-met">
                      ：已滿足門檻 (NT$ {{ formatPrice(promo.threshold) }})，可享折扣！
                    </span>
                    <span v-else class="promo-unmet">
                      ：再湊 <span class="highlight">NT$ {{ formatPrice(promo.threshold - promo.currentTotal) }}</span> 滿足門檻
                    </span>
                  </template>
                  <template v-else>
                    ：{{ promo.description }}
                  </template>
                </span>
              </div>
            </div>

            <div class="cart-list">
              <div
                v-for="item in group.items"
                :key="`${item.productId}-${item.variantId}`"
                class="cart-item"
                :class="{ 'is-vacation': item.storeStatus === 2 }"
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
              >
                <el-tag v-if="item.storeStatus === 2" type="warning" size="small" effect="dark" class="mr-1">休假中</el-tag>
                {{ item.name }}
              </div>
              <div v-if="item.specLabel" class="item-spec">{{ item.specLabel }}</div>
              <div class="item-price">
                <template v-if="item.promoPrice">
                  <span class="original-price">NT$ {{ formatPrice(item.price) }}</span>
                  <span class="promo-price">NT$ {{ formatPrice(item.promoPrice) }}</span>
                </template>
                <template v-else>
                  NT$ {{ formatPrice(item.price) }}
                </template>
              </div>
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
              NT$ {{ formatPrice((item.promoPrice ?? item.price) * item.quantity) }}
            </div>
          </div>
            </div>
          </div>
        </div>

        <!-- 底部結帳列 -->
        <div class="cart-footer">
          <div class="footer-left" v-if="totalStoreDiscount > 0">
            <span class="discount-label">活動折扣：</span>
            <span class="discount-amount">- NT$ {{ formatPrice(totalStoreDiscount) }}</span>
          </div>
          <div class="footer-right">
            <span class="total-label">合計 ({{ cartStore.selectedQuantity }} 件)：</span>
            <span class="total-price">NT$ {{ formatPrice(finalTotal) }}</span>
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
.vacation-warning {
  margin-bottom: 16px;
}
.cart-groups {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 24px;
}
.store-group {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 12px rgba(0,0,0,0.05);
}
.store-header {
  padding: 12px 20px;
  background: #fafafa;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  align-items: center;
  gap: 8px;
}
.store-icon {
  color: #606266;
}
.store-name {
  font-weight: 600;
  color: #303133;
}
.promotion-alerts {
  padding: 10px 20px;
  background: #fffafa;
  border-bottom: 1px solid #fff0f0;
}
.promo-alert {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
  margin-bottom: 4px;
}
.promo-alert:last-child {
  margin-bottom: 0;
}
.promo-text {
  color: #606266;
}
.promo-name {
  font-weight: 500;
  color: #303133;
}
.promo-met {
  color: #67C23A;
  font-weight: 500;
}
.promo-unmet {
  color: #909399;
}
.highlight {
  color: #f56c6c;
  font-weight: 600;
}
.cart-list {
  background: white;
}
.cart-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.2s;
}
.cart-item.is-vacation {
  background-color: #fcfcfc;
}
.cart-item.is-vacation .item-image {
  filter: grayscale(0.5);
  opacity: 0.8;
}
.mr-1 {
  margin-right: 4px;
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
  display: flex;
  flex-direction: column;
}
.original-price {
  text-decoration: line-through;
  color: #909399;
  font-size: 12px;
}
.promo-price {
  color: #EE4D2D;
  font-weight: 500;
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
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 -4px 20px rgba(0,0,0,0.1);
  margin-top: 20px;
}
.footer-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.discount-label {
  font-size: 14px;
  color: #606266;
}
.discount-amount {
  color: #f56c6c;
  font-weight: 600;
  font-size: 16px;
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
