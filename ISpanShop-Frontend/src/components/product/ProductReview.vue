<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { fetchProductReviews } from '@/api/review'

const props = defineProps<{
  productId: number
}>()

const reviews = ref<any[]>([])
const loading = ref(false)

const averageRating = computed(() => {
  if (reviews.value.length === 0) return 0
  const sum = reviews.value.reduce((acc, cur) => acc + cur.rating, 0)
  return (sum / reviews.value.length).toFixed(1)
})

const getRatingCount = (star: number) => {
  return reviews.value.filter(r => r.rating === star).length
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

onMounted(loadReviews)
</script>

<template>
  <div class="product-reviews mt-8 bg-white p-6 rounded shadow-sm">
    <h3 class="text-xl font-bold mb-6 border-b pb-4">商品評價</h3>
    
    <!-- 總覽 -->
    <div v-if="reviews.length > 0" class="rating-summary flex items-center p-6 bg-orange-50 rounded mb-8 border border-orange-100">
      <div class="text-center px-10 border-r border-orange-200">
        <div class="text-3xl font-bold text-orange-600">
          <span class="text-5xl">{{ averageRating }}</span> <span class="text-xl">/ 5</span>
        </div>
        <el-rate v-model="averageRating" disabled text-color="#ff9900" />
      </div>
      <div class="flex-1 px-8 flex flex-wrap gap-2">
        <el-button size="small" plain>全部 ({{ reviews.length }})</el-button>
        <el-button v-for="i in [5,4,3,2,1]" :key="i" size="small" plain>
          {{ i }} 星 ({{ getRatingCount(i) }})
        </el-button>
      </div>
    </div>

    <!-- 列表 -->
    <div v-loading="loading" class="review-list">
      <el-empty v-if="reviews.length === 0" description="尚無商品評價" />
      
      <div v-for="review in reviews" :key="review.id" class="review-item py-6 border-b last:border-0">
        <div class="flex items-start gap-4">
          <el-avatar :size="40" :src="review.userAvatar" />
          <div class="flex-1">
            <div class="flex justify-between items-center mb-1">
              <span class="font-bold text-sm">{{ review.userName }}</span>
            </div>
            <el-rate v-model="review.rating" disabled size="small" />
            
            <div class="text-xs text-gray-400 my-2">
              <span>{{ formatDate(review.createdAt) }}</span>
              <span v-if="review.variantName" class="ml-4">規格：{{ review.variantName }}</span>
            </div>
            
            <div class="text-sm text-gray-700 leading-relaxed mb-4">
              {{ review.comment }}
            </div>

            <!-- 賣家回覆 -->
            <div v-if="review.storeReply" class="bg-gray-50 p-4 rounded border-l-4 border-orange-400 mt-4">
              <div class="text-xs font-bold text-gray-500 mb-1">賣家回覆：</div>
              <div class="text-sm text-gray-600 italic">{{ review.storeReply }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.review-item:hover { background-color: #fafafa; transition: 0.2s; }
</style>
