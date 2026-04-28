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
        v-if="order?.status === 6"
        title="退款已完成"
        type="success"
        description="您的退款申請已審核通過並完成撥款。退款金額已依原支付方式退還，實際入帳時間請依銀行或第三方支付通知為準。"
        show-icon
        :closable="false"
        class="status-alert"
      />
      <el-alert
        v-else-if="order?.status === 5"
        title="退貨/退款申請審核中"
        type="warning"
        description="賣家正在審核您的退貨申請，這通常需要 1-3 個工作天。審核通過後，系統將會更新您的退款進度。"
        show-icon
        :closable="false"
        class="status-alert"
      />

      <!-- 退貨商品明細 -->
      <el-card class="info-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span class="header-title">退貨商品明細</span>
          </div>
        </template>
        
        <div class="returned-items-list">
          <div v-for="(item, idx) in order?.returnInfo?.items" :key="idx" class="returned-item">
            <el-image :src="item.coverImage" class="item-img" fit="cover" />
            <div class="item-info">
              <div class="name">{{ item.productName }}</div>
              <PromotionTags :tags="item.promotionTags" />
              <div class="variant" v-if="item.variantName">規格：{{ item.variantName }}</div>
              <div class="price-qty">
                <span class="price">${{ formatPrice(item.price) }}</span>
                <span class="qty">退貨數量：{{ item.returnQuantity }}</span>
              </div>
            </div>
          </div>
          <div v-if="!order?.returnInfo?.items?.length" class="empty-items-tip">
            (整筆訂單退貨)
          </div>
        </div>

        <!-- 費用折抵分攤明細 -->
        <div v-if="order?.returnInfo" class="refund-summary-detail">
          <div class="summary-row">
            <span>退貨商品原價小計</span>
            <span>NT$ {{ formatPrice(returnedItemsSubtotal) }}</span>
          </div>
          <!-- 全額退貨時，顯示退還運費 -->
          <div v-if="isFullReturn && (order.shippingFee || 0) > 0" class="summary-row">
            <span>運費 (全額退貨退還)</span>
            <span>NT$ {{ formatPrice(order.shippingFee || 0) }}</span>
          </div>
          <div v-if="promotionDiscountShare !== 0" class="summary-row">
            <span class="label-with-hint">
              活動促銷折抵分攤
              <el-tooltip content="按商品金額比例分攤當初享有的活動折扣" placement="top">
                <el-icon class="info-icon"><InfoFilled /></el-icon>
              </el-tooltip>
            </span>
            <span class="discount">- NT$ {{ formatPrice(Math.abs(promotionDiscountShare)) }}</span>
          </div>
          <div v-if="levelDiscountShare !== 0" class="summary-row">
            <span class="label-with-hint">
              會員等級折抵分攤
              <el-tooltip content="按商品金額比例分攤當初享有的會員折扣" placement="top">
                <el-icon class="info-icon"><InfoFilled /></el-icon>
              </el-tooltip>
            </span>
            <span class="discount">- NT$ {{ formatPrice(Math.abs(levelDiscountShare)) }}</span>
          </div>
          <div v-if="couponDiscountShare !== 0" class="summary-row">
            <span>優惠券折抵分攤</span>
            <span class="discount">- NT$ {{ formatPrice(Math.abs(couponDiscountShare)) }}</span>
          </div>
          <div v-if="pointDiscountShare !== 0" class="summary-row">
            <span>點數折抵分攤</span>
            <span class="discount">- NT$ {{ formatPrice(Math.abs(pointDiscountShare)) }}</span>
          </div>
          <div class="summary-row final">
            <span>退款金額</span>
            <span class="price">NT$ {{ formatPrice(order.returnInfo.refundAmount) }}</span>
          </div>
          <div class="refund-hint">
            <small v-if="isFullReturn">* 已退還訂單最終實付金額 (含運費)。</small>
            <small v-else>* 部分退貨已依比例扣除折抵金額，且不退還運費。</small>
          </div>
        </div>
      </el-card>

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
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, InfoFilled } from '@element-plus/icons-vue';
import { getOrderDetailApi } from '@/api/order';
import type { OrderDetail } from '@/types/order';
import { ElMessage } from 'element-plus';
import { useChatStore } from '@/stores/chat';
import PromotionTags from '@/components/common/PromotionTags.vue';

const route = useRoute();
const router = useRouter();
const chatStore = useChatStore();
const loading = ref(false);
const order = ref<OrderDetail | null>(null);

// 計算屬性：退貨商品原價小計
const returnedItemsSubtotal = computed(() => {
  if (!order.value?.returnInfo?.items) return 0;
  return order.value.returnInfo.items.reduce((sum, item) => sum + (item.price * item.returnQuantity), 0);
});

// 計算屬性：判斷是否為全額退貨
const isFullReturn = computed(() => {
  if (!order.value || returnedItemsSubtotal.value === 0) return false;
  return Math.abs(returnedItemsSubtotal.value - order.value.totalAmount) < 1;
});

// 計算分攤比例
const ratio = computed(() => {
  if (!order.value || order.value.totalAmount === 0) return 0;
  return returnedItemsSubtotal.value / order.value.totalAmount;
});

// 各項折抵分攤
const levelDiscountShare = computed(() => {
  if (!order.value) return 0;
  return isFullReturn.value ? (order.value.levelDiscount || 0) : Math.round((order.value.levelDiscount || 0) * ratio.value);
});

const couponDiscountShare = computed(() => {
  if (!order.value) return 0;
  return isFullReturn.value ? (order.value.discountAmount || 0) : Math.round((order.value.discountAmount || 0) * ratio.value);
});

const pointDiscountShare = computed(() => {
  if (!order.value) return 0;
  return isFullReturn.value ? (order.value.pointDiscount || 0) : Math.round((order.value.pointDiscount || 0) * ratio.value);
});

const promotionDiscountShare = computed(() => {
  if (!order.value) return 0;
  return isFullReturn.value ? (order.value.promotionDiscount || 0) : Math.round((order.value.promotionDiscount || 0) * ratio.value);
});


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
  if (order.value?.sellerId) {
    chatStore.openChatWithUser(order.value.sellerId, order.value.storeName);
  } else {
    ElMessage.warning('無法取得賣家資訊');
  }
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

.returned-items-list {
  .returned-item {
    display: flex;
    gap: 15px;
    padding: 15px 0;
    border-bottom: 1px solid #f0f0f0;
    &:last-child { border-bottom: none; }

    .item-img {
      width: 64px;
      height: 64px;
      border-radius: 4px;
      border: 1px solid #eee;
      flex-shrink: 0;
    }

    .item-info {
      flex: 1;
      .name { font-size: 14px; font-weight: 500; color: #333; margin-bottom: 4px; }
      .variant { font-size: 12px; color: #999; margin-bottom: 8px; }
      .price-qty {
        display: flex;
        justify-content: space-between;
        .price { color: #666; }
        .qty { color: #ee4d2d; font-weight: 500; }
      }
    }
  }
}

.refund-summary-detail {
  margin-top: 10px;
  padding: 15px;
  background-color: #fafafa;
  border-radius: 4px;
  border: 1px dashed #e4e4e4;

  .summary-row {
    display: flex;
    justify-content: space-between;
    margin-bottom: 8px;
    font-size: 13px;
    color: #666;

    .discount { color: #ee4d2d; }
    .label-with-hint { display: flex; align-items: center; gap: 4px; }
    .info-icon { font-size: 14px; color: #909399; cursor: pointer; }

    &.final {
      margin-top: 12px;
      padding-top: 12px;
      border-top: 1px solid #e4e4e4;
      font-size: 15px;
      font-weight: bold;
      color: #333;
      .price { color: #ee4d2d; font-size: 18px; }
    }
  }

  .refund-hint {
    margin-top: 10px;
    text-align: right;
    color: #999;
    font-size: 11px;
  }
}

.empty-items-tip {
  text-align: center;
  color: #999;
  padding: 20px 0;
  font-style: italic;
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
