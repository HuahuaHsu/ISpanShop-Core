<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  totalAmount: number       // 商品小計
  shippingFee?: number | null      // 運費
  pointDiscount?: number | null    // 點數折抵
  discountAmount?: number | null   // 優惠券折抵
  couponTitle?: string | null      // 優惠券名稱
  levelDiscount?: number | null    // 會員等級折抵
  finalAmount: number       // 最終總計
  paymentMethod?: string    // 付款方式
  showPaymentMethod?: boolean // 是否顯示付款方式 (選填，預設 true)
}

const props = withDefaults(defineProps<Props>(), {
  shippingFee: 0,
  pointDiscount: 0,
  discountAmount: 0,
  levelDiscount: 0,
  couponTitle: '',
  paymentMethod: '線上支付',
  showPaymentMethod: true
})

const formatPrice = (price: number | null | undefined) => {
  return new Intl.NumberFormat('zh-TW').format(price || 0)
}
</script>

<template>
  <div class="order-summary-component">
    <div class="summary-row">
      <span class="label">商品總金額</span>
      <span class="value">${{ formatPrice(totalAmount) }}</span>
    </div>
    
    <div class="summary-row">
      <span class="label">運費</span>
      <span class="value">${{ formatPrice(shippingFee) }}</span>
    </div>

    <div v-if="pointDiscount && pointDiscount > 0" class="summary-row">
      <span class="label">點數折抵</span>
      <span class="value discount">-${{ formatPrice(pointDiscount) }}</span>
    </div>

    <div v-if="discountAmount && discountAmount > 0" class="summary-row">
      <span class="label">
        優惠券折抵
        <span v-if="couponTitle" class="coupon-name">({{ couponTitle }})</span>
      </span>
      <span class="value discount">-${{ formatPrice(discountAmount) }}</span>
    </div>

    <div v-if="levelDiscount && levelDiscount > 0" class="summary-row">
      <span class="label">會員等級折抵</span>
      <span class="value discount">-${{ formatPrice(levelDiscount) }}</span>
    </div>

    <div class="summary-row total">
      <span class="label">訂單總金額</span>
      <span class="value final-price">${{ formatPrice(finalAmount) }}</span>
    </div>

    <div v-if="showPaymentMethod" class="summary-row payment-method">
      <span class="label">付款方式</span>
      <span class="value">{{ paymentMethod }}</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.order-summary-component {
  padding: 20px;
  background-color: #fffbf8;

  .summary-row {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    margin-bottom: 12px;
    
    .label {
      min-width: 150px;
      width: auto;
      white-space: nowrap;
      text-align: right;
      color: rgba(0,0,0,.54);
      font-size: 14px;
      padding-right: 20px;
      border-right: 1px solid #e1e1e1;
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;
    }

    .coupon-name {
      color: #ee4d2d;
      font-size: 12px;
      font-weight: normal;
    }

    .value {
      width: 150px;
      text-align: right;
      padding-left: 20px;
      font-size: 14px;
      color: rgba(0,0,0,.8);

      &.discount {
        color: #ee4d2d;
      }
    }

    &.total {
      margin-top: 20px;
      .final-price {
        color: #ee4d2d;
        font-size: 24px;
        font-weight: 500;
      }
    }

    &.payment-method {
      border-top: 1px solid #f0f0f0;
      padding-top: 15px;
    }
  }
}

@media (max-width: 768px) {
  .order-summary-component .summary-row {
    .label { width: 120px; font-size: 13px; }
    .value { width: 110px; font-size: 13px; }
    &.total .final-price { font-size: 20px; }
  }
}
</style>
