<template>
  <div class="order-detail-page">
    <div class="order-detail-container" v-loading="loading">
      <!-- 頂部導航/狀態 -->
      <div class="detail-header-card">
        <div class="header-top">
          <el-button link @click="router.back()">
            <el-icon><ArrowLeft /></el-icon> 返回
          </el-button>
          <div class="header-right">
            <span class="order-no">訂單編號. {{ order?.orderNumber }}</span>
            <el-divider direction="vertical" />
            <span class="status-text">{{ order?.statusName }}</span>
          </div>
        </div>
        
        <!-- 狀態進度 -->
        <OrderSteps 
          :status="order?.status"
          :created-at="order?.createdAt"
          :payment-date="order?.paymentDate"
          :completed-at="order?.completedAt"
        />
      </div>

      <!-- 地址資訊與物流 -->
      <div class="info-grid">
        <div class="address-card">
          <h3 class="card-title">收貨地址</h3>
          <div class="address-content">
            <div class="recipient-name">{{ order?.recipientName }}</div>
            <div class="recipient-phone">{{ order?.recipientPhone }}</div>
            <div class="recipient-address">{{ order?.recipientAddress }}</div>
          </div>
        </div>
        <div class="logistics-card">
          <h3 class="card-title">訂單備註</h3>
          <div class="logistics-content">
            <p v-if="order?.note" class="note-text">{{ order.note }}</p>
            <p v-else class="no-note">無備註資訊</p>
          </div>
        </div>
      </div>

      <!-- 商品清單 -->
      <div class="items-card">
        <div class="store-header">
          <span class="store-name">{{ order?.storeName }}</span>
          <el-button size="small">查看賣場</el-button>
        </div>
        
        <div v-for="item in order?.items" :key="item.id" class="order-item">
          <div class="item-main">
            <el-image :src="item.coverImage || '/placeholder.png'" class="item-image" fit="cover" />
            <div class="item-info">
              <h4 class="item-name">{{ item.productName }}</h4>
              <div class="item-variant">{{ item.variantName }}</div>
              <div class="item-qty">x{{ item.quantity }}</div>
            </div>
          </div>
          <div class="item-price">
            <span class="unit-price">${{ formatPrice(item.price) }}</span>
          </div>
        </div>

        <!-- 價格結算 (使用抽取的組件) -->
        <OrderSummary 
          v-if="order"
          :total-amount="order.totalAmount"
          :shipping-fee="order.shippingFee"
          :point-discount="order.pointDiscount"
          :discount-amount="order.discountAmount"
          :coupon-title="order.couponTitle"
          :level-discount="order.levelDiscount"
          :final-amount="order.finalAmount"
        />
      </div>

      <!-- 底部動作按鈕區 (獨立區塊) -->
      <div class="actions-card" v-if="order">
        <OrderActionButtons 
          :order-id="order.id" 
          :order-number="order.orderNumber"
          :status="order.status" 
          :is-reviewed="order.isReviewed || (order as any).IsReviewed"
          :is-detail="true"
          @refresh="fetchOrderDetail"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft } from '@element-plus/icons-vue';
import { getOrderDetailApi } from '@/api/order';
import type { OrderDetail } from '@/types/order';
import { ElMessage } from 'element-plus';
import OrderSteps from '@/components/order/OrderSteps.vue';
import OrderActionButtons from '@/components/order/OrderActionButtons.vue';
import OrderSummary from '@/components/order/OrderSummary.vue';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const order = ref<OrderDetail | null>(null);

const fetchOrderDetail = async () => {
  const id = Number(route.params.id);
  if (isNaN(id)) return;

  loading.value = true;
  try {
    const res = await getOrderDetailApi(id);
    order.value = res.data;
  } catch (error) {
    console.error('獲取訂單詳情失敗', error);
    ElMessage.error('獲取訂單詳情失敗');
  } finally {
    loading.value = false;
  }
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

onMounted(() => {
  fetchOrderDetail();
});
</script>
<style scoped lang="scss">
.order-detail-page {
  background-color: #f5f5f5;
  min-height: calc(100vh - 200px);
  padding: 20px 0;
}

.order-detail-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 10px;
}

/* 頂部標題卡片 */
.detail-header-card {
  background: #fff;
  padding: 20px;
  border-radius: 2px;
  margin-bottom: 12px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
  
  .header-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 30px;
    border-bottom: 1px solid #f0f0f0;
    padding-bottom: 15px;

    .header-right {
      display: flex;
      align-items: center;
      color: #757575;
      font-size: 14px;
    
      .status-text {
        color: #ee4d2d;
        font-weight: 500;
        text-transform: uppercase;
    }
  }
  }

  .status-steps {
    padding: 20px 0;
    }
  }

/* 資訊網格 */
.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 12px;

  .address-card, .logistics-card {
    background: #fff;
    padding: 20px;
    border-radius: 2px;
    box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);

    .card-title {
      font-size: 18px;
      font-weight: normal;
      color: rgba(0,0,0,.8);
      margin: 0 0 20px 0;
    }
  }
        
  .address-content {
    .recipient-name {
      font-weight: 500;
      font-size: 16px;
      margin-bottom: 8px;
    }
    .recipient-phone, .recipient-address {
      color: rgba(0,0,0,.54);
      font-size: 14px;
      margin-bottom: 4px;
    }
  }
        
  .logistics-content {
    .note-text {
      color: rgba(0,0,0,.8);
      font-size: 14px;
      line-height: 1.6;
    }
    .no-note {
      color: rgba(0,0,0,.26);
      font-size: 14px;
    }
  }
}

/* 商品清單卡片 */
.items-card {
  background: #fff;
  border-radius: 2px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
  overflow: hidden;

  .store-header {
    padding: 15px 20px;
    border-bottom: 1px solid #f0f0f0;
    display: flex;
    justify-content: space-between;
    align-items: center;

    .store-name {
      font-weight: 500;
    }
  }

  .order-item {
    padding: 20px;
    display: flex;
    justify-content: space-between;
    border-bottom: 1px solid #f0f0f0;

    .item-main {
      display: flex;
      gap: 15px;

      .item-image {
        width: 80px;
        height: 80px;
        border: 1px solid #e1e1e1;
}

      .item-info {
        .item-name {
          font-weight: normal;
          font-size: 16px;
          margin: 0 0 5px 0;
        }
        .item-variant {
          color: rgba(0,0,0,.54);
          font-size: 14px;
          margin-bottom: 5px;
        }
        .item-qty {
          font-size: 14px;
        }
      }
}

    .item-price {
  display: flex;
  align-items: center;
      .unit-price {
        color: #ee4d2d;
}
}
}
} /* 這裡正確閉合 items-card */

/* 底部動作按鈕區 (獨立區塊) */
.actions-card {
  background: #fff;
  padding: 20px;
  margin-top: 12px; /* 拉大間距，確保有感 */
  border-radius: 2px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .info-grid {
    grid-template-columns: 1fr;
}

  .order-summary .summary-row {
    .label { width: 100px; }
    .value { width: 120px; }
  }
}
</style>
