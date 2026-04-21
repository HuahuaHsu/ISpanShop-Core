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
        <div class="status-steps">
          <el-steps :active="activeStep" align-center finish-status="success">
            <el-step 
              v-for="(step, index) in orderSteps" 
              :key="index"
              :title="step.title" 
              :description="step.description"
            ></el-step>
          </el-steps>
        </div>
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

        <!-- 價格結算 -->
        <div class="order-summary">
          <div class="summary-row">
            <span class="label">商品總金額</span>
            <span class="value">${{ formatPrice(order?.totalAmount || 0) }}</span>
          </div>
          <div class="summary-row">
            <span class="label">運費</span>
            <span class="value">${{ formatPrice(order?.shippingFee || 0) }}</span>
          </div>
          <div class="summary-row total">
            <span class="label">訂單總金額</span>
            <span class="value final-price">${{ formatPrice(order?.finalAmount || 0) }}</span>
          </div>
          <div class="summary-row payment-method">
            <span class="label">付款方式</span>
            <span class="value">線上支付</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft } from '@element-plus/icons-vue';
import { getOrderDetailApi } from '@/api/order';
import type { OrderDetail } from '@/types/order';
import { ElMessage } from 'element-plus';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const order = ref<OrderDetail | null>(null);

/**
 * 訂單狀態說明:
 * 0: 待付款 (Pending Payment)
 * 1: 待出貨 (To Ship)
 * 2: 運送中 (Shipping)
 * 3: 已完成 (Completed)
 * 4: 已取消 (Cancelled)
 * 5: 退貨/款中 (Returning)
 * 6: 已退款 (Refunded)
 */

// 根據訂單狀態動態生成進度條步驟
const orderSteps = computed(() => {
  if (!order.value) return [];
  
  const status = order.value.status;
  
  // 情況四：已取消
  if (status === 4) {
    return [
      { title: '訂單已成立', description: formatDate(order.value.createdAt) },
      { title: '已取消', description: formatDate(order.value.completedAt || order.value.paymentDate) }
    ];
  }
  
  // 情況三：退貨/退款中 或 已退款
  if (status === 5 || status === 6) {
    return [
      { title: '訂單已成立', description: formatDate(order.value.createdAt) },
      { title: '付款成功', description: formatDate(order.value.paymentDate) },
      { title: '退貨/款中' },
      { title: '已退款', description: formatDate(order.value.completedAt) }
    ];
  }
  
  // 情況二：運送中 (未完成前)
  if (status === 2) {
    return [
      { title: '訂單已成立', description: formatDate(order.value.createdAt) },
      { title: '付款成功', description: formatDate(order.value.paymentDate) },
      { title: '待出貨' },
      { title: '運送中' }
    ];
  }
  
  // 情況一：預設情況 (待付款、待出貨、已完成)
  return [
    { title: '訂單已成立', description: formatDate(order.value.createdAt) },
    { title: '付款成功', description: formatDate(order.value.paymentDate) },
    { title: '待出貨' },
    { title: '訂單已完成', description: formatDate(order.value.completedAt) }
  ];
});

// 計算當前活耀的步驟索引
const activeStep = computed(() => {
  if (!order.value) return 0;
  
  const status = order.value.status;
  
  switch (status) {
    case 0: return 1; // 待付款 -> 停在「訂單已成立」之後
    case 1: return 2; // 待出貨 -> 「付款成功」已完成
    case 2: return 3; // 運送中 -> 「待出貨」已完成
    case 3: return 4; // 已完成 -> 全部完成
    case 4: return 1; // 已取消 -> 「訂單已成立」已完成
    case 5: return 2; // 退貨中 -> 「付款成功」已完成
    case 6: return 4; // 已退款 -> 全部完成
    default: return 0;
  }
});

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

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
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

  .order-summary {
    padding: 20px;
    background-color: #fffbf8;

    .summary-row {
  display: flex;
      justify-content: flex-end;
  align-items: center;
      margin-bottom: 12px;
      
      .label {
        width: 150px;
        text-align: right;
        color: rgba(0,0,0,.54);
        font-size: 14px;
        padding-right: 20px;
        border-right: 1px solid #e1e1e1;
}

      .value {
        width: 150px;
        text-align: right;
        padding-left: 20px;
        font-size: 14px;
        color: rgba(0,0,0,.8);
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
