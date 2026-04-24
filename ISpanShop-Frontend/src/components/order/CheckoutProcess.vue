<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElLoading } from 'element-plus'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { useAddressStore } from '@/stores/address'
import { checkoutApi, type CheckoutRequest } from '@/api/checkout'
import { getMemberProfile, getLevelInfo } from '@/api/member'
import AddressCard from '@/components/member/AddressCard.vue'
import AddressFormDialog from '@/components/member/AddressFormDialog.vue'
import { Plus, ArrowRight, ArrowLeft, Location, Phone, User, Trophy as TrophyIcon, Star as StarIcon } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const cartStore = useCartStore()
const authStore = useAuthStore()
const addressStore = useAddressStore()

// 地址相關
const selectedAddressId = ref<number | null>(null)
const addressDialogVisible = ref(false)

// 會員等級相關
const memberLevel = ref({
  name: '',
  discountRate: 1.0 // 預設不打折
})

// 表單資料 (手動填寫用)
const recipient = ref({
  name: '',
  phone: '',
  city: '',
  region: '',
  street: '',
  address: ''
})

const cities = [
  '台北市', '新北市', '桃園市', '台中市', '台南市', '高雄市',
  '基隆市', '新竹市', '嘉義市', '新竹縣', '苗栗縣', '彰化縣',
  '南投縣', '雲林縣', '嘉義縣', '屏東縣', '宜蘭縣', '花蓮縣',
  '台東縣', '澎湖縣', '金門縣', '連江縣'
]

// 當地址選中時，同步到表單
const handleSelectAddress = (addr: any) => {
  selectedAddressId.value = addr.id
  recipient.value.name = addr.recipientName
  recipient.value.phone = addr.recipientPhone
  recipient.value.city = addr.city
  recipient.value.region = addr.region
  recipient.value.street = addr.street
  recipient.value.address = `${addr.city}${addr.region}${addr.street}`
}

const handleAddAddressSuccess = async (form: any) => {
  const success = await addressStore.addAddress(form)
  if (success) {
    addressDialogVisible.value = false
    // 新增成功後，Store 會自動 fetchAddresses，我們選取最新的一筆
    if (addressStore.addresses.length > 0) {
      handleSelectAddress(addressStore.addresses[0])
    }
  }
}

const paymentMethod = ref('ECPay')
const availableCoupons = ref<any[]>([])
const selectedCouponId = ref<number | null>(null)
const usePoints = ref(false)
const walletBalance = ref(0)
const showCouponModal = ref(false)

// ── 結帳模式（從路由 query 取得）────────────────────────────────
// type=direct               → 直接購買（商品詳情頁「立即購買」）
// 無 query                  → 一般購物車結帳
const isDirectBuyMode = computed<boolean>(() =>
  route.query.type === 'direct'
)
const directBuyData = ref<any[]>([])

// ── 核心數據切換 ──
const checkoutItems = computed(() => {
  if (isDirectBuyMode.value) return directBuyData.value
  return cartStore.selectedItems
})

const subtotal = computed(() => {
  return checkoutItems.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
})

// ── 檢查休假狀態 ──
const hasVacationItems = computed(() => {
  return checkoutItems.value.some(item => item.storeStatus === 2)
})

const shippingFee = ref(60)
const selectedCoupon = computed(() => availableCoupons.value.find(c => c.id === selectedCouponId.value))

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

const levelDiscount = computed(() => {
  if (memberLevel.value.discountRate >= 1.0) return 0
  // 計算折扣金額 (小計扣除優惠券後再計算等級折扣)
  const afterCoupon = subtotal.value - couponDiscount.value
  return Math.round(afterCoupon * (1 - memberLevel.value.discountRate))
})

const pointDiscount = computed(() => {
  if (!usePoints.value) return 0
  const remaining = subtotal.value - couponDiscount.value - levelDiscount.value
  return Math.min(walletBalance.value, remaining)
})

// 會員等級配色與權益
const levelColors: Record<string, string> = {
  '銅牌會員': '#EE4D2D',
  '銀牌會員': '#64748b',
  '金牌會員': '#f59e0b'
}

const currentLevelColor = computed(() => {
  return levelColors[memberLevel.value.name] || '#94a3b8'
})

const levelBenefits = computed(() => {
  const name = memberLevel.value.name || ''
  if (name.includes('銀牌')) {
    return [
      { text: '全站商品享 9.0 折專屬折扣' },
      { text: '專屬銀牌會員限定活動參與權' }
    ]
  } else if (name.includes('金牌')) {
    return [
      { text: '全站商品享 8.0 折專屬折扣' },
      { text: '享有最優先的出貨處理順序' },
      { text: '金牌專屬客服優先服務通道' }
    ]
  }
  return []
})

const finalAmount = computed(() => {
  const amt = subtotal.value + shippingFee.value - couponDiscount.value - levelDiscount.value - pointDiscount.value
  return Math.max(amt, 0)
})

onMounted(async () => {
  // 1. 處理「直接購買」模式
  if (isDirectBuyMode.value) {
    const stored = sessionStorage.getItem('TEMP_CHECKOUT_DATA')
    if (!stored) {
      ElMessage.warning('結帳資訊已過期')
      router.push('/')
      return
    }
    directBuyData.value = JSON.parse(stored)
  }

  // 2. 檢查是否有結帳項目
  if (checkoutItems.value.length === 0) {
    router.push('/cart')
    return
  }

  // 3. 加載結帳所需資訊 (優惠券、錢包、個人資料、等級資訊)
  try {
    const memberId = authStore.memberInfo?.memberId || 0
    const [couponsRes, walletRes, profileRes, levelRes] = await Promise.all([
      checkoutApi.getAvailableCoupons(
        checkoutItems.value[0].storeId,
        subtotal.value,
        checkoutItems.value.map(i => i.productId)
      ),
      checkoutApi.getWalletBalance(),
      memberId ? getMemberProfile(memberId) : Promise.resolve({ data: null }),
      getLevelInfo()
    ])
    
    availableCoupons.value = couponsRes.data
    walletBalance.value = walletRes.data.pointBalance ?? walletRes.data.balance ?? 0

    // 更新等級資訊
    if (levelRes.data) {
      console.log('Level API Response:', levelRes.data)
      const { currentLevelName, levels, totalSpending } = levelRes.data
      const spending = Number(totalSpending || 0)
      
      // 1. 優先嘗試從 levels 中尋找對應名稱的物件
      let currentLevelData = levels.find((l: any) => l.levelName === currentLevelName)
      
      // 2. 如果沒對應到，或後端沒給名稱，改用消費金額手動計算 (與 LevelView 邏輯一致)
      if (levels && levels.length > 0) {
        const sorted = [...levels].sort((a, b) => Number(b.minSpending) - Number(a.minSpending))
        const detectedLevel = sorted.find((l: any) => spending >= Number(l.minSpending))
        
        // 如果手動算出來的等級更高，則採用手動算的
        if (detectedLevel && (!currentLevelData || Number(detectedLevel.minSpending) > Number(currentLevelData.minSpending))) {
          currentLevelData = detectedLevel
          console.log('Detected higher level based on spending:', currentLevelData.levelName)
        }
      }

      if (currentLevelData) {
        memberLevel.value = {
          name: currentLevelData.levelName,
          discountRate: Number(currentLevelData.discountRate)
        }
        console.log('Final Member Level:', memberLevel.value)
      } else {
        memberLevel.value = {
          name: currentLevelName || '一般會員',
          discountRate: 1.0
        }
      }
    }

    // 載入地址簿
    await addressStore.fetchAddresses()
    if (addressStore.defaultAddress) {
      handleSelectAddress(addressStore.defaultAddress)
    } else if (profileRes && profileRes.data) {
      recipient.value.name = profileRes.data.fullName || ''
      recipient.value.phone = profileRes.data.phoneNumber || ''
      recipient.value.city = profileRes.data.city || ''
      recipient.value.region = profileRes.data.region || ''
      recipient.value.street = profileRes.data.address || ''
      recipient.value.address = `${recipient.value.city}${recipient.value.region}${recipient.value.street}`
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
    console.error('Checkout init failed', err)
  }
})

function selectCoupon(id: number | null) {
  selectedCouponId.value = id
  showCouponModal.value = false
}

async function handleSubmit() {

  if (hasVacationItems.value) {
    ElMessage.error('包含休假中賣場的商品，暫時無法結帳')
    return
  }

  // 確保地址是最新的 (手動填寫模式下需要拼接)
  if (!selectedAddressId.value) {
    recipient.value.address = `${recipient.value.city}${recipient.value.region}${recipient.value.street}`
  }

  if (!recipient.value.name || !recipient.value.phone || !recipient.value.address || recipient.value.address.length < 5) {
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
      levelDiscount: levelDiscount.value,
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
      // ── 自動儲存手動輸入的地址 ──
      if (!selectedAddressId.value) {
        try {
          await addressStore.addAddress({
            recipientName: recipient.value.name,
            recipientPhone: recipient.value.phone,
            city: recipient.value.city,
            region: recipient.value.region,
            street: recipient.value.street,
            isDefault: addressStore.addresses.length === 0 // 如果原本沒地址，設為預設
          })
        } catch (err) {
          console.warn('自動儲存地址失敗，但不影響訂單流程')
        }
      }

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

function formatPrice(val: number) { return val.toLocaleString('zh-TW') }
</script>

<template>
  <div class="checkout-process">
    <div class="checkout-container">
      <div class="page-header">
        <el-button :icon="ArrowLeft" circle @click="router.back()" class="back-btn" />
        <h1 class="page-title">結帳</h1>
      </div>

      <!-- 🏖️ 賣場休假提示 -->
      <div v-if="hasVacationItems" class="vacation-warning">
        <el-alert
          title="包含休假中賣場的商品"
          type="warning"
          description="訂單內有商品所屬賣場正在休假，請移除該商品或待賣場恢復營業後再下單。"
          show-icon
          :closable="false"
        />
      </div>

      <!-- 🛒 訂單商品 -->
      <el-card class="section-card">
        <template #header><div class="card-header">🛒 訂單商品</div></template>
        <div v-for="item in checkoutItems" :key="item.productId + (item.variantId || '')" class="item-row">
          <el-image :src="item.image || item.coverImage" class="item-img" />
          <div class="item-info">
            <div class="item-name">
              <el-tag v-if="item.storeStatus === 2" type="warning" size="small" effect="dark" class="mr-1">休假中</el-tag>
              {{ item.name || item.productName }}
            </div>
            <div class="item-price">NT$ {{ formatPrice(item.price) }} x {{ item.quantity }}</div>
          </div>
          <div class="item-total">NT$ {{ formatPrice(item.price * item.quantity) }}</div>
        </div>
      </el-card>

      <!-- 📍 收件資訊 -->
      <el-card class="section-card">
        <template #header>
          <div class="card-header">
            <span>📍 收件資訊</span>
            <el-button type="primary" link :icon="Plus" @click="addressDialogVisible = true">
              新增收件地址
            </el-button>
          </div>
        </template>

        <!-- 已有地址清單 -->
        <div v-if="addressStore.addresses.length > 0" class="address-selection">
          <p class="selection-hint">請選擇收件地址：</p>
          <el-scrollbar max-height="320px">
            <el-row :gutter="10">
              <el-col v-for="addr in addressStore.addresses" :key="addr.id" :span="24">
                <AddressCard
                  :address="addr"
                  selectable
                  :selected="selectedAddressId === addr.id"
                  @select="handleSelectAddress"
                />
              </el-col>
            </el-row>
          </el-scrollbar>
        </div>

        <!-- 手動填寫表單 (無地址時顯示) -->
        <div v-else class="manual-form">
          <el-form label-width="100px" label-position="top">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="收件人姓名">
                  <el-input v-model="recipient.name" placeholder="請輸入姓名" :prefix-icon="User" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="手機號碼">
                  <el-input v-model="recipient.phone" placeholder="請輸入電話" :prefix-icon="Phone" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="縣市">
                  <el-select v-model="recipient.city" placeholder="選擇縣市" class="w-full">
                    <el-option v-for="c in cities" :key="c" :label="c" :value="c" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="行政區">
                  <el-input v-model="recipient.region" placeholder="如：大安區" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="詳細地址">
              <el-input v-model="recipient.street" type="textarea" :rows="2" placeholder="街道名稱、門牌號碼" />
            </el-form-item>
          </el-form>
        </div>
      </el-card>

      <!-- 🧧 優惠與折抵 -->
      <el-card class="section-card">
        <template #header><div class="card-header">🧧 優惠與折抵</div></template>
        <div class="discount-row" @click="showCouponModal = true">
          <div class="label">優惠券</div>
          <div class="value clickable">
            <span v-if="selectedCoupon" class="coupon-tag">{{ selectedCoupon.title }}</span>
            <span v-else>選擇優惠券</span>
            <el-icon><ArrowRight /></el-icon>
          </div>
        </div>
        <div class="discount-row" v-if="memberLevel.discountRate < 1.0">
          <div class="label">{{ memberLevel.name }} 專屬權益</div>
          <div class="value discount">{{ memberLevel.discountRate * 10 }} 折</div>
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
          <el-radio value="ECPay" border>綠界支付</el-radio>
          <el-radio value="NewebPay" border>藍新支付</el-radio>
        </el-radio-group>
      </el-card>

      <!-- 🏆 會員專屬權益 -->
      <el-card class="section-card benefit-card" v-if="levelBenefits.length > 0">
        <div class="benefit-container">
          <div class="benefit-header" :style="{ color: currentLevelColor }">
            <el-icon class="trophy-icon"><trophy-icon /></el-icon>
            <span class="benefit-title">{{ memberLevel.name }} 專屬權益</span>
          </div>
          <div class="benefit-list">
            <div v-for="(benefit, index) in levelBenefits" :key="index" class="benefit-item">
              <el-icon class="star-icon" :style="{ color: currentLevelColor }"><star-icon /></el-icon>
              <span>{{ benefit.text }}</span>
            </div>
          </div>
          <div class="benefit-footer">
            本訂單已為您節省 NT$ {{ formatPrice(levelDiscount) }} ({{ (memberLevel.discountRate * 10).toFixed(1) }} 折)
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
        
        <div v-if="couponDiscount > 0" class="summary-row">
          <span>優惠券折抵</span>
          <span class="discount">- NT$ {{ formatPrice(couponDiscount) }}</span>
        </div>
        <div v-if="levelDiscount > 0" class="summary-row">
          <span>{{ memberLevel.name }} 專屬折扣 ({{ memberLevel.discountRate * 10 }}折)</span>
          <span class="discount">- NT$ {{ formatPrice(levelDiscount) }}</span>
        </div>
        <div v-if="pointDiscount > 0" class="summary-row">
          <span>點數折抵</span>
          <span class="discount">- NT$ {{ formatPrice(pointDiscount) }}</span>
        </div>

        <div class="summary-row final">
          <span>訂單總計</span>
          <span class="price">NT$ {{ formatPrice(finalAmount) }}</span>
        </div>
        <el-button 
          type="primary" 
          size="large" 
          class="submit-btn" 
          @click="handleSubmit"
          :disabled="hasVacationItems"
        >
          {{ hasVacationItems ? '賣場休假中' : '下單' }}
        </el-button>
      </div>
    </div>


    <!-- 新增地址彈窗 -->
    <AddressFormDialog v-model="addressDialogVisible" @submit="handleAddAddressSuccess" />

    <!-- 優惠券彈窗 -->
    <el-dialog v-model="showCouponModal" title="選擇優惠券" width="450px">
      <div v-if="availableCoupons.length === 0" class="empty-coupons">目前沒有可用的優惠券</div>
      <div v-else class="coupon-list">
        <div v-for="c in availableCoupons" :key="c.id" class="coupon-item" :class="{ selected: selectedCouponId === c.id }" @click="selectCoupon(c.id)">
          <div class="coupon-title">{{ c.title }}</div>
          <div class="coupon-desc">{{ c.couponType === 1 ? `現折 $${c.discountValue}` : `打 ${c.discountValue} 折` }}<span v-if="c.minimumSpend > 0">，滿 ${{ c.minimumSpend }} 可用</span></div>
        </div>
        <div class="coupon-item none" @click="selectCoupon(null)">不使用優惠券</div>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped>
.checkout-process {
  padding: 20px 0;
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
.vacation-warning {
  margin-bottom: 20px;
}
.mr-1 {
  margin-right: 4px;
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
.item-img { width: 64px; height: 64px; border-radius: 8px; margin-right: 16px; background: #f1f5f9; }
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
  border: 1px solid #ebeef5;
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

/* 會員權益卡片樣式 */
.benefit-card {
  border: 1px dashed #dcdfe6;
  background-color: #fafafa;
}
.benefit-container {
  padding: 8px 0;
}
.benefit-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}
.trophy-icon {
  font-size: 24px;
}
.benefit-title {
  font-size: 18px;
  font-weight: bold;
}
.benefit-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 16px;
}
.benefit-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #606266;
}
.star-icon {
  font-size: 16px;
}
.benefit-footer {
  font-size: 13px;
  color: #909399;
  border-top: 1px solid #ebeef5;
  padding-top: 12px;
  font-style: italic;
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
