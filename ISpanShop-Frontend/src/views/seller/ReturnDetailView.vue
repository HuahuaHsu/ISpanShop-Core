<template>
  <div class="return-detail-container" v-loading="loading">
    <div class="page-header">
      <el-button @click="router.back()" icon="ArrowLeft" plain>返回列表</el-button>
      <div class="header-content">
        <h2 class="title">退貨退款審核</h2>
        <span class="order-no">{{ detail?.orderNumber }}</span>
      </div>
      <el-tag :type="statusTagType(detail?.status)" effect="dark" class="status-tag">
        {{ detail?.statusName }}
      </el-tag>
    </div>

    <el-row :gutter="24" v-if="detail">
      <!-- ⬅️ 左側：證據與商品區 (70%) -->
      <el-col :span="16">
        <!-- 1. 申請原因 -->
        <el-card class="info-card mb-4" shadow="never">
          <template #header>
            <div class="card-header">
              <el-icon><InfoFilled /></el-icon>
              <span>退貨申請原因</span>
            </div>
          </template>
          <div class="reason-section">
            <div class="reason-type">
              <span class="label">原因類型：</span>
              <el-tag type="danger" effect="plain">{{ detail.reasonCategory }}</el-tag>
            </div>
            <div class="reason-time">
              <span class="label">申請時間：</span>
              <span>{{ formatDate(detail.requestCreatedAt) }}</span>
            </div>
            <div class="reason-desc">
              <div class="label">詳細說明：</div>
              <div class="desc-content">{{ detail.reasonDescription || '買家未提供詳細描述' }}</div>
            </div>
          </div>
        </el-card>

        <!-- 2. 商品清單 -->
        <el-card class="info-card mb-4" shadow="never">
          <template #header>
            <div class="card-header">
              <el-icon><Goods /></el-icon>
              <span>本次退貨商品明細</span>
            </div>
          </template>
          <el-table :data="detail.items" style="width: 100%">
            <el-table-column label="商品圖片" width="100">
              <template #default="{ row }">
                <el-image 
                  :src="row.coverImage" 
                  class="table-product-img clickable" 
                  fit="cover" 
                  @click="router.push(`/product/${row.productId}`)"
                />
              </template>
            </el-table-column>
            <el-table-column label="商品資訊" min-width="200">
              <template #default="{ row }">
                <div class="product-info clickable" @click="router.push(`/product/${row.productId}`)">
                  <div class="product-name">{{ row.productName }}</div>
                  <PromotionTags :tags="row.promotionTags" />
                  <div class="product-variant" v-if="row.variantName">規格：{{ row.variantName }}</div>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="price" label="單價" width="120" align="right">
              <template #default="{ row }">NT$ {{ row.price.toLocaleString() }}</template>
            </el-table-column>
            <el-table-column prop="quantity" label="退貨數量" width="100" align="center">
              <template #default="{ row }">
                <span class="qty-highlight">{{ row.quantity }}</span>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- 3. 憑證圖片 -->
        <el-card class="info-card" shadow="never">
          <template #header>
            <div class="card-header">
              <el-icon><Picture /></el-icon>
              <span>憑證圖片 ({{ detail.imageUrls.length }})</span>
            </div>
          </template>
          <div class="image-list">
            <el-image
              v-for="(url, index) in detail.imageUrls"
              :key="index"
              :src="url"
              class="evidence-img"
              :preview-src-list="detail.imageUrls"
              :initial-index="index"
              fit="cover"
            />
            <el-empty v-if="detail.imageUrls.length === 0" description="買家未提供圖片" :image-size="60" />
          </div>
        </el-card>
      </el-col>

      <!-- ➡️ 右側：結算與操作區 (30%) -->
      <el-col :span="8">
        <!-- 1. 買家資訊 -->
        <el-card class="side-card mb-4" shadow="never">
          <div class="buyer-info-box">
            <el-avatar :size="40" icon="UserFilled" />
            <div class="buyer-text">
              <div class="account">{{ detail.buyerAccount }}</div>
              <div class="order-date">訂單成立：{{ formatDate(detail.orderCreatedAt) }}</div>
            </div>
          </div>
        </el-card>

        <!-- 2. 退款明細 -->
        <div class="summary-wrapper mb-4">
          <div class="summary-title">退款結算明細</div>
          <RefundSummary
            :order="detail"
            :selected-item-ids="detail.items.map(i => i.id)"
            :return-quantities="returnQuantitiesMap"
          />
        </div>

        <!-- 3. 審核操作 -->
        <el-card v-if="detail.status === 5" class="side-card review-box" shadow="never">
          <template #header><span class="fw-bold">審核決策</span></template>
          <el-form :model="reviewForm" label-position="top">
            <el-form-item label="處理意向">
              <el-radio-group v-model="reviewForm.isApproved" class="w-full-radios">
                <el-radio-button :label="true">同意退款</el-radio-button>
                <el-radio-button :label="false">拒絕退款</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item :label="reviewForm.isApproved ? '給買家的備註' : '拒絕原因 (必填)'">
              <el-input
                v-model="reviewForm.remark"
                type="textarea"
                :rows="4"
                placeholder="請輸入處理說明..."
              />
            </el-form-item>
            <el-button
              type="primary"
              size="large"
              class="w-full submit-btn"
              :loading="submitting"
              @click="submitReview"
            >
              提交審核結果
            </el-button>
          </el-form>
        </el-card>

        <!-- 已處理結果 -->
        <el-card v-else class="side-card result-box" shadow="never">
          <template #header><span class="fw-bold">處理結果</span></template>
          <div class="result-status" :class="statusTagType(detail.status)">
            {{ detail.statusName }}
          </div>
          <div v-if="detail.resolvedAt" class="resolve-time">
            處理時間：{{ formatDate(detail.resolvedAt) }}
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
import { ArrowLeft, UserFilled, InfoFilled, Picture, Goods } from '@element-plus/icons-vue'
import { getSellerReturnDetailApi, reviewReturnApi } from '@/api/store'
import type { SellerReturnDetail } from '@/types/store'
import RefundSummary from '@/components/order/RefundSummary.vue'
import PromotionTags from '@/components/common/PromotionTags.vue'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const submitting = ref(false)
const detail = ref<SellerReturnDetail | null>(null)

const reviewForm = ref({
  isApproved: true,
  remark: ''
})

const orderId = computed(() => route.params.id as string)

const returnQuantitiesMap = computed(() => {
  if (!detail.value) return {}
  return detail.value.items.reduce((acc, item) => {
    acc[item.id] = item.quantity
    return acc
  }, {} as Record<number, number>)
})

const fetchDetail = async () => {
  if (!orderId.value) return
  loading.value = true
  try {
    const res = await getSellerReturnDetailApi(orderId.value)
    detail.value = res.data
    ElMessage.error(error.response?.data?.message || '取得詳情失敗')
    router.back()
  } finally {
    loading.value = false
  }
}

const submitReview = async () => {
  if (!reviewForm.value.isApproved && !reviewForm.value.remark.trim()) {
    ElMessage.warning('請填寫拒絕原因')
    return
  }

  try {
    await ElMessageBox.confirm(
      reviewForm.value.isApproved
        ? '確定要同意退款嗎？此操作不可撤回，系統將自動退款給買家。'
        : '確定要拒絕此退款申請嗎？',
      '提交審核',
      { type: reviewForm.value.isApproved ? 'warning' : 'info' }
    )

    submitting.value = true
    await reviewReturnApi(Number(orderId.value), reviewForm.value)
    ElMessage.success('處理成功')
    fetchDetail()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || '操作失敗')
    }
  } finally {
    submitting.value = false
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

const statusTagType = (status: any) => {
  if (status === 5) return 'warning'
  if (status === 6) return 'success'
  if (status === 4) return 'info'
  return 'info'
}

onMounted(fetchDetail)
</script>

<style scoped lang="scss">
.return-detail-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
  background-color: #f8fafc;
  min-height: 100vh;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;

  .header-content {
    flex: 1;
    display: flex;
    align-items: baseline;
    gap: 12px;

    .title { margin: 0; font-size: 22px; font-weight: 600; color: #1e293b; }
    .order-no { font-size: 14px; color: #64748b; font-family: monospace; }
  }

  // 專屬狀態標籤樣式：統一成品牌橘
  .status-tag {
    background-color: #ee4d2d;
    border-color: #ee4d2d;
    color: #fff;
    font-weight: 600;
  }
}

.info-card {
  border-radius: 8px;
  .card-header {
    display: flex;
    align-items: center;
    gap: 8px;
    font-weight: 600;
    color: #334155;
    .el-icon { color: #64748b; }
  }
}

.reason-section {
  .reason-type, .reason-time {
    margin-bottom: 12px;
    font-size: 14px;
    .label { color: #64748b; width: 80px; display: inline-block; }
  }
  .reason-desc {
    .label { font-size: 14px; color: #64748b; margin-bottom: 8px; }
    .desc-content {
      background: #f1f5f9;
      padding: 16px;
      border-radius: 6px;
      font-size: 14px;
      line-height: 1.6;
      color: #334155;
    }
  }
}

.image-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  .evidence-img {
    width: 120px;
    height: 120px;
    border-radius: 6px;
    cursor: pointer;
    border: 1px solid #e2e8f0;
    transition: transform 0.2s;
    &:hover { transform: scale(1.05); }
  }
}

.table-product-img { width: 50px; height: 50px; border-radius: 4px; }
.product-name { font-size: 14px; font-weight: 500; color: #1e293b; }
.clickable {
  cursor: pointer;
  transition: opacity 0.2s;
  &:hover {
    opacity: 0.8;
  }
}
.product-variant { font-size: 12px; color: #64748b; margin-top: 4px; }
.qty-highlight { font-weight: 700; color: #ee4d2d; font-size: 16px; }

.side-card {
  border-radius: 8px;
}

.buyer-info-box {
  display: flex;
  align-items: center;
  gap: 12px;
  .buyer-text {
    .account { font-weight: 600; font-size: 15px; color: #1e293b; }
    .order-date { font-size: 12px; color: #94a3b8; margin-top: 2px; }
  }
}

.summary-wrapper {
  .summary-title {
    font-size: 14px;
    font-weight: 600;
    color: #475569;
    margin-bottom: 12px;
    padding-left: 4px;
    border-left: 4px solid #ee4d2d;
  }
  :deep(.summary-section) {
    background: #fff !important;
    border: 1px solid #e2e8f0 !important;
    border-style: solid !important;
  }
}

.w-full-radios {
  display: flex;
  width: 100%;
  :deep(.el-radio-button) {
    flex: 1;
    .el-radio-button__inner { width: 100%; }
  }
}

.submit-btn {
  width: 100%;
  background-color: #ee4d2d;
  border-color: #ee4d2d;
  margin-top: 8px;
  &:hover { background-color: #dc4425; }
}

.result-status {
  font-size: 20px;
  font-weight: 700;
  margin-bottom: 8px;
  &.success { color: #10b981; }
  &.warning { color: #f59e0b; }
  &.info { color: #64748b; }
}
.resolve-time { font-size: 12px; color: #94a3b8; }

.mb-4 { margin-bottom: 16px; }
.w-full { width: 100%; }
.fw-bold { font-weight: bold; }
</style>
