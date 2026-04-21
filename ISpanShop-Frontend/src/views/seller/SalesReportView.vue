<template>
  <div class="sales-report-container">
    <div class="page-header">
      <h2 class="title">銷售報表</h2>
      <div class="header-actions">
        <el-tag type="info">數據更新時間：{{ lastUpdateTime }}</el-tag>
      </div>
    </div>

    <div v-loading="loading" class="status-content">
      <!-- 1. 尚未申請：留在本頁顯示申請引導 -->
      <div v-if="status === 'NotApplied'" class="status-box">
        <el-empty description="您尚未擁有賣場">
          <el-button type="primary" @click="router.push('/member/seller-apply')">
            立即申請成為賣家
          </el-button>
        </el-empty>
      </div>

      <!-- 2. 審核中：留在本頁顯示提示 -->
      <div v-else-if="status === 'Pending'" class="status-box">
        <el-empty image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" description="賣場申請審核中">
          <template #extra>
            <p class="status-tip">管理員正在審核您的申請，請耐心等候。</p>
            <el-button @click="checkStatus">重新整理狀態</el-button>
          </template>
        </el-empty>
      </div>

      <!-- 3. 審核被駁回：留在本頁顯示駁回訊息 -->
      <div v-else-if="status === 'Rejected'" class="status-box">
        <el-result icon="error" title="申請被駁回" sub-title="很抱歉，您的賣場申請未通過審核。">
          <template #extra>
            <el-button type="primary" @click="router.push('/member/seller-apply')">查看詳情並重新申請</el-button>
          </template>
        </el-result>
      </div>

      <!-- 4. 審核通過：顯示轉場並立即跳轉 -->
      <div v-else-if="status === 'Approved'" class="status-box">
        <el-result icon="success" title="審核已通過" sub-title="正在為您導向賣家中心...">
        </el-result>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { Money, Document, Goods, Warning } from '@element-plus/icons-vue';
import { getSellerDashboardApi } from '@/api/store';
import type { SellerDashboardData } from '@/types/store';
import { ElMessage } from 'element-plus';

const apexchart = VueApexCharts;
const loading = ref(false);
const status = ref<StoreStatus | ''>('');

const lastUpdateTime = computed(() => {
  return new Date().toLocaleString();
});

const chartOptions = ref({
  chart: {
    id: 'sales-trend',
    toolbar: { show: false }
  },
  xaxis: {
    categories: [] as string[]
  },
  colors: ['#ee4d2d'],
  stroke: {
    curve: 'smooth',
    width: 3
  },
  markers: {
    size: 4
  },
  yaxis: {
    labels: {
      formatter: (val: number) => `$${val.toLocaleString()}`
    }
  }
});

const fetchDashboardData = async () => {
  loading.value = true;
  try {
    const res = await getStoreStatusApi();
    status.value = res.data.status;

    if (status.value === 'Approved') {
      // 同步更新前端 AuthStore 身分，確保路由守衛放行
      authStore.updateSellerStatus(true);
      
      // 延時一小段時間讓使用者看清狀態，然後跳轉至正式賣家中心 (/seller)
      setTimeout(() => {
        router.replace('/seller');
      }, 800);
    } else {
      // 若未通過，確保身分為 false (同步資料庫最新狀態)
      authStore.updateSellerStatus(false);
    }
  } catch (error: any) {
    console.error('檢查賣場狀態失敗', error);
    ElMessage.error('無法取得賣場狀態');
  } finally {
    loading.value = false;
  }
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

onMounted(() => {
  checkStatus();
});
</script>

<style scoped lang="scss">
.sales-report-container {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  
  .title { 
    margin: 0; 
    font-size: 22px; 
    font-weight: 700;
    color: #1e293b;
  }
}

.kpi-row {
  margin-bottom: 24px;
}

.kpi-card {
  border: 1px solid #e8eaf0;
  border-radius: 12px;
  margin-bottom: 16px;

  :deep(.el-card__body) {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 24px;
  }

  .kpi-icon {
    width: 52px;
    height: 52px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
    flex-shrink: 0;
    
    &.revenue { background-color: #fff1f0; color: #ff4d4f; }
    &.orders { background-color: #e6f7ff; color: #1890ff; }
    &.products { background-color: #f6ffed; color: #52c41a; }
    &.warning { background-color: #fffbe6; color: #faad14; }
  }

  .kpi-info {
    .kpi-label { font-size: 13px; color: #64748b; margin-bottom: 4px; }
    .kpi-value { font-size: 24px; font-weight: bold; color: #1e293b; }
    .warning-text { color: #faad14; }
  }
}

.main-row {
  .chart-card, .top-products-card {
    height: 100%;
    margin-bottom: 24px;
    border: 1px solid #e8eaf0;
    border-radius: 12px;
  }
}

.card-header {
  font-weight: bold;
  color: #1e293b;
}

.chart-wrapper {
  padding: 10px 0;
}
</style>
