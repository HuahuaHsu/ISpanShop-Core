<template>
  <div class="my-store-container">
    <div class="page-header">
      <div class="header-left">
        <el-button @click="router.back()" circle icon="ArrowLeft" />
        <h2 class="title">賣場數據中心</h2>
      </div>
      <div class="header-right">
        <el-button type="primary" @click="goToSellerAdmin">
          <el-icon><Monitor /></el-icon> 前往賣家管理後台
        </el-button>
      </div>
    </div>

    <div v-loading="loading" class="dashboard-content">
      <!-- KPI Cards -->
      <el-row :gutter="20" class="kpi-row">
        <el-col :xs="24" :sm="12" :md="6">
          <el-card shadow="hover" class="kpi-card">
            <div class="kpi-icon revenue"><el-icon><Money /></el-icon></div>
            <div class="kpi-info">
              <div class="kpi-label">總營收</div>
              <div class="kpi-value">${{ formatPrice(data?.kpis.totalRevenue || 0) }}</div>
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-card shadow="hover" class="kpi-card">
            <div class="kpi-icon orders"><el-icon><Document /></el-icon></div>
            <div class="kpi-info">
              <div class="kpi-label">總訂單數</div>
              <div class="kpi-value">{{ data?.kpis.totalOrders || 0 }}</div>
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-card shadow="hover" class="kpi-card">
            <div class="kpi-icon products"><el-icon><Goods /></el-icon></div>
            <div class="kpi-info">
              <div class="kpi-label">在架商品</div>
              <div class="kpi-value">{{ data?.kpis.totalProducts || 0 }}</div>
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-card shadow="hover" class="kpi-card">
            <div class="kpi-icon warning"><el-icon><Warning /></el-icon></div>
            <div class="kpi-info">
              <div class="kpi-label">低庫存警告</div>
              <div class="kpi-value warning-text">{{ data?.kpis.lowStockCount || 0 }}</div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- Charts & Tables -->
      <el-row :gutter="20" class="main-row">
        <el-col :xs="24" :lg="16">
          <el-card shadow="never" class="chart-card">
            <template #header>
              <div class="card-header">
                <span>近七日銷售趨勢</span>
              </div>
            </template>
            <div class="chart-wrapper">
              <apexchart
                v-if="data"
                type="line"
                height="350"
                :options="chartOptions"
                :series="data.salesTrend.series"
              ></apexchart>
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :lg="8">
          <el-card shadow="never" class="top-products-card">
            <template #header>
              <div class="card-header">
                <span>熱銷商品排行</span>
              </div>
            </template>
            <el-table :data="data?.topProducts" style="width: 100%" size="small">
              <el-table-column type="index" label="排名" width="50" align="center" />
              <el-table-column prop="productName" label="商品名稱" show-overflow-tooltip />
              <el-table-column prop="salesVolume" label="銷量" width="60" align="right" />
              <el-table-column label="營收" width="90" align="right">
                <template #default="{ row }">
                  ${{ formatPrice(row.salesRevenue) }}
                </template>
              </el-table-column>
            </el-table>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowLeft, Money, Document, Goods, Warning, Monitor } from '@element-plus/icons-vue';
import { getSellerDashboardApi } from '@/api/store';
import type { SellerDashboardData } from '@/types/store';
import { ElMessage } from 'element-plus';
import VueApexCharts from 'vue3-apexcharts';

const apexchart = VueApexCharts;
const router = useRouter();
const loading = ref(false);
const data = ref<SellerDashboardData | null>(null);

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
    const res = await getSellerDashboardApi();
    data.value = res.data;
    chartOptions.value.xaxis.categories = res.data.salesTrend.labels;
  } catch (error: any) {
    console.error('獲取賣場數據失敗', error);
    ElMessage.error(error.response?.data?.message || '獲取賣場數據失敗');
  } finally {
    loading.value = false;
  }
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

const goToSellerAdmin = () => {
  // 導向外部賣家管理系統，或內部另一個 SPA 路由
  // 這裡假設是 /seller
  router.push('/seller');
};

onMounted(() => {
  fetchDashboardData();
});
</script>

<style scoped lang="scss">
.my-store-container {
  padding: 10px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  
  .header-left {
    display: flex;
    align-items: center;
    gap: 15px;
    .title { margin: 0; font-size: 1.5rem; }
  }
}

.kpi-row {
  margin-bottom: 20px;
}

.kpi-card {
  display: flex;
  align-items: center;
  padding: 10px;
  margin-bottom: 15px;

  :deep(.el-card__body) {
    display: flex;
    align-items: center;
    gap: 20px;
    width: 100%;
  }

  .kpi-icon {
    width: 50px;
    height: 50px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
    
    &.revenue { background-color: #fff1f0; color: #ff4d4f; }
    &.orders { background-color: #e6f7ff; color: #1890ff; }
    &.products { background-color: #f6ffed; color: #52c41a; }
    &.warning { background-color: #fffbe6; color: #faad14; }
  }

  .kpi-info {
    .kpi-label { font-size: 14px; color: #8c8c8c; margin-bottom: 4px; }
    .kpi-value { font-size: 20px; font-weight: bold; color: #262626; }
    .warning-text { color: #faad14; }
  }
}

.main-row {
  .chart-card, .top-products-card {
    height: 100%;
    margin-bottom: 20px;
  }
}

.card-header {
  font-weight: bold;
  color: #262626;
}

.chart-wrapper {
  padding: 10px 0;
}
</style>
