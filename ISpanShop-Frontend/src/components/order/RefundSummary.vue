<!-- src/components/order/RefundSummary.vue -->
<script setup lang="ts">
import { computed, watch } from 'vue'
import { InfoFilled } from '@element-plus/icons-vue'

interface OrderItem {
  id: number
  price: number
  quantity: number
}

interface OrderData {
  totalAmount?: number
  finalAmount?: number
  levelDiscount?: number
  discountAmount?: number
  pointDiscount?: number
  items?: OrderItem[]
}

interface Props {
  order: OrderData | any // 使用 any 以兼容後端可能的不同 DTO
  selectedItemIds: number[]
  returnQuantities: Record<number, number>
}

const props = defineProps<Props>()

// 1. 安全取得數值 (處理大小寫問題)
const getVal = (obj: any, key: string) => {
  if (!obj) return 0
  const lowerKey = key.charAt(0).toLowerCase() + key.slice(1)
  const upperKey = key.charAt(0).toUpperCase() + key.slice(1)
  return obj[lowerKey] ?? obj[upperKey] ?? 0
}

const totalAmount = computed(() => getVal(props.order, 'totalAmount'))
const finalAmount = computed(() => getVal(props.order, 'finalAmount'))
const levelDiscount = computed(() => getVal(props.order, 'levelDiscount'))
const discountAmount = computed(() => getVal(props.order, 'discountAmount'))
const pointDiscount = computed(() => getVal(props.order, 'pointDiscount'))

// 2. 退貨商品原價小計
const itemsSubtotal = computed(() => {
  if (!props.order || !props.order.items) return 0
  return props.selectedItemIds.reduce((sum, id) => {
    const item = props.order.items.find((i: any) => i.id === id)
    return sum + (item ? item.price * (props.returnQuantities[id] || 0) : 0)
  }, 0)
})

// 3. 判斷是否為「全額退貨」
// 在賣家審核端，如果商品小計等於訂單總原價，我們就視為全額退貨
const isFullReturn = computed(() => {
  if (totalAmount.value === 0) return false
  // 誤差容許值 1 元
  return Math.abs(itemsSubtotal.value - totalAmount.value) < 1
})

// 4. 計算各項分攤比例 (部分退款時使用)
const ratio = computed(() => {
  if (totalAmount.value === 0) return 0
  return itemsSubtotal.value / totalAmount.value
})

// 5. 各項折抵分攤計算
const levelDiscountShare = computed(() => 
  isFullReturn.value ? levelDiscount.value : Math.round(levelDiscount.value * ratio.value)
)

const couponDiscountShare = computed(() => 
  isFullReturn.value ? discountAmount.value : Math.round(discountAmount.value * ratio.value)
)

const pointDiscountShare = computed(() => 
  isFullReturn.value ? pointDiscount.value : Math.round(pointDiscount.value * ratio.value)
)

// 6. 最終預計退款金額
const finalRefundAmount = computed(() => {
  if (isFullReturn.value) return finalAmount.value
  const amount = itemsSubtotal.value - levelDiscountShare.value - couponDiscountShare.value - pointDiscountShare.value
  return amount > 0 ? amount : 0
})

const formatPrice = (val: number) => val?.toLocaleString('zh-TW') || '0'

defineExpose({
  finalRefundAmount
})
</script>

<template>
  <div v-if="order" class="refund-summary-box">
    <div class="summary-section">
      <div class="summary-row">
        <span>退貨商品小計</span>
        <span>NT$ {{ formatPrice(itemsSubtotal) }}</span>
      </div>

      <!-- 會員等級折抵分攤 -->
      <div v-if="levelDiscountShare !== 0" class="summary-row">
        <span class="label-with-hint">
          會員等級折抵分攤
          <el-tooltip content="按商品金額比例分攤當初享有的會員折扣" placement="top">
            <el-icon class="info-icon"><InfoFilled /></el-icon>
          </el-tooltip>
        </span>
        <span class="discount">- NT$ {{ formatPrice(Math.abs(levelDiscountShare)) }}</span>
      </div>

      <!-- 優惠券折抵分攤 -->
      <div v-if="couponDiscountShare !== 0" class="summary-row">
        <span>優惠券折抵分攤</span>
        <span class="discount">- NT$ {{ formatPrice(Math.abs(couponDiscountShare)) }}</span>
      </div>

      <!-- 點數折抵分攤 -->
      <div v-if="pointDiscountShare !== 0" class="summary-row">
        <span>點數折抵分攤</span>
        <span class="discount">- NT$ {{ formatPrice(Math.abs(pointDiscountShare)) }}</span>
      </div>

      <div class="summary-row final">
        <span>預計退款金額</span>
        <span class="price">NT$ {{ formatPrice(finalRefundAmount) }}</span>
      </div>

      <div class="refund-hint">
        <small v-if="isFullReturn">* 全額退貨將退還訂單最終實付金額 (含運費)。</small>
        <small v-else>* 部分退貨將依比例扣除折抵金額，且不退還運費。</small>
      </div>
    </div>
  </div>
</template>

<style scoped>
.summary-section {
  background: #fafafa;
  padding: 20px;
  border-radius: 4px;
  border: 1px dashed #e4e4e4;
}
.summary-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
  font-size: 14px;
  color: #666;
}
.discount { color: #ee4d2d; }
.final {
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #e4e4e4;
  font-size: 16px;
  font-weight: bold;
  color: #333;
}
.price { color: #ee4d2d; font-size: 22px; }
.label-with-hint { display: flex; align-items: center; gap: 4px; }
.info-icon { font-size: 14px; color: #909399; cursor: pointer; }
.refund-hint {
  margin-top: 12px;
  text-align: right;
  color: #999;
  font-size: 12px;
}
</style>
