<template>
  <div class="dashboard">
    <div class="page-header">
      <h1 class="page-title">賣家中心首頁</h1>
      <span class="page-date">{{ todayStr }}</span>
    </div>

    <!-- 區塊 A：頂部統計卡片 -->
    <el-row :gutter="16" class="stat-cards">
      <el-col :xs="24" :sm="12" :lg="6" v-for="stat in statCards" :key="stat.label">
        <el-card class="stat-card clickable-card" shadow="never" @click="router.push(stat.route)">
          <div class="stat-inner">
            <div class="stat-icon" :style="{ background: stat.iconBg }">
              <el-icon :size="22" :color="stat.iconColor">
                <component :is="stat.icon" />
              </el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-main">
                <div class="stat-value">{{ stat.value }}</div>
                <div class="stat-change" :class="stat.changeType" v-if="stat.showChange">
                  <el-icon :size="12" v-if="stat.changeType !== 'neutral'">
                    <component :is="stat.changeType === 'up' ? CaretTop : CaretBottom" />
                  </el-icon>
                  <el-icon :size="12" v-else><Minus /></el-icon>
                  {{ stat.change }}
                </div>
              </div>
              <div class="stat-label">{{ stat.label }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 區塊 B：賣家數據中心 -->
    <el-card class="data-center" shadow="never">
      <template #header>
        <div class="card-header">
          <span class="card-title">📈 賣家數據中心</span>
          <div style="display:flex;align-items:center;gap:12px">
            <el-tag type="info" size="small">全部時間</el-tag>
            <el-button text type="primary" size="small" @click="router.push('/seller/analytics/sales')">
              更多 &gt;
            </el-button>
          </div>
        </div>
      </template>
      <el-row :gutter="0" class="analytics-row">
        <el-col
          v-for="metric in analyticsMetrics"
          :key="metric.label"
          :xs="12" :sm="12" :lg="6"
          class="metric-col"
        >
          <div class="metric-item">
            <div class="metric-value">{{ metric.value }}</div>
            <div class="metric-label">
              {{ metric.label }}
              <el-tooltip v-if="metric.hasTooltip" :content="metric.tooltip" placement="top">
                <el-icon class="info-icon"><QuestionFilled /></el-icon>
              </el-tooltip>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 區塊 C：快捷操作 -->
    <el-card class="quick-actions" shadow="never">
      <template #header>
        <span class="card-title">⚡ 快捷操作</span>
      </template>
      <el-row :gutter="16">
        <el-col :xs="12" :sm="6" v-for="action in quickActions" :key="action.label">
          <div class="action-card" @click="router.push(action.route)">
            <div class="action-icon" :style="{ background: action.bg }">
              <el-icon :size="28" :color="action.color">
                <component :is="action.icon" />
              </el-icon>
            </div>
            <div class="action-label">{{ action.label }}</div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 區塊 D：近期訂單列表 -->
    <el-card class="recent-orders" shadow="never">
      <template #header>
        <div class="card-header">
          <span class="card-title">📋 近期訂單 (前 10 筆)</span>
          <el-button text type="primary" @click="router.push('/seller/orders')">查看全部</el-button>
        </div>
      </template>
      <el-table 
        :data="recentOrders" 
        stripe 
        class="orders-table clickable-table"
        @row-click="handleRowClick"
      >
        <el-table-column label="訂單資訊" min-width="280">
          <template #default="{ row }">
            <div class="order-info-cell">
              <el-image :src="row.productImage || row.ProductImage" fit="cover" class="product-img">
                <template #error>
                  <div class="image-placeholder">📦</div>
                </template>
              </el-image>
              <div class="order-meta">
                <div class="order-no">{{ row.orderNumber }}</div>
                <div class="product-name">{{ row.productName }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="buyerName" label="買家" width="120" />
        <el-table-column prop="amount" label="金額" width="140" align="right">
          <template #default="{ row }">
            <span class="order-amount">NT$ {{ row.amount.toLocaleString() }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="狀態" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTagType(row.status)" size="small" effect="light">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="建立時間" width="160" align="right">
          <template #default="{ row }">
            <span class="time-text">{{ row.createdAt }}</span>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-if="recentOrders.length === 0" description="暫無訂單" :image-size="60" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Document, Box, WarningFilled,
  CaretTop, CaretBottom,
  Plus, List, StarFilled, DataLine, QuestionFilled
} from '@element-plus/icons-vue'
import { getStoreStatusApi, getSellerDashboardApi } from '@/api/store'
import { useAuthStore } from '@/stores/auth'
import type { SellerDashboardData } from '@/types/store'
import { ElMessage } from 'element-plus'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const dashboardData = ref<SellerDashboardData | null>(null)
const isRedirecting = ref(false)

/** 安全性複核：確保使用者確實具備賣家身分 */
const checkAccess = async () => {
  if (isRedirecting.value) return
  loading.value = true
  try {
    const res = await getStoreStatusApi()
    if (res.data.status !== 'Approved') {
      isRedirecting.value = true
      authStore.updateSellerStatus(false)
      ElMessage.warning('您的賣家權限已變更')
      await router.replace('/member/mystore')
      return
    }
    const dashboardRes = await getSellerDashboardApi()
    dashboardData.value = dashboardRes.data
  } catch (error) {
    console.error('取得儀表板資料失敗', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  checkAccess()
})

const todayStr = computed<string>(() => {
  const d = new Date()
  return `${d.getFullYear()}/${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')}`
})

type ChangeType = 'up' | 'down' | 'neutral'

// 區塊 A 統計卡片
const statCards = computed(() => {
  const kpis = dashboardData.value?.kpis
  return [
    {
      label: '待出貨訂單',
      value: kpis?.pendingOrders?.toString() || '0',
      showChange: false,
      icon: Document,
      iconBg: '#fff7ed',
      iconColor: '#ee4d2d',
      route: '/seller/orders'
    },
    {
      label: '待審核退貨',
      value: kpis?.pendingRefundCount?.toString() || '0',
      showChange: false,
      icon: List,
      iconBg: '#f0fdf4',
      iconColor: '#22c55e',
      route: '/seller/returns'
    },
    {
      label: '低庫存警告',
      value: kpis?.lowStockCount?.toString() || '0',
      showChange: false,
      icon: WarningFilled,
      iconBg: '#fef9c3',
      iconColor: '#eab308',
      route: '/seller/products'
    },
    {
      label: '已上架商品',
      value: kpis?.totalProducts?.toString() || '0',
      showChange: false,
      icon: Box,
      iconBg: '#eff6ff',
      iconColor: '#3b82f6',
      route: '/seller/products'
    }
    ]
    })

// 區塊 B 數據中心
const analyticsMetrics = computed(() => {
  const kpis = dashboardData.value?.kpis
  return [
    { label: '總累積營收', value: `NT$ ${kpis?.totalRevenue?.toLocaleString() || '0'}` },
    { label: '商品點擊數', value: kpis?.totalViews?.toLocaleString() || '0' },
    { label: '訂單數', value: kpis?.totalOrders?.toLocaleString() || '0' },
    { 
      label: '訂單轉換率', 
      value: kpis?.conversionRate || '0.00%', 
      hasTooltip: true,
      tooltip: '計算公式：(總訂單數 / 商品總瀏覽數) x 100%。反映進店訪客轉化為實際買家的比例。'
    },
  ]
})

// 區塊 C 快捷操作
const quickActions = [
  { label: '新增商品', route: '/seller/products/new', icon: Plus,       bg: '#fff7ed', color: '#ee4d2d' },
  { label: '查看訂單', route: '/seller/orders',        icon: List,       bg: '#f0fdf4', color: '#22c55e' },
  { label: '建立活動', route: '/seller/promotions',    icon: StarFilled, bg: '#fef9c3', color: '#eab308' },
  { label: '查看數據', route: '/seller/analytics/traffic', icon: DataLine, bg: '#eff6ff', color: '#3b82f6' },
]

// 區塊 D 近期訂單
const recentOrders = computed(() => {
  return dashboardData.value?.recentOrders || []
})

const handleRowClick = (row: any) => {
  router.push(`/seller/orders/${row.orderId}`)
}

function statusTagType(status: string): 'success' | 'warning' | 'danger' | 'info' {
  const map: Record<string, 'success' | 'warning' | 'danger' | 'info'> = {
    '已完成': 'success',
    '待出貨': 'warning',
    '已取消': 'danger',
    '待付款': 'info',
    '運送中': 'primary' as any,
  }
  return map[status] ?? 'info'
}
</script>

<style scoped>
.dashboard {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
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

/* 統計卡片 */
.stat-cards { margin-bottom: 20px; }
.stat-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
  margin-bottom: 16px;
  height: 100px;
  display: flex;
  align-items: center;
}

.clickable-card {
  cursor: pointer;
  transition: all 0.2s;
}
.clickable-card:hover {
  border-color: #ee4d2d !important;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}
:deep(.el-card__body) {
  width: 100%;
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
}
.stat-change.up    { color: #22c55e; }
.stat-change.down  { color: #ef4444; }
.stat-change.neutral { color: #94a3b8; }

/* 數據中心 */
.data-center {
  margin-bottom: 20px;
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
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
.analytics-row { border-top: 1px solid #f1f5f9; }
.metric-col {
  border-right: 1px solid #f1f5f9;
}
.metric-col:last-child { border-right: none; }
.metric-item {
  padding: 20px 16px;
  text-align: center;
}
.metric-value {
  font-size: 24px;
  font-weight: 700;
  color: #ee4d2d;
}
.metric-label {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
}
.info-icon {
  font-size: 14px;
  color: #94a3b8;
  cursor: help;
}
.info-icon:hover {
  color: #ee4d2d;
}

/* 快捷操作 */
.quick-actions {
  margin-bottom: 20px;
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
}
.action-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 20px 8px;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
  border: 2px solid transparent;
}
.action-card:hover {
  border-color: #ee4d2d;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(238, 77, 45, 0.15);
}
.action-icon {
  width: 60px;
  height: 60px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.action-label {
  font-size: 14px;
  font-weight: 600;
  color: #334155;
}

/* 近期訂單 */
.recent-orders {
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
}
.orders-table { width: 100%; }

.order-info-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}
.product-img {
  width: 44px;
  height: 44px;
  border-radius: 6px;
  background: #f8fafc;
  flex-shrink: 0;
  border: 1px solid #f1f5f9;
}
.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  background: #f1f5f9;
}
.order-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow: hidden;
}
.order-no {
  font-size: 13px;
  font-weight: 600;
  color: #1e293b;
}
.product-name {
  font-size: 12px;
  color: #64748b;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.order-amount {
  font-weight: 600;
  color: #1e293b;
}
.time-text {
  font-size: 12px;
  color: #94a3b8;
}

.clickable-table :deep(.el-table__row) {
  cursor: pointer;
}

</style>
