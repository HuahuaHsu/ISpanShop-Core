<script setup lang="ts">
import { ref, onMounted, watch, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { getOrders } from '@/api/order';
import { createTicket } from '@/api/support';
import { SUPPORT_CATEGORIES, type SupportTicket } from '@/types/support';
import { Calendar, ShoppingBag, ChatLineRound } from '@element-plus/icons-vue';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';

const router = useRouter();
const loading = ref(false);
const orders = ref<OrderListItem[]>([]);
const total = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);

// --- 聯絡客服彈窗相關 ---
const supportDialogVisible = ref(false);
const submitLoading = ref(false);
const supportFormRef = ref<FormInstance>();
const supportForm = reactive({
  category: 0,
  subject: '',
  description: '',
  orderId: 0 as number | null
});

const supportRules: FormRules = {
  subject: [{ required: true, message: '請輸入主旨', trigger: 'blur' }],
  description: [{ required: true, message: '請描述您遇到的問題', trigger: 'blur' }],
};

const openSupportDialog = (row: OrderListItem) => {
  supportForm.orderId = row.id;
  supportForm.subject = `關於訂單 #${row.orderNumber} 的問題`;
  supportForm.description = '';
  supportForm.category = 0; // 預設為訂單問題
  supportDialogVisible.value = true;
};

const handleSupportSubmit = async () => {
  if (!supportFormRef.value) return;
  await supportFormRef.value.validate(async (valid) => {
    if (valid) {
      submitLoading.value = true;
      try {
        await createTicket(supportForm);
        ElMessage.success('客服單已送出！');
        supportDialogVisible.value = false;
        // 詢問是否前往客服中心
        router.push('/member/support');
      } catch (error) {
        ElMessage.error('送出失敗，請稍後再試');
      } finally {
        submitLoading.value = false;
      }
    }
  });
};
// -----------------------

// 標籤頁狀態： 'all' 或 'pending' (待付款: status 0)
const activeTab = ref('all');
// ... (fetchOrders 等邏輯保持不變)
const fetchOrders = async () => {
  loading.value = true;
  try {
    const params: any = {
      pageNumber: currentPage.value,
      pageSize: pageSize.value
    };

    // 如果是待付款標籤，加入狀態過濾 (0: Pending)
    if (activeTab.value === 'pending') {
      params.statuses = [0];
    }

    const response = await getOrders(params);
    orders.value = response.items;
    total.value = response.totalCount;
  } catch (error) {
    console.error('獲取訂單列表失敗', error);
  } finally {
    loading.value = false;
  }
};

// 監聽標籤切換
watch(activeTab, () => {
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
    case 0: return 'danger'; // 待付款用紅色醒目一點
    case 1: return 'primary';
    case 3: return 'success';
    case 4: return 'info';
    case 5: return 'warning';
    default: return 'info';
  }
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

onMounted(fetchOrders);
</script>

<template>
  <div class="order-list-page">
    <div class="header">
      <el-breadcrumb separator="/">
        <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
        <el-breadcrumb-item :to="{ path: '/member' }">會員中心</el-breadcrumb-item>
        <el-breadcrumb-item>我的訂單</el-breadcrumb-item>
      </el-breadcrumb>
      <div class="title-row">
        <h2>我的訂單</h2>
      </div>
    </div>

    <!-- 標籤頁篩選 -->
    <el-tabs v-model="activeTab" class="order-tabs">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane label="待付款" name="pending" />
    </el-tabs>

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

        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <div class="operation-btns">
              <el-button type="primary" link @click="viewDetail(row.id)">
                詳情
              </el-button>
              <el-button type="danger" link :icon="ChatLineRound" @click="openSupportDialog(row)">
                聯絡客服
              </el-button>
            </div>
          </template>
        </el-table-column>

        <template #empty>
          <div class="empty-state">
            <el-icon class="empty-icon"><ShoppingBag /></el-icon>
            <p v-if="activeTab === 'pending'">目前沒有待付款的訂單</p>
            <p v-else>目前尚無訂單紀錄</p>
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

    <!-- 聯絡客服彈窗 -->
    <el-dialog
      v-model="supportDialogVisible"
      title="聯絡客服 / 售後申請"
      width="500px"
      destroy-on-close
    >
      <el-form
        ref="supportFormRef"
        :model="supportForm"
        :rules="supportRules"
        label-position="top"
      >
        <el-form-item label="相關訂單">
          <el-input :value="'#' + orders.find(o => o.id === supportForm.orderId)?.orderNumber" disabled />
        </el-form-item>

        <el-form-item label="主旨" prop="subject">
          <el-input v-model="supportForm.subject" />
        </el-form-item>

        <el-form-item label="詳細描述您的問題" prop="description">
          <el-input
            v-model="supportForm.description"
            type="textarea"
            :rows="4"
            placeholder="請詳細描述商品狀況或您的需求，以便客服人員快速處理。"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="supportDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSupportSubmit" :loading="submitLoading">
            提交申請
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.order-list-page {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;

  .header {
    margin-bottom: 20px;
    .title-row {
      margin-top: 16px;
    }
    h2 {
      margin: 0;
      font-size: 24px;
      color: #303133;
    }
  }

  .order-tabs {
    background: #fff;
    padding: 0 20px;
    border-radius: 8px 8px 0 0;
    :deep(.el-tabs__header) {
      margin: 0;
    }
  }

  .list-card {
    border-top: none;
    border-radius: 0 0 8px 8px;
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

  .operation-btns {
    display: flex;
    align-items: center;
    gap: 8px;
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
