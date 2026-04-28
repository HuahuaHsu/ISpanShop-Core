<template>
  <div class="promotion-page">
    <!-- Hero Banner -->
    <div class="promotion-hero" :style="heroBg">
      <div class="promotion-hero-overlay"></div>
      <div class="promotion-hero-content">
        <span v-if="promotion" class="promo-type-tag" :class="promotion.type">
          {{ promotion.typeLabel }}
        </span>
        <h1>{{ promotion?.title ?? '活動載入中…' }}</h1>
        <p v-if="promotion?.subtitle">{{ promotion.subtitle }}</p>
        <div v-if="promotion?.endDate" class="promo-countdown">
          活動截止：{{ formatDate(promotion.endDate) }}
        </div>
      </div>
    </div>

    <!-- 活動優惠提示橫幅 -->
    <div v-if="isDiscountType" class="promo-benefit-banner">
      <div class="benefit-icon">🎉</div>
      <div class="benefit-text">
        <strong>滿額折扣優惠</strong>
        <span v-if="discountRule">本活動商品消費滿 <em>NT$ {{ formatNumber(discountRule.threshold) }}</em> 立即折抵 <em>NT$ {{ formatNumber(discountRule.discountValue) }}</em>，結帳時自動折扣！</span>
        <span v-else>本活動商品享滿額折扣優惠，結帳時自動折扣！</span>
      </div>
    </div>
    <div v-if="isLimitedType" class="promo-benefit-banner limited">
      <div class="benefit-icon">⏰</div>
      <div class="benefit-text">
        <strong>限量搶購中</strong>
        <span>商品數量有限，每人限購，售完為止！</span>
      </div>
    </div>

    <div class="promotion-body">
      <!-- 篩選排序列 -->
      <div class="promotion-toolbar">
        <span class="toolbar-label">排序：</span>
        <div class="sort-btns">
          <button
            v-for="opt in sortOptions"
            :key="opt.value"
            class="sort-btn"
            :class="{ active: sortBy === opt.value && priceOrder === '' }"
            @click="handleSort(opt.value)"
          >{{ opt.label }}</button>
          <el-dropdown trigger="click" @command="handlePriceSort">
            <button
              class="sort-btn"
              :class="{ active: priceOrder !== '' }"
            >
              {{ priceLabel }}
              <el-icon style="margin-left:4px;vertical-align:middle;"><ArrowDown /></el-icon>
            </button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="asc">價格：低到高</el-dropdown-item>
                <el-dropdown-item command="desc">價格：高到低</el-dropdown-item>
                <el-dropdown-item command="">全部價格</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
        <span class="toolbar-count">共 {{ pagination.totalCount }} 件商品</span>
      </div>

      <!-- 商品格線 -->
      <div v-if="loading" class="product-grid">
        <el-skeleton v-for="n in 8" :key="n" animated class="skeleton-card">
          <template #template>
            <el-skeleton-item variant="image" class="skeleton-img" />
            <div style="padding:10px 12px;">
              <el-skeleton-item variant="p" style="width:90%" />
              <el-skeleton-item variant="p" style="width:60%;margin-top:8px;" />
            </div>
          </template>
        </el-skeleton>
      </div>

      <div v-else-if="products.length > 0" class="product-grid">
        <div
          v-for="item in products"
          :key="item.productId"
          class="product-card"
          @click="goToProduct(item.productId)"
        >
          <div class="product-img-wrap">
            <img :src="formatImg(item.imageUrl)" :alt="item.productName" />
            <span class="promo-badge" :class="{ discount: isDiscountType, limited: isLimitedType }">
              {{ isDiscountType ? '滿額折扣' : isLimitedType ? '限量搶購' : '促銷中' }}
            </span>
          </div>
          <div class="product-info">
            <h3 class="product-name">{{ item.productName }}</h3>
            <div class="price-row">
              <!-- 條件一：當商品有實質打折時 -->
              <template v-if="item.discountPrice && item.discountPrice < item.originalPrice && item.discountPrice > 0">
                <span class="current-price">${{ formatNumber(item.discountPrice) }}</span>
                <span class="original-price">${{ formatNumber(item.originalPrice) }}</span>
                <span v-if="item.discountPercent" class="discount-tag">{{ item.discountPercent }}折</span>
              </template>
              <!-- 條件二：當商品是滿額折扣或沒有打折時 -->
              <template v-else>
                <span class="current-price">${{ formatNumber(item.originalPrice) }}</span>
                <span v-if="isDiscountType" class="full-discount-tag">符合滿額折</span>
              </template>
            </div>
            <div class="sales-info">已售出 {{ item.soldCount }}</div>
          </div>
        </div>
      </div>

      <el-empty v-else description="此活動目前沒有商品" :image-size="160" />

      <!-- 分頁 -->
      <div v-if="pagination.totalPages > 1" class="pagination-wrap">
        <el-pagination
          layout="prev, pager, next"
          :total="pagination.totalCount"
          :page-size="pagination.pageSize"
          :current-page="pagination.page"
          @current-change="handlePageChange"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowDown } from '@element-plus/icons-vue'
import { fetchPromotionById, fetchPublicPromotionProducts } from '@/api/promotion'
import type { PromotionProductItem, PromotionProductsResult } from '@/api/promotion'
import type { Promotion as PromotionInfo } from '@/types/promotion'

const route = useRoute()
const router = useRouter()

const promotionId = computed(() => Number(route.params['id']))

const promotion = ref<PromotionInfo | null>(null)
const products = ref<PromotionProductItem[]>([])
const loading = ref<boolean>(false)
const sortBy = ref<string>('default')
const priceOrder = ref<string>('')

interface DiscountRule { threshold: number; discountValue: number }
const discountRule = ref<DiscountRule | null>(null)

const isDiscountType = computed(() => promotion.value?.type === 'discount')
const isLimitedType  = computed(() => promotion.value?.type === 'limitedBuy')

function formatNumber(num: number | null | undefined): string {
  return Number(num ?? 0).toLocaleString()
}
const pagination = ref<Omit<PromotionProductsResult, 'items'>>({
  totalCount: 0,
  page: 1,
  pageSize: 20,
  totalPages: 0,
})

const API_BASE = (import.meta.env['VITE_API_BASE_URL'] as string) || 'https://localhost:7125'

function formatImg(url: string | null | undefined): string {
  if (!url) return ''
  return url.startsWith('http') ? url : API_BASE + url
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('zh-TW')
}

const typeGradients: Record<string, string> = {
  flashSale:  'linear-gradient(135deg, #ff6b35 0%, #f7c948 100%)',
  discount:   'linear-gradient(135deg, #ee4d2d 0%, #ff7849 100%)',
  limitedBuy: 'linear-gradient(135deg, #7c3aed 0%, #a78bfa 100%)',
  other:      'linear-gradient(135deg, #1a1a2e 0%, #16213e 100%)',
}

const heroBg = computed(() => {
  const img = promotion.value?.bannerImageUrl
  if (img) return { backgroundImage: `url(${formatImg(img)})`, backgroundSize: 'cover', backgroundPosition: 'center' }
  const gradient = typeGradients[promotion.value?.type ?? ''] ?? typeGradients['other'] ?? '#1a1a2e'
  return { background: gradient }
})

const sortOptions = [
  { value: 'default', label: '預設' },
  { value: 'sales',   label: '銷量' },
]

const priceLabel = computed<string>(() => {
  if (priceOrder.value === 'asc') return '價格：低到高'
  if (priceOrder.value === 'desc') return '價格：高到低'
  return '價格'
})

async function loadPromotion(): Promise<void> {
  try {
    const res = await fetchPromotionById(promotionId.value)
    if (res.success) promotion.value = res.data
  } catch {
    // 靜默失敗，hero 顯示漸層色
  }
}

async function loadProducts(): Promise<void> {
  loading.value = true
  try {
    const res = await fetchPublicPromotionProducts(promotionId.value, {
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      sortBy: sortBy.value,
      priceOrder: priceOrder.value,
    })
    if (res.success) {
      products.value = res.data.items
      pagination.value = { ...pagination.value, ...res.data }
    }
  } catch {
    products.value = []
  } finally {
    loading.value = false
  }
}

function handleSort(value: string): void {
  sortBy.value = value
  priceOrder.value = ''
  pagination.value.page = 1
  void loadProducts()
}

function handlePriceSort(command: string): void {
  priceOrder.value = command
  sortBy.value = command ? 'price' : 'default'
  pagination.value.page = 1
  void loadProducts()
}

function handlePageChange(page: number): void {
  pagination.value.page = page
  void loadProducts()
}

function goToProduct(id: number): void {
  console.log('[活動商品] 跳轉商品 ID:', id)
  void router.push(`/product/${id}`)
}

onMounted(() => {
  void loadPromotion()
  void loadProducts()
})

watch(promotionId, () => {
  promotion.value = null
  products.value = []
  pagination.value.page = 1
  sortBy.value = 'default'
  priceOrder.value = ''
  void loadPromotion()
  void loadProducts()
})
</script>

<style scoped>
.promotion-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px 16px 48px;
}

/* ── Hero ── */
.promotion-hero {
  position: relative;
  border-radius: 12px;
  overflow: hidden;
  min-height: 200px;
  display: flex;
  align-items: center;
  margin-bottom: 24px;
}
.promotion-hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, rgba(0,0,0,0.6) 0%, rgba(0,0,0,0.1) 100%);
}
.promotion-hero-content {
  position: relative;
  z-index: 2;
  padding: 40px;
  color: #fff;
}
.promotion-hero-content h1 {
  font-size: 28px;
  font-weight: 700;
  margin: 8px 0 12px;
  text-shadow: 0 2px 8px rgba(0,0,0,0.3);
}
.promotion-hero-content p {
  font-size: 15px;
  opacity: 0.9;
  margin-bottom: 8px;
}
.promo-type-tag {
  display: inline-block;
  padding: 4px 14px;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 600;
  background: rgba(238,77,45,0.85);
}
.promo-type-tag.flashSale  { background: #ff6b35; }
.promo-type-tag.discount   { background: #ee4d2d; }
.promo-type-tag.limitedBuy { background: #7c3aed; }
.promo-type-tag.other      { background: #555; }
.promo-countdown { margin-top: 8px; opacity: 0.85; font-size: 14px; }

/* ── Body ── */
.promotion-body {
  background: #fff;
  border-radius: 12px;
  padding: 20px 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}

/* ── 排序列 ── */
.promotion-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding-bottom: 16px;
  margin-bottom: 16px;
  border-bottom: 1px solid #f0f0f0;
}
.toolbar-label { font-size: 13px; color: #606266; flex-shrink: 0; }
.sort-btns { display: flex; gap: 6px; }
.sort-btn {
  padding: 5px 16px;
  border: 1px solid #dcdfe6;
  background: white;
  border-radius: 4px;
  font-size: 13px;
  color: #606266;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  align-items: center;
}
.sort-btn:hover { border-color: #EE4D2D; color: #EE4D2D; }
.sort-btn.active { background: #EE4D2D; border-color: #EE4D2D; color: white; font-weight: 600; }
.toolbar-count { margin-left: auto; font-size: 13px; color: #909399; }

/* ── 商品格線 ── */
.product-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}
@media (max-width: 1024px) { .product-grid { grid-template-columns: repeat(3, 1fr); } }
@media (max-width: 768px)  { .product-grid { grid-template-columns: repeat(2, 1fr); } }

.skeleton-card { border-radius: 8px; overflow: hidden; border: 1px solid #f1f5f9; }
.skeleton-img  { width: 100%; aspect-ratio: 1 / 1; }

/* ── 商品卡 ── */
.product-card {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
  border: 1px solid #f0f0f0;
}
.product-card:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  transform: translateY(-2px);
}
.product-img-wrap {
  position: relative;
  padding-top: 100%;
  background: #f9f9f9;
}
.product-img-wrap img {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.promo-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  background: #ee4d2d;
  color: #fff;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
}
.product-info { padding: 10px 12px 12px; }
.product-name {
  font-size: 13px;
  margin: 0 0 8px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #333;
}
.price-row { display: flex; align-items: baseline; gap: 6px; flex-wrap: wrap; }
.current-price { color: #ee4d2d; font-size: 17px; font-weight: 600; }
.original-price { color: #999; font-size: 12px; text-decoration: line-through; }
.discount-tag {
  background: #fff0ed;
  color: #ee4d2d;
  font-size: 11px;
  padding: 1px 6px;
  border-radius: 3px;
}
.full-discount-tag {
  background-color: #ffebee;
  color: #e53935;
  font-size: 12px;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: 8px;
  font-weight: 500;
}
.sales-info { color: #999; font-size: 12px; margin-top: 4px; }

/* ── 分頁 ── */
.pagination-wrap {
  display: flex;
  justify-content: center;
  padding-top: 16px;
  border-top: 1px solid #f0f0f0;
}

/* ── 活動優惠提示橫幅 ── */
.promo-benefit-banner {
  display: flex;
  align-items: center;
  gap: 14px;
  background: linear-gradient(135deg, #ff7849 0%, #f97316 100%);
  border-radius: 10px;
  padding: 14px 20px;
  margin-bottom: 16px;
  color: #fff;
  box-shadow: 0 2px 8px rgba(249, 115, 22, 0.25);
}
.promo-benefit-banner.limited {
  background: linear-gradient(135deg, #7c3aed 0%, #a78bfa 100%);
  box-shadow: 0 2px 8px rgba(124, 58, 237, 0.25);
}
.benefit-icon {
  font-size: 28px;
  flex-shrink: 0;
  line-height: 1;
}
.benefit-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
  font-size: 14px;
}
.benefit-text strong {
  font-size: 16px;
  font-weight: 700;
  letter-spacing: 0.5px;
}
.benefit-text em {
  font-style: normal;
  font-weight: 700;
  font-size: 15px;
  text-decoration: underline;
  text-underline-offset: 2px;
}

/* ── 商品徽章 ── */
.promo-badge.discount { background: #f97316; }
.promo-badge.limited  { background: #7c3aed; }
</style>
