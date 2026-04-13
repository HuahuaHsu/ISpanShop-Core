<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { getOrderDetail, cancelOrder, returnOrder } from '@/api/order';
import type { Order } from '@/types/order';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Loading } from '@element-plus/icons-vue';

const route = useRoute();
const order = ref<Order | null>(null);
const isLoading = ref(true);
const isOperating = ref(false); // 正在執行取消或退貨中

// 訂單狀態文字對應
const statusTexts: Record<number, string> = {
  0: '待付款',
  1: '待出貨',
  2: '待收貨',
  3: '已完成',
  4: '已取消',
  5: '退貨中',
  6: '已退貨',
};

// 計算進度條進行到哪一步 (對應 el-steps 的 active)
// 0:待付款(Step 1), 1:待出貨(Step 2), 2:待收貨(Step 3), 3:已完成(Step 4)
const currentStep = computed(() => {
  if (!order.value) return 0;
  if (order.value.status >= 4) return -1; // 已取消、退貨等不顯示進度
  return order.value.status + 1;
});

// 是否可以取消 (待付款 0 或 待出貨 1)
const canCancel = computed(() => order.value && (order.value.status === 0 || order.value.status === 1));

// 是否可以退貨 (已完成 3)
const canReturn = computed(() => order.value && order.value.status === 3);

// 抓取訂單詳情
const fetchOrderDetail = async () => {
  const id = route.params.id as string;
  if (!id) return;
  
  try {
    isLoading.value = true;
    const data = await getOrderDetail(id);
    order.value = data;
  } catch (error) {
    console.error('Failed to fetch order detail:', error);
    ElMessage.error('無法讀取訂單資訊');
  } finally {
    isLoading.value = false;
  }
};

// 處理取消訂單
const handleCancelOrder = async () => {
  if (!order.value) return;
  try {
    await ElMessageBox.confirm('確定要取消這筆訂單嗎？', '取消訂單', {
      confirmButtonText: '確定取消',
      cancelButtonText: '再想想',
      type: 'warning',
    });
    
    isOperating.value = true;
    await cancelOrder(order.value.id);
    ElMessage.success('訂單已成功取消');
    await fetchOrderDetail(); // 重新整理資料
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Cancel order failed:', error);
      ElMessage.error('取消失敗，請稍後再試');
    }
  } finally {
    isOperating.value = false;
  }
};

// 處理申請退貨
const handleReturnOrder = async () => {
  if (!order.value) return;
  try {
    await ElMessageBox.confirm('確定要申請退貨嗎？商品需保持完整。', '申請退貨', {
      confirmButtonText: '確定申請',
      cancelButtonText: '取消',
      type: 'warning',
    });
    
    isOperating.value = true;
    await returnOrder(order.value.id);
    ElMessage.success('已收到您的退貨申請');
    await fetchOrderDetail(); // 重新整理資料
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Return order failed:', error);
      ElMessage.error('申請失敗，請稍後再試');
    }
  } finally {
    isOperating.value = false;
  }
};

onMounted(fetchOrderDetail);
</script>

<template>
  <div class="order-detail-view container" v-loading="isLoading">
    <div v-if="order" class="order-content">
      <!-- 頂部標題與狀態 -->
      <div class="header">
        <h1>訂單詳情 #{{ order.orderNumber }}</h1>
        <el-tag :type="order.status === 3 ? 'success' : order.status >= 4 ? 'info' : 'warning'">
          {{ statusTexts[order.status] }}
        </el-tag>
      </div>

      <!-- 進度條 (若未取消/退貨則顯示) -->
      <div class="section progress-section" v-if="order.status < 4">
        <el-steps :active="currentStep" finish-status="success" align-center>
          <el-step title="待付款" />
          <el-step title="待出貨" />
          <el-step title="待收貨" />
          <el-step title="已完成" />
        </el-steps>
      </div>

      <!-- 訂單基本資訊 -->
      <div class="section info-section">
        <el-descriptions title="訂單資訊" border :column="2">
          <el-descriptions-item label="訂單編號">{{ order.orderNumber }}</el-descriptions-item>
          <el-descriptions-item label="下單時間">{{ new Date(order.createdAt).toLocaleString() }}</el-descriptions-item>
          <el-descriptions-item label="收件人">{{ order.receiverName }}</el-descriptions-item>
          <el-descriptions-item label="聯繫電話">{{ order.receiverPhone }}</el-descriptions-item>
          <el-descriptions-item label="收件地址" :span="2">{{ order.receiverAddress }}</el-descriptions-item>
          <el-descriptions-item label="總金額">
            <span class="final-amount">NT$ {{ order.finalAmount.toLocaleString() }}</span>
          </el-descriptions-item>
        </el-descriptions>
      </div>

      <!-- 商品清單表格 -->
      <div class="section items-section">
        <h3>商品項目</h3>
        <el-table :data="order.items" stripe style="width: 100%">
          <el-table-column label="商品" min-width="250">
            <template #default="{ row }">
              <div class="item-info">
                <el-image :src="row.imageUrl" class="item-img" fit="cover" />
                <div class="item-text">
                  <div class="name">{{ row.productName }}</div>
                  <el-tag size="small" type="info">{{ row.variantName }}</el-tag>
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="單價" width="120">
            <template #default="{ row }">
              NT$ {{ row.unitPrice.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="quantity" label="數量" width="100" />
          <el-table-column label="小計" width="120" align="right">
            <template #default="{ row }">
              <span class="subtotal">NT$ {{ row.subTotal.toLocaleString() }}</span>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="actions">
        <el-button @click="$router.back()">返回訂單列表</el-button>
        
        <!-- 操作按鈕 -->
        <el-button 
          v-if="canCancel" 
          type="danger" 
          plain 
          :loading="isOperating"
          @click="handleCancelOrder"
        >
          取消訂單
        </el-button>
        
        <el-button 
          v-if="canReturn" 
          type="warning" 
          plain 
          :loading="isOperating"
          @click="handleReturnOrder"
        >
          申請退貨
        </el-button>
      </div>
    </div>

    <!-- 錯誤或找不到訂單 -->
    <el-empty v-else-if="!isLoading" description="找不到該訂單資訊" />
  </div>
</template>

<style scoped>
.order-detail-view {
  padding: 40px 20px;
}

.container {
  max-width: 1000px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.section {
  background: #fff;
  padding: 24px;
  border-radius: 8px;
  margin-bottom: 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.progress-section {
  padding: 40px 24px;
}

.final-amount {
  color: #f56c6c;
  font-weight: bold;
  font-size: 1.2rem;
}

.item-info {
  display: flex;
  gap: 15px;
  align-items: center;
}

.item-img {
  width: 60px;
  height: 60px;
  border-radius: 4px;
}

.item-text .name {
  font-weight: 500;
  margin-bottom: 4px;
}

.subtotal {
  font-weight: bold;
  color: #333;
}

.actions {
  margin-top: 30px;
  text-align: center;
}

h3 {
  margin-top: 0;
  margin-bottom: 20px;
  font-size: 18px;
  color: #303133;
}
</style>
