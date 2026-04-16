<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getPublicCoupons, claimCoupon, type Coupon } from '@/api/coupon';
import { ElMessage } from 'element-plus';
import { Ticket, Timer, CircleCheck, InfoFilled } from '@element-plus/icons-vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const coupons = ref<Coupon[]>([]);
const loading = ref(true);

const fetchCoupons = async () => {
  loading.value = true;
  try {
    const res = await getPublicCoupons();
    coupons.value = res.data;
  } catch (error) {
    console.error('Failed to fetch coupons:', error);
    ElMessage.error('無法載入優惠券列表');
  } finally {
    loading.value = false;
  }
};

const handleClaim = async (coupon: Coupon) => {
  // 1. 檢查登入狀態
  if (!authStore.isLoggedIn) {
    ElMessage.warning('請先登入後再領取優惠券');
    router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } });
    return;
  }

  if (coupon.isClaimed) return;
  
  try {
    const res = await claimCoupon(coupon.id);
    ElMessage.success(res.data.message || '領取成功！');
    // 更新本地狀態
    coupon.isClaimed = true;
    coupon.usedQuantity += 1;
  } catch (error: any) {
    // 優先抓取後端回傳的 message
    const errorMsg = error.response?.data?.message || error.message || '領取失敗，請稍後再試';
    ElMessage.error(errorMsg);
  }
};

onMounted(() => {
  fetchCoupons();
});

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr);
  return `${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`;
};
</script>

<template>
  <div class="coupons-page">
    <div class="container">
      <div class="header">
        <h1><el-icon><Ticket /></el-icon> 領券中心</h1>
        <p>領取限時優惠券，購物更划算！</p>
      </div>

      <div v-loading="loading" class="coupon-grid">
        <template v-if="coupons.length > 0">
          <div v-for="coupon in coupons" :key="coupon.id" class="coupon-card" :class="{ 'claimed': coupon.isClaimed }">
            <div class="coupon-left">
              <div class="value">
                <template v-if="coupon.couponType === 1">
                  <span class="symbol">$</span>
                  <span class="amount">{{ coupon.discountValue }}</span>
                </template>
                <template v-else>
                  <span class="amount">{{ (100 - coupon.discountValue) / 10 }}</span>
                  <span class="symbol">折</span>
                </template>
              </div>
              <div class="condition">滿 ${{ coupon.minimumSpend }} 使用</div>
            </div>
            <div class="coupon-right">
              <div class="coupon-info">
                <h3 class="title">{{ coupon.title }}</h3>
                <div class="code">序號: {{ coupon.couponCode }}</div>
                <div class="expiry">
                  <el-icon><Timer /></el-icon>
                  有效期至 {{ formatDate(coupon.endTime) }}
                </div>
              </div>
              <div class="coupon-action">
                <el-button 
                  :type="coupon.isClaimed ? 'info' : 'primary'" 
                  round
                  :disabled="coupon.isClaimed"
                  @click="handleClaim(coupon)"
                >
                  {{ coupon.isClaimed ? '已領取' : '立即領取' }}
                </el-button>
                <div class="progress-info">
                  已領 {{ Math.round((coupon.usedQuantity / coupon.totalQuantity) * 100) }}%
                </div>
              </div>
            </div>
            <div v-if="coupon.isClaimed" class="claimed-badge">
              <el-icon><CircleCheck /></el-icon>
            </div>
          </div>
        </template>
        <el-empty v-else-if="!loading" description="目前沒有可領取的優惠券" />
      </div>

      <div class="tips">
        <h3><el-icon><InfoFilled /></el-icon> 領券說明</h3>
        <ul>
          <li>每張優惠券每人領取次數有限，領完為止。</li>
          <li>領取後的優惠券可在「會員中心 > 我的優惠券」中查看。</li>
          <li>結帳時符合條件即可選用折抵。</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>
.coupons-page {
  padding: 40px 0;
  background-color: #f5f5f5;
  min-height: calc(100vh - 200px);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 15px;
}

.header {
  text-align: center;
  margin-bottom: 40px;
}

.header h1 {
  font-size: 32px;
  color: #ee4d2d;
  margin-bottom: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.header p {
  color: #666;
  font-size: 16px;
}

.coupon-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(550px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

@media (max-width: 600px) {
  .coupon-grid {
    grid-template-columns: 1fr;
  }
}

.coupon-card {
  display: flex;
  background-color: #fff;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  position: relative;
  height: 140px;
  transition: transform 0.3s;
}

.coupon-card:hover {
  transform: translateY(-5px);
}

.coupon-left {
  width: 160px;
  background: linear-gradient(135deg, #ee4d2d 0%, #ff7337 100%);
  color: #fff;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 10px;
  border-right: 2px dashed rgba(255, 255, 255, 0.3);
}

.coupon-card.claimed .coupon-left {
  background: #999;
}

.value {
  margin-bottom: 5px;
}

.amount {
  font-size: 36px;
  font-weight: bold;
}

.symbol {
  font-size: 18px;
  margin: 0 2px;
}

.condition {
  font-size: 13px;
  opacity: 0.9;
}

.coupon-right {
  flex: 1;
  padding: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.coupon-info .title {
  font-size: 18px;
  margin-bottom: 8px;
  color: #333;
}

.coupon-info .code {
  font-size: 14px;
  color: #888;
  margin-bottom: 5px;
}

.coupon-info .expiry {
  font-size: 12px;
  color: #999;
  display: flex;
  align-items: center;
  gap: 4px;
}

.coupon-action {
  text-align: center;
}

.progress-info {
  font-size: 11px;
  color: #999;
  margin-top: 8px;
}

.claimed-badge {
  position: absolute;
  top: -10px;
  right: -10px;
  font-size: 60px;
  color: rgba(0, 0, 0, 0.05);
  pointer-events: none;
}

.tips {
  background-color: #fff;
  padding: 25px;
  border-radius: 8px;
  color: #666;
}

.tips h3 {
  margin-bottom: 15px;
  color: #333;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tips ul {
  padding-left: 20px;
}

.tips li {
  margin-bottom: 8px;
  font-size: 14px;
}
</style>
