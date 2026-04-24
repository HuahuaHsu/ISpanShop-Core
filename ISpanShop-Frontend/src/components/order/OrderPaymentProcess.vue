<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElLoading } from 'element-plus'
import { getOrderDetailApi } from '@/api/order'
import { checkoutApi } from '@/api/checkout'
import { ArrowLeft, Location, Phone, User, InfoFilled } from '@element-plus/icons-vue'

interface Props {
  orderId: number | string
}

const props = defineProps<Props>()
const router = useRouter()

const orderData = ref<any>(null)
const paymentMethod = ref('ECPay')
const loading = ref(true)

const fetchOrderData = async () => {
  loading.value = true
  try {
    const res = await getOrderDetailApi(Number(props.orderId))
    const order = res.data
    
    // 安全檢查：只有「待付款 (0)」狀態的訂單可以進入支付流程
    if (order.status !== 0) {
      ElMessage.warning('該訂單目前狀態不可支付')
      router.back()
      return
    }

    orderData.value = order
  } catch (err) {
    ElMessage.error('無法載入訂單資訊')
    router.back()
  } finally {
    loading.value = false
  }
}

const handlePayment = async () => {
  const load = ElLoading.service({ text: '正在準備支付頁面...' })
  try {
    const res = await checkoutApi.getRepaymentUrl(Number(props.orderId))
    if (res.data.success) {
      const backendBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'
      const targetUrl = res.data.paymentUrl.startsWith('http') 
        ? res.data.paymentUrl 
        : `${backendBase.replace(/\/$/, '')}${res.data.paymentUrl}`
        
      ElMessage.success('正在導向支付頁面...')
      window.location.href = targetUrl
    }
  } catch (err: any) {
    const msg = err.response?.data?.message || '導向支付失敗，請稍後再試'
    ElMessage.error(msg)
  } finally {
    load.close()
  }
}

const formatPrice = (val: number) => val?.toLocaleString('zh-TW') || '0'

onMounted(fetchOrderData)
</script>

<template>
  <div class="order-payment-process" v-loading="loading">
    <div v-if="orderData" class="payment-container">
      <div class="page-header">
        <el-button :icon="ArrowLeft" circle @click="router.back()" class="back-btn" />
        <h1 class="page-title">訂單支付</h1>
      </div>

      <el-alert
        title="此訂單已成立，請於期限內完成支付"
        type="info"
        show-icon
        :closable="false"
        class="mb-4"
      />

      <!-- 🛒 訂單內容 (唯讀) -->
      <el-card class="section-card">
        <template #header><div class="card-header">🛒 訂單內容</div></template>
        <div v-for="item in orderData.items" :key="item.id" class="item-row">
          <el-image :src="item.coverImage" class="item-img" />
          <div class="item-info">
            <div class="item-name">{{ item.productName }}</div>
            <div class="item-spec" v-if="item.variantName">{{ item.variantName }}</div>
            <div class="item-price">NT$ {{ formatPrice(item.price) }} x {{ item.quantity }}</div>
          </div>
          <div class="item-total">NT$ {{ formatPrice(item.price * item.quantity) }}</div>
        </div>
      </el-card>

      <!-- 📍 收件資訊 (唯讀) -->
      <el-card class="section-card">
        <template #header><div class="card-header">📍 收件資訊</div></template>
        <div class="info-display">
          <div class="info-item">
            <el-icon><User /></el-icon>
            <span class="label">收件人：</span>
            <span class="value">{{ orderData.recipientName }}</span>
          </div>
          <div class="info-item">
            <el-icon><Phone /></el-icon>
            <span class="label">電話：</span>
            <span class="value">{{ orderData.recipientPhone }}</span>
          </div>
          <div class="info-item">
            <el-icon><Location /></el-icon>
            <span class="label">地址：</span>
            <span class="value">{{ orderData.recipientAddress }}</span>
          </div>
        </div>
      </el-card>

      <!-- 💳 支付方式選擇 -->
      <el-card class="section-card">
        <template #header><div class="card-header">💳 選擇支付方式</div></template>
        <el-radio-group v-model="paymentMethod">
          <el-radio value="ECPay" border>綠界支付</el-radio>
          <el-radio value="NewebPay" border>藍新支付</el-radio>
        </el-radio-group>
      </el-card>

      <!-- 💰 金額總計 -->
      <div class="summary-section">
        <div class="summary-row">
          <span>商品小計</span>
          <span>NT$ {{ formatPrice(orderData.totalAmount) }}</span>
        </div>
        <div class="summary-row">
          <span>運費</span>
          <span>NT$ {{ formatPrice(orderData.shippingFee) }}</span>
        </div>
        <div v-if="orderData.discountAmount > 0" class="summary-row">
          <span>優惠折抵</span>
          <span class="discount">- NT$ {{ formatPrice(orderData.discountAmount) }}</span>
        </div>
        <div v-if="orderData.pointDiscount > 0" class="summary-row">
          <span>點數折抵</span>
          <span class="discount">- NT$ {{ formatPrice(orderData.pointDiscount) }}</span>
        </div>
        
        <div class="summary-row final">
          <span>應付總計</span>
          <span class="price">NT$ {{ formatPrice(orderData.finalAmount) }}</span>
        </div>

        <el-button 
          type="primary" 
          size="large" 
          class="submit-btn" 
          @click="handlePayment"
        >
          立即付款
        </el-button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.order-payment-process {
  padding: 20px 0;
}
.payment-container {
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
.mb-4 {
  margin-bottom: 16px;
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
.item-name { font-size: 14px; font-weight: 500; }
.item-spec { font-size: 12px; color: #666; margin-top: 4px; }
.item-price { font-size: 12px; color: #999; margin-top: 2px; }
.item-total { font-weight: bold; }

.info-display {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.info-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}
.info-item .el-icon {
  color: #909399;
}
.info-item .label {
  color: #606266;
  width: 70px;
}
.info-item .value {
  color: #303133;
  font-weight: 500;
}

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
</style>
