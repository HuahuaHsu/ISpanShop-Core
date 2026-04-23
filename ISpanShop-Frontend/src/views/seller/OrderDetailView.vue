<template>
  <div class="order-detail-container" v-loading="loading">
    <div class="page-header">
      <el-button @click="router.back()" icon="ArrowLeft">返回列表</el-button>
      <h2 class="title">訂單詳情：{{ order?.orderNumber }}</h2>
      <div class="actions">
        <!-- 待出貨狀態才顯示出貨按鈕 -->
        <el-button 
          v-if="order?.status === 2" 
          type="primary" 
          size="large"
          @click="handleShip"
        >
          確認出貨
        </el-button>
        <el-button 
          type="info" 
          plain 
          @click="contactBuyer"
        >
          聯繫買家
        </el-button>
      </div>
    </div>

    <el-row :gutter="20" v-if="order">
      <!-- 左側：訂單資訊與商品明細 -->
      <el-col :span="16">
        <!-- 訂單狀態卡片 -->
        <el-card class="status-card" shadow="never">
          <div class="status-box">
            <div class="status-label">目前訂單狀態</div>
            <div class="status-value" :class="statusClass">{{ order.statusName }}</div>
          </div>
          <el-divider />
          <div class="timeline">
            <div class="time-item">下單時間：{{ order.createdAt }}</div>
            <div v-if="order.paymentDate" class="time-item">付款時間：{{ order.paymentDate }}</div>
            <div v-if="order.completedAt" class="time-item">完成時間：{{ order.completedAt }}</div>
          </div>
        </el-card>

        <!-- 商品明細 -->
        <el-card class="items-card" shadow="never" header="商品明細">
          <el-table :data="order.items" style="width: 100%">
            <el-table-column label="商品資訊" min-width="300">
              <template #default="{ row }">
                <div class="product-info">
                  <el-image 
                    :src="row.coverImage" 
                    class="product-img" 
                    fit="cover"
                  >
                    <template #error><div class="image-slot"><el-icon><Picture /></el-icon></div></template>
                  </el-image>
                  <div class="product-text">
                    <div class="name">{{ row.productName }}</div>
                    <div class="variant" v-if="row.variantName">規格：{{ row.variantName }}</div>
                    <div class="sku">SKU: {{ row.skuCode }}</div>
                  </div>
                </div>
              </template>
            </el-table-column>
            <el-table-column label="單價" width="120" align="center">
              <template #default="{ row }">
                NT$ {{ row.price.toLocaleString() }}
              </template>
            </el-table-column>
            <el-table-column prop="quantity" label="數量" width="100" align="center" />
            <el-table-column label="小計" width="120" align="right">
              <template #default="{ row }">
                <span class="subtotal">NT$ {{ row.subtotal.toLocaleString() }}</span>
              </template>
            </el-table-column>
          </el-table>

          <div class="order-summary">
            <div class="summary-line">
              <span>商品總額</span>
              <span>NT$ {{ order.totalAmount.toLocaleString() }}</span>
            </div>
            <div class="summary-line">
              <span>運費</span>
              <span>NT$ {{ order.shippingFee.toLocaleString() }}</span>
            </div>
            <div class="summary-line discount" v-if="order.discountAmount > 0">
              <span>活動折扣</span>
              <span>- NT$ {{ order.discountAmount.toLocaleString() }}</span>
            </div>
            <div class="summary-line discount" v-if="order.pointDiscount > 0">
              <span>點數折抵</span>
              <span>- NT$ {{ order.pointDiscount.toLocaleString() }}</span>
            </div>
            <el-divider />
            <div class="summary-line total">
              <span>買家實付</span>
              <span class="final-amount">NT$ {{ order.finalAmount.toLocaleString() }}</span>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 右側：買家與收件資訊 -->
      <el-col :span="8">
        <el-card class="info-card" shadow="never" header="買家資訊">
          <div class="info-group">
            <div class="label">買家帳號</div>
            <div class="value">{{ order.buyerAccount }}</div>
          </div>
          <div class="info-group">
            <div class="label">聯絡姓名</div>
            <div class="value">{{ order.buyerName }}</div>
          </div>
          <div class="info-group">
            <div class="label">電話</div>
            <div class="value">{{ order.buyerPhone }}</div>
          </div>
          <div class="info-group">
            <div class="label">Email</div>
            <div class="value">{{ order.buyerEmail }}</div>
          </div>
        </el-card>

        <el-card class="info-card" shadow="never" header="收件資訊">
          <div class="info-group">
            <div class="label">收件人</div>
            <div class="value">{{ order.recipientName }}</div>
          </div>
          <div class="info-group">
            <div class="label">電話</div>
            <div class="value">{{ order.recipientPhone }}</div>
          </div>
          <div class="info-group">
            <div class="label">地址</div>
            <div class="value">{{ order.recipientAddress }}</div>
          </div>
          <div class="info-group" v-if="order.note">
            <div class="label">買家留言</div>
            <div class="value note">{{ order.note }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Picture, ArrowLeft } from '@element-plus/icons-vue'
import { getSellerOrderDetailApi, updateSellerOrderStatusApi } from '@/api/store'
import type { SellerOrderDetail } from '@/types/store'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const order = ref<SellerOrderDetail | null>(null)

const orderId = computed(() => route.params.id as string)

const fetchDetail = async () => {
  if (!orderId.value) return
  loading.value = true
  try {
    const res = await getSellerOrderDetailApi(orderId.value)
    order.value = res.data
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '取得詳情失敗')
    router.back()
  } finally {
    loading.value = false
  }
}

const statusClass = computed(() => {
  if (!order.value) return ''
  const statusMap: Record<number, string> = {
    1: 'pending-payment', // 待付款
    2: 'processing',      // 待出貨
    3: 'shipping',        // 運送中
    4: 'completed',       // 已完成
    5: 'cancelled',       // 已取消
    6: 'refunding',       // 退貨中
  }
  return statusMap[order.value.status] || ''
})

const handleShip = async () => {
  if (!order.value) return
  
  try {
    await ElMessageBox.confirm('確定要將此訂單標記為「出貨中」嗎？', '確認出貨', {
      confirmButtonText: '確定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    
    loading.value = true
    // 假設 3 代表運送中 (需對應後端 OrderStatus Enum)
    await updateSellerOrderStatusApi(order.value.id, 3)
    ElMessage.success('訂單已標記為出貨中')
    fetchDetail()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('出貨操作失敗', error)
      ElMessage.error('操作失敗')
    }
  } finally {
    loading.value = false
  }
}

const contactBuyer = () => {
  ElMessage.info('即時聊天功能開發中')
}

onMounted(() => {
  fetchDetail()
})
</script>

<style scoped>
.order-detail-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
}
.page-header .title {
  margin: 0;
  flex: 1;
  font-size: 20px;
  color: #1e293b;
}

.status-card {
  margin-bottom: 20px;
  border-radius: 8px;
}
.status-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.status-label {
  font-size: 14px;
  color: #64748b;
}
.status-value {
  font-size: 24px;
  font-weight: 700;
}
.status-value.processing { color: #f59e0b; }
.status-value.completed  { color: #10b981; }
.status-value.cancelled  { color: #ef4444; }
.status-value.shipping   { color: #3b82f6; }

.timeline {
  font-size: 13px;
  color: #94a3b8;
  display: flex;
  gap: 24px;
}

.items-card {
  margin-bottom: 20px;
  border-radius: 8px;
}

.product-info {
  display: flex;
  gap: 12px;
}
.product-img {
  width: 60px;
  height: 60px;
  border-radius: 4px;
  flex-shrink: 0;
}
.product-text .name {
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 4px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.product-text .variant {
  font-size: 12px;
  color: #64748b;
}
.product-text .sku {
  font-size: 11px;
  color: #94a3b8;
  font-family: monospace;
}

.subtotal {
  font-weight: 600;
  color: #1e293b;
}

.order-summary {
  margin-top: 24px;
  width: 300px;
  margin-left: auto;
}
.summary-line {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 14px;
  color: #64748b;
}
.summary-line.discount {
  color: #ef4444;
}
.summary-line.total {
  font-weight: 700;
  color: #1e293b;
  font-size: 16px;
}
.final-amount {
  color: #ee4d2d;
  font-size: 20px;
}

.info-card {
  margin-bottom: 20px;
  border-radius: 8px;
}
.info-group {
  margin-bottom: 16px;
}
.info-group:last-child {
  margin-bottom: 0;
}
.info-group .label {
  font-size: 12px;
  color: #94a3b8;
  margin-bottom: 4px;
}
.info-group .value {
  font-size: 14px;
  color: #1e293b;
  font-weight: 500;
}
.info-group .value.note {
  background: #f8fafc;
  padding: 8px;
  border-radius: 4px;
  color: #64748b;
  font-style: italic;
}
</style>
