<template>
  <div class="pd-page">
    <!-- ── 骨架屏 ── -->
    <template v-if="loading">
      <div class="pd-container">
        <el-skeleton :rows="1" style="margin-bottom: 16px" animated />
        <div class="pd-main-section">
          <el-skeleton style="width: 45%" animated>
            <template #template>
              <el-skeleton-item variant="image" style="width: 100%; aspect-ratio: 1/1" />
            </template>
          </el-skeleton>
          <div style="flex: 1; padding-left: 32px">
            <el-skeleton :rows="8" animated />
          </div>
        </div>
      </div>
    </template>

    <!-- ── 錯誤頁 ── -->
    <template v-else-if="loadError || (!loading && !product)">
      <div class="pd-container">
        <el-result
          icon="warning"
          :title="loadError ?? '商品載入失敗或不存在'"
          sub-title="請確認商品連結是否正確"
        >
          <template #extra>
            <el-button @click="router.back()">返回上一頁</el-button>
            <el-button type="primary" @click="router.push('/')">回首頁</el-button>
          </template>
        </el-result>
      </div>
    </template>

    <!-- ── 正常內容 ── -->
    <template v-else-if="isReady">
      <div class="pd-container">

        <!-- 麵包屑 -->
        <el-breadcrumb separator="/" class="pd-breadcrumb">
          <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
          <el-breadcrumb-item
            v-if="mainCategoryItem"
            :to="{ path: '/', query: { categoryId: String(mainCategoryItem.id) } }"
          >{{ mainCategoryItem.name }}</el-breadcrumb-item>
          <el-breadcrumb-item
            v-if="subCategoryItem && mainCategoryItem"
            :to="{
              path: '/',
              query: {
                categoryId: String(mainCategoryItem.id),
                subCategoryId: String(subCategoryItem.id),
              },
            }"
          >{{ subCategoryItem.name }}</el-breadcrumb-item>
          <el-breadcrumb-item class="pd-breadcrumb-product">{{ safeProduct.name }}</el-breadcrumb-item>
        </el-breadcrumb>

        <!-- 主區塊：左圖 + 右資訊 -->
        <div class="pd-main-section">

          <!-- 左側圖片區 -->
          <div class="pd-gallery">
            <div class="pd-main-image-wrap" style="position:relative;overflow:hidden;">
              <el-image
                :src="activeImageUrl || fallbackImage"
                :alt="safeProduct.name"
                fit="contain"
                class="pd-main-image"
              >
                <template #error>
                  <div class="image-error-placeholder">
                    <el-icon :size="64"><Picture /></el-icon>
                  </div>
                </template>
              </el-image>
              <div v-if="isSoldOut" style="position:absolute;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.5);display:flex;align-items:center;justify-content:center;z-index:1;pointer-events:none;">
                <span style="color:#fff;font-size:24px;font-weight:bold;background:rgba(0,0,0,0.7);padding:10px 28px;border-radius:4px;">已售完</span>
              </div>
            </div>

            <!-- 縮圖列 -->
            <div v-if="safeProduct.images.length > 1" class="pd-thumbnails">
              <div class="thumb-scroll">
                <div
                  v-for="img in safeProduct.images.slice(0, 5)"
                  :key="img.id"
                  class="pd-thumb"
                  :class="{ active: activeImageUrl === img.url }"
                  @click="activeImageUrl = img.url"
                >
                  <el-image :src="img.url" fit="cover" class="thumb-img" />
                </div>
              </div>
            </div>
          </div>

          <!-- 右側資訊區 -->
          <div class="pd-info">
            <h1 class="pd-name">{{ safeProduct.name }}</h1>

            <div class="pd-rating-row">
              <template v-if="safeProduct.rating !== null">
                <el-rate :model-value="safeProduct.rating" disabled show-score />
                <span class="pd-review-count">{{ safeProduct.reviewCount }} 評價</span>
              </template>
              <template v-else>
                <span class="pd-no-rating">暫無評價</span>
                <span class="pd-review-count">{{ formatSoldCount(safeProduct.soldCount) }} 已售出</span>
              </template>
            </div>

            <div class="pd-price-block">
              <div class="pd-price-row">
                <span class="pd-price-main">
                  <template v-if="selectedVariant">
                    ${{ formatPrice(selectedVariant.price) }}
                  </template>
                  <template v-else>
                    ${{ formatPrice(safeProduct.priceRange.min) }}
                    <template v-if="safeProduct.priceRange.max !== safeProduct.priceRange.min">
                      &nbsp;-&nbsp;${{ formatPrice(safeProduct.priceRange.max) }}
                    </template>
                  </template>
                </span>
                <span
                  v-if="safeProduct.discountRate !== null"
                  class="pd-discount-tag"
                >{{ safeProduct.discountRate.toFixed(1) }} 折</span>
              </div>
              <div v-if="safeProduct.originalPriceRange" class="pd-original-price">
                原價：
                <span class="pd-strikethrough">
                  ${{ formatPrice(safeProduct.originalPriceRange.min) }}
                  <template v-if="safeProduct.originalPriceRange.max !== safeProduct.originalPriceRange.min">
                    &nbsp;-&nbsp;${{ formatPrice(safeProduct.originalPriceRange.max) }}
                  </template>
                </span>
              </div>
            </div>

            <!-- 賣場休假提示 -->
            <div v-if="safeProduct.store?.status === 2" class="pd-vacation-alert">
              <el-alert
                title="賣場休假中，暫時無法下單"
                type="warning"
                description="您可以將商品加入購物車，待賣場恢復營業後再行結帳。"
                show-icon
                :closable="false"
              />
            </div>

            <!-- 規格選擇器 -->
            <div v-if="safeProduct.specs.length > 0" class="pd-spec-selector">
              <div v-for="spec in safeProduct.specs" :key="spec.name" class="pd-spec-row">
                <div class="pd-spec-label">{{ spec.name }}</div>
                <div class="pd-spec-options">
                  <button
                    v-for="option in spec.options"
                    :key="option"
                    class="pd-spec-btn"
                    :class="{
                      selected: selectedSpecs[spec.name] === option,
                      unavailable: getOptionStatus(spec.name, option) !== 'available',
                    }"
                    :disabled="isSoldOut || getOptionStatus(spec.name, option) !== 'available'"
                    @click="selectSpec(spec.name, option)"
                  >
                    <img
                      v-if="getSpecOptionImage(spec.name, option)"
                      :src="getSpecOptionImage(spec.name, option)!"
                      class="pd-spec-btn-img"
                      :alt="option"
                    />
                    <span>{{ option }}</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- 數量選擇器 -->
            <div class="pd-quantity-row">
              <span class="pd-quantity-label">數量</span>
              <el-input-number
                v-model="quantity"
                :min="1"
                :max="currentStock"
                :disabled="isSoldOut || !allSpecsSelected"
                controls-position="right"
                size="default"
              />
              <span class="pd-stock-hint" :class="{ 'pd-stock-hint--active': !!selectedVariant }">
                庫存 {{ selectedVariant ? selectedVariant.stock : safeProduct.totalStock }} 件
              </span>
            </div>

            <div class="pd-action-buttons">
              <el-button
                class="btn-cart"
                :disabled="isSoldOut"
                @click="handleAddToCart"
              >加入購物車</el-button>
              <el-button
                type="primary"
                class="btn-buy"
                :disabled="isSoldOut || safeProduct.store?.status === 2"
                @click="handleBuyNow"
              >
                {{ safeProduct.store?.status === 2 ? '賣場休假中' : '直接購買' }}
              </el-button>
            </div>
          </div>
        </div>

        <!-- 商品規格卡 -->
        <el-card class="pd-detail-card" shadow="never">
          <template #header><span class="card-title">商品規格</span></template>
          <div class="pd-spec-table">
            <div class="pd-spec-item"><span class="spec-key">分類</span><span class="spec-val">
              <template v-if="safeProduct.categoryPath.length > 0">
                <span v-for="(cat, idx) in safeProduct.categoryPath" :key="cat.id">
                  {{ cat.name }}<span v-if="idx < safeProduct.categoryPath.length - 1"> &gt; </span>
                </span>
              </template>
              <template v-else>—</template>
            </span></div>
            <div class="pd-spec-item"><span class="spec-key">品牌</span><span class="spec-val">{{ safeProduct.brandName }}</span></div>
            <div class="pd-spec-item"><span class="spec-key">店家</span><span class="spec-val">{{ safeProduct.storeName }}</span></div>
            <div class="pd-spec-item"><span class="spec-key">庫存</span><span class="spec-val">{{ safeProduct.totalStock }}</span></div>
            <div class="pd-spec-item"><span class="spec-key">上架日</span><span class="spec-val">{{ safeProduct.createdAt }}</span></div>
          </div>
        </el-card>

        <!-- 賣家資訊卡 -->
        <el-card v-if="safeProduct.store" class="pd-detail-card pd-store-card" shadow="never">
          <template #header><span class="card-title">賣家資訊</span></template>
          <div class="pd-store-inner">
            <!-- 左側區塊 -->
            <div class="store-left">
              <div class="store-avatar-wrap">
                <el-avatar v-if="safeProduct.store.logoUrl" :src="safeProduct.store.logoUrl" :size="64" />
                <el-avatar v-else :size="64" class="store-avatar-fallback">{{ safeProduct.store.name.charAt(0) }}</el-avatar>
              </div>
              <div class="store-info">
                <div class="store-name">{{ safeProduct.store.name }}</div>
                <div class="store-online">{{ formatRelativeTime(safeProduct.createdAt) }}上線</div>
                <div class="store-actions">
                  <button class="chat-btn" @click="handleOpenChat">
                    <el-icon><ChatDotRound /></el-icon>好聊
                  </button>
                  <button class="store-btn" @click="handleViewStore">
                    <el-icon><Shop /></el-icon>查看賣場
                  </button>
                </div>
              </div>
            </div>
            
            <!-- 右側區塊 -->
            <div class="store-right">
              <div class="store-stat-item">
                <span class="stat-label">商品評價</span>
                <span class="stat-value">{{ safeProduct.reviewCount !== null ? safeProduct.reviewCount : '—' }} <span class="stat-unit">筆</span></span>
              </div>
              <div class="store-stat-item">
                <span class="stat-label">商品</span>
                <span class="stat-value">{{ safeProduct.store.productCount || 0 }} <span class="stat-unit">件</span></span>
              </div>
              <div class="store-stat-item">
                <span class="stat-label">加入時間</span>
                <span class="stat-value">{{ formatJoinedTime(safeProduct.store.joinedYearsAgo) }}</span>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 商品描述 -->
        <el-card class="pd-detail-card" shadow="never">
          <template #header><span class="card-title">商品描述</span></template>
          <pre class="pd-description">{{ safeProduct.description }}</pre>
        </el-card>

        <!-- 逛逛賣場其他好物 -->
        <div v-if="relatedLoading || relatedProducts.length > 0" class="pd-related-section">
          <h2 class="related-title">逛逛賣場其他好物</h2>
          <div v-if="relatedLoading" class="related-skeleton-grid">
            <el-skeleton v-for="n in 6" :key="n" animated />
          </div>
          <div v-else class="related-grid">
            <ProductCard v-for="p in relatedProducts" :key="p.id" :product="p" />
          </div>
        </div>

      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Picture, ChatDotRound, Shop } from '@element-plus/icons-vue'
import ProductCard from '@/components/product/ProductCard.vue'
import { fetchProductDetail, fetchRelatedProducts } from '@/api/product'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { formatPrice, formatSoldCount, formatRelativeTime } from '@/utils/format'
import type {
  ProductDetail,
  ProductListItem,
  ProductVariant,
  CategoryPathItem,
  ProductImage,
  PriceRange,
  ProductSpec,
  StoreInfo,
} from '@/types/product'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()
const chatStore = useChatStore()

const product = ref<ProductDetail | null>(null)
const loading = ref(false)
const loadError = ref<string | null>(null)
const relatedProducts = ref<ProductListItem[]>([])
const relatedLoading = ref(false)
const activeImageUrl = ref('')
const quantity = ref(1)
const selectedSpecs = ref<Record<string, string | null>>({})

const fallbackImage = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII='
const isReady = computed(() => !loading.value && product.value !== null)

const safeProduct = computed(() => {
  const p = product.value!
  return {
    ...p,
    name: p.name || '（無名稱）',
    description: p.description || '賣家尚未提供商品描述',
    storeName: p.store?.name || '—',
    brandName: p.brand?.name || '—',
    totalStock: p.totalStock || 0,
    priceRange: p.priceRange || { min: 0, max: 0 },
    categoryPath: (p.categoryPath || []) as CategoryPathItem[],
    images: (p.images || []) as ProductImage[],
    specs: (p.specs || []) as ProductSpec[],
    variants: (p.variants || []) as ProductVariant[],
    createdAt: p.createdAt ? p.createdAt.substring(0, 10) : '—',
  }
})

const mainCategoryItem = computed(() => isReady.value ? safeProduct.value.categoryPath[0] : null)
const subCategoryItem = computed(() => isReady.value ? safeProduct.value.categoryPath[1] : null)
const isSoldOut = computed(() => isReady.value && safeProduct.value.totalStock === 0)
const hasSpecs = computed(() => isReady.value && safeProduct.value.specs.length > 0)
const allSpecsSelected = computed(() => {
  if (!isReady.value) return false
  return safeProduct.value.specs.every(s => selectedSpecs.value[s.name] !== null)
})

const selectedVariant = computed(() => {
  if (!isReady.value || !allSpecsSelected.value) return null
  return safeProduct.value.variants.find(v => 
    safeProduct.value.specs.every(s => v.specValues[s.name] === selectedSpecs.value[s.name])
  ) || null
})

const currentStock = computed(() => {
  if (!isReady.value) return 1
  return selectedVariant.value ? selectedVariant.value.stock : safeProduct.value.totalStock
})

function getOptionStatus(specName: string, optionValue: string) {
  if (!isReady.value) return 'unavailable'
  const matching = safeProduct.value.variants.filter(v => {
    if (v.specValues[specName] !== optionValue) return false
    for (const [name, value] of Object.entries(selectedSpecs.value)) {
      if (name !== specName && value !== null && v.specValues[name] !== value) return false
    }
    return true
  })
  if (matching.length === 0) return 'unavailable'
  return matching.some(v => v.stock > 0) ? 'available' : 'soldOut'
}

function getSpecOptionImage(specName: string, optionValue: string): string | null {
  const variant = safeProduct.value.variants.find(
    v => v.specValues[specName] === optionValue && v.imageUrl
  )
  return variant?.imageUrl ?? null
}

function selectSpec(specName: string, optionValue: string) {
  if (getOptionStatus(specName, optionValue) !== 'available') return
  selectedSpecs.value[specName] = selectedSpecs.value[specName] === optionValue ? null : optionValue
  quantity.value = 1
  if (selectedVariant.value?.imageUrl) activeImageUrl.value = selectedVariant.value.imageUrl
}

async function loadProduct(id: number) {
  loading.value = true
  try {
    const res = await fetchProductDetail(id)
    if (res.success) {
      product.value = res.data
      const mainImg = res.data.images.find(img => img.isMain) || res.data.images[0]
      activeImageUrl.value = mainImg?.url || ''
      res.data.specs.forEach(s => selectedSpecs.value[s.name] = null)
      void loadRelated(id)
    }
  } catch (err) {
    loadError.value = '載入失敗'
  } finally {
    loading.value = false
  }
}

async function loadRelated(id: number) {
  relatedLoading.value = true
  try {
    const res = await fetchRelatedProducts(id, 12)
    relatedProducts.value = Array.isArray(res.data) ? res.data : []
  } catch (err) {
    relatedProducts.value = []
  } finally {
    relatedLoading.value = false
  }
}

function handleAddToCart() {
  if (hasSpecs.value && !allSpecsSelected.value) { ElMessage.warning('請選擇規格'); return }
  const p = safeProduct.value
  const variant = selectedVariant.value
  const price = variant ? variant.price : p.priceRange.min
  cartStore.addItem({
    productId: p.id,
    variantId: variant?.id ?? null,
    name: p.name,
    image: activeImageUrl.value || p.images[0]?.url || '',
    price,
    quantity: quantity.value,
    specLabel: variant ? Object.entries(variant.specValues).map(([k, v]) => `${k}: ${v}`).join('、') : '',
    storeId: p.store?.id ?? 0,
    storeName: p.storeName,
    storeStatus: p.store?.status ?? 1,
  })
  ElMessage.success('已加入購物車')
}

function handleBuyNow(): void {
  // 1. 檢查規格是否已選
  if (hasSpecs.value && !allSpecsSelected.value) {
    ElMessage.warning('請選擇規格')
    return
  }

  // 2. 準備商品資訊
  const p = safeProduct.value
  const variant = selectedVariant.value
  const image = activeImageUrl.value || p.images[0]?.url || ''
  const price = variant ? variant.price : p.priceRange.min
  const variantId = variant?.id ?? null
  const specLabel = variant
    ? Object.entries(variant.specValues).map(([k, v]) => `${k}: ${v}`).join('、')
    : ''

  const checkoutItem = {
    productId: p.id,
    variantId,
    name: p.name,
    image,
    price,
    quantity: quantity.value,
    specLabel,
    storeId: p.store?.id ?? 0,
    storeName: p.storeName,
  }

  // 3. 存入臨時結帳資訊 (SessionStorage) 並導向結帳頁，不影響購物車
  sessionStorage.setItem('TEMP_CHECKOUT_DATA', JSON.stringify([checkoutItem]))
  ElMessage.success('正在為您準備結帳...')
  router.push('/checkout?type=direct')
}

function handleViewStore() {
  if (safeProduct.value.store?.id) router.push(`/store/${safeProduct.value.store.id}`)
}

function handleOpenChat() {
  if (!authStore.isLoggedIn) {
    ElMessage.warning('請先登入後再使用好聊功能')
    router.push('/login')
    return
  }
  const store = safeProduct.value.store
  if (store) {
    chatStore.openChatWithUser(store.userId || 1, store.name || '賣家')
  }
}

/**
 * 格式化加入時間顯示
 * 修正 0 年前的問題，改為顯示月或天的精度
 */
function formatJoinedTime(years: number | null | undefined): string {
  const y = years ?? 0
  if (y === 0) return '新加入'
  if (y < 1) {
    // 小於 1 年，轉換成月份顯示（假設 joinedYearsAgo 可能有小數）
    const months = Math.floor(y * 12)
    if (months < 1) return '新加入'
    return `${months}個月前`
  }
  return `${y}年前`
}

onMounted(() => {
  const id = Number(route.params.id)
  if (id) loadProduct(id)
})

watch(() => route.params.id, (newId) => {
  if (newId) { window.scrollTo({ top: 0, behavior: 'smooth' }); loadProduct(Number(newId)) }
})
</script>

<style scoped>
.pd-page { background: #f5f5f5; min-height: 100vh; padding: 16px 0 48px; }
.pd-container { max-width: 1200px; margin: 0 auto; padding: 0 16px; }
.pd-breadcrumb { margin-bottom: 16px; font-size: 12px; }
.pd-breadcrumb :deep(.el-breadcrumb__inner a) { color: #475569; font-weight: normal; text-decoration: none; }
.pd-breadcrumb :deep(.el-breadcrumb__inner a:hover) { color: #EE4D2D; text-decoration: underline; }
.pd-main-section { display: flex; gap: 32px; background: #fff; border-radius: 4px; padding: 24px; margin-bottom: 16px; }
.pd-gallery { flex: 0 0 45%; }
.pd-main-image-wrap { width: 100%; aspect-ratio: 1/1; background: #f8fafc; border-radius: 4px; overflow: hidden; display: flex; align-items: center; justify-content: center; }
.pd-main-image { width: 100%; height: 100%; }
.pd-thumbnails { margin-top: 12px; }
.thumb-scroll { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 4px; }
.pd-thumb { flex: 0 0 68px; width: 68px; height: 68px; border: 2px solid #e2e8f0; border-radius: 4px; overflow: hidden; cursor: pointer; transition: border-color 0.2s; }
.pd-thumb:hover, .pd-thumb.active { border-color: #EE4D2D; }
.pd-info { flex: 1; min-width: 0; }
.pd-name { font-size: 22px; font-weight: 700; color: #1e293b; line-height: 1.4; margin: 0 0 12px; }
.pd-rating-row { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; padding-bottom: 16px; border-bottom: 1px solid #f1f5f9; }
.pd-vacation-alert { margin-bottom: 20px; }
.pd-price-block { background: #fffbf8; border-radius: 4px; padding: 16px; margin-bottom: 20px; }
.pd-price-main { font-size: 30px; font-weight: 700; color: #EE4D2D; line-height: 1; }
.pd-discount-tag { background: #EE4D2D; color: #fff; font-size: 12px; font-weight: 600; padding: 2px 8px; border-radius: 2px; }
.pd-original-price { margin-top: 8px; font-size: 13px; color: #94a3b8; }
.pd-strikethrough { text-decoration: line-through; }
.pd-spec-row { display: flex; align-items: flex-start; gap: 16px; margin-bottom: 24px; }
.pd-spec-label { flex: 0 0 80px; font-size: 14px; color: #757575; padding-top: 10px; }
.pd-spec-options { display: flex; flex-wrap: wrap; gap: 10px; }
.pd-spec-btn { display: inline-flex; align-items: center; gap: 8px; padding: 8px 16px; border: 1px solid #e0e0e0; border-radius: 4px; background: #fff; cursor: pointer; font-size: 14px; color: #333; transition: all 0.2s; min-height: 40px; position: relative; }
.pd-spec-btn:hover:not(:disabled) { border-color: #EE4D2D; color: #EE4D2D; }
.pd-spec-btn.selected { border-color: #EE4D2D; color: #EE4D2D; }
.pd-spec-btn.selected::after { content: '✓'; position: absolute; bottom: 0; right: 0; background: #EE4D2D; color: #fff; font-size: 10px; width: 16px; height: 16px; display: flex; align-items: center; justify-content: center; border-radius: 4px 0 4px 0; }
.pd-spec-btn.unavailable { opacity: 0.4; cursor: not-allowed; border-color: #e0e0e0; color: #333; }
.pd-spec-btn-img { width: 24px; height: 24px; object-fit: cover; border-radius: 2px; }
.pd-quantity-row { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
.pd-stock-hint { font-size: 13px; color: #94a3b8; }
.pd-action-buttons { display: flex; gap: 12px; }
.btn-cart { color: #EE4D2D !important; border-color: #EE4D2D !important; min-width: 160px; height: 44px; font-weight: 600; }
.btn-buy { background: #EE4D2D !important; border-color: #EE4D2D !important; min-width: 160px; height: 44px; font-weight: 600; }
.pd-detail-card { margin-bottom: 16px; border-radius: 4px; }
.card-title { font-size: 16px; font-weight: 700; color: #1e293b; }
.pd-spec-table { display: grid; grid-template-columns: 1fr 1fr; gap: 12px 32px; }
.pd-spec-item { display: flex; gap: 12px; }
.spec-key { flex: 0 0 60px; font-size: 13px; color: #94a3b8; }
.spec-val { font-size: 13px; color: #334155; }
.pd-store-inner { display: flex; align-items: flex-start; gap: 32px; padding: 8px 0; }

/* 左側區塊 */
.store-left { display: flex; gap: 16px; flex: 1; }
.store-avatar-wrap { flex-shrink: 0; }
.store-avatar-fallback { background: linear-gradient(135deg, #EE4D2D 0%, #F3826C 100%); color: white; font-size: 24px; font-weight: 700; }
.store-info { flex: 1; }
.store-name { font-size: 16px; font-weight: 700; color: #1e293b; margin-bottom: 6px; }
.store-online { font-size: 13px; color: #64748b; margin-bottom: 12px; }
.store-actions { display: flex; gap: 8px; }

/* 好聊按鈕 */
.chat-btn {
  background-color: #EE4D2D;
  color: white;
  border: none;
  padding: 10px 24px;
  font-size: 14px;
  border-radius: 4px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-width: 120px;
  height: 40px;
  justify-content: center;
  transition: background-color 0.2s;
  font-weight: 500;
}
.chat-btn:hover {
  background-color: #d63c1f;
}

/* 查看賣場按鈕 */
.store-btn {
  background-color: white;
  color: #555;
  border: 1px solid #999;
  padding: 10px 24px;
  font-size: 14px;
  border-radius: 4px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-width: 120px;
  height: 40px;
  justify-content: center;
  transition: all 0.2s;
  font-weight: 500;
}
.store-btn:hover {
  border-color: #EE4D2D;
  color: #EE4D2D;
}

/* 右側區塊 */
.store-right { 
  display: grid; 
  grid-template-columns: repeat(3, 1fr); 
  gap: 24px; 
  flex: 1;
  border-left: 1px solid #e2e8f0;
  padding-left: 32px;
}
.store-stat-item { 
  display: flex; 
  flex-direction: column; 
  align-items: center;
  text-align: center;
}
.stat-label { 
  font-size: 13px; 
  color: #94a3b8; 
  margin-bottom: 6px; 
}
.stat-value { 
  font-size: 18px; 
  font-weight: 700; 
  color: #EE4D2D; 
}
.stat-unit { 
  font-size: 13px; 
  font-weight: 400; 
  color: #64748b; 
  margin-left: 2px;
}

.pd-description { white-space: pre-wrap; font-size: 14px; color: #334155; line-height: 1.8; margin: 0; font-family: inherit; }
.pd-related-section { margin-top: 24px; }
.related-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 0 0 16px; padding-bottom: 12px; border-bottom: 2px solid #EE4D2D; display: inline-block; }
.related-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 12px; }
</style>
