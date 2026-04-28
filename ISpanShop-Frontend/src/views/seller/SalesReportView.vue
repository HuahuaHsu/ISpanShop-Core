<template>
  <div class="sales-report-container">
    <div class="page-header">
      <div class="header-left">
        <h1 class="page-title">銷售報表</h1>
        <span class="page-date">更新時間: {{ lastUpdateTime }}</span>
      </div>
      <div class="header-right">
        <el-radio-group v-model="timeRange" size="small" class="time-toggle">
          <el-radio-button :label="7">最近 7 天</el-radio-button>
          <el-radio-button :label="30">最近 30 天</el-radio-button>
        </el-radio-group>
      </div>
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
          <el-col :xs="24" :sm="12" :lg="12">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon revenue">
                  <el-icon :size="22"><Money /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-main">
                    <div class="stat-value">${{ formatPrice(dashboardData.kpis.revenueLast7Days) }}</div>
                    <div class="stat-change" :class="dashboardData.kpis.revenueGrowthType">
                      <el-icon :size="12" v-if="dashboardData.kpis.revenueGrowthType !== 'neutral'">
                        <component :is="dashboardData.kpis.revenueGrowthType === 'up' ? CaretTop : CaretBottom" />
                      </el-icon>
                      <el-icon :size="12" v-else><Minus /></el-icon>
                      {{ dashboardData.kpis.revenueGrowthRate }}
                    </div>
                  </div>
                  <div class="stat-label">近 {{ timeRange }} 天營收</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="24" :sm="12" :lg="12">
            <el-card class="stat-card" shadow="never">
              <div class="stat-inner">
                <div class="stat-icon orders">
                  <el-icon :size="22"><Document /></el-icon>
                </div>
                <div class="stat-content">
                  <div class="stat-main">
                    <div class="stat-value">{{ dashboardData.kpis.ordersLast7Days }}</div>
                    <div class="stat-change" :class="dashboardData.kpis.ordersGrowthType">
                      <el-icon :size="12" v-if="dashboardData.kpis.ordersGrowthType !== 'neutral'">
                        <component :is="dashboardData.kpis.ordersGrowthType === 'up' ? CaretTop : CaretBottom" />
                      </el-icon>
                      <el-icon :size="12" v-else><Minus /></el-icon>
                      {{ dashboardData.kpis.ordersGrowthRate }}
                    </div>
                  </div>
                  <div class="stat-label">近 {{ timeRange }} 天訂單數</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>

        <!-- 圖表區域 (滿版) -->
        <el-row :gutter="16" class="main-row">
          <el-col :span="24">
            <el-card class="chart-card" shadow="never">
              <template #header>
                <div class="card-header">
                  <span class="card-title">📈 訂單數量趨勢 (近 {{ timeRange }} 天)</span>
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
        </el-row>

        <!-- 熱銷商品排行 (滿版橫幅) -->
        <el-row :gutter="16">
          <el-col :span="24">
            <el-card class="top-products-card" shadow="never">
              <template #header>
                <div class="card-header">
                  <span class="card-title">🏆 熱銷商品排行 (近 {{ timeRange }} 天)</span>
                </div>
              </template>
              <el-table 
                :data="dashboardData.topProducts" 
                stripe 
                style="width: 100%"
                class="rank-table"
                @row-click="handleRowClick"
              >
                <el-table-column label="排行" width="80" align="center">
                  <template #default="scope">
                    <span :class="['rank-num', { 'top-three': scope.$index < 3 }]">{{ scope.$index + 1 }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="商品資訊" min-width="400">
                  <template #default="{ row }">
                    <div class="product-info">
                      <el-image :src="row.productImage || row.ProductImage" fit="cover" class="product-img">
                        <template #error>
                          <div class="image-placeholder">🖼️</div>
                        </template>
                      </el-image>
                      <span class="product-name">{{ row.productName || row.ProductName }}</span>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column prop="salesVolume" label="總銷量" width="150" align="right">
                  <template #default="scope">
                    <span class="sales-value">{{ scope.row.salesVolume || scope.row.SalesVolume }}</span>
                    <span class="unit"> 件</span>
                  </template>
                </el-table-column>
                <el-table-column prop="salesRevenue" label="銷售額" width="180" align="right">
                  <template #default="scope">
                    <span class="revenue-value">NT$ {{ formatPrice(scope.row.salesRevenue || scope.row.SalesRevenue) }}</span>
                  </template>
                </el-table-column>
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
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { getStoreStatusApi, getSellerDashboardApi } from '@/api/store';
import type { SellerDashboardData } from '@/types/store';
import { ElMessage } from 'element-plus';
import { Money, Document, CaretTop, CaretBottom, Minus, Picture } from '@element-plus/icons-vue';
import VueApexCharts from 'vue3-apexcharts';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const apexchart = VueApexCharts;

const loading = ref(false);
const status = ref<string>('Pending');
const timeRange = ref(7); // 預設 7 天
const dashboardData = ref<SellerDashboardData | null>(null);

const lastUpdateTime = computed(() => {
  const d = new Date();
  return `${d.getFullYear()}/${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')} ${d.toLocaleTimeString()}`;
});

const chartOptions = computed(() => ({
  chart: {
    id: 'sales-trend',
    toolbar: { show: false },
    fontFamily: 'inherit'
  },
  xaxis: {
    categories: dashboardData.value?.salesTrend?.labels || [],
    axisBorder: { show: false },
    axisTicks: { show: false },
    labels: {
      rotate: -45,
      rotateAlways: false,
      hideOverlappingLabels: true,
      style: {
        fontSize: '12px',
        colors: '#94a3b8'
      }
    },
    // 根據天數決定顯示多少個刻度標籤
    tickAmount: timeRange.value === 30 ? 10 : undefined 
  },
  grid: {
    borderColor: '#f1f5f9',
    strokeDashArray: 4
  },
  colors: ['#ee4d2d'],
  stroke: {
    curve: 'straight',
    width: 3
  },
  markers: {
    size: 4,
    strokeWidth: 2,
    hover: { size: 6 }
  },
  yaxis: {
    labels: {
      formatter: (val: number) => `${Math.round(val)} 筆`
    },
    min: 0,
    forceNiceScale: true
  },
  tooltip: {
    y: {
      formatter: (val: number) => `${val} 筆`
    }
  }
}));

const formatPrice = (price: number) => {
  return price.toLocaleString();
};

const handleRowClick = (row: any) => {
  const id = row.productId || row.ProductId;
  if (id) {
    router.push(`/product/${id}`);
  }
};

const checkStatus = async () => {
  loading.value = true;
  try {
    const res = await getStoreStatusApi();
    status.value = res.data.status;

    if (status.value === 'Approved') {
      authStore.updateSellerStatus(true);
      await fetchDashboardData();
    } else {
      authStore.updateSellerStatus(false);
    }
  } catch (error) {
    console.error('取得資料失敗', error);
    ElMessage.error('無法取得賣場狀態或數據');
  } finally {
    loading.value = false;
  }
};

const fetchDashboardData = async () => {
  try {
    const dashboardRes = await getSellerDashboardApi({ days: timeRange.value });
    dashboardData.value = dashboardRes.data;
  } catch (error) {
    ElMessage.error('無法更新報表數據');
  }
};

// 監聽時間維度變化
watch(timeRange, () => {
  fetchDashboardData();
});

onMounted(() => {
  checkStatus();
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
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
}
.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.page-date {
  font-size: 13px;
  color: #94a3b8;
  margin-top: 4px;
  display: block;
}

.time-toggle :deep(.el-radio-button__inner) {
  padding: 8px 16px;
}
.time-toggle :deep(.el-radio-button__original-radio:checked + .el-radio-button__inner) {
  background-color: #ee4d2d;
  border-color: #ee4d2d;
  box-shadow: -1px 0 0 0 #ee4d2d;
}

/* 統計卡片樣式同步首頁 */
.stat-cards {
  margin-bottom: 20px;
}
.stat-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
  margin-bottom: 16px;
  height: 100px;
  display: flex;
  align-items: center;
  
  :deep(.el-card__body) {
    padding: 20px;
    width: 100%;
  }
}
.stat-inner {
  display: flex;
  align-items: center;
  gap: 16px;
}
.stat-main {
  display: flex;
  align-items: baseline;
  gap: 8px;
}
.stat-icon {
  width: 48px;
  height: 48px;
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
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  line-height: 1;
}
.stat-label {
  font-size: 13px;
  color: #64748b;
  margin-top: 6px;
}
.stat-change {
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 2px;
  font-weight: 600;
  
  &.up    { color: #22c55e; }
  &.down  { color: #ef4444; }
  &.neutral { color: #94a3b8; }
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
}

.status-box {
  padding: 80px 0;
  text-align: center;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e8eaf0;
}

.status-tip {
  margin-bottom: 15px;
  color: #64748b;
}

.rank-table :deep(.el-table__row) {
  cursor: pointer;
}

.rank-num {
  width: 24px;
  height: 24px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  background: #f5f7fa;
  font-size: 12px;
  font-weight: 700;
  color: #94a3b8;
}
.rank-num.top-three {
  background: #ee4d2d;
  color: white;
}

.product-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
.product-img {
  width: 40px;
  height: 40px;
  border-radius: 4px;
  background: #f8fafc;
  flex-shrink: 0;
}
.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  background: #f1f5f9;
}
.product-name {
  font-size: 13px;
  font-weight: 500;
  color: #334155;
}

.sales-value {
  font-weight: 700;
  color: #ee4d2d;
  font-size: 15px;
}
.unit {
  font-size: 12px;
  color: #94a3b8;
}
.revenue-value {
  font-weight: 600;
  color: #1e293b;
}
</style>
