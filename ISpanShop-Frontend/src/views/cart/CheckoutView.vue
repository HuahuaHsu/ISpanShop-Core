<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElLoading } from 'element-plus'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { checkoutApi, type CheckoutRequest } from '@/api/checkout'
import { getMemberProfile } from '@/api/member'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

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

// 計算金額
const subtotal = computed(() => cartStore.totalPrice)
const shippingFee = ref(60)

const selectedCoupon = computed(() => 
  availableCoupons.value.find(c => c.id === selectedCouponId.value)
)

const couponDiscount = computed(() => {
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
  if (!usePoints.value) return 0
  const remaining = subtotal.value - couponDiscount.value
  return Math.min(walletBalance.value, remaining)
})

const finalAmount = computed(() => 
  subtotal.value + shippingFee.value - couponDiscount.value - pointDiscount.value
)

// 初始化
onMounted(async () => {
  if (cartStore.items.length === 0) {
    router.push('/cart')
    return
  }

  try {
    const memberId = authStore.memberInfo?.memberId || 0
    const [couponsRes, walletRes, profileRes] = await Promise.all([
      checkoutApi.getAvailableCoupons(
        cartStore.items[0].storeId,
        subtotal.value,
        cartStore.items.map(i => i.productId)
      ),
      checkoutApi.getWalletBalance(),
      memberId ? getMemberProfile(memberId) : Promise.resolve({ data: null })
    ])
    console.log('Checkout Data JSON:', JSON.stringify({ coupons: couponsRes.data, wallet: walletRes.data }))
    
    availableCoupons.value = couponsRes.data
    // 支援 balance 或 pointBalance 欄位
    walletBalance.value = walletRes.data.pointBalance ?? walletRes.data.balance ?? 0
    
    // 同步更新 store 中的資料並持久化
    authStore.updatePoints(walletBalance.value)

    // ── 自動帶入最佳優惠券 ──
    if (availableCoupons.value.length > 0) {
      let bestCouponId = null
      let maxDiscount = -1

      availableCoupons.value.forEach(c => {
        let discount = 0
        if (c.couponType === 1) {
          discount = c.discountValue
        } else if (c.couponType === 2) {
          discount = Math.round(subtotal.value * (c.discountValue / 100), 0)
          if (c.maximumDiscount) discount = Math.min(discount, c.maximumDiscount)
        }
        
        // 確保折扣不超過小計且找到最大值
        discount = Math.min(discount, subtotal.value)
        if (discount > maxDiscount) {
          maxDiscount = discount
          bestCouponId = c.id
        }
      })

      if (bestCouponId !== null) {
        selectedCouponId.value = bestCouponId
      }
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
  if (!recipient.value.name || !recipient.value.phone || !recipient.value.address) {
    ElMessage.warning('請填寫完整的收件資訊')
    return
  }

  const loading = ElLoading.service({ text: '正在建立訂單...' })
  
  try {
    const payload: CheckoutRequest = {
      userId: authStore.memberInfo?.memberId || 0,
      storeId: cartStore.items[0].storeId,
      usePoints: usePoints.value,
      couponId: selectedCouponId.value,
      items: cartStore.items.map(i => ({
        productId: i.productId,
        variantId: i.variantId || 0,
        unitPrice: i.price,
        quantity: i.quantity,
        productName: i.name,
        variantName: i.variantName || '預設規格'
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
      cartStore.clearCart()

      // 修正：跳轉至後端 Payment 控制器
      const backendBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'
      // 確保沒有重複的斜線，並直接導向 Pay 方法
      const targetUrl = `${backendBase.replace(/\/$/, '')}${res.data.paymentUrl}`
      console.log('Redirecting to:', targetUrl)
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
      <div class="page-header">
        <el-button @click="router.back()" circle icon="ArrowLeft" class="back-btn" />
        <h1 class="page-title">結帳</h1>
      </div>

      <!-- 收件資訊 -->
      <el-card class="section-card">
        <template #header><div class="card-header">📍 收件資訊</div></template>
        <el-form label-width="80px">
          <el-form-item label="收件人">
            <el-input v-model="recipient.name" placeholder="請輸入姓名" />
          </el-form-item>
          <el-form-item label="電話">
            <el-input v-model="recipient.phone" placeholder="請輸入電話" />
          </el-form-item>
          <el-form-item label="地址">
            <el-input v-model="recipient.address" placeholder="請輸入詳細地址" />
          </el-form-item>
        </el-form>
      </el-card>

      <!-- 支付方式 -->
      <el-card class="section-card">
        <template #header><div class="card-header">💳 支付方式</div></template>
        <el-radio-group v-model="paymentMethod">
          <el-radio label="ECPay" border>綠界支付</el-radio>
          <el-radio label="NewebPay" border>藍新支付</el-radio>
        </el-radio-group>
      </el-card>

      <!-- 商品清單 -->
      <el-card class="section-card">
        <template #header><div class="card-header">🛒 訂單商品</div></template>
        <div v-for="item in cartStore.items" :key="item.productId" class="item-row">
          <el-image :src="item.image" class="item-img" />
          <div class="item-info">
            <div class="item-name">{{ item.name }}</div>
            <div class="item-price">NT$ {{ formatPrice(item.price) }} x {{ item.quantity }}</div>
          </div>
          <div class="item-total">NT$ {{ formatPrice(item.price * item.quantity) }}</div>
        </div>
      </el-card>

      <!-- 折抵選項 -->
      <el-card class="section-card">
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
        <div v-if="couponDiscount > 0" class="summary-row discount">
          <span>優惠券折抵</span>
          <span>- NT$ {{ formatPrice(couponDiscount) }}</span>
        </div>
        <div v-if="pointDiscount > 0" class="summary-row discount">
          <span>點數折抵</span>
          <span>- NT$ {{ formatPrice(pointDiscount) }}</span>
        </div>
        <div class="summary-row final">
          <span>訂單總計</span>
          <span class="price">NT$ {{ formatPrice(finalAmount) }}</span>
        </div>
        <el-button type="primary" size="large" class="submit-btn" @click="handleSubmit">
          下單
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
