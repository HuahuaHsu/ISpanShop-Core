<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElLoading } from 'element-plus'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { checkoutApi, type CheckoutRequest } from '@/api/checkout'
import { getMemberProfile } from '@/api/member'
import { getOrderDetailApi } from '@/api/order'

const router = useRouter()
const route = useRoute()
const cartStore = useCartStore()
const authStore = useAuthStore()

// ── 模式判定 ──
const existingOrderId = computed(() => route.query.orderId ? Number(route.query.orderId) : null)
const isPaymentMode = computed(() => existingOrderId.value !== null)
const isDirectBuyMode = computed(() => route.query.type === 'direct')

// ── 資料來源 ──
const existingOrderData = ref<any>(null)
const directBuyData = ref<any[]>([])

// 表單資料
const recipient = ref({
  name: '資展國際',
  phone: '03-4533013#6524',
  address: '桃園市中壢區新生路二段421號'
})
const paymentMethod = ref('ECPay')

// 優惠券與點數
const availableCoupons = ref<any[]>([])
const selectedCouponId = ref<number | null>(null)
const usePoints = ref(false)
const walletBalance = ref(0)
const showCouponModal = ref(false)

// ── 核心數據切換 ──
const checkoutItems = computed(() => {
  if (isPaymentMode.value) return existingOrderData.value?.items || []
  if (isDirectBuyMode.value) return directBuyData.value
  return cartStore.selectedItems
})

const subtotal = computed(() => {
  if (isPaymentMode.value && existingOrderData.value) {
    return existingOrderData.value.totalAmount
  }
  return checkoutItems.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
})

const shippingFee = ref(60)

const selectedCoupon = computed(() => 
  availableCoupons.value.find(c => c.id === selectedCouponId.value)
)

const couponDiscount = computed(() => {
  if (isPaymentMode.value && existingOrderData.value) {
    const d = existingOrderData.value
    return d.discountAmount ?? d.DiscountAmount ?? 0
  }
  if (!selectedCoupon.value) return 0
  const c = selectedCoupon.value
  if (c.couponType === 1) return c.discountValue
  if (c.couponType === 2) {
    let disc = Math.round(subtotal.value * (c.discountValue / 100), 0)
    if (c.maximumDiscount) disc = Math.min(disc, c.maximumDiscount)
    return disc
  }
  return 0
})

const pointDiscount = computed(() => {
  if (isPaymentMode.value && existingOrderData.value) {
    const d = existingOrderData.value
    return d.pointDiscount ?? d.PointDiscount ?? 0
  }
  if (!usePoints.value) return 0
  const remaining = subtotal.value - couponDiscount.value
  return Math.min(walletBalance.value, remaining)
})

const finalAmount = computed(() => {
  if (isPaymentMode.value && existingOrderData.value) {
    return existingOrderData.value.finalAmount
  }
  return subtotal.value + shippingFee.value - couponDiscount.value - pointDiscount.value
})

// 初始化
onMounted(async () => {
  // 1. 優先處理「支付舊訂單」模式
  if (isPaymentMode.value) {
    try {
      const res = await getOrderDetailApi(existingOrderId.value!)
      existingOrderData.value = res.data
      recipient.value.name = res.data.recipientName
      recipient.value.phone = res.data.recipientPhone
      recipient.value.address = res.data.recipientAddress
      shippingFee.value = res.data.shippingFee || 0
      return
    } catch (err) {
      ElMessage.error('無法載入訂單資訊')
      router.push('/member/orders')
      return
    }
  }

  // 2. 處理「直接購買」模式
  if (isDirectBuyMode.value) {
    const stored = sessionStorage.getItem('TEMP_CHECKOUT_DATA')
    if (!stored) {
      ElMessage.warning('結帳資訊已過期')
      router.push('/')
      return
    }
    directBuyData.value = JSON.parse(stored)
  }

  // 3. 檢查是否有結帳項目
  if (checkoutItems.value.length === 0) {
    router.push('/cart')
    return
  }

  // 4. 加載結帳所需資訊 (優惠券、錢包、個人資料)
  try {
    const memberId = authStore.memberInfo?.memberId || 0
    const [couponsRes, walletRes, profileRes] = await Promise.all([
      checkoutApi.getAvailableCoupons(
        checkoutItems.value[0].storeId,
        subtotal.value,
        checkoutItems.value.map(i => i.productId)
      ),
      checkoutApi.getWalletBalance(),
      memberId ? getMemberProfile(memberId) : Promise.resolve({ data: null })
    ])
    
    availableCoupons.value = couponsRes.data
    walletBalance.value = walletRes.data.pointBalance ?? walletRes.data.balance ?? 0
    
    if (profileRes && profileRes.data) {
      recipient.value.name = profileRes.data.fullName || ''
      recipient.value.phone = profileRes.data.phoneNumber || ''
      const city = profileRes.data.city || ''
      const region = profileRes.data.region || ''
      const street = profileRes.data.address || ''
      if (city || region || street) recipient.value.address = `${city}${region}${street}`
    }
    
    authStore.updatePoints(walletBalance.value)

    // 自動帶入最佳優惠券
    if (availableCoupons.value.length > 0) {
      let bestCouponId = null
      let maxDiscount = -1
      availableCoupons.value.forEach(c => {
        let discount = 0
        if (c.couponType === 1) discount = c.discountValue
        else if (c.couponType === 2) {
          discount = Math.round(subtotal.value * (c.discountValue / 100), 0)
          if (c.maximumDiscount) discount = Math.min(discount, c.maximumDiscount)
        }
        discount = Math.min(discount, subtotal.value)
        if (discount > maxDiscount) {
          maxDiscount = discount
          bestCouponId = c.id
        }
      })
      if (bestCouponId !== null) selectedCouponId.value = bestCouponId
    }
  } catch (err) {
    console.error('Failed to load checkout data', err)
  }
})

// 選擇優惠券
function selectCoupon(id: number | null) {
  selectedCouponId.value = id
  showCouponModal.value = false
}

// 提交訂單
async function handleSubmit() {
  if (isPaymentMode.value && existingOrderData.value) {
    const backendBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'
    const controller = paymentMethod.value === 'NewebPay' ? 'PaymentNewebPay' : 'Payment'
    const targetUrl = `${backendBase.replace(/\/$/, '')}/${controller}/Pay?orderNumber=${existingOrderData.value.orderNumber}`
    ElMessage.success('正在導向支付頁面...')
    window.location.href = targetUrl
    return
  }

  if (!recipient.value.name || !recipient.value.phone || !recipient.value.address) {
    ElMessage.warning('請填寫完整的收件資訊')
    return
  }

  const loading = ElLoading.service({ text: '正在建立訂單...' })
  
  try {
    const payload: CheckoutRequest = {
      userId: authStore.memberInfo?.memberId || 0,
      storeId: checkoutItems.value[0].storeId,
      usePoints: usePoints.value,
      couponId: selectedCouponId.value,
      items: checkoutItems.value.map(i => ({
        productId: i.productId,
        variantId: i.variantId || 0,
        unitPrice: i.price,
        quantity: i.quantity,
        productName: i.name,
        variantName: i.variantName || i.specLabel || '預設規格'
      })),
      recipientName: recipient.value.name,
      recipientPhone: recipient.value.phone,
      recipientAddress: recipient.value.address,
      paymentMethod: paymentMethod.value
    }

    const res = await checkoutApi.createOrder(payload)
    loading.close()
    
    if (res.data.success) {
      ElMessage.success('訂單已建立')
      
      // ── 結帳後清理 ──
      if (isDirectBuyMode.value) {
        sessionStorage.removeItem('TEMP_CHECKOUT_DATA')
      } else {
        // 只有一般購物車結帳才清除購物車中勾選的項目
        cartStore.clearSelectedItems()
      }

      const backendBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'
      const targetUrl = `${backendBase.replace(/\/$/, '')}${res.data.paymentUrl}`
      window.location.href = targetUrl
    }
  } catch (err: any) {
    loading.close()
    ElMessage.error(err.response?.data?.message || '結帳失敗')
  }
}

function formatPrice(val: number) {
  return val.toLocaleString('zh-TW')
}
</script>

<template>
  <div class="checkout-page">
    <div class="checkout-container">
      <h1 class="page-title">{{ isPaymentMode ? '訂單支付' : '結帳' }}</h1>

      <!-- 🛒 訂單商品 -->
      <el-card class="section-card">
        <template #header><div class="card-header">🛒 訂單商品</div></template>
        <div v-for="item in checkoutItems" :key="item.productId + (item.variantId || '')" class="item-row">
          <el-image :src="item.image || item.coverImage" class="item-img" />
          <div class="item-info">
            <div class="item-name">{{ item.name || item.productName }}</div>
            <div class="item-price">NT$ {{ formatPrice(item.price) }} x {{ item.quantity }}</div>
          </div>
          <div class="item-total">NT$ {{ formatPrice(item.price * item.quantity) }}</div>
        </div>
      </el-card>

      <!-- 📍 收件資訊 -->
      <el-card class="section-card">
        <template #header><div class="card-header">📍 收件資訊</div></template>
        <el-form label-width="80px">
          <el-form-item label="收件人">
            <el-input v-model="recipient.name" :disabled="isPaymentMode" placeholder="請輸入姓名" />
          </el-form-item>
          <el-form-item label="電話">
            <el-input v-model="recipient.phone" :disabled="isPaymentMode" placeholder="請輸入電話" />
          </el-form-item>
          <el-form-item label="地址">
            <el-input v-model="recipient.address" :disabled="isPaymentMode" placeholder="請輸入詳細地址" />
          </el-form-item>
        </el-form>
      </el-card>

      <!-- 🧧 優惠與折抵 (僅支付模式顯示舊資訊，其餘顯示互動區塊) -->
      <el-card class="section-card" v-if="isPaymentMode && (pointDiscount > 0 || couponDiscount > 0)">
        <template #header><div class="card-header">🧧 原始訂單折抵資訊</div></template>
        <div class="discount-row" v-if="couponDiscount > 0">
          <div class="label">
            優惠券折抵
            <small v-if="existingOrderData?.couponTitle" class="coupon-name">
              ({{ existingOrderData.couponTitle }})
            </small>
          </div>
          <div class="value discount">- NT$ {{ formatPrice(couponDiscount) }}</div>
        </div>
        <div class="discount-row" v-if="pointDiscount > 0">
          <div class="label">點數折抵</div>
          <div class="value discount">- NT$ {{ formatPrice(pointDiscount) }}</div>
        </div>
      </el-card>

      <el-card class="section-card" v-if="!isPaymentMode">
        <template #header><div class="card-header">🧧 優惠與折抵</div></template>
        <div class="discount-row" @click="showCouponModal = true">
          <div class="label">優惠券</div>
          <div class="value clickable">
            {{ selectedCoupon ? selectedCoupon.title : '選擇優惠券' }}
            <el-icon><ArrowRight /></el-icon>
          </div>
        </div>
        <div class="discount-row">
          <div class="label">
            點數折抵
            <small class="hint">可用 {{ walletBalance }} 點</small>
          </div>
          <div class="value">
            <el-switch v-model="usePoints" :disabled="walletBalance <= 0" />
          </div>
        </div>
      </el-card>

      <!-- 💳 支付方式 -->
      <el-card class="section-card">
        <template #header><div class="card-header">💳 支付方式</div></template>
        <el-radio-group v-model="paymentMethod">
          <el-radio label="ECPay" border>綠界支付</el-radio>
          <el-radio label="NewebPay" border>藍新支付</el-radio>
        </el-radio-group>
      </el-card>

      <!-- 總計資訊 -->
      <div class="summary-section">
        <div class="summary-row">
          <span>商品小計</span>
          <span>NT$ {{ formatPrice(subtotal) }}</span>
        </div>
        <div class="summary-row">
          <span>運費</span>
          <span>NT$ {{ formatPrice(shippingFee) }}</span>
        </div>
        
        <div v-if="couponDiscount > 0" class="summary-row">
          <span>
            優惠券折抵
            <small v-if="isPaymentMode && existingOrderData?.couponTitle" class="coupon-name">
              ({{ existingOrderData.couponTitle }})
            </small>
          </span>
          <span class="discount">- NT$ {{ formatPrice(couponDiscount) }}</span>
        </div>
        <div v-if="pointDiscount > 0" class="summary-row">
          <span>點數折抵</span>
          <span class="discount">- NT$ {{ formatPrice(pointDiscount) }}</span>
        </div>

        <div class="summary-row final">
          <span>{{ isPaymentMode ? '應付總計' : '訂單總計' }}</span>
          <span class="price">NT$ {{ formatPrice(finalAmount) }}</span>
        </div>
        <el-button type="primary" size="large" class="submit-btn" @click="handleSubmit">
          {{ isPaymentMode ? '立即付款' : '下單' }}
        </el-button>
      </div>
    </div>

    <!-- 優惠券選擇 Modal -->
    <el-dialog v-model="showCouponModal" title="選擇優惠券" width="400px">
      <div v-if="availableCoupons.length === 0" class="empty-coupons">
        目前沒有可用的優惠券
      </div>
      <div v-else class="coupon-list">
        <div 
          v-for="c in availableCoupons" 
          :key="c.id" 
          class="coupon-item"
          :class="{ selected: selectedCouponId === c.id }"
          @click="selectCoupon(c.id)"
        >
          <div class="coupon-title">{{ c.title }}</div>
          <div class="coupon-desc">
            {{ c.couponType === 1 ? `現折 $${c.discountValue}` : `打 ${c.discountValue} 折` }}
            <span v-if="c.minimumSpend > 0">，滿 ${{ c.minimumSpend }} 可用</span>
          </div>
        </div>
        <div class="coupon-item none" @click="selectCoupon(null)">不使用優惠券</div>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped>
.checkout-page {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 40px 20px;
}
.checkout-container {
  max-width: 800px;
  margin: 0 auto;
}
.page-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}
.page-title {
  margin: 0;
  font-size: 24px;
  font-weight: bold;
}
.back-btn {
  font-size: 18px;
}
.section-card {
  margin-bottom: 16px;
}
.card-header {
  font-weight: bold;
}
.item-row {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #eee;
}
.item-row:last-child { border-bottom: none; }
.item-img {
  width: 50px;
  height: 50px;
  border-radius: 4px;
  margin-right: 12px;
}
.item-info { flex: 1; }
.item-name { font-size: 14px; }
.item-price { font-size: 12px; color: #999; }
.item-total { font-weight: bold; }

.discount-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #eee;
}
.discount-row:last-child { border-bottom: none; }
.clickable { cursor: pointer; color: #ee4d2d; }
.hint { color: #999; margin-left: 8px; font-weight: normal; }
.coupon-name { color: #ee4d2d; margin-left: 4px; font-weight: normal; }

.summary-section {
  background: white;
  padding: 24px;
  border-radius: 8px;
  margin-top: 24px;
  text-align: right;
}
.summary-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 14px;
}
.discount { color: #ee4d2d; }
.final {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #eee;
  font-size: 18px;
  font-weight: bold;
}
.price { color: #ee4d2d; font-size: 24px; }
.submit-btn {
  margin-top: 24px;
  width: 200px;
}

.coupon-item {
  padding: 16px;
  border: 1px solid #eee;
  border-radius: 8px;
  margin-bottom: 12px;
  cursor: pointer;
}
.coupon-item:hover { border-color: #ee4d2d; background: #fffcfb; }
.coupon-item.selected { border-color: #ee4d2d; background: #fffcfb; border-width: 2px; }
.coupon-title { font-weight: bold; margin-bottom: 4px; }
.coupon-desc { font-size: 12px; color: #666; }
.coupon-item.none { text-align: center; color: #999; }
</style>