<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import CheckoutProcess from '@/components/order/CheckoutProcess.vue'
import OrderPaymentProcess from '@/components/order/OrderPaymentProcess.vue'

const route = useRoute()

// 判斷是否為「既有訂單支付」模式
const isPaymentMode = computed(() => 
  route.query.mode === 'payment' && !!route.query.orderId
)

const orderId = computed(() => route.query.orderId as string)
</script>

<template>
  <div class="checkout-view">
    <!-- 根據模式切換顯示組件 -->
    <OrderPaymentProcess v-if="isPaymentMode" :order-id="orderId" />
    <CheckoutProcess v-else />
  </div>
</template>

<style scoped>
.checkout-view {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 40px 20px;
}
</style>
