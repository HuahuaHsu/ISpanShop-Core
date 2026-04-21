<template>
  <div class="sales-report-container">
    <div class="page-header">
      <h2 class="title">銷售報表</h2>
      <div class="header-actions">
        <el-tag type="info">數據更新時間：{{ lastUpdateTime }}</el-tag>
      </div>
    </div>

    <div v-loading="loading" class="report-content">
      <!-- 報表內容實作預留區 -->
      <el-empty description="報表數據讀取中或暫無數據" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { getSellerDashboardApi } from '@/api/store';
import type { SellerDashboardData } from '@/types/store';
import { ElMessage } from 'element-plus';

const loading = ref(false);
const dashboardData = ref<SellerDashboardData | null>(null);

const lastUpdateTime = computed(() => {
  return new Date().toLocaleString();
});

const fetchDashboardData = async () => {
  loading.value = true;
  try {
    const res = await getSellerDashboardApi();
    dashboardData.value = res.data;
  } catch (error: any) {
    console.error('取得銷售數據失敗', error);
    ElMessage.error('無法取得銷售報表數據');
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

.report-content {
  min-height: 400px;
  background: #fff;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 12px rgba(0,0,0,0.05);
}
</style>
