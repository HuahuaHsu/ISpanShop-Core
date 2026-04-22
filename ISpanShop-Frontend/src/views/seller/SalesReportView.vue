<template>
  <div class="sales-report-container">
    <div class="page-header">
      <h1 class="page-title">銷售報表</h1>
      <span class="page-date">{{ lastUpdateTime }}</span>
    </div>

    <div v-loading="loading" class="status-content">
      <!-- 1. 尚未申請 -->
      <div v-if="status === 'NotApplied'" class="status-box">
        <el-empty description="您尚未擁有賣場">
          <el-button type="primary" @click="router.push('/member/seller-apply')">
            立即申請成為賣家
          </el-button>
        </el-empty>
      </div>

      <!-- 2. 審核中 -->
      <div v-else-if="status === 'Pending'" class="status-box">
        <el-empty image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" description="賣場申請審核中">
          <template #extra>
            <p class="status-tip">管理員正在審核您的申請，請耐心等候。</p>
            <el-button @click="checkStatus">重新整理狀態</el-button>
          </template>
        </el-empty>
      </div>

      <!-- 3. 審核被駁回 -->
      <div v-else-if="status === 'Rejected'" class="status-box">
        <el-result icon="error" title="申請被駁回" sub-title="很抱歉，您的賣場申請未通過審核。">
          <template #extra>
            <el-button type="primary" @click="router.push('/member/seller-apply')">查看詳情並重新申請</el-button>
          </template>
        </el-result>
      </div>

      <!-- 4. 審核通過：顯示正式報表內容 -->
      <div v-else-if="status === 'Approved' && dashboardData" class="report-content">
        <!-- KPI 數據卡片 -->
        <el-row :gutter="16" class="stat-cards">
          <el-col :xs="24" :sm="12" :lg="6">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon revenue">
                  <el-icon :size="22"><Money /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-value">${{ formatPrice(dashboardData.kpis.totalRevenue) }}</div>
                  <div class="stat-label">總累積營收</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="24" :sm="12" :lg="6">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon orders">
                  <el-icon :size="22"><Document /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-value">{{ dashboardData.kpis.totalOrders }}</div>
                  <div class="stat-label">總訂單數</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="24" :sm="12" :lg="6">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon products">
                  <el-icon :size="22"><Goods /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-value">{{ dashboardData.kpis.totalProducts }}</div>
                  <div class="stat-label">架上商品數</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="24" :sm="12" :lg="6">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon warning">
                  <el-icon :size="22"><Warning /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-value" :class="{ 'warning-text': dashboardData.kpis.lowStockCount > 0 }">
                    {{ dashboardData.kpis.lowStockCount }}
                  </div>
                  <div class="stat-label">低庫存警告</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>

        <!-- 圖表與排行 -->
        <el-row :gutter="16" class="main-row">
          <el-col :lg="16" :md="24">
            <el-card class="chart-card" shadow="never">
              <template #header>
                <div class="card-header">
                  <span class="card-title">📈 銷售金額趨勢 (近 30 天)</span>
                </div>
              </template>
              <div class="chart-wrapper">
                <apexchart
                  type="line"
                  height="350"
                  :options="chartOptions"
                  :series="dashboardData.salesTrend.series"
                />
              </div>
            </el-card>
          </el-col>
          <el-col :lg="8" :md="24">
            <el-card class="top-products-card" shadow="never">
              <template #header>
                <div class="card-header">
                  <span class="card-title">🏆 熱銷商品排行</span>
                </div>
              </template>
              <el-table :data="dashboardData.topProducts" stripe style="width: 100%">
                <el-table-column type="index" label="#" width="50" align="center" />
                <el-table-column prop="productName" label="商品名稱" show-overflow-tooltip />
                <el-table-column prop="salesVolume" label="銷量" width="80" align="right" />
              </el-table>
              <el-empty v-if="dashboardData.topProducts.length === 0" description="暫無數據" :image-size="60" />
            </el-card>
          </el-col>
        </el-row>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { Money, Document, Goods, Warning } from '@element-plus/icons-vue';
import { getSellerDashboardApi, getStoreStatusApi } from '@/api/store';
import type { SellerDashboardData, StoreStatus } from '@/types/store';
import { ElMessage } from 'element-plus';
import VueApexCharts from 'vue3-apexcharts';

const router = useRouter();
const authStore = useAuthStore();
const apexchart = VueApexCharts;

const loading = ref(false);
const dashboardData = ref<SellerDashboardData | null>(null);

const lastUpdateTime = computed(() => {
  const d = new Date();
  return `${d.getFullYear()}/${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')} ${d.toLocaleTimeString()}`;
});

const chartOptions = ref({
  chart: {
    id: 'sales-trend',
    toolbar: { show: false },
    fontFamily: 'inherit'
  },
  xaxis: {
    categories: [] as string[],
    axisBorder: { show: false },
    axisTicks: { show: false }
  },
  grid: {
    borderColor: '#f1f5f9',
    strokeDashArray: 4
  },
  colors: ['#ee4d2d'],
  stroke: {
    curve: 'smooth',
    width: 3
  },
  markers: {
    size: 4,
    strokeWidth: 2,
    hover: { size: 6 }
  },
  yaxis: {
    labels: {
      formatter: (val: number) => `$${val.toLocaleString()}`
    }
  },
  tooltip: {
    y: {
      formatter: (val: number) => `$ ${val.toLocaleString()}`
    }
  }
});

const dashboardData = ref<SellerDashboardData | null>(null);

const checkStatus = async () => {
  loading.value = true;
  try {
    const res = await getStoreStatusApi();
    status.value = res.data.status;

    if (status.value === 'Approved') {
      authStore.updateSellerStatus(true);
      const dashboardRes = await getSellerDashboardApi();
      dashboardData.value = dashboardRes.data;
      
      if (dashboardData.value.salesTrend) {
        chartOptions.value.xaxis.categories = dashboardData.value.salesTrend.labels;
      }
    } else {
      authStore.updateSellerStatus(false);
    }
    }
    console.error('取得資料失敗', error);
    ElMessage.error('無法取得賣場數據');
    ElMessage.error('無法取得賣場狀態');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchDashboardData();
});
</script>

<style scoped lang="scss">
.sales-report-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.page-date {
  color: #94a3b8;
  font-size: 14px;
}

/* 統計卡片樣式同步首頁 */
.stat-cards {
  margin-bottom: 20px;
}
.stat-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
  margin-bottom: 16px;
  
  :deep(.el-card__body) {
    padding: 20px;
  }
}
.stat-inner {
  display: flex;
  align-items: center;
  gap: 16px;
}
.stat-icon {
  width: 52px;
  height: 52px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  
  &.revenue { background-color: #fff7ed; color: #ee4d2d; }
  &.orders  { background-color: #f0fdf4; color: #22c55e; }
  &.products { background-color: #eff6ff; color: #3b82f6; }
  &.warning  { background-color: #fef9c3; color: #eab308; }
}
.stat-content {
  flex: 1;
}
.stat-value {
  font-size: 26px;
  font-weight: 700;
  color: #1e293b;
  line-height: 1.2;
}
.stat-label {
  font-size: 13px;
  color: #64748b;
  margin-top: 2px;
}
.warning-text {
  color: #ef4444;
}

/* 圖表與排行卡片 */
.chart-card, .top-products-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
  margin-bottom: 20px;
  height: calc(100% - 20px);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.card-title {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
}

.chart-wrapper {
  padding: 10px 0;
  padding: 10px 0;
}

.status-box {
  padding: 80px 0;
  text-align: center;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e8eaf0;
}
</style>
