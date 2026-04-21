<template>
  <div class="refund-detail-page">
    <div class="refund-detail-container" v-loading="loading">
      <div class="page-header">
        <el-button link @click="router.back()">
          <el-icon><ArrowLeft /></el-icon> 返回
        </el-button>
        <h2 class="title">退貨/退款詳情</h2>
      </div>

      <!-- 狀態提示區 -->
      <el-alert
        title="退貨/退款申請審核中"
        type="warning"
        description="賣家正在審核您的退貨申請，這通常需要 1-3 個工作天。審核通過後，系統將會更新您的退款進度。"
        show-icon
        :closable="false"
        class="status-alert"
      />

      <!-- 退款資訊快照 -->
      <el-card class="info-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span class="header-title">申請資訊快照</span>
            <span class="apply-time">申請時間：{{ formatDate(order?.returnInfo?.createdAt) }}</span>
          </div>
        </template>

        <div class="info-grid">
          <div class="info-row">
            <span class="label">原因類別</span>
            <span class="value">{{ order?.returnInfo?.reasonCategory || '未提供' }}</span>
          </div>
          <div class="info-row">
            <span class="label">退款金額</span>
            <span class="value amount">${{ formatPrice(order?.returnInfo?.refundAmount || 0) }}</span>
          </div>
          <div class="info-row full-width">
            <span class="label">詳細說明</span>
            <div class="value description">{{ order?.returnInfo?.reasonDescription || '無' }}</div>
          </div>
          <div class="info-row full-width">
            <span class="label">憑證圖片</span>
            <div class="image-gallery">
              <el-image 
                v-for="(url, index) in order?.returnInfo?.imageUrls" 
                :key="index"
                :src="url" 
                class="proof-img"
                fit="cover"
                :preview-src-list="order?.returnInfo?.imageUrls"
              />
              <div v-if="!order?.returnInfo?.imageUrls?.length" class="no-image">未上傳圖片</div>
            </div>
          </div>
        </div>
      </el-card>

      <!-- 溫馨提醒 -->
      <div class="tips-section">
        <h3 class="tips-title">溫馨小提醒：</h3>
        <ul class="tips-list">
          <li>若賣家逾期未處理您的申請，系統將會自動介入。</li>
          <li>請保留好原始包裝與商品，以便後續退貨寄回。</li>
          <li>若有任何疑問，可以隨時透過「聊聊」與賣家聯繫。</li>
        </ul>
      </div>

      <!-- 底部動作 -->
      <div class="footer-actions">
        <el-button @click="router.back()">關閉</el-button>
        <el-button type="primary" plain @click="handleContactSeller">聯繫賣家</el-button>
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

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const order = ref<OrderDetail | null>(null);

const fetchOrderDetail = async () => {
  const id = Number(route.params.id);
  loading.value = true;
  try {
    const res = await getOrderDetailApi(id);
    order.value = res.data;
    if (!order.value.returnInfo) {
      ElMessage.warning('找不到相關退貨資訊');
    }
  } catch (error) {
    ElMessage.error('獲取詳情失敗');
  } finally {
    loading.value = false;
  }
};

const handleContactSeller = () => {
  ElMessage.info('正在開啟聊聊...');
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('zh-TW');
};

onMounted(fetchOrderDetail);
</script>

<style scoped lang="scss">
.refund-detail-page {
  background-color: #f5f5f5;
  min-height: calc(100vh - 100px);
  padding: 20px 0;
}

.refund-detail-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 15px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 20px;
  .title { margin: 0; font-size: 20px; }
}

.status-alert {
  margin-bottom: 20px;
  border: 1px solid #faecd8;
}

.info-card {
  border-radius: 4px;
  margin-bottom: 20px;
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    .header-title { font-weight: bold; color: #333; }
    .apply-time { font-size: 13px; color: #999; }
  }
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 25px;

  .info-row {
    display: flex;
    flex-direction: column;
    gap: 8px;

    &.full-width { grid-column: span 2; }

    .label { font-size: 13px; color: #999; }
    .value { 
      font-size: 15px; color: #333; 
      &.amount { color: #ee4d2d; font-weight: bold; font-size: 18px; }
      &.description { background: #fafafa; padding: 12px; border-radius: 4px; white-space: pre-wrap; line-height: 1.6; }
    }
  }
}

.image-gallery {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-top: 5px;

  .proof-img {
    width: 100px;
    height: 100px;
    border-radius: 4px;
    border: 1px solid #eee;
    cursor: pointer;
  }
  
  .no-image { color: #ccc; font-style: italic; font-size: 13px; }
}

.tips-section {
  background: transparent;
  padding: 0 10px;
  margin-bottom: 30px;
  
  .tips-title { font-size: 15px; color: #666; margin-bottom: 12px; }
  .tips-list {
    margin: 0; padding-left: 20px;
    li { font-size: 14px; color: #888; margin-bottom: 8px; }
  }
}

.footer-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
}
</style>
