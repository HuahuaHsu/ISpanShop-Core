<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { fetchProductReviews, generateMockReviews } from '@/api/review'
import { ElMessage } from 'element-plus'
import { MagicStick } from '@element-plus/icons-vue'

const props = defineProps<{
  productId: number
}>()

const emit = defineEmits(['refresh'])

const reviews = ref<any[]>([])
const loading = ref(false)
const generating = ref(false)
const activeFilter = ref('all') // 'all', '5', '4', '3', '2', '1', 'withComment', 'withMedia'
const currentPage = ref(1)
const pageSize = ref(5)

const averageRating = computed(() => {
  if (reviews.value.length === 0) return 0
  const sum = reviews.value.reduce((acc, cur) => acc + cur.rating, 0)
  return (sum / reviews.value.length).toFixed(1)
})

const counts = computed(() => {
  return {
    all: reviews.value.length,
    5: reviews.value.filter(r => r.rating === 5).length,
    4: reviews.value.filter(r => r.rating === 4).length,
    3: reviews.value.filter(r => r.rating === 3).length,
    2: reviews.value.filter(r => r.rating === 2).length,
    1: reviews.value.filter(r => r.rating === 1).length,
    withComment: reviews.value.filter(r => r.comment && r.comment.trim()).length,
    withMedia: reviews.value.filter(r => r.imageUrls && r.imageUrls.length > 0).length
  }
})

const filteredReviews = computed(() => {
  let result = reviews.value
  if (activeFilter.value !== 'all') {
    if (['5', '4', '3', '2', '1'].includes(activeFilter.value)) {
      result = reviews.value.filter(r => r.rating === parseInt(activeFilter.value))
    } else if (activeFilter.value === 'withComment') {
      result = reviews.value.filter(r => r.comment && r.comment.trim())
    } else if (activeFilter.value === 'withMedia') {
      result = reviews.value.filter(r => r.imageUrls && r.imageUrls.length > 0)
    }
  }
  return result
})

const paginatedReviews = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredReviews.value.slice(start, end)
})

const handleFilterChange = (filter: string) => {
  activeFilter.value = filter
  currentPage.value = 1
}

const formatDate = (date: string) => {
  return new Date(date).toLocaleString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

const loadReviews = async () => {
  loading.value = true
  try {
    reviews.value = await fetchProductReviews(props.productId)
  } catch (error) {
    console.error('載入評價失敗', error)
  } finally {
    loading.value = false
  }
}

const handleGenerateMock = async () => {
  generating.value = true
  try {
    await generateMockReviews(props.productId, 5)
    ElMessage.success('已成功生成 5 筆測試評論')
    await loadReviews()
    emit('refresh')
  } catch (error) {
    ElMessage.error('生成失敗')
    console.error(error)
  } finally {
    generating.value = false
  }
}

onMounted(loadReviews)
</script>

<template>
  <div class="product-reviews-section">
    <el-card class="review-card" shadow="never">
      <template #header>
        <div class="reviews-header">
          <h3 class="reviews-title">商品評價</h3>
          <el-button 
            type="warning" 
            plain 
            size="small" 
            :loading="generating"
            @click="handleGenerateMock"
          >
            <el-icon class="mr-1"><MagicStick /></el-icon>
            一鍵生成測試評價 (展示用)
          </el-button>
        </div>
      </template>
      
      <!-- 總覽 (Shopee 風格) -->
      <div v-if="reviews.length > 0" class="rating-summary">
        <div class="summary-score">
          <div class="score-value">
            <span class="big-score">{{ averageRating }}</span>
            <span class="total-score">/5</span>
          </div>
          <div class="score-stars">
            <el-rate v-model="averageRating" disabled />
          </div>
        </div>
        
        <div class="filter-options">
          <el-button 
            :class="{ 'filter-active': activeFilter === 'all' }"
            size="default" 
            plain 
            @click="handleFilterChange('all')"
          >全部 ({{ counts.all }})</el-button>
          
          <el-button 
            v-for="i in [5,4,3,2,1]" 
            :key="i" 
            :class="{ 'filter-active': activeFilter === String(i) }"
            size="default" 
            plain 
            @click="handleFilterChange(String(i))"
          >{{ i }} 星 ({{ counts[i] }})</el-button>
          
          <el-button 
            :class="{ 'filter-active': activeFilter === 'withComment' }"
            size="default" 
            plain 
            @click="handleFilterChange('withComment')"
          >附上評論 ({{ counts.withComment }})</el-button>
          
          <el-button 
            :class="{ 'filter-active': activeFilter === 'withMedia' }"
          size="default" 
          plain 
          @click="handleFilterChange('withMedia')"
        >附上照片/影片 ({{ counts.withMedia }})</el-button>
      </div>
    </div>

    <!-- 列表 -->
    <div v-loading="loading" class="review-list">
      <el-empty v-if="filteredReviews.length === 0 && activeFilter !== 'all'" description="找不到符合條件的評價" />
      
      <div v-for="review in paginatedReviews" :key="review.id" class="review-item">
        <div class="review-user-avatar">
          <el-avatar :size="40" :src="review.userAvatar" icon="UserFilled" />
        </div>
        <div class="review-main-content">
          <div class="review-user-name">{{ review.userName }}</div>
          <div class="review-rating">
            <el-rate v-model="review.rating" disabled size="small" />
          </div>
          
          <div class="review-meta">
            <span>{{ formatDate(review.createdAt) }}</span>
            <span v-if="review.variantName" class="meta-divider">|</span>
            <span v-if="review.variantName">規格：{{ review.variantName }}</span>
          </div>
          
          <div class="review-comment">
            {{ review.comment }}
          </div>

          <!-- 評價圖片 -->
          <div v-if="review.imageUrls && review.imageUrls.length > 0" class="review-images">
            <div v-for="(url, index) in review.imageUrls" :key="index" class="review-img-wrap">
              <el-image 
                :src="url" 
                :preview-src-list="review.imageUrls"
                :initial-index="index"
                fit="cover"
                class="review-img"
              />
            </div>
          </div>

          <!-- 賣家回覆 -->
          <div v-if="review.storeReply" class="store-reply">
            <div class="reply-content">
              <span class="reply-label">賣家回覆：</span>
              {{ review.storeReply }}
            </div>
          </div>
        </div>
      </div>

      <!-- 分頁器 -->
      <div v-if="filteredReviews.length > pageSize" class="pagination-wrap">
        <el-pagination
          v-model:current-page="currentPage"
          :page-size="pageSize"
          :total="filteredReviews.length"
          layout="prev, pager, next"
          background
          hide-on-single-page
        />
      </div>
    </div>
    </el-card>
  </div>
</template>

<style scoped>
.product-reviews-section {
  margin-top: 16px;
}

.review-card {
  border-radius: 4px;
}

.reviews-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.reviews-title {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.rating-summary {
  display: flex;
  align-items: center;
  padding: 32px;
  background-color: #fffbf8;
  border-radius: 2px;
  margin-bottom: 24px;
  border: 1px solid #f9ede5;
}

.summary-score {
  text-align: center;
  padding-right: 48px;
}

.score-value {
  color: #ee4d2d;
  font-weight: 500;
}

.big-score {
  font-size: 30px;
}

.total-score {
  font-size: 18px;
}

.score-stars {
  margin-top: 8px;
}

.filter-options {
  flex: 1;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.filter-active {
  border-color: #ee4d2d !important;
  color: #ee4d2d !important;
  background-color: #fff !important;
}

:deep(.el-rate) {
  --el-rate-fill-color: #ee4d2d;
}

.review-item {
  display: flex;
  gap: 16px;
  padding: 24px 0;
  border-bottom: 1px solid #f1f5f9;
}

.review-item:last-child {
  border-bottom: none;
}

.review-item:hover {
  background-color: #fafafa;
  transition: 0.2s;
}

.review-user-avatar {
  flex-shrink: 0;
}

.review-main-content {
  flex: 1;
}

.review-user-name {
  font-size: 12px;
  color: #1e293b;
  margin-bottom: 4px;
}

.review-meta {
  font-size: 12px;
  color: #94a3b8;
  margin: 8px 0;
  display: flex;
  align-items: center;
}

.meta-divider {
  margin: 0 8px;
  color: #e2e8f0;
}

.review-comment {
  font-size: 14px;
  color: #334155;
  line-height: 1.6;
  margin-bottom: 12px;
  white-space: pre-wrap;
}

.review-images {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}

.review-img-wrap {
  width: 80px;
  height: 80px;
  overflow: hidden;
  border-radius: 2px;
  border: 1px solid #f1f5f9;
  flex-shrink: 0;
  cursor: zoom-in;
}

.review-img {
  width: 100%;
  height: 100%;
}

.store-reply {
  background-color: #f8fafc;
  padding: 12px 16px;
  border-radius: 4px;
  margin-top: 12px;
}

.reply-content {
  font-size: 13px;
  color: #64748b;
  line-height: 1.6;
}

.reply-label {
  font-weight: 600;
  color: #475569;
}

.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: 24px;
}

.mr-1 {
  margin-right: 4px;
}
</style>
