<template>
  <div class="return-detail-container" v-loading="loading">
    <div class="page-header">
      <el-button @click="router.back()" icon="ArrowLeft">返回列表</el-button>
      <h2 class="title">退貨退款審核：{{ detail?.orderNumber }}</h2>
    </div>

    <el-row :gutter="20" v-if="detail">
      <!-- 左側：申請詳情與證據圖片 -->
      <el-col :span="14">
        <el-card class="detail-card" shadow="never" header="申請詳情">
          <div class="info-item">
            <span class="label">退款原因：</span>
            <span class="value">{{ detail.reasonCategory }}</span>
          </div>
          <div class="info-item">
            <span class="label">原因描述：</span>
            <div class="value description">{{ detail.reasonDescription || '無詳細描述' }}</div>
          </div>
          <div class="info-item">
            <span class="label">申請金額：</span>
            <span class="value price">NT$ {{ detail.refundAmount.toLocaleString() }}</span>
          </div>
          <div class="info-item">
            <span class="label">申請時間：</span>
            <span class="value">{{ formatDate(detail.requestCreatedAt) }}</span>
          </div>
          
          <el-divider />
          
          <div class="image-section">
            <h4 class="section-title">證據圖片 ({{ detail.imageUrls.length }})</h4>
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
          </div>
        </el-card>

        <el-card class="items-card" shadow="never" header="原訂單商品">
          <el-table :data="detail.items" style="width: 100%">
            <el-table-column label="商品" min-width="200">
              <template #default="{ row }">
                <div class="product-info">
                  <div class="name">{{ row.productName }}</div>
                  <div class="variant" v-if="row.variantName">規格：{{ row.variantName }}</div>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="price" label="單價" width="100" align="right">
              <template #default="{ row }">NT$ {{ row.price.toLocaleString() }}</template>
            </el-table-column>
            <el-table-column prop="quantity" label="數量" width="80" align="center" />
          </el-table>
        </el-card>
      </el-col>

      <!-- 右側：買家資訊與審核操作 -->
      <el-col :span="10">
        <el-card class="buyer-card" shadow="never" header="買家資訊">
          <div class="buyer-box">
            <el-avatar :size="48" icon="UserFilled" />
            <div class="buyer-info">
              <div class="account">{{ detail.buyerAccount }}</div>
              <div class="order-time">訂單成立：{{ formatDate(detail.orderCreatedAt) }}</div>
            </div>
          </div>
        </el-card>

        <!-- 審核操作區 -->
        <el-card v-if="detail.status === 5" class="review-card" shadow="never" header="審核操作">
          <el-form :model="reviewForm" label-position="top">
            <el-form-item label="處理意見">
              <el-radio-group v-model="reviewForm.isApproved">
                <el-radio :label="true" border>同意退款</el-radio>
                <el-radio :label="false" border>拒絕退款</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item :label="reviewForm.isApproved ? '備註 (選填)' : '拒絕原因 (必填)'">
              <el-input 
                v-model="reviewForm.remark" 
                type="textarea" 
                :rows="4" 
                placeholder="請輸入給買家的回覆..."
              />
            </el-form-item>
            <div class="form-actions">
              <el-button 
                type="primary" 
                size="large" 
                long 
                class="submit-btn"
                :loading="submitting"
                @click="submitReview"
              >
                提交審核結果
              </el-button>
            </div>
          </el-form>
        </el-card>

        <!-- 已處理顯示 -->
        <el-card v-else class="result-card" shadow="never" header="處理結果">
          <div class="status-result" :class="statusTagType(detail.status)">
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
import { ArrowLeft, UserFilled } from '@element-plus/icons-vue'
import { getSellerReturnDetailApi, reviewReturnApi } from '@/api/store'
import type { SellerReturnDetail } from '@/types/store'

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

const fetchDetail = async () => {
  if (!orderId.value) return
  loading.value = true
  try {
    const res = await getSellerReturnDetailApi(orderId.value)
    detail.value = res.data
  } catch (error: any) {
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
        ? '確定要同意退款嗎？此操作不可撤回。' 
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

const statusTagType = (status: number) => {
  if (status === 5) return 'warning'
  if (status === 6) return 'success'
  if (status === 3) return 'info'
  return 'info'
}

onMounted(() => {
  fetchDetail()
})
</script>

<style scoped>
.return-detail-container {
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
  font-size: 20px;
}

.detail-card, .items-card, .buyer-card, .review-card, .result-card {
  margin-bottom: 20px;
  border-radius: 8px;
}

.info-item {
  margin-bottom: 12px;
  font-size: 14px;
  display: flex;
}
.info-item .label {
  color: #64748b;
  width: 80px;
  flex-shrink: 0;
}
.info-item .value {
  color: #1e293b;
  font-weight: 500;
}
.info-item .value.description {
  background: #f8fafc;
  padding: 12px;
  border-radius: 4px;
  flex: 1;
}
.info-item .value.price {
  color: #ee4d2d;
  font-size: 18px;
}

.image-section .section-title {
  font-size: 14px;
  color: #64748b;
  margin-bottom: 12px;
}
.image-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
.evidence-img {
  width: 100px;
  height: 100px;
  border-radius: 4px;
  cursor: pointer;
  border: 1px solid #e2e8f0;
}

.buyer-box {
  display: flex;
  align-items: center;
  gap: 16px;
}
.buyer-info .account {
  font-weight: 600;
  font-size: 16px;
}
.buyer-info .order-time {
  font-size: 12px;
  color: #94a3b8;
  margin-top: 4px;
}

.submit-btn {
  width: 100%;
  background-color: #ee4d2d;
  border-color: #ee4d2d;
}
.submit-btn:hover {
  background-color: #f05d40;
  border-color: #f05d40;
}

.status-result {
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 12px;
}
.status-result.success { color: #10b981; }
.status-result.info { color: #64748b; }

.resolve-time {
  font-size: 13px;
  color: #94a3b8;
}
</style>
