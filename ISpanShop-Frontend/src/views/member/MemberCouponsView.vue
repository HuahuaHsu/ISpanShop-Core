<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getMyCoupons } from '@/api/coupon';
import { ElMessage } from 'element-plus';
import { ArrowLeft, Ticket } from '@element-plus/icons-vue';

const router = useRouter();
const coupons = ref<any[]>([]);
const loading = ref(true);

const fetchMyCoupons = async () => {
  loading.value = true;
  try {
    const res = await getMyCoupons();
    coupons.value = res.data;
  } catch (error) {
    console.error('Failed to fetch my coupons:', error);
    ElMessage.error('無法載入我的優惠券');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchMyCoupons();
});

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr);
  return `${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`;
};

const getStatusLabel = (status: number) => {
  switch (status) {
    case 0: return '未使用';
    case 1: return '已使用';
    case 2: return '已過期';
    case 3: return '鎖定中';
    default: return '未知';
  }
};

const getStatusType = (status: number) => {
  switch (status) {
    case 0: return 'success';
    case 1: return 'info';
    case 2: return 'danger';
    case 3: return 'warning';
    default: return '';
  }
};
</script>

<template>
  <div class="member-coupons-page">
    <div class="header">
      <el-button :icon="ArrowLeft" circle @click="router.push('/member')" />
      <h2>我的優惠券</h2>
    </div>

    <div v-loading="loading" class="content">
      <div v-if="coupons.length > 0" class="coupon-list">
        <div v-for="item in coupons" :key="item.couponId" class="coupon-card" :class="{ 'used': item.usageStatus !== 0 }">
          <div class="coupon-left">
            <div class="discount">
              <template v-if="item.couponType === 1">
                <span class="symbol">$</span>
                <span class="value">{{ item.discountValue }}</span>
              </template>
              <template v-else>
                <span class="value">{{ (100 - item.discountValue) / 10 }}</span>
                <span class="symbol">折</span>
              </template>
            </div>
          </div>
          <div class="coupon-right">
            <div class="info">
              <h3 class="title">{{ item.title }}</h3>
              <p class="code">優惠碼: {{ item.couponCode }}</p>
              <p class="expiry">有效期至 {{ formatDate(item.endTime) }}</p>
            </div>
            <div class="status">
              <el-tag :type="getStatusType(item.usageStatus)">{{ getStatusLabel(item.usageStatus) }}</el-tag>
            </div>
          </div>
        </div>
      </div>
      <el-empty v-else-if="!loading" description="目前沒有任何優惠券">
        <el-button type="primary" @click="router.push('/coupons')">去領券中心看看</el-button>
      </el-empty>
    </div>
  </div>
</template>

<style scoped>
.member-coupons-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px;
}

.header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 25px;
  max-width: 800px;
  margin-left: auto;
  margin-right: auto;
}

.header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: bold;
}

.content {
  max-width: 800px;
  margin: 0 auto;
}

.coupon-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.coupon-card {
  display: flex;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
  height: 110px;
}

.coupon-card.used {
  filter: grayscale(1);
  opacity: 0.7;
}

.coupon-left {
  width: 120px;
  background: #ee4d2d;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  border-right: 1px dashed rgba(255,255,255,0.5);
}

.discount .value {
  font-size: 32px;
  font-weight: bold;
}

.discount .symbol {
  font-size: 14px;
  margin: 0 2px;
}

.coupon-right {
  flex: 1;
  padding: 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info .title {
  margin: 0 0 5px 0;
  font-size: 16px;
  color: #333;
}

.info .code {
  margin: 0 0 3px 0;
  font-size: 13px;
  color: #888;
}

.info .expiry {
  margin: 0;
  font-size: 12px;
  color: #999;
}
</style>
