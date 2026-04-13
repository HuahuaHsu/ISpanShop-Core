<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getOrders } from '@/api/order';
import type { OrderListItem } from '@/types/order';
import { Calendar, ShoppingBag } from '@element-plus/icons-vue';

const router = useRouter();
const route = useRoute();
const loading = ref(false);
const orders = ref<OrderListItem[]>([]);
const total = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);

// 優先從網址參數取得 userId (?userId=1)，若無則預設為 1
const userId = computed(() => Number(route.query.userId) || 1);

const fetchOrders = async () => {
  loading.value = true;
  try {
    const response = await getOrders(userId.value, currentPage.value, pageSize.value);
    orders.value = response.items;
    total.value = response.totalCount;
  } catch (error) {
    console.error('獲取訂單列表失敗', error);
  } finally {
    loading.value = false;
  }
};

// 監聽網址參數變化 (當 userId 改變時重新抓取)
watch(() => route.query.userId, () => {
  currentPage.value = 1;
  fetchOrders();
});

const handlePageChange = (page: number) => {
  currentPage.value = page;
  fetchOrders();
};

const viewDetail = (id: number) => {
  router.push(`/member/order/${id}`);
};

const getStatusType = (status: number) => {
  switch (status) {
    case 0: return 'warning';
    case 1: return 'primary';
    case 3: return 'success';
    case 4: return 'info';
    case 5: return 'danger';
    case 6: return 'info';
    default: return 'info';
  }
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

onMounted(fetchOrders);
</script>

<template>
  <div class="order-list-page">
    <div class="header">
      <el-breadcrumb separator="/">
        <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
        <el-breadcrumb-item>會員中心</el-breadcrumb-item>
        <el-breadcrumb-item>我的訂單</el-breadcrumb-item>
      </el-breadcrumb>
      <div class="title-row">
        <h2>我的訂單</h2>
        <el-tag v-if="route.query.userId" type="info">正在查看會員 ID: {{ userId }}</el-tag>
      </div>
    </div>

    <el-card shadow="never" class="list-card">
      <el-table :data="orders" v-loading="loading" style="width: 100%">
        <el-table-column label="訂單資訊" min-width="200">
          <template #default="{ row }">
            <div class="order-info">
              <span class="order-number">#{{ row.orderNumber }}</span>
              <div class="order-meta">
                <el-icon><Calendar /></el-icon>
                <span>{{ formatDate(row.createdAt) }}</span>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="商店" prop="storeName" width="150" />

        <el-table-column label="總金額" width="150">
          <template #default="{ row }">
            <span class="amount">NT$ {{ row.totalAmount.toLocaleString() }}</span>
          </template>
        </el-table-column>

        <el-table-column label="狀態" width="120">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ row.statusName }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link @click="viewDetail(row.id)">
              查看詳情
            </el-button>
          </template>
        </el-table-column>

        <template #empty>
          <div class="empty-state">
            <el-icon class="empty-icon"><ShoppingBag /></el-icon>
            <p>目前尚無訂單紀錄 (UserId: {{ userId }})</p>
            <el-button type="primary" @click="router.push('/')">去逛逛吧</el-button>
          </div>
        </template>
      </el-table>

      <div class="pagination-container" v-if="total > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :total="total"
          layout="prev, pager, next"
          @current-change="handlePageChange"
          background
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped lang="scss">
.order-list-page {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;

  .header {
    margin-bottom: 24px;
    .title-row {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-top: 16px;
    }
    h2 {
      margin: 0;
      font-size: 24px;
      color: #303133;
    }
  }

  .list-card {
    border-radius: 8px;
  }

  .order-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
    .order-number {
      font-weight: bold;
      color: #409EFF;
    }
    .order-meta {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: #909399;
    }
  }

  .amount {
    font-weight: bold;
    color: #f56c6c;
  }

  .pagination-container {
    display: flex;
    justify-content: center;
    margin-top: 24px;
  }

  .empty-state {
    padding: 60px 0;
    text-align: center;
    .empty-icon {
      font-size: 48px;
      color: #dcdfe6;
      margin-bottom: 16px;
    }
    p {
      color: #909399;
      margin-bottom: 24px;
    }
  }
}
</style>
