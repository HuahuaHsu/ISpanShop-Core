<template>
  <div class="seller-returns-page">
    <div class="page-header">
      <h2 class="page-title">退貨/退款管理</h2>
    </div>

    <el-card shadow="never" class="main-card">
      <el-tabs v-model="activeTab" class="custom-tabs" @tab-change="handleTabChange">
        <el-tab-pane label="待處理" name="pending" />
        <el-tab-pane label="已處理" name="processed" />
        <el-tab-pane label="全部" name="all" />
      </el-tabs>

      <div v-loading="loading" class="list-container">
        <el-table :data="returns" stripe style="width: 100%">
          <el-table-column prop="orderNumber" label="訂單編號" min-width="180" />
          <el-table-column prop="buyerName" label="買家" width="120" />
          <el-table-column label="退款金額" width="120">
            <template #default="{ row }">
              <span class="price">NT$ {{ row.refundAmount.toLocaleString() }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="reasonCategory" label="退貨原因" min-width="150" show-overflow-tooltip />
          <el-table-column label="申請時間" width="160">
            <template #default="{ row }">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="狀態" width="100">
            <template #default="{ row }">
              <el-tag :type="statusTagType(row.status)" size="small">
                {{ row.statusName }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="120" fixed="right">
            <template #default="{ row }">
              <el-button 
                :type="row.status === 5 ? 'primary' : 'default'" 
                size="small"
                @click="goToDetail(row.id)"
                class="action-btn"
              >
                {{ row.status === 5 ? '審核' : '詳情' }}
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <el-empty v-if="returns.length === 0" description="暫無退貨申請" />

        <div class="pagination-wrapper" v-if="totalCount > 0">
          <el-pagination
            v-model:current-page="currentPage"
            v-model:page-size="pageSize"
            :total="totalCount"
            layout="total, prev, pager, next"
            background
            @current-change="fetchReturns"
          />
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getSellerReturnsApi } from '@/api/store'
import type { SellerReturnItem } from '@/types/store'
import { ElMessage } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const returns = ref<SellerReturnItem[]>([])
const totalCount = ref(0)
const activeTab = ref('pending')
const currentPage = ref(1)
const pageSize = ref(10)

const fetchReturns = async () => {
  loading.value = true
  try {
    let isProcessed: boolean | undefined = undefined
    if (activeTab.value === 'pending') isProcessed = false
    else if (activeTab.value === 'processed') isProcessed = true

    const res = await getSellerReturnsApi({
      isProcessed,
      page: currentPage.value,
      pageSize: pageSize.value
    })
    returns.value = res.data.items
    totalCount.value = res.data.totalCount
  } catch (error) {
    console.error('取得退貨列表失敗', error)
    ElMessage.error('取得資料失敗')
  } finally {
    loading.value = false
  }
}

const handleTabChange = () => {
  currentPage.value = 1
  fetchReturns()
}

const goToDetail = (id: number) => {
  router.push(`/seller/returns/${id}`)
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-TW', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const statusTagType = (status: number) => {
  if (status === 5) return 'warning' // 退貨中
  if (status === 6) return 'success' // 已退款
  if (status === 3) return 'info'    // 已完成 (拒絕後恢復)
  return 'info'
}

onMounted(() => {
  fetchReturns()
})
</script>

<style scoped>
.seller-returns-page {
  max-width: 1200px;
  margin: 0 auto;
}
.page-header {
  margin-bottom: 20px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
}
.main-card {
  border-radius: 8px;
}
.custom-tabs {
  margin-bottom: 10px;
}
:deep(.el-tabs__item.is-active) {
  color: #ee4d2d;
}
:deep(.el-tabs__active-bar) {
  background-color: #ee4d2d;
}
.price {
  color: #ee4d2d;
  font-weight: 600;
}
.action-btn {
  border-radius: 4px;
  min-width: 70px;
}
.el-button--primary {
  background-color: #ee4d2d;
  border-color: #ee4d2d;
}
.el-button--primary:hover {
  background-color: #f05d40;
  border-color: #f05d40;
}
.pagination-wrapper {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
