<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { fetchProductReviews } from '@/api/review'

const props = defineProps<{
  productId: number
}>()

const reviews = ref<any[]>([])
const loading = ref(false)
const activeFilter = ref('all') // 'all', '5', '4', '3', '2', '1', 'withComment', 'withMedia'

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
  if (activeFilter.value === 'all') return reviews.value
  if (['5', '4', '3', '2', '1'].includes(activeFilter.value)) {
    return reviews.value.filter(r => r.rating === parseInt(activeFilter.value))
  }
  if (activeFilter.value === 'withComment') {
    return reviews.value.filter(r => r.comment && r.comment.trim())
  }
  if (activeFilter.value === 'withMedia') {
    return reviews.value.filter(r => r.imageUrls && r.imageUrls.length > 0)
  }
  return reviews.value
})

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

onMounted(loadReviews)
</script>

<template>
  <div class="product-reviews mt-8 bg-white p-6 rounded shadow-sm border border-gray-100">
    <h3 class="text-xl font-bold mb-6 pb-4">商品評價</h3>
    
    <!-- 總覽 (Shopee 風格) -->
    <div v-if="reviews.length > 0" class="rating-summary flex items-center p-8 bg-[#fffbf8] rounded-sm mb-8 border border-[#f9ede5]">
      <div class="text-center px-12">
        <div class="text-3xl font-medium text-[#ee4d2d]">
          <span class="text-3xl">{{ averageRating }}</span> <span class="text-xl">滿分 5</span>
        </div>
        <div class="mt-2 text-xl">
          <el-rate v-model="averageRating" disabled text-color="#ee4d2d" />
        </div>
      </div>
      
      <div class="flex-1 ml-4 flex flex-wrap gap-3">
        <el-button 
          :class="{ 'filter-active': activeFilter === 'all' }"
          size="default" 
          plain 
          @click="activeFilter = 'all'"
        >全部 ({{ counts.all }})</el-button>
        
        <el-button 
          v-for="i in [5,4,3,2,1]" 
          :key="i" 
          :class="{ 'filter-active': activeFilter === String(i) }"
          size="default" 
          plain 
          @click="activeFilter = String(i)"
        >{{ i }} 星 ({{ counts[i] }})</el-button>
        
        <el-button 
          :class="{ 'filter-active': activeFilter === 'withComment' }"
          size="default" 
          plain 
          @click="activeFilter = 'withComment'"
        >附上評論 ({{ counts.withComment }})</el-button>
        
        <el-button 
          :class="{ 'filter-active': activeFilter === 'withMedia' }"
          size="default" 
          plain 
          @click="activeFilter = 'withMedia'"
        >附上照片/影片 ({{ counts.withMedia }})</el-button>
      </div>
    </div>

    <!-- 列表 -->
    <div v-loading="loading" class="review-list">
      <el-empty v-if="filteredReviews.length === 0" :description="activeFilter === 'all' ? '尚無商品評價' : '找不到符合條件的評價'" />
      
      <div v-for="review in filteredReviews" :key="review.id" class="review-item py-8 border-b border-gray-200 last:border-0">
        <div class="flex items-start gap-4">
          <el-avatar :size="40" :src="review.userAvatar" icon="UserFilled" />
          <div class="flex-1">
            <div class="flex justify-between items-center mb-1">
              <span class="text-xs text-gray-800">{{ review.userName }}</span>
            </div>
            <el-rate v-model="review.rating" disabled size="small" />
            
            <div style="font-size: 12px; color: #999; margin: 8px 0; display: flex; align-items: center;">
              <span>{{ formatDate(review.createdAt) }}</span>
              <span v-if="review.variantName" style="margin: 0 8px; color: #eee;">|</span>
              <span v-if="review.variantName">規格：{{ review.variantName }}</span>
            </div>
            
            <div style="font-size: 17px; color: #333; line-height: 1.5; margin-bottom: 15px; white-space: pre-wrap;">
              {{ review.comment }}
            </div>

            <!-- 評價圖片 -->
            <div v-if="review.imageUrls && review.imageUrls.length > 0" class="flex flex-wrap gap-2 mb-4">
              <div v-for="(url, index) in review.imageUrls" :key="index" 
                   style="width: 80px; height: 80px; overflow: hidden; border-radius: 2px; border: 1px solid #eee; flex-shrink: 0; cursor: zoom-in;">
                <el-image 
                  :src="url" 
                  :preview-src-list="review.imageUrls"
                  :initial-index="index"
                  fit="cover"
                  style="width: 100%; height: 100%; display: block;"
                />
              </div>
            </div>

            <!-- 賣家回覆 -->
            <div v-if="review.storeReply" class="bg-[#f5f5f5] p-4 rounded-sm mt-4 relative">
              <div class="text-sm text-gray-600">
                <span class="font-medium text-gray-700">賣家回覆：</span>
                {{ review.storeReply }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.rating-summary {
  min-height: 100px;
}

.filter-active {
  border-color: #ee4d2d !important;
  color: #ee4d2d !important;
  background-color: #fff !important;
}

:deep(.el-rate) {
  --el-rate-fill-color: #ee4d2d;
}

.review-item:hover { background-color: #fafafa; transition: 0.2s; }

.review-img-wrap {
  width: 80px !important;
  height: 80px !important;
  overflow: hidden;
  border-radius: 2px;
  border: 1px solid #eee;
  flex-shrink: 0;
  cursor: zoom-in;
}

.review-img {
  width: 100% !important;
  height: 100% !important;
}

.review-meta {
  font-size: 11px;
  color: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
}

.meta-divider {
  margin: 0 8px;
  color: #eee;
}

.review-content {
  font-size: 15px;
  color: rgba(0, 0, 0, 0.87);
  line-height: 1.5;
  margin-top: 10px;
  word-break: break-word;
  white-space: pre-wrap;
}

:deep(.review-img img) {
  width: 100% !important;
  height: 100% !important;
  object-fit: cover !important;
}
</style>
