<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { 
  TrendCharts, 
  ShoppingCart, 
  Box, 
  Warning,
  Money,
  Document,
  ArrowRight
} from '@element-plus/icons-vue';
import { 
  getDashboardKpis, 
  getMonthlyTrend, 
  getTopCategories 
} from '@/api/dashboard';
import type { DashboardKpi, ApexChartData, TopProductSales } from '@/types/dashboard';

const storeId = ref(1);
const period = ref('month');
const loading = ref(false);

const kpiData = ref<DashboardKpi | null>(null);
const trendData = ref<ApexChartData | null>(null);
const topCategories = ref<TopProductSales[]>([]);

// 圖表設定 (保留核心營收趨勢)
const trendChartOptions = computed(() => ({
  chart: {
    type: 'area',
    toolbar: { show: false },
    fontFamily: 'inherit'
  },
  colors: ['#409EFF'],
  stroke: { curve: 'smooth', width: 3 },
  fill: {
    type: 'gradient',
    gradient: { opacityFrom: 0.6, opacityTo: 0.1 }
  },
  xaxis: {
    categories: trendData.value?.labels || [],
    axisBorder: { show: false },
    axisTicks: { show: false }
  },
  yaxis: {
    labels: {
      formatter: (val: number) => `NT$ ${new Intl.NumberFormat('zh-TW').format(val)}`
    }
  },
  tooltip: {
    y: {
      formatter: (val: number) => `NT$ ${new Intl.NumberFormat('zh-TW').format(val)}`
    }
  },
  grid: {
    borderColor: '#f1f1f1',
    strokeDashArray: 4
  }
}));

const formatNumber = (num: number) => new Intl.NumberFormat('zh-TW').format(num);

const calculateGrowth = (current: number, prev: number) => {
  if (prev === 0) return current > 0 ? 100 : 0;
  return Math.round(((current - prev) / prev) * 100);
};

const fetchData = async () => {
  loading.value = true;
  try {
    const [kpis, trend, categories] = await Promise.all([
      getDashboardKpis(storeId.value, period.value),
      getMonthlyTrend(storeId.value, new Date().getFullYear()),
      getTopCategories(storeId.value, period.value, 'revenue')
    ]);
    
    kpiData.value = kpis;
    trendData.value = trend;
    topCategories.value = categories;
  } catch (error) {
    console.error('獲取數據失敗', error);
  } finally {
    loading.value = false;
  }
};

onMounted(fetchData);

// 賣家核心 KPI
const sellerKpis = computed(() => {
  if (!kpiData.value) return [];
  
  const aov = kpiData.value.totalOrders > 0 
    ? Math.round(kpiData.value.netRevenue / kpiData.value.totalOrders) 
    : 0;
    
  return [
    { title: '累計營收', value: `NT$ ${formatNumber(kpiData.value.netRevenue)}`, growth: calculateGrowth(kpiData.value.netRevenue, kpiData.value.prevNetRevenue), icon: Money, color: '#409EFF' },
    { title: '訂單總數', value: formatNumber(kpiData.value.totalOrders), growth: calculateGrowth(kpiData.value.totalOrders, kpiData.value.prevTotalOrders), icon: ShoppingCart, color: '#67C23A' },
    { title: '平均客單價', value: `NT$ ${formatNumber(aov)}`, icon: TrendCharts, color: '#8E44AD', isAov: true },
    { title: '售出件數', value: formatNumber(kpiData.value.totalItemsSold), growth: calculateGrowth(kpiData.value.totalItemsSold, kpiData.value.prevTotalItemsSold), icon: Box, color: '#E6A23C' }
  ];
});
</script>

<template>
  <div class="seller-dashboard" v-loading="loading">
    <div class="page-header">
      <div class="title-section">
        <h2>賣家中心儀表板</h2>
        <p class="subtitle">歡迎回來！這是您賣場目前的營運現況。</p>
      </div>
      <el-radio-group v-model="period" size="default" @change="fetchData">
        <el-radio-button value="7days">近 7 天</el-radio-button>
        <el-radio-button value="month">本月</el-radio-button>
        <el-radio-button value="3months">本季</el-radio-button>
      </el-radio-group>
    </div>

    <!-- 區塊一：待辦事項中心 (置頂、醒目) -->
    <div class="todo-section">
      <div class="todo-card danger" @click="$router.push('/member/orders?status=1')">
        <div class="todo-icon"><Box /></div>
        <div class="todo-info">
          <span class="count">{{ kpiData?.pendingShipmentCount || 0 }}</span>
          <span class="label">待出貨訂單</span>
        </div>
        <el-icon class="arrow"><ArrowRight /></el-icon>
      </div>
      <div class="todo-card warning">
        <div class="todo-icon"><Warning /></div>
        <div class="todo-info">
          <span class="count">{{ kpiData?.pendingRefundCount || 0 }}</span>
          <span class="label">待處理退款</span>
        </div>
        <el-icon class="arrow"><ArrowRight /></el-icon>
      </div>
      <div class="todo-card info">
        <div class="todo-icon"><Warning /></div>
        <div class="todo-info">
          <span class="count">{{ kpiData?.lowStockProductCount || 0 }}</span>
          <span class="label">庫存預警商品</span>
        </div>
        <el-icon class="arrow"><ArrowRight /></el-icon>
      </div>
    </div>

    <!-- 區塊二：核心數據卡片 -->
    <el-row :gutter="20" class="kpi-row">
      <el-col :xs="24" :sm="12" :md="6" v-for="card in sellerKpis" :key="card.title">
        <el-card shadow="never" class="kpi-card">
          <div class="kpi-content">
            <div class="kpi-main">
              <span class="kpi-title">{{ card.title }}</span>
              <span class="kpi-value">{{ card.value }}</span>
              <div v-if="!card.isAov" class="kpi-trend" :class="card.growth >= 0 ? 'up' : 'down'">
                {{ card.growth >= 0 ? '▲' : '▼' }} {{ Math.abs(card.growth) }}% <small>較前一期</small>
              </div>
              <div v-else class="kpi-trend info">
                穩定增長中
              </div>
            </div>
            <div class="kpi-icon" :style="{ color: card.color, backgroundColor: card.color + '10' }">
              <el-icon><component :is="card.icon" /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 區塊三：趨勢圖與熱銷排行 -->
    <el-row :gutter="20">
      <el-col :xs="24" :lg="15">
        <el-card shadow="never" class="main-card">
          <template #header>
            <div class="card-header">
              <span>營收走勢圖</span>
              <el-tooltip content="顯示目前年度各月份的累計營收金額" placement="top">
                <el-icon class="help-icon"><Warning /></el-icon>
              </el-tooltip>
            </div>
          </template>
          <div class="chart-container">
            <apexchart 
              v-if="trendData"
              height="350" 
              :options="trendChartOptions" 
              :series="trendData.series" 
            />
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="9">
        <el-card shadow="never" class="main-card">
          <template #header>
            <div class="card-header">
              <span>店內熱銷排行</span>
              <el-button link type="primary">查看全部</el-button>
            </div>
          </template>
          <div class="top-products">
            <div v-for="(item, index) in topCategories.slice(0, 5)" :key="index" class="product-item">
              <div class="rank" :class="'rank-' + (index + 1)">{{ index + 1 }}</div>
              <div class="product-info">
                <span class="name">{{ item.productName }}</span>
                <span class="sales">銷售 {{ item.salesVolume }} 件</span>
              </div>
              <div class="revenue">
                NT$ {{ formatNumber(item.salesRevenue) }}
              </div>
            </div>
            <div v-if="topCategories.length === 0" class="empty-state">
              暫無銷售數據
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped lang="scss">
.seller-dashboard {
  padding: 30px;
  background-color: #f8fafc;
  min-height: 100vh;

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-end;
    margin-bottom: 30px;
    .title-section {
      h2 { margin: 0 0 8px 0; font-size: 26px; font-weight: 700; color: #1e293b; }
      .subtitle { margin: 0; color: #64748b; font-size: 15px; }
    }
  }

  // 待辦事項樣式
  .todo-section {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 20px;
    margin-bottom: 30px;

    .todo-card {
      display: flex;
      align-items: center;
      padding: 24px;
      background: white;
      border-radius: 16px;
      cursor: pointer;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      border: 1px solid #e2e8f0;
      position: relative;

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
      }

      .todo-icon {
        width: 56px; height: 56px;
        border-radius: 14px;
        display: flex; align-items: center; justify-content: center;
        font-size: 26px; margin-right: 20px;
      }

      .todo-info {
        flex: 1;
        display: flex; flex-direction: column;
        .count { font-size: 28px; font-weight: 800; line-height: 1.2; }
        .label { font-size: 14px; color: #64748b; margin-top: 4px; }
      }

      .arrow { color: #cbd5e1; font-size: 18px; }

      &.danger {
        .todo-icon { background: #fef2f2; color: #ef4444; }
        .count { color: #ef4444; }
        &:hover { border-color: #fca5a5; }
      }
      &.warning {
        .todo-icon { background: #fffbeb; color: #f59e0b; }
        .count { color: #f59e0b; }
        &:hover { border-color: #fcd34d; }
      }
      &.info {
        .todo-icon { background: #f0f9ff; color: #0ea5e9; }
        .count { color: #0ea5e9; }
        &:hover { border-color: #7dd3fc; }
      }
    }
  }

  .kpi-row { margin-bottom: 30px; }
  .kpi-card {
    border-radius: 16px; border: 1px solid #e2e8f0;
    .kpi-content { display: flex; justify-content: space-between; align-items: flex-start; }
    .kpi-main {
      display: flex; flex-direction: column;
      .kpi-title { font-size: 13px; font-weight: 600; color: #64748b; margin-bottom: 12px; letter-spacing: 0.5px; }
      .kpi-value { font-size: 22px; font-weight: 700; color: #1e293b; margin-bottom: 8px; }
      .kpi-trend {
        font-size: 12px; font-weight: 600;
        &.up { color: #10b981; }
        &.down { color: #ef4444; }
        &.info { color: #6366f1; }
        small { color: #94a3b8; font-weight: 400; margin-left: 4px; }
      }
    }
    .kpi-icon {
      width: 48px; height: 48px; border-radius: 12px;
      display: flex; align-items: center; justify-content: center; font-size: 22px;
    }
  }

  .main-card {
    border-radius: 16px; border: 1px solid #e2e8f0;
    .card-header {
      display: flex; justify-content: space-between; align-items: center;
      span { font-size: 16px; font-weight: 700; color: #1e293b; }
      .help-icon { color: #94a3b8; cursor: help; font-size: 16px; }
    }
  }

  // 熱銷排行樣式
  .top-products {
    .product-item {
      display: flex; align-items: center; padding: 16px 0;
      border-bottom: 1px solid #f1f5f9;
      &:last-child { border-bottom: none; }

      .rank {
        width: 28px; height: 28px; border-radius: 8px;
        display: flex; align-items: center; justify-content: center;
        font-size: 13px; font-weight: 700; margin-right: 15px;
        background: #f1f5f9; color: #64748b;
        &.rank-1 { background: #fef3c7; color: #d97706; }
        &.rank-2 { background: #f1f5f9; color: #475569; }
        &.rank-3 { background: #fff7ed; color: #c2410c; }
      }

      .product-info {
        flex: 1; display: flex; flex-direction: column;
        .name { font-size: 14px; font-weight: 600; color: #334155; margin-bottom: 2px; }
        .sales { font-size: 12px; color: #94a3b8; }
      }

      .revenue { font-size: 14px; font-weight: 700; color: #409EFF; }
    }
    .empty-state { padding: 40px; text-align: center; color: #94a3b8; font-size: 14px; }
  }
}

// 響應式微調
@media (max-width: 768px) {
  .seller-dashboard { padding: 15px; }
  .page-header { flex-direction: column; align-items: flex-start; gap: 15px; }
  .todo-section { grid-template-columns: 1fr; }
}
</style>
