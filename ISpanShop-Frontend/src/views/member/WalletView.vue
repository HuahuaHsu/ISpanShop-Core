<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { getWalletBalance, getPointHistory, type PointHistory } from '@/api/member';
import axios from '@/api/axios';

const router = useRouter();
const balance = ref(0);
const history = ref<PointHistory[]>([]);
const loading = ref(true);

async function loadData() {
  loading.value = true;
  try {
    const [balanceRes, historyRes] = await Promise.all([
      getWalletBalance(),
      getPointHistory()
    ]);
    balance.value = balanceRes.data.pointBalance || balanceRes.data.balance || 0;
    history.value = historyRes.data;
  } catch (err) {
    console.error('載入錢包資料失敗', err);
    ElMessage.error('載入錢包資料失敗');
  } finally {
    loading.value = false;
  }
}

async function handleTestAddPoints() {
  try {
    const res = await axios.get('/api/member/add-points-to-me');
    ElMessage.success(res.data.message || '儲值成功');
    loadData();
  } catch (err) {
    ElMessage.error('儲值失敗');
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString('zh-TW');
}

onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="page-container">
    <div class="header">
      <el-button @click="router.back()" circle icon="ArrowLeft" />
      <h2>我的錢包</h2>
    </div>

    <div class="wallet-summary">
      <el-card class="balance-card">
        <div class="balance-label">當前點數餘額</div>
        <div class="balance-value">
          <span class="currency">💎</span>
          {{ balance.toLocaleString() }}
        </div>
        <div class="balance-actions">
          <el-button type="primary" @click="handleTestAddPoints">測試儲值 (1000點)</el-button>
        </div>
      </el-card>
    </div>

    <div class="history-section">
      <h3>交易紀錄</h3>
      <el-table :data="history" v-loading="loading" style="width: 100%" border stripe>
        <el-table-column label="時間" width="180">
          <template #default="scope">
            {{ formatDate(scope.row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column prop="description" label="內容" />
        <el-table-column label="變動金額" width="120">
          <template #default="scope">
            <span :class="scope.row.changeAmount > 0 ? 'pos' : 'neg'">
              {{ scope.row.changeAmount > 0 ? '+' : '' }}{{ scope.row.changeAmount }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="balanceAfter" label="變動後餘額" width="120" />
      </el-table>
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
.wallet-summary {
  margin-bottom: 24px;
}
.balance-card {
  text-align: center;
  padding: 20px;
}
.balance-label {
  color: #909399;
  font-size: 14px;
  margin-bottom: 10px;
}
.balance-value {
  font-size: 48px;
  font-weight: bold;
  color: #ee4d2d;
  margin-bottom: 20px;
}
.currency {
  font-size: 24px;
  margin-right: 8px;
}
.history-section h3 {
  margin-bottom: 16px;
}
.pos { color: #67c23a; font-weight: bold; }
.neg { color: #f56c6c; font-weight: bold; }
</style>
