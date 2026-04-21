<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getMyCoupons } from '@/api/coupon';
import { ElMessage } from 'element-plus';

const router = useRouter();
const coupons = ref<any[]>([]);
const loading = ref(true);

async function loadCoupons() {
  loading.value = true;
  try {
    const res = await getMyCoupons();
    coupons.value = res.data;
  } catch (err) {
    ElMessage.error('載入優惠券失敗');
  } finally {
    loading.value = false;
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('zh-TW');
}

function getStatusText(status: number) {
  switch (status) {
    case 0: return '未使用';
    case 1: return '已使用';
    case 2: return '已過期';
    case 3: return '鎖定中';
    default: return '未知';
  }
}

function getStatusType(status: number) {
  switch (status) {
    case 0: return 'success';
    case 1: return 'info';
    case 2: return 'danger';
    case 3: return 'warning';
    default: return '';
  }
}

onMounted(() => {
  loadCoupons();
});
</script>

<template>
  <div class="page-container">
    <div class="header">
      <el-button @click="router.back()" circle icon="ArrowLeft" />
      <h2>我的優惠券</h2>
    </div>

    <div class="coupon-list" v-loading="loading">
      <div v-if="coupons.length === 0" class="empty">
        <el-empty description="目前沒有持有的優惠券" />
      </div>
      <div v-else class="grid">
        <el-card v-for="c in coupons" :key="c.couponId" class="coupon-card" :class="{ used: c.usageStatus !== 0 }">
          <div class="coupon-content">
            <div class="discount-box">
              <span class="value">{{ c.discountValue }}</span>
              <span class="unit">{{ c.couponType === 1 ? '元' : '折' }}</span>
            </div>
            <div class="info-box">
              <div class="title">{{ c.title }}</div>
              <div class="code">代碼：{{ c.couponCode }}</div>
              <div class="time">{{ formatDate(c.startTime) }} - {{ formatDate(c.endTime) }}</div>
            </div>
            <div class="status-box">
              <el-tag :type="getStatusType(c.usageStatus)">{{ getStatusText(c.usageStatus) }}</el-tag>
            </div>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px;
}
.header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 24px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}
.coupon-card {
  border-left: 5px solid #ee4d2d;
}
.coupon-card.used {
  border-left-color: #909399;
  opacity: 0.7;
}
.coupon-content {
  display: flex;
  align-items: center;
  gap: 15px;
}
.discount-box {
  background: #fff5f2;
  color: #ee4d2d;
  padding: 15px;
  border-radius: 8px;
  min-width: 80px;
  text-align: center;
}
.discount-box .value { font-size: 24px; font-weight: bold; }
.info-box { flex: 1; }
.info-box .title { font-weight: bold; margin-bottom: 5px; }
.info-box .code { font-size: 12px; color: #999; }
.info-box .time { font-size: 11px; color: #999; margin-top: 5px; }
</style>
