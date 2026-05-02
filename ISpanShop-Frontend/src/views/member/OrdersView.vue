<template>
  <div class="orders-page">
    <div class="orders-container">
      <!-- 導航 Tabs -->
      <div class="tabs-card">
        <el-tabs v-model="activeTab" class="custom-tabs" @tab-change="handleTabChange">
          <el-tab-pane label="全部" name="all"></el-tab-pane>
          <el-tab-pane label="待付款" name="0"></el-tab-pane>
          <el-tab-pane label="待出貨" name="1"></el-tab-pane>
          <el-tab-pane label="運送中" name="2"></el-tab-pane>
          <el-tab-pane label="已完成" name="3"></el-tab-pane>
          <el-tab-pane label="已取消" name="4"></el-tab-pane>
          <el-tab-pane label="退貨/款中" name="5"></el-tab-pane>
          <el-tab-pane label="已退款" name="6"></el-tab-pane>
        </el-tabs>
      </div>

      <!-- 搜尋欄 (可選，模擬蝦皮風格) -->
      <div class="search-section">
        <el-input
          v-model="searchQuery"
          placeholder="您可以透過訂單編號、商品名稱或賣場名稱進行搜尋"
          prefix-icon="Search"
          class="order-search"
        />
      </div>

      <!-- 訂單列表 -->
      <div v-loading="loading" class="order-list">
        <el-empty v-if="filteredOrders.length === 0" description="暫無訂單資料" />
        
        <div v-for="order in pagedOrders" :key="order.id" class="order-card">
          <div class="order-card-header">
            <div class="store-info">
              <span class="store-tag">賣場</span>
              <span class="store-name">{{ order.storeName }}</span>
              <el-button size="small" class="header-action-btn" @click.stop="handleChat(order)">好聊</el-button>
              <el-button size="small" class="header-action-btn" @click.stop="handleGoToStore(order.storeId)">查看賣場</el-button>
            </div>
            <div class="status-info">
              <span class="status-text" :class="getStatusClass(order.status)">
                {{ order.statusName }}
              </span>
            </div>
          </div>

          <div class="order-card-content" @click="goToDetail(order.id)">
            <div class="product-info">
              <div class="image-wrapper">
                <el-image 
                  :src="order.firstProductImage || '/placeholder.png'" 
                  class="product-image"
                  fit="cover"
                />
              </div>
              <div class="product-detail">
                <h4 class="product-name">{{ order.firstProductName }}</h4>
                <div class="item-meta">
                  <span class="item-count">共 {{ order.totalItemCount }} 件商品</span>
                </div>
              </div>
            </div>
            <div class="price-info">
              <OrderDiscountTags 
                :discount-amount="order.discountAmount"
                :level-discount="order.levelDiscount"
                :point-discount="order.pointDiscount"
                :promotion-discount="order.promotionDiscount"
                class="mb-2"
              />
              <div class="total-label">訂單金額：</div>
              <div class="total-amount">${{ formatPrice(order.finalAmount) }}</div>
            </div>
          </div>

          <div class="order-card-footer">
            <div class="footer-left">
              <span class="order-no">訂單編號: {{ order.orderNumber }}</span>
              <span class="order-time">下單時間: {{ formatDate(order.createdAt) }}</span>
            </div>
            <div class="footer-right">
              <OrderActionButtons 
                :order-id="order.id" 
                :order-number="order.orderNumber"
                :status="order.status" 
                :is-reviewed="order.isReviewed || (order as any).IsReviewed"
                :has-appealed="order.hasAppealed || (order as any).HasAppealed"
                @refresh="fetchOrders"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- 分頁 -->
      <div v-if="filteredOrders.length > 0" class="pagination-wrap">
        <el-pagination
          background
          layout="prev, pager, next, jumper, total"
          :total="filteredOrders.length"
          :page-size="pageSize"
          v-model:current-page="currentPage"
          :disabled="loading"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { Search } from '@element-plus/icons-vue';
import { getMyOrdersApi } from '@/api/order';
import type { OrderListItem } from '@/types/order';
import { ElMessage } from 'element-plus';
import OrderActionButtons from '@/components/order/OrderActionButtons.vue';
import OrderDiscountTags from '@/components/order/OrderDiscountTags.vue';
import { useChatStore } from '@/stores/chat';

const router = useRouter();
const route = useRoute();
const chatStore = useChatStore();
const loading = ref(false);
const orders = ref<OrderListItem[]>([]);
const activeTab = ref('all');
const searchQuery = ref('');

// 分頁狀態
const currentPage = ref(1);
const pageSize = ref(5); // 訂單卡片較大，每頁顯示 5 筆較合適

// 監聽路由參數變化，以便在頁面內切換標籤時也能生效
watch(() => route.query.tab, (newTab) => {
  if (newTab) {
    activeTab.value = newTab.toString();
  } else {
    activeTab.value = 'all';
  }
  currentPage.value = 1; // 切換標籤時重置分頁
});

// 監聽搜尋字串變化，重置分頁
watch(searchQuery, () => {
  currentPage.value = 1;
});

const fetchOrders = async () => {
  loading.value = true;
  try {
    const res = await getMyOrdersApi();
    orders.value = res.data;
    console.log('Orders data:', res.data);
  } catch (error) {
    console.error('獲取訂單失敗', error);
    ElMessage.error('獲取訂單失敗，請稍後再試');
  } finally {
    loading.value = false;
  }
};

const filteredOrders = computed(() => {
  let result = orders.value;
  
  if (activeTab.value !== 'all') {
    result = result.filter(o => o.status.toString() === activeTab.value);
  }
  
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(o => 
      o.orderNumber.toLowerCase().includes(q) || 
      o.productNames.toLowerCase().includes(q) ||
      o.storeName.toLowerCase().includes(q)
    );
  }
  
  return result;
});

const pagedOrders = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredOrders.value.slice(start, end);
});

const handleTabChange = (tabName: string) => {
  // 當使用者點擊標籤時，同步更新 URL 參數（選用，增加體驗）
  router.replace({ query: { ...route.query, tab: tabName === 'all' ? undefined : tabName } });
};

const goToDetail = (id: number) => {
  router.push(`/member/orders/${id}`);
};

const handleChat = (order: OrderListItem) => {
  if (order.sellerId) {
    chatStore.openChatWithUser(order.sellerId, order.storeName);
  } else {
    ElMessage.warning('無法取得賣家資訊');
  }
};

const handleGoToStore = (storeId: number) => {
  router.push(`/store/${storeId}`);
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  const date = new Date(dateStr);
  return date.toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const getStatusClass = (status: number) => {
  switch (status) {
    case 0: return 'status-pending';
    case 1: return 'status-processing';
    case 2: return 'status-shipped';
    case 3: return 'status-completed';
    case 4: return 'status-cancelled';
    case 5: return 'status-returning';
    case 6: return 'status-refunded';
    default: return '';
  }
};

onMounted(() => {
  // 初始化時讀取 URL 參數
  if (route.query.tab) {
    activeTab.value = route.query.tab.toString();
  }
  fetchOrders();
});
</script>

<style scoped lang="scss">
.orders-page {
  /* 移除原本的背景色與 padding */
}

.orders-container {
  max-width: 1000px; /* 稍微縮小寬度以適應側邊欄後的空間 */
  margin: 0;
  padding: 0;
}

.pagination-wrap {
  margin-top: 24px;
  display: flex;
  justify-content: center;
  padding-bottom: 40px;

  :deep(.el-pagination.is-background .el-pager li:not(.is-disabled).is-active) {
    background-color: #ee4d2d;
  }
  
  :deep(.el-pagination.is-background .el-pager li:not(.is-disabled):hover) {
    color: #ee4d2d;
  }
}

/* Tabs 樣式優化 */
.tabs-card {
  background: #fff;
  border-radius: 2px;
  margin-bottom: 12px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
}

.custom-tabs :deep(.el-tabs__header) {
  margin: 0;
}

.custom-tabs :deep(.el-tabs__nav-scroll) {
  display: flex;
  justify-content: center;
}

.custom-tabs :deep(.el-tabs__item) {
  height: 54px;
  line-height: 54px;
  font-size: 16px;
  padding: 0 32px;
  
  &:hover {
    color: #ee4d2d;
  }
  
  &.is-active {
    color: #ee4d2d;
  }
}

.custom-tabs :deep(.el-tabs__active-bar) {
  background-color: #ee4d2d;
}

/* 搜尋欄樣式 */
.search-section {
  margin-bottom: 12px;
}

.order-search :deep(.el-input__wrapper) {
  background-color: #eaeaea;
  box-shadow: none;
  padding: 8px 16px;
  
  &.is-focus {
    background-color: #fff;
    box-shadow: 0 0 0 1px #ee4d2d inset;
  }
}

/* 訂單卡片設計 */
.order-card {
  background: #fff;
  border-radius: 2px;
  margin-bottom: 12px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
  transition: transform 0.2s;

  &:hover {
    box-shadow: 0 1px 20px 0 rgba(0,0,0,.05);
  }

  .order-card-header {
    padding: 12px 20px;
    border-bottom: 1px solid rgba(0,0,0,.09);
    display: flex;
    justify-content: space-between;
    align-items: center;

    .store-info {
      display: flex;
      align-items: center;
      gap: 6px;

      .store-tag {
        background-color: #ee4d2d;
        color: #fff;
        font-size: 12px;
        padding: 1px 4px;
        border-radius: 2px;
      }

      .store-name {
        font-weight: 500;
        color: #333;
        margin-right: 2px;
      }

      .header-action-btn {
        margin-left: 0;
        height: 24px;
        padding: 0 8px;
        font-size: 12px;
        border-color: #dcdfe6;
        color: #606266;

        &:hover {
          color: #ee4d2d;
          border-color: #f7a696;
          background-color: #fffbf8;
        }
      }
    }

    .status-info {
      .status-text {
        text-transform: uppercase;
        font-weight: 500;
        
        &.status-pending { color: #ee4d2d; }
        &.status-processing { color: #26aa99; }
        &.status-shipped { color: #26aa99; }
        &.status-completed { color: #ee4d2d; }
        &.status-cancelled { color: #929292; }
        &.status-returning { color: #faad14; }
        &.status-refunded { color: #929292; }
      }

      .status-subtext {
        color: #ee4d2d;
        font-size: 14px;
      }
    }
  }

  .order-card-content {
    padding: 20px;
    display: flex;
    justify-content: space-between;
    align-items: flex-end;
    cursor: pointer;

    .product-info {
      display: flex;
      gap: 12px;
      flex: 1;

      .image-wrapper {
        border: 1px solid #e1e1e1;
        border-radius: 2px;
        overflow: hidden;
      }

      .product-image {
        width: 80px;
        height: 80px;
        display: block;
      }

      .product-detail {
        display: flex;
        flex-direction: column;
        justify-content: flex-start;

        .product-name {
          font-size: 16px;
          margin: 0 0 8px 0;
          color: #333;
          font-weight: normal;
          display: -webkit-box;
          -webkit-line-clamp: 2;
          -webkit-box-orient: vertical;
          overflow: hidden;
        }

        .item-meta {
          color: #929292;
          font-size: 14px;
        }
      }
    }

    .price-info {
      display: flex;
      align-items: center;
      gap: 10px;

      .discount-tags {
        display: flex;
        gap: 4px;
        margin-right: 4px;
      }

      .total-label {
        font-size: 14px;
        color: #333;
      }

      .total-amount {
        font-size: 24px;
        color: #ee4d2d;
        font-weight: 500;
      }
    }
  }

  .order-card-footer {
    padding: 20px;
    background-color: #fffbf8;
    border-top: 1px dotted rgba(0,0,0,.09);
    display: flex;
    justify-content: space-between;
    align-items: center;

    .footer-left {
      display: flex;
      flex-direction: column;
      gap: 4px;
      font-size: 12px;
      color: #929292;
    }

    .footer-right {
      display: flex;
      gap: 10px;

      :deep(.el-button + .el-button),
      :deep(.order-actions + .el-button) {
        margin-left: 0;
      }

      .el-button {
        min-width: 120px;
        
        &.detail-btn:hover {
          color: #ee4d2d;
          border-color: #ee4d2d;
          background-color: #fff;
        }
      }

      .el-button--primary {
        background-color: #ee4d2d;
        border-color: #ee4d2d;
        
        &:hover {
          background-color: #f05d40;
          border-color: #f05d40;
        }

        &.is-plain {
          color: #ee4d2d;
          background-color: #fff;
          border-color: #ee4d2d;
          
          &:hover {
            background-color: #fffbf8;
          }
        }
      }
    }
  }
}

/* 移動端適配 */
@media (max-width: 768px) {
  .custom-tabs :deep(.el-tabs__item) {
    padding: 0 15px;
    font-size: 14px;
  }

  .order-card-content {
    flex-direction: column;
    align-items: flex-start !important;
    gap: 15px;
    
    .price-info {
      width: 100%;
      justify-content: flex-end;
    }
  }

  .order-card-footer {
    flex-direction: column;
    gap: 15px;
    align-items: flex-start !important;
    
    .footer-right {
      width: 100%;
      justify-content: flex-end;
    }
  }
}
</style>
