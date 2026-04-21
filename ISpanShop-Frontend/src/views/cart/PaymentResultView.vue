<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Check, Close } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()

const isSuccess = ref(false)
const orderNumber = ref('')
const message = ref('')

onMounted(() => {
  const status = route.query.status as string
  orderNumber.value = route.query.orderNumber as string || '未知'
  
  if (status === 'SUCCESS' || status === '1') {
    isSuccess.value = true
    message.value = '訂單付款成功！我們會盡快為您處理。'
  } else {
    isSuccess.value = false
    message.value = '訂單付款失敗，請確認您的支付資訊或聯繫客服。'
  }
})

function goToHome() {
  router.push('/')
}

function goToOrder() {
  router.push(`/member/orders/${orderNumber.value}`)
}
</script>

<template>
  <div class="payment-result-page">
    <el-card class="result-card">
      <div class="result-icon-wrapper" :class="{ success: isSuccess, fail: !isSuccess }">
        <el-icon v-if="isSuccess" :size="60"><Check /></el-icon>
        <el-icon v-else :size="60"><Close /></el-icon>
      </div>

      <h1 class="result-title">{{ isSuccess ? '付款成功' : '付款失敗' }}</h1>
      <p class="result-message">{{ message }}</p>

      <div class="order-info">
        <div class="info-row">
          <span class="label">訂單編號：</span>
          <span class="value">{{ orderNumber }}</span>
        </div>
      </div>

      <div class="action-buttons">
        <el-button type="primary" size="large" @click="goToOrder">查看訂單</el-button>
        <el-button size="large" @click="goToHome">回首頁</el-button>
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.payment-result-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  background-color: #f5f7fa;
  padding: 20px;
}

.result-card {
  max-width: 500px;
  width: 100%;
  text-align: center;
  padding: 40px 20px;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.08);
}

.result-icon-wrapper {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 24px;
}

.result-icon-wrapper.success {
  background-color: #f0f9eb;
  color: #67c23a;
}

.result-icon-wrapper.fail {
  background-color: #fef0f0;
  color: #f56c6c;
}

.result-title {
  font-size: 24px;
  margin-bottom: 12px;
  color: #303133;
}

.result-message {
  font-size: 16px;
  color: #606266;
  margin-bottom: 32px;
}

.order-info {
  background-color: #f8f9fa;
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 32px;
  text-align: left;
}

.info-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.info-row:last-child {
  margin-bottom: 0;
}

.label {
  color: #909399;
}

.value {
  color: #303133;
  font-weight: 500;
}

.action-buttons {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.el-button {
  min-width: 140px;
}
</style>
