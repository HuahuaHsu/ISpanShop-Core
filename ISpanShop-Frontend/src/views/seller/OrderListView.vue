<template>
  <div class="seller-orders-page">
    <!-- ── 頂部 ── -->
    <div class="page-header">
      <h2 class="page-title">訂單管理</h2>
    </div>

    <!-- ── 主卡片 ── -->
    <el-card shadow="never" class="main-card">
      <!-- ── 一級 Tab ── -->
      <el-tabs v-model="activeTab" class="level1-tabs" @tab-change="handleTabChange">
        <el-tab-pane label="全部" name="all" />
        <el-tab-pane label="待付款" name="0" />
        <el-tab-pane label="待出貨" name="1" />
        <el-tab-pane label="運送中" name="2" />
        <el-tab-pane label="已完成" name="3" />
        <el-tab-pane label="已取消" name="4" />
        <el-tab-pane label="退貨/款中" name="5" />
      </el-tabs>

      <!-- ── 搜尋列 ── -->
      <div class="search-section">
        <el-row :gutter="12" align="middle">
          <el-col :xs="24" :sm="10">
            <el-input
              v-model="searchQuery"
              placeholder="搜尋 訂單編號, 買家名稱"
              clearable
              @keyup.enter="handleSearch"
              @clear="handleSearch"
            >
              <template #prefix><el-icon><Search /></el-icon></template>
            </el-input>
          </el-col>
          <el-col :xs="24" :sm="14">
            <el-button class="search-btn" @click="handleSearch">搜尋</el-button>
            <el-button @click="handleReset">重設</el-button>
          </el-col>
        </el-row>
        
        <div class="result-count">
          共 <strong class="count-num">{{ totalCount }}</strong> 筆訂單
        </div>
      </div>

      <!-- ── 訂單列表區域 ── -->
      <div v-loading="loading" class="order-area">
        <el-empty v-if="orders.length === 0" description="暫無訂單資料" :image-size="100" />
        
        <div v-for="order in orders" :key="order.id" class="order-card">
          <!-- 卡片頭部：買家資訊 -->
          <div class="order-card-header">
            <div class="buyer-info">
              <span class="buyer-tag">買家</span>
              <span class="buyer-name">{{ order.buyerName }}</span>
              <el-button link type="primary" size="small" icon="ChatDotRound" class="chat-btn">聊聊</el-button>
            </div>
            <div class="status-info">
              <span class="status-text" :class="getStatusClass(order.status)">
                {{ order.statusName }}
              </span>
            </div>
          </div>

          <!-- 卡片內容：商品資訊 -->
          <div class="order-card-content" @click="goToDetail(order.id)">
            <div class="product-info">
              <div class="image-wrapper">
                <el-image 
                  :src="order.firstProductImage || '/placeholder.png'" 
                  class="product-image"
                  fit="cover"
                >
                  <template #error>
                    <div class="image-slot"><el-icon><Picture /></el-icon></div>
                  </template>
                </el-image>
              </div>
              <div class="product-detail">
                <h4 class="product-name">{{ order.firstProductName }}</h4>
                <div class="order-meta">
                  <span class="order-no">訂單編號: {{ order.orderNumber }}</span>
                  <span class="order-time">下單時間: {{ formatDate(order.createdAt) }}</span>
                </div>
              </div>
            </div>
            <div class="price-info">
              <div class="total-label">訂單總額：</div>
              <div class="total-amount">NT$ {{ order.finalAmount.toLocaleString() }}</div>
            </div>
          </div>

          <!-- 卡片底部：動作按鈕與收件資訊 -->
          <div class="order-card-footer">
            <div class="footer-left">
              <div class="recipient-box">
                <el-icon><Location /></el-icon>
                <span class="recipient-name">{{ order.recipientName }}</span>
                <span class="item-count">共 {{ order.totalItemCount }} 件商品</span>
              </div>
            </div>
            <div class="footer-right">
              <!-- 賣家動作按鈕 -->
              <template v-if="order.status === 1">
                <el-button type="primary" class="action-btn" @click="handleShip(order.id)">安排出貨</el-button>
                <el-button class="action-btn secondary" @click="handleCancel(order.id)">取消訂單</el-button>
              </template>
              
              <template v-else-if="order.status === 2">
                <el-button type="primary" plain class="action-btn" @click="handleComplete(order.id)">模擬買家收貨</el-button>
              </template>

              <el-button @click="goToDetail(order.id)" class="action-btn detail-btn">
                查看訂單詳情
              </el-button>
            </div>
          </div>
        </div>
      </div>

      <!-- ── 分頁 ── -->
      <div class="pagination-wrapper" v-if="totalCount > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="totalCount"
          background
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { Search, Picture, Location, ChatDotRound } from '@element-plus/icons-vue';
import { getSellerOrdersApi, updateSellerOrderStatusApi } from '@/api/store';
import type { SellerOrder } from '@/types/store';
import { ElMessage, ElMessageBox } from 'element-plus';

const router = useRouter();
const route = useRoute();
const loading = ref(false);
const orders = ref<SellerOrder[]>([]);
const totalCount = ref(0);
const activeTab = ref('all');
const searchQuery = ref('');
const currentPage = ref(1);
const pageSize = ref(10);

const fetchOrders = async () => {
  loading.value = true;
  try {
    const status = activeTab.value === 'all' ? undefined : parseInt(activeTab.value);
    const res = await getSellerOrdersApi({
      status,
      page: currentPage.value,
      pageSize: pageSize.value
    });
    
    orders.value = res.data.items;
    totalCount.value = res.data.totalCount;
  } catch (error) {
    console.error('獲取訂單失敗', error);
    ElMessage.error('獲取訂單失敗');
  } finally {
    loading.value = false;
  }
};

const handleTabChange = () => {
  currentPage.value = 1;
  fetchOrders();
};

const handlePageChange = (page: number) => {
  currentPage.value = page;
  fetchOrders();
};

const handleSizeChange = (size: number) => {
  pageSize.value = size;
  currentPage.value = 1;
  fetchOrders();
};

const handleSearch = () => {
  currentPage.value = 1;
  fetchOrders();
};

const handleReset = () => {
  searchQuery.value = '';
  currentPage.value = 1;
  activeTab.value = 'all';
  fetchOrders();
};

const handleShip = async (orderId: number) => {
  try {
    await ElMessageBox.confirm('確定要安排出貨嗎？', '出貨確認');
    await updateSellerOrderStatusApi(orderId, 2); // 2 = Shipped
    ElMessage.success('已安排出貨');
    fetchOrders();
  } catch (err) {
    if (err !== 'cancel') ElMessage.error('出貨失敗');
  }
};

const handleComplete = async (orderId: number) => {
  try {
    await updateSellerOrderStatusApi(orderId, 3); // 3 = Completed
    ElMessage.success('訂單已完成');
    fetchOrders();
  } catch (err) {
    ElMessage.error('更新失敗');
  }
}

const handleCancel = (orderId: number) => {
  ElMessage.info('取消訂單功能開發中...');
};

const goToDetail = (id: number) => {
  router.push(`/seller/orders/${id}`);
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('zh-TW', {
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
    default: return '';
  }
};

onMounted(() => {
  if (route.query.status) {
    activeTab.value = route.query.status.toString();
  }
  fetchOrders();
});
</script>

<style scoped lang="scss">
/* ─ 佈局 ─────────────────────────────────────────────────────── */
.seller-orders-page {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.main-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 8px !important;
}
/* 把 el-card 的 padding 壓掉，讓 tabs 可以貼邊 */
:deep(.main-card > .el-card__body) { padding: 0; }

/* ─ 一級 Tab ──────────────────────────────────────────────────── */
:deep(.level1-tabs) { padding: 0 20px; }
:deep(.level1-tabs .el-tabs__nav-wrap::after) { background: #f1f5f9; height: 1px; }
:deep(.level1-tabs .el-tabs__active-bar)      { background: #ee4d2d; }
:deep(.level1-tabs .el-tabs__item.is-active)  { color: #ee4d2d; font-weight: 700; }
:deep(.level1-tabs .el-tabs__item:hover)      { color: #ee4d2d; }

/* ─ 搜尋列 ─────────────────────────────────────────────────────── */
.search-section {
  padding: 16px 20px 12px;
  border-bottom: 1px solid #f1f5f9;
}
.search-btn {
  border-color: #ee4d2d !important;
  color: #ee4d2d !important;
  background: white !important;
  margin-left: 8px;
}
.search-btn:hover { background: #fff7ed !important; }

.result-count {
  font-size: 13px;
  color: #64748b;
  margin-top: 10px;
}
.count-num { color: #ee4d2d; }

/* ─ 訂單區域 ────────────────────────────────────────────────────── */
.order-area {
  padding: 16px 20px;
  min-height: 240px;
  background-color: #f8fafc; /* 模仿商品頁面的區域背景 */
}

/* 訂單卡片設計 - 融合蝦皮與商品頁面風格 */
.order-card {
  background: #fff;
  border-radius: 4px;
  margin-bottom: 12px;
  box-shadow: 0 1px 1px 0 rgba(0,0,0,.05);
  transition: all 0.2s;
  border: 1px solid #e8eaf0;

  &:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    border-color: #ee4d2d;
  }

  .order-card-header {
    padding: 12px 20px;
    border-bottom: 1px solid #f1f5f9;
    display: flex;
    justify-content: space-between;
    align-items: center;

    .buyer-info {
      display: flex;
      align-items: center;
      gap: 8px;

      .buyer-tag {
        background-color: #3b82f6;
        color: #fff;
        font-size: 12px;
        padding: 1px 6px;
        border-radius: 2px;
      }

      .buyer-name {
        font-weight: 500;
        color: #333;
      }
      
      .chat-btn { margin-left: 5px; }
    }

    .status-info {
      .status-text {
        font-weight: 600;
        
        &.status-pending { color: #ee4d2d; }
        &.status-processing { color: #26aa99; }
        &.status-shipped { color: #26aa99; }
        &.status-completed { color: #ee4d2d; }
        &.status-cancelled { color: #929292; }
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
      gap: 15px;
      flex: 1;

      .image-wrapper {
        border: 1px solid #e1e1e1;
        border-radius: 2px;
        overflow: hidden;
        width: 80px;
        height: 80px;
        flex-shrink: 0;
      }

      .product-image {
        width: 80px;
        height: 80px;
        display: block;
      }

      .image-slot {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 100%;
        height: 100%;
        background: #f5f7fa;
        color: #909399;
        font-size: 24px;
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

        .order-meta {
          color: #929292;
          font-size: 13px;
          display: flex;
          flex-direction: column;
          gap: 2px;
        }
      }
    }

    .price-info {
      text-align: right;
      .total-label { font-size: 14px; color: #333; }
      .total-amount { font-size: 24px; color: #ee4d2d; font-weight: 500; margin-top: 5px; }
    }
  }

  .order-card-footer {
    padding: 16px 20px;
    background-color: #fffbf8;
    border-top: 1px dotted rgba(0,0,0,.09);
    display: flex;
    justify-content: space-between;
    align-items: center;

    .footer-left {
      .recipient-box {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 13px;
        color: #555;
        .el-icon { color: #ee4d2d; }
        .recipient-name { font-weight: 500; }
        .item-count { margin-left: 10px; color: #929292; }
      }
    }

    .footer-right {
      display: flex;
      gap: 10px;

      .action-btn {
        min-width: 110px;
        height: 36px;
        border-radius: 2px;
        
        &.detail-btn:hover {
          color: #ee4d2d;
          border-color: #ee4d2d;
          background-color: #fff;
        }
        
        &.secondary {
          border-color: rgba(0,0,0,.09);
          color: #555;
          &:hover { background: #f8f8f8; }
        }
      }
    }
  }
}

/* ─ 分頁 ─────────────────────────────────────────────────────────── */
.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  padding: 12px 20px 16px;
  border-top: 1px solid #f1f5f9;
  background-color: #fff;
}

/* 主按鈕樣式覆蓋 (排除 link 按鈕) */
.el-button--primary:not(.is-link) {
  background-color: #ee4d2d;
  border-color: #ee4d2d;
  color: #fff;
  &:hover { background-color: #f05d40; border-color: #f05d40; color: #fff; }
  
  &.is-plain {
    color: #ee4d2d;
    border-color: #ee4d2d;
    background-color: #fff;
    &:hover { background-color: #fffbf8; }
  }
}

/* 聊聊按鈕特別修正 */
.chat-btn.el-button--primary.is-link {
  color: #ee4d2d;
  background: transparent;
  border: none;
  &:hover { color: #f05d40; text-decoration: underline; }
}
</style>
