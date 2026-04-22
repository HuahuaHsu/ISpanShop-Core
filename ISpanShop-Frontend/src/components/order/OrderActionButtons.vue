<template>
  <div class="order-actions">
    <!-- 狀態 0: 待付款 -->
    <template v-if="status === 0">
      <el-button type="primary" size="default" @click="handlePay" :loading="loading">立即付款</el-button>
      <el-button @click="handleCancel" :loading="loading" size="default">取消訂單</el-button>
    </template>

    <!-- 狀態 1: 待出貨 -->
    <template v-if="status === 1">
      <el-button @click="handleRefund" :loading="loading" size="default">申請退款</el-button>
    </template>

    <!-- 狀態 2: 運送中 -->
    <template v-if="status === 2">
      <el-button type="primary" @click="handleConfirmReceipt" :loading="loading" size="default">確認收貨</el-button>
      <el-button @click="handleRefund" :loading="loading" size="default">申請退貨/退款</el-button>
    </template>

    <!-- 狀態 3: 已完成 -->
    <template v-if="status === 3">
      <el-button type="primary" plain size="default" @click="handleRebuy">再次購買</el-button>
      <el-button @click="handleRefund" :loading="loading" size="default">申請退貨/退款</el-button>
    </template>

    <!-- 狀態 4: 已取消 -->
    <template v-if="status === 4">
      <el-button type="primary" plain size="default" @click="handleRebuy">再次購買</el-button>
    </template>

    <!-- 狀態 5: 退貨/款中 -->
    <template v-if="status === 5 && isDetail">
      <el-button size="default" @click="handleRefundDetail">查看退貨/款詳情</el-button>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { cancelOrderApi, confirmReceiptApi, getOrderDetailApi } from '@/api/order';
import { useCartStore } from '@/stores/cart';
import type { CartItem } from '@/stores/cart';

interface Props {
  orderId: number;
  orderNumber: string;
  status: number;
  isDetail?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  isDetail: false
});

const emit = defineEmits(['refresh']);
const router = useRouter();
const loading = ref(false);
const cartStore = useCartStore();

// ── 動作處理 ──

const handlePay = async () => {
  // 對於已存在的訂單，我們導向結帳頁並帶入 orderId 參數
  // 讓結帳頁進入「支付確認模式」，而非「建立訂單模式」
  router.push({
    path: '/checkout',
    query: { orderId: props.orderId }
  });
};

const handleCancel = async () => {
  try {
    await ElMessageBox.confirm('確定要取消這筆訂單嗎？', '提示', {
      confirmButtonText: '確定取消',
      cancelButtonText: '再想想',
      type: 'warning'
    });
    
    loading.value = true;
    await cancelOrderApi(props.orderId);
    ElMessage.success('訂單已取消');
    emit('refresh');
  } catch (error) {
    if (error !== 'cancel') {
      const msg = (error as any).response?.data?.message || '操作失敗';
      ElMessage.error(msg);
    }
  } finally {
    loading.value = false;
  }
};

const handleConfirmReceipt = async () => {
  try {
    await ElMessageBox.confirm('確認已收到商品且無誤嗎？確認後訂單將轉為已完成。', '確認收貨', {
      confirmButtonText: '確認收貨',
      cancelButtonText: '取消',
      type: 'success'
    });
    
    loading.value = true;
    await confirmReceiptApi(props.orderId);
    ElMessage.success('訂單已完成，感謝您的購物！');
    emit('refresh');
  } catch (error) {
    if (error !== 'cancel') {
      const msg = (error as any).response?.data?.message || '操作失敗';
      ElMessage.error(msg);
    }
  } finally {
    loading.value = false;
  }
};

const handleRefund = () => {
  router.push(`/member/orders/${props.orderId}/refund`);
};

const handleRefundDetail = () => {
  router.push(`/member/orders/${props.orderId}/refund/detail`);
};

const handleRebuy = () => {
  // 再次購買邏輯，通常是導向商品頁或直接加購物車
  ElMessage.info('再次購買功能開發中...');
};
</script>

<style scoped>
.order-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
}

:deep(.el-button + .el-button) {
  margin-left: 0;
}
</style>
