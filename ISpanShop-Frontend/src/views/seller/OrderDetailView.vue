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
          <OrderItemsTable :items="order.items" />

          <!-- 價格結算 -->
          <div class="seller-summary-wrapper">
            <OrderSummary 
              v-if="order"
              :total-amount="order.totalAmount"
              :shipping-fee="order.shippingFee"
              :point-discount="order.pointDiscount"
              :discount-amount="order.discountAmount"
              :level-discount="order.levelDiscount"
              :promotion-discount="order.promotionDiscount"
              :final-amount="order.finalAmount"
              payment-method="線上支付"
            />
          </div>
        </el-card>

        <!-- 買家評價 -->
        <el-card v-if="order?.review" class="review-card" shadow="never" header="買家評價">
          <div class="review-content">
            <div class="review-header">
              <el-rate v-model="order.review.rating" disabled show-score text-color="#ff9900" />
              <span class="review-date">{{ formatDate(order.review.createdAt) }}</span>
            </div>
            <div class="buyer-comment">
              {{ order.review.comment }}
            </div>
            <div v-if="order.review.imageUrls?.length" class="review-images">
              <el-image
                v-for="(url, index) in order.review.imageUrls"
                :key="index"
                :src="url"
                class="review-img"
                :preview-src-list="order.review.imageUrls"
                :initial-index="index"
                fit="cover"
              />
            </div>
            
            <div class="store-reply-section">
              <div v-if="order.review.storeReply" class="reply-box">
                <div class="reply-label">您的回覆：</div>
                <div class="reply-content">{{ order.review.storeReply }}</div>
                <el-button link type="primary" size="small" @click="openReplyDialog" class="edit-reply-btn">修改回覆</el-button>
              </div>
              <div v-else class="no-reply">
                <el-button type="warning" plain @click="openReplyDialog">回應評價</el-button>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 右側：資訊欄 -->
      <el-col :span="8">
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

    <!-- 評價回覆對話框 -->
    <el-dialog
      v-model="replyDialogVisible"
      title="回應買家評價"
      width="500px"
      destroy-on-close
    >
      <el-form :model="replyForm" label-position="top">
        <el-form-item label="您的回覆：">
          <el-input
            v-model="replyForm.replyText"
            type="textarea"
            :rows="4"
            placeholder="請輸入您對買家評價的回應..."
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="replyDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="submitReply" :loading="submittingReply">
            提交回覆
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { getSellerOrderDetailApi, updateSellerOrderStatusApi, replyToReviewApi } from '@/api/store'
import type { SellerOrderDetail } from '@/types/store'
import { useChatStore } from '@/stores/chat'
import OrderSteps from '@/components/order/OrderSteps.vue'
import OrderSummary from '@/components/order/OrderSummary.vue'
import OrderItemsTable from '@/components/order/OrderItemsTable.vue'

const route = useRoute()
const router = useRouter()
const chatStore = useChatStore()
const loading = ref(false)
const order = ref<SellerOrderDetail | null>(null)

const orderId = computed(() => route.params.id as string)

// 評價回覆相關
const replyDialogVisible = ref(false)
const submittingReply = ref(false)
const replyForm = ref({
  replyText: ''
})

const openReplyDialog = () => {
  if (order.value?.review) {
    replyForm.value.replyText = order.value.review.storeReply || ''
    replyDialogVisible.value = true
  }
}

const submitReply = async () => {
  if (!order.value || !replyForm.value.replyText.trim()) {
    ElMessage.warning('請輸入回覆內容')
    return
  }

  submittingReply.value = true
  try {
    await replyToReviewApi({
      orderId: order.value.id,
      replyText: replyForm.value.replyText
    })
    ElMessage.success('回覆成功')
    replyDialogVisible.value = false
    fetchDetail()
  } catch (error) {
    ElMessage.error('回覆失敗')
  } finally {
    submittingReply.value = false
  }
}

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

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const statusClass = computed(() => {
  if (!order.value) return ''
  const statusMap: Record<number, string> = {
    0: 'status-pending',
    1: 'status-processing',
    2: 'status-shipped',
    3: 'status-completed',
    4: 'status-cancelled',
    5: 'status-refunding',
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
    await updateSellerOrderStatusApi(order.value.id, 3)
    ElMessage.success('訂單已標記為出貨中')
    fetchDetail()
  } catch (error) {
    if (error !== 'cancel') ElMessage.error('操作失敗')
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

onMounted(fetchDetail)
</script>

<style scoped lang="scss">
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
  .title {
    margin: 0;
    flex: 1;
    font-size: 20px;
    color: #1e293b;
  }
}

.steps-card {
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
  &.status-pending { color: #ee4d2d; }
  &.status-processing { color: #26aa99; }
  &.status-shipped { color: #26aa99; }
  &.status-completed { color: #ee4d2d; }
  &.status-cancelled { color: #929292; }
  &.status-refunding { color: #ee4d2d; }
}

.items-card {
  margin-bottom: 20px;
  border-radius: 8px;
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
  .label { font-size: 12px; color: #94a3b8; margin-bottom: 4px; }
  .value {
    font-size: 14px;
    color: #1e293b;
    font-weight: 500;
    &.note {
      background: #f8fafc;
      padding: 8px;
      border-radius: 4px;
      color: #64748b;
      font-style: italic;
    }
  }
}

.review-card {
  margin-top: 20px;
  border-radius: 8px;
}
.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.review-date { font-size: 13px; color: #94a3b8; }
.buyer-comment {
  font-size: 15px;
  color: #1e293b;
  line-height: 1.6;
  margin-bottom: 16px;
  white-space: pre-wrap;
}
.review-images {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 20px;
}
.review-img { width: 80px; height: 80px; border-radius: 4px; cursor: zoom-in; }

.store-reply-section {
  border-top: 1px solid #f1f5f9;
  padding-top: 16px;
}
.reply-box {
  background-color: #fffbf8;
  padding: 12px 16px;
  border-radius: 4px;
  border-left: 3px solid #ee4d2d;
  position: relative;
}
.reply-label { font-size: 13px; font-weight: 600; color: #ee4d2d; margin-bottom: 4px; }
.reply-content { font-size: 14px; color: #1e293b; line-height: 1.5; }
.edit-reply-btn { margin-top: 8px; padding: 0; }
.no-reply { text-align: right; }
</style>
