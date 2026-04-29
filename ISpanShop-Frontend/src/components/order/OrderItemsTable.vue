<template>
  <div class="order-items-table">
    <el-table :data="items" style="width: 100%" stripe>
      <el-table-column label="商品資訊" min-width="300">
        <template #default="{ row }">
          <div class="product-info-wrapper clickable" @click="goToProduct(row.productId)">
            <el-image 
              :src="row.coverImage" 
              class="product-img" 
              fit="cover"
            >
              <template #error><div class="image-slot"><el-icon><Picture /></el-icon></div></template>
            </el-image>
            <div class="product-text">
              <div class="name">{{ row.productName }}</div>
              <PromotionTags :tags="row.promotionTags" />
              <div class="variant" v-if="row.variantName">規格：{{ row.variantName }}</div>
              <!-- SKU 已根據需求移除 -->
            </div>
          </div>
        </template>
      </el-table-column>
      
      <el-table-column label="單價" width="120" align="center">
        <template #default="{ row }">
          NT$ {{ formatPrice(row.price) }}
        </template>
      </el-table-column>
      
      <el-table-column :prop="quantityProp" :label="quantityLabel" width="100" align="center">
        <template #default="{ row }">
          <span :class="{ 'qty-highlight': highlightQuantity }">{{ row[quantityProp] }}</span>
        </template>
      </el-table-column>
      
      <el-table-column v-if="showSubtotal" label="小計" width="120" align="right">
        <template #default="{ row }">
          <span class="subtotal">NT$ {{ formatPrice(row.subtotal || (row.price * row.quantity)) }}</span>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Picture } from '@element-plus/icons-vue'
import PromotionTags from '@/components/common/PromotionTags.vue'

interface OrderItem {
  productId: number
  productName: string
  coverImage: string
  price: number
  quantity: number
  variantName?: string
  promotionTags?: string[]
  subtotal?: number
  [key: string]: any
}

const props = withDefaults(defineProps<{
  items: OrderItem[]
  quantityLabel?: string
  quantityProp?: string
  showSubtotal?: boolean
  highlightQuantity?: boolean
}>(), {
  items: () => [],
  quantityLabel: '數量',
  quantityProp: 'quantity',
  showSubtotal: true,
  highlightQuantity: false
})

const router = useRouter()

function goToProduct(productId: number) {
  if (!productId) {
    ElMessage.warning('無法取得商品編號')
    return
  }
  router.push(`/product/${productId}`)
}

function formatPrice(price: number) {
  return (price || 0).toLocaleString()
}
</script>

<style scoped lang="scss">
.product-info-wrapper {
  display: flex;
  gap: 12px;
  padding: 8px 0;
  
  &.clickable {
    cursor: pointer;
    transition: opacity 0.2s;
    &:hover {
      opacity: 0.8;
    }
  }
}

.product-img {
  width: 60px;
  height: 60px;
  border-radius: 4px;
  flex-shrink: 0;
  border: 1px solid #f0f0f0;
}

.product-text {
  .name {
    font-weight: 600;
    color: #1e293b;
    margin-bottom: 4px;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
  .variant {
    font-size: 12px;
    color: #64748b;
  }
}

.subtotal {
  font-weight: 600;
  color: #1e293b;
}

.qty-highlight {
  font-weight: 700;
  color: #ee4d2d;
  font-size: 16px;
}

.image-slot {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
  background: #f5f7fa;
  color: #a8abb2;
}
</style>
