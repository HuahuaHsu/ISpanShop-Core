<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { createOrderApi } from '@/api/checkout'
import type { CheckoutRequest, CheckoutCartItem } from '@/types/checkout'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

// ── 狀態 ──────────────────────────────────────────
const loading = ref(false)
const usePoints = ref(false)

const form = ref({
  recipientName: authStore.memberInfo?.memberName || '',
  recipientPhone: '',
  recipientAddress: '',
})

// ── 計算屬性 ──────────────────────────────────────
const cartItems = computed(() => cartStore.items)

// 依商店分組（雖然目前後端 API 僅支援單一商店結帳，但前端先做分組展示）
const itemsByStore = computed(() => {
  const groups: Record<number, { storeName: string; items: typeof cartStore.items }> = {}
  cartItems.value.forEach(item => {
    if (!groups[item.storeId]) {
      groups[item.storeId] = { storeName: item.storeName, items: [] }
    }
    groups[item.storeId].items.push(item)
  })
  return groups
})

const subtotal = computed(() => cartStore.totalPrice)
const shippingFee = ref(60) // 假設固定運費
const totalAmount = computed(() => subtotal.value + shippingFee.value)

// ── 行為 ──────────────────────────────────────────
const handleCheckout = async () => {
  if (cartItems.value.length === 0) {
    ElMessage.warning('購物車內沒有商品')
    return
  }

  if (!form.value.recipientName || !form.value.recipientPhone || !form.value.recipientAddress) {
    ElMessage.warning('請填寫完整的收件資訊')
    return
  }

  loading.value = true
  try {
    // 目前簡化邏輯：取購物車中第一個商店的 ID 作為代表 (配合後端 DTO)
    const firstStoreId = cartItems.value[0]?.storeId || 0

    const request: CheckoutRequest = {
      storeId: firstStoreId,
      usePoints: usePoints.value,
      recipientName: form.value.recipientName,
      recipientPhone: form.value.recipientPhone,
      recipientAddress: form.value.recipientAddress,
      items: cartItems.value.map(item => ({
        productId: item.productId,
        variantId: item.variantId || 0,
        productName: item.name,
        variantName: item.specLabel,
        unitPrice: item.price,
        quantity: item.quantity
      }))
    }

    const res = await createOrderApi(request)
    if (res.data.success) {
      ElMessage.success('訂單已建立！')
      cartStore.clearCart()
      // 導向付款頁面（此處可由後端回傳 URL）
      if (res.data.paymentUrl) {
        window.location.href = res.data.paymentUrl
      } else {
        router.push('/member/orders')
      }
    }
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '結帳失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (cartItems.value.length === 0) {
    router.replace('/cart')
  }
})

function formatPrice(price: number): string {
  return price.toLocaleString('zh-TW')
}
</script>

<template>
  <div class="checkout-page">
    <div class="checkout-container">
      <div class="checkout-header">
        <el-button circle @click="router.back()">
          <el-icon><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 18l-6-6 6-6"/></svg></el-icon>
        </el-button>
        <h1 class="checkout-title">結帳</h1>
      </div>

      <div class="checkout-layout">
        <!-- 左側：收件資訊與商品清單 -->
        <div class="checkout-main">
          <!-- 收件資訊 -->
          <el-card class="checkout-card" shadow="never">
            <template #header>
              <div class="card-header">
                <span class="card-title">收件資訊</span>
              </div>
            </template>
            <el-form label-position="top">
              <el-form-item label="收件人姓名" required>
                <el-input v-model="form.recipientName" placeholder="請輸入收件人姓名" />
              </el-form-item>
              <el-form-item label="聯絡電話" required>
                <el-input v-model="form.recipientPhone" placeholder="請輸入聯絡電話" />
              </el-form-item>
              <el-form-item label="收件地址" required>
                <el-input v-model="form.recipientAddress" placeholder="請輸入完整收件地址" />
              </el-form-item>
            </el-form>
          </el-card>

          <!-- 商品清單 -->
          <el-card
            v-for="(group, storeId) in itemsByStore"
            :key="storeId"
            class="checkout-card product-card"
            shadow="never"
          >
            <template #header>
              <div class="store-header">
                <span class="store-icon">🏪</span>
                <span class="store-name">{{ group.storeName }}</span>
              </div>
            </template>
            <div class="order-item-list">
              <div v-for="item in group.items" :key="`${item.productId}-${item.variantId}`" class="order-item">
                <el-image :src="item.image" class="item-image" fit="cover" />
                <div class="item-info">
                  <div class="item-name">{{ item.name }}</div>
                  <div class="item-spec">{{ item.specLabel }}</div>
                  <div class="item-price-qty">
                    <span class="item-price">NT$ {{ formatPrice(item.price) }}</span>
                    <span class="item-qty">x {{ item.quantity }}</span>
                  </div>
                </div>
                <div class="item-subtotal">
                  NT$ {{ formatPrice(item.price * item.quantity) }}
                </div>
              </div>
            </div>
          </el-card>
        </div>

        <!-- 右側：結帳金額摘要 -->
        <div class="checkout-aside">
          <el-card class="summary-card" shadow="never">
            <template #header>
              <span class="card-title">付款摘要</span>
            </template>
            <div class="summary-row">
              <span>商品小計</span>
              <span>NT$ {{ formatPrice(subtotal) }}</span>
            </div>
            <div class="summary-row">
              <span>運費</span>
              <span>NT$ {{ formatPrice(shippingFee) }}</span>
            </div>
            
            <el-divider />
            
            <div class="summary-row points-row">
              <div class="points-label">
                <span>使用點數折抵</span>
                <span class="points-hint">(餘額: {{ authStore.memberInfo?.pointBalance || 0 }})</span>
              </div>
              <el-switch v-model="usePoints" />
            </div>

            <el-divider />

            <div class="summary-row total-row">
              <span class="total-label">總計</span>
              <span class="total-price">NT$ {{ formatPrice(totalAmount) }}</span>
            </div>

            <el-button
              type="primary"
              size="large"
              class="checkout-btn"
              :loading="loading"
              @click="handleCheckout"
            >
              確認下單
            </el-button>
          </el-card>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.checkout-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px 0 60px;
}
.checkout-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 0 16px;
}
.checkout-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
}
.checkout-title {
  font-size: 24px;
  font-weight: 700;
  margin: 0;
}
.checkout-layout {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: 20px;
}
@media (max-width: 900px) {
  .checkout-layout {
    grid-template-columns: 1fr;
  }
}
.checkout-card {
  margin-bottom: 20px;
  border-radius: 8px;
}
.card-title {
  font-size: 16px;
  font-weight: 700;
}
.store-header {
  display: flex;
  align-items: center;
  gap: 8px;
}
.store-icon {
  font-size: 18px;
}
.store-name {
  font-weight: 600;
  color: #333;
}
.order-item-list {
  display: flex;
  flex-direction: column;
}
.order-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 0;
  border-bottom: 1px solid #f0f0f0;
}
.order-item:last-child {
  border-bottom: none;
}
.item-image {
  width: 70px;
  height: 70px;
  border-radius: 4px;
  flex-shrink: 0;
}
.item-info {
  flex: 1;
  min-width: 0;
}
.item-name {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.item-spec {
  font-size: 12px;
  color: #909399;
  margin-bottom: 6px;
}
.item-price-qty {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.item-price {
  font-size: 13px;
  color: #606266;
}
.item-qty {
  font-size: 13px;
  color: #909399;
}
.item-subtotal {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  min-width: 100px;
  text-align: right;
}

.checkout-aside {
  position: sticky;
  top: 20px;
  height: fit-content;
}
.summary-card {
  border-radius: 8px;
}
.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 14px;
  color: #606266;
}
.points-row {
  margin: 16px 0;
}
.points-label {
  display: flex;
  flex-direction: column;
}
.points-hint {
  font-size: 11px;
  color: #909399;
}
.total-row {
  margin-top: 8px;
  color: #333;
}
.total-label {
  font-size: 16px;
  font-weight: 700;
}
.total-price {
  font-size: 24px;
  font-weight: 700;
  color: #EE4D2D;
}
.checkout-btn {
  width: 100%;
  margin-top: 24px;
  height: 50px;
  font-size: 16px;
  font-weight: 700;
  background-color: #EE4D2D !important;
  border-color: #EE4D2D !important;
}
</style>
