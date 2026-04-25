<template>
  <div class="order-detail-container" v-loading="loading">
    <div class="page-header">
      <el-button @click="router.back()" icon="ArrowLeft">返回列表</el-button>
      <h2 class="title">訂單詳情：{{ order?.orderNumber }}</h2>
      <div class="actions">
        <!-- 待出貨狀態才顯示出貨按鈕 -->
        <el-button
          v-if="order?.status === 1"
          type="primary"
          @click="handleShip"
        >
          確認出貨
        </el-button>
        <!-- 退貨中狀態顯示審核按鈕 -->
        <el-button
          v-if="order?.status === 5"
          type="primary"
          @click="router.push(`/seller/returns/${order?.id}`)"
        >
          前往審核退貨
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
        <!-- 訂單進度條 -->
        <el-card class="steps-card" shadow="never">
          <OrderSteps 
            :status="order.status"
            :created-at="order.createdAt"
            :payment-date="order.paymentDate"
            :completed-at="order.completedAt"
          />
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

          <!-- 價格結算 (使用抽取的組件) -->
          <div class="seller-summary-wrapper">
            <OrderSummary 
              v-if="order"
              :total-amount="order.totalAmount"
              :shipping-fee="order.shippingFee"
              :point-discount="order.pointDiscount"
              :discount-amount="order.discountAmount"
              :level-discount="order.levelDiscount"
              :final-amount="order.finalAmount"
              payment-method="線上支付"
            />
          </div>
        </el-card>
      </el-col>

      <!-- 右側：訂單狀態、買家與收件資訊 -->
      <el-col :span="8">
        <!-- 精簡版訂單狀態 -->
        <el-card class="info-card condensed-status-card" shadow="never">
          <div class="status-box">
            <div class="status-label">目前訂單狀態</div>
            <div class="status-value" :class="statusClass">{{ order.statusName }}</div>
          </div>
        </el-card>

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
import { useChatStore } from '@/stores/chat'
import OrderSteps from '@/components/order/OrderSteps.vue'
import OrderSummary from '@/components/order/OrderSummary.vue'

const route = useRoute()
const router = useRouter()
const chatStore = useChatStore()
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
  if (order.value?.userId) {
    chatStore.openChatWithUser(order.value.userId, order.value.buyerName || order.value.buyerAccount);
  } else {
    ElMessage.warning('無法取得買家資訊');
  }
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

.steps-card {
  margin-bottom: 20px;
  border-radius: 8px;
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

.seller-summary-wrapper {
  margin-top: 24px;
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
