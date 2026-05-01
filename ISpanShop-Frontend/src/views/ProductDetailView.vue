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

        <!-- 促銷活動橫幅（麵包屑下方、商品主區塊上方） -->
        <div v-if="activePromotion" class="promo-banner-strip" :class="`promo-banner--${activePromotion.type}`">
          <div class="promo-banner-content">
            <span class="promo-banner-tag">{{ activePromotion.typeLabel }}</span>
            <span class="promo-banner-title">{{ activePromotion.title }}</span>
            <span class="promo-banner-sep">·</span>
            <span class="promo-banner-desc">{{ promoBannerDesc }}</span>
            <template v-if="countdownText">
              <span class="promo-banner-sep">·</span>
              <span class="promo-banner-countdown">⏱ {{ countdownText }}</span>
            </template>
            <a :href="activePromotion.linkUrl" class="promo-banner-link">查看活動 ›</a>
          </div>
        </div>

        <!-- 主區塊：左圖 + 右資訊 -->
        <div class="pd-main-section">

          <!-- 左側圖片區 -->
          <div class="pd-gallery">
            <div class="pd-main-image-wrap" style="position:relative;overflow:hidden;">
              <el-image
                :src="getFullImageUrl(activeImageUrl) || fallbackImage"
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
                  <el-image :src="getFullImageUrl(img.url)" fit="cover" class="thumb-img" />
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
                <span class="pd-review-count">{{ formatSoldCount(safeProduct.soldCount) }}</span>
              </template>
            </div>

            <div class="pd-price-block">
              <!-- 折扣價 + 折數標籤 -->
              <div class="pd-price-row">
                <span class="pd-price-main">
                  ${{ formatPrice(displayPriceInfo.current) }}
                  <template v-if="displayPriceInfo.currentMax != null">
                    &nbsp;-&nbsp;${{ formatPrice(displayPriceInfo.currentMax) }}
                  </template>
                </span>
                <span v-if="displayPriceInfo.discountLabel" class="pd-discount-tag">
                  {{ displayPriceInfo.discountLabel }}
                </span>
                <span v-else-if="safeProduct.discountRate !== null" class="pd-discount-tag">
                  {{ safeProduct.discountRate.toFixed(1) }} 折
                </span>
              </div>
              <!-- 原價刪除線（只在有活動折扣時顯示） -->
              <div v-if="displayPriceInfo.original != null" class="pd-price-original-row">
                <span class="pd-price-original">
                  原價 ${{ formatPrice(displayPriceInfo.original) }}
                  <template v-if="displayPriceInfo.originalMax != null">
                    &nbsp;-&nbsp;${{ formatPrice(displayPriceInfo.originalMax) }}
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
                      :src="getFullImageUrl(getSpecOptionImage(spec.name, option))!"
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
                :max="Math.max(1, currentStock || 1)"
                :disabled="isSoldOut || currentStock <= 0 || !allSpecsSelected"
                controls-position="right"
                size="default"
              />
              <span class="pd-stock-hint" :class="{ 'pd-stock-hint--active': !!selectedVariant }">
                庫存 {{ selectedVariant ? selectedVariant.stock : safeProduct.totalStock }} 件
              </span>
            </div>

            <div class="pd-action-buttons">
              <template v-if="isPreview">
                <el-alert type="info" :closable="false" show-icon style="margin-bottom: 0">
                  預覽模式下無法購買。商品上架後，買家將看到『加入購物車』和『立即購買』按鈕。
                </el-alert>
              </template>
              <template v-else>
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
              </template>
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
            
            <!-- 動態屬性 -->
            <div v-for="attr in displayAttributes" :key="attr.id" class="pd-spec-item">
              <span class="spec-key">{{ attr.label }}</span>
              <span class="spec-val">{{ attr.value }}</span>
            </div>
          </div>
        </el-card>

        <!-- 賣家資訊卡 -->
        <el-card v-if="safeProduct.store" class="pd-detail-card pd-store-card" shadow="never">
          <template #header><span class="card-title">賣家資訊</span></template>
          <div class="pd-store-inner">
            <!-- 左側區塊 -->
            <div class="store-left">
              <div class="store-avatar-wrap">
                <el-avatar v-if="safeProduct.storeLogo" :src="getFullImageUrl(safeProduct.storeLogo)" :size="64" />
                <el-avatar v-else :size="64" class="store-avatar-fallback">{{ safeProduct.storeName.charAt(0) }}</el-avatar>
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
                <span class="stat-label">評價</span>
                <span class="stat-value">{{ safeProduct.store?.rating != null ? safeProduct.store.rating.toFixed(1) : '—' }} <span class="stat-unit">/ 5.0</span></span>
              </div>
              <div class="store-stat-item">
                <span class="stat-label">商品</span>
                <span class="stat-value">{{ safeProduct.store?.productCount || 0 }} <span class="stat-unit">件</span></span>
              </div>
              <div class="store-stat-item">
                <span class="stat-label">加入時間</span>
                <span class="stat-value">{{ formatJoinedTime(safeProduct.store?.joinedYearsAgo) }}</span>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 商品描述 -->
        <el-card class="pd-detail-card" shadow="never">
          <template #header><span class="card-title">商品描述</span></template>
          <div class="pd-description" v-html="safeProduct.description"></div>
        </el-card>

        <!-- 商品評價 -->
        <ProductReview v-if="safeProduct.id" :product-id="safeProduct.id" @refresh="loadProduct(safeProduct.id, true)" />

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
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Picture, ChatDotRound, Shop } from '@element-plus/icons-vue'
import ProductCard from '@/components/product/ProductCard.vue'
import ProductReview from '@/components/product/ProductReview.vue'
import { fetchProductDetail, fetchRelatedProducts, fetchProductPromotions, getSellerProductDetail } from '@/api/product'
import { getCategoryAttributes } from '@/api/categoryAttribute'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { formatPrice, formatSoldCount, formatRelativeTime, getFullImageUrl } from '@/utils/format'
import type {
  ProductDetail,
  ProductListItem,
  ProductVariant,
  CategoryPathItem,
  ProductImage,
  PriceRange,
  ProductSpec,
  StoreInfo,
  ProductPromotion,
  SellerProductDetail,
} from '@/types/product'
import type { CategoryAttribute } from '@/api/categoryAttribute'

const props = withDefaults(defineProps<{
  previewId?: number
  isPreview?: boolean
}>(), {
  previewId: undefined,
  isPreview: false,
})

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()
const chatStore = useChatStore()

const product = ref<ProductDetail | null>(null)
const categoryAttributes = ref<CategoryAttribute[]>([])
const productPromotions = ref<ProductPromotion[]>([])

/** 目前選中/最低價格（含活動折扣計算） */
const displayPriceInfo = computed(() => {
  const p = product.value
  const nullResult = { current: 0, currentMax: null as number | null, original: null as number | null, originalMax: null as number | null, discountLabel: null as string | null }
  if (!p) return nullResult

  // 基準價：選了規格用規格價，否則用有庫存的最低/最高價
  const baseMin = selectedVariant.value ? selectedVariant.value.price : (availableMinPrice.value ?? p.priceRange?.min ?? 0)
  const baseMax = selectedVariant.value ? selectedVariant.value.price : (availableMaxPrice.value ?? p.priceRange?.max ?? 0)

  const promo = activePromotion.value
  const hasDiscount =
    promo != null &&
    (promo.type === 'flashSale' || promo.type === 'limitedBuy') &&
    promo.discountPercent != null &&
    promo.discountPercent > 0

  if (hasDiscount) {
    const pct = promo!.discountPercent!
    const discMin = Math.round(baseMin * pct / 100)
    const discMax = Math.round(baseMax * pct / 100)
    return {
      current: discMin,
      currentMax: discMax !== discMin ? discMax : null,
      original: baseMin,
      originalMax: baseMax !== baseMin ? baseMax : null,
      discountLabel: `${pct}折`,
    }
  }

  return {
    current: baseMin,
    currentMax: baseMax !== baseMin ? baseMax : null,
    original: null,
    originalMax: null,
    discountLabel: null,
  }
})

/** 第一個進行中活動（優先 flashSale > limitedBuy > discount） */
const activePromotion = computed<ProductPromotion | null>(() => {
  if (!productPromotions.value.length) return null
  const order = ['flashSale', 'limitedBuy', 'discount', 'other']
  return [...productPromotions.value].sort(
    (a, b) => order.indexOf(a.type) - order.indexOf(b.type)
  )[0] ?? null
})

/** 滿額折扣活動的規則文字 */
const discountRuleText = computed<string>(() => {
  const promo = activePromotion.value
  if (!promo || promo.type !== 'discount' || !promo.rule) return ''
  const r = promo.rule
  if (r.discountType === 1) return `滿 ${r.threshold} 折 ${r.discountValue} 元`
  if (r.discountType === 2) return `滿 ${r.threshold} 打 ${r.discountValue} 折`
  return `滿 ${r.threshold} 享優惠`
})

/** 橫幅顯示的優惠摘要（不含具體價格） */
const promoBannerDesc = computed<string>(() => {
  const promo = activePromotion.value
  if (!promo) return ''
  if (promo.type === 'flashSale') {
    if (promo.discountPercent != null) return `${promo.discountPercent}% OFF`
    return '限時特惠'
  }
  if (promo.type === 'discount') return discountRuleText.value || '滿額享折扣'
  if (promo.type === 'limitedBuy') return promo.quantityLimit ? `每人限購 ${promo.quantityLimit} 件` : '限量搶購中'
  return '活動進行中'
})

function getRemainingDays(endDate: string): number {
  if (!endDate) return 0
  const diff = Math.ceil((new Date(endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
}

/** 活動結束倒數（格式：HH:mm:ss 或 X 天 HH:mm:ss） */
const countdownText = ref('')
let countdownTimer: ReturnType<typeof setInterval> | null = null

function updateCountdown() {
  const promo = activePromotion.value
  if (!promo) { countdownText.value = ''; return }
  const diff = new Date(promo.endDate).getTime() - Date.now()
  if (diff <= 0) { countdownText.value = '已結束'; return }
  const totalSec = Math.floor(diff / 1000)
  const days = Math.floor(totalSec / 86400)
  const hrs  = Math.floor((totalSec % 86400) / 3600).toString().padStart(2, '0')
  const mins = Math.floor((totalSec % 3600) / 60).toString().padStart(2, '0')
  const secs = (totalSec % 60).toString().padStart(2, '0')
  countdownText.value = days > 0 ? `${days} 天 ${hrs}:${mins}:${secs}` : `${hrs}:${mins}:${secs}`
}
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
    storeLogo: p.store?.logoUrl || '',
    totalStock: p.totalStock || 0,
    priceRange: p.priceRange || { min: 0, max: 0 },
    categoryPath: (p.categoryPath || []) as CategoryPathItem[],
    images: (p.images || []) as ProductImage[],
    specs: (p.specs || []) as ProductSpec[],
    variants: (p.variants || []) as ProductVariant[],
    createdAt: p.createdAt ? p.createdAt.substring(0, 10) : '—',
  }
})

/** 解析並翻譯屬性字典 — 修復數字自填消失問題並實作多選合併 */
const displayAttributes = computed(() => {
  const p = product.value
  if (!p || !p.attributesJson) return []

  try {
    const parsed = JSON.parse(p.attributesJson)
    const flatList = parsed.map((attr: any) => {
      const attrId = attr.AttributeId || attr.attributeId
      const optId = attr.OptionId || attr.optionId
      let customVal = attr.CustomValue || attr.customValue || attr.Value || attr.value

      const def = categoryAttributes.value?.find(c => c.id === attrId)
      const labelName = def ? def.name : `屬性#${attrId}`

      let finalValue = customVal

      // 如果沒有明確的自填字串，但有 optId
      if (!finalValue && optId !== null && optId !== undefined) {
        if (def && def.options) {
          // 用 == 弱型別比對，避免數字與字串型別差異
          const opt = def.options.find(o => o.id == optId)
          if (opt) {
            finalValue = opt.value // 找到對應選項，顯示選項文字
          } else {
            // 【關鍵防呆】找不到該選項！代表這個 optId 其實是手填的數字內容
            finalValue = String(optId)
          }
        } else {
          finalValue = String(optId)
        }
      }

      return { id: attrId, label: labelName, value: finalValue }
    }).filter(a => a.value !== null && a.value !== undefined && a.value !== '')

    // 群組化邏輯：將相同 ID 的屬性合併
    const grouped: Record<number, { id: number; label: string; values: string[] }> = {}
    flatList.forEach((item) => {
      const id = item.id
      if (!grouped[id]) {
        grouped[id] = { id, label: item.label, values: [String(item.value)] }
      } else {
        grouped[id].values.push(String(item.value))
      }
    })

    // 將合併後的陣列轉回適合渲染的格式
    return Object.values(grouped).map((g) => ({
      id: g.id,
      label: g.label,
      value: g.values.join('、'),
    }))
  } catch (e) {
    console.error('屬性解析錯誤:', e)
    return []
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

/** 有庫存（stock > 0）的規格 */
const availableVariants = computed(() => {
  if (!isReady.value) return []
  return safeProduct.value.variants.filter(v => v.stock > 0)
})

/** 有庫存規格的最低價；無庫存規格時回退至 priceRange.min */
const availableMinPrice = computed<number>(() => {
  const prices = availableVariants.value.map(v => v.price)
  return prices.length > 0 ? Math.min(...prices) : (safeProduct.value.priceRange.min ?? 0)
})

/** 有庫存規格的最高價；無庫存規格時回退至 priceRange.max */
const availableMaxPrice = computed<number>(() => {
  const prices = availableVariants.value.map(v => v.price)
  return prices.length > 0 ? Math.max(...prices) : (safeProduct.value.priceRange.max ?? 0)
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

/** 套用活動折扣後的實際價格（flashSale / limitedBuy 時打折，其餘原價） */
function getEffectivePrice(originalPrice: number): number {
  const promo = activePromotion.value
  if (
    promo != null &&
    (promo.type === 'flashSale' || promo.type === 'limitedBuy') &&
    promo.discountPercent != null &&
    promo.discountPercent > 0
  ) {
    return Math.round(originalPrice * promo.discountPercent / 100)
  }
  return originalPrice
}

function selectSpec(specName: string, optionValue: string) {
  if (getOptionStatus(specName, optionValue) !== 'available') return
  selectedSpecs.value[specName] = selectedSpecs.value[specName] === optionValue ? null : optionValue
  quantity.value = 1
  if (selectedVariant.value?.imageUrl) activeImageUrl.value = selectedVariant.value.imageUrl
}

/** 把賣家 API 的 SellerProductDetail 轉換成前台 ProductDetail 格式 */
function mapSellerToProductDetail(seller: SellerProductDetail): ProductDetail {
  // 解析規格組合（先解析 variants，以便稍後推導規格選項）
  const variants: ProductVariant[] = seller.variants.map(v => {
    let specValues: Record<string, string> = {}
    if (v.specValueJson) {
      try { specValues = JSON.parse(v.specValueJson) } catch { /* ignore */ }
    }
    return {
      id: v.id,
      specValues,
      price: v.price,
      originalPrice: null,
      stock: v.stock ?? 0,
      imageUrl: null,
    }
  })

  // 解析規格定義；格式為 [{name, options:[{name}|string]}]
  // 若 options 為空，從 variants 的 specValues 反推選項
  let specs: ProductSpec[] = []
  if (seller.specDefinitionJson) {
    try {
      const parsed = JSON.parse(seller.specDefinitionJson) as Array<{
        name: string
        options?: Array<{ name: string } | string>
        values?: Array<{ name: string } | string>
      }>
      if (Array.isArray(parsed) && parsed.length > 0) {
        specs = parsed.map(s => {
          const rawOpts = s.options ?? s.values ?? []
          const optStrings = rawOpts.map(o => (typeof o === 'string' ? o : o.name)).filter(Boolean)
          return { name: s.name, options: optStrings }
        })
      }
    } catch { /* ignore */ }
  }

  // 若規格選項仍為空，從 variants.specValues 反推
  if (specs.length > 0 && specs.every(s => s.options.length === 0)) {
    const optMap = new Map<string, Set<string>>()
    variants.forEach(v => {
      Object.entries(v.specValues).forEach(([name, val]) => {
        if (!optMap.has(name)) optMap.set(name, new Set())
        optMap.get(name)!.add(val)
      })
    })
    specs = specs.map(s => ({
      name: s.name,
      options: Array.from(optMap.get(s.name) ?? []),
    }))
  } else if (specs.length === 0 && variants.length > 0) {
    // 完全沒有 specDefinitionJson，純從 variants 反推
    const specNamesOrder: string[] = []
    const optMap = new Map<string, Set<string>>()
    variants.forEach(v => {
      Object.keys(v.specValues).forEach(name => {
        if (!specNamesOrder.includes(name)) specNamesOrder.push(name)
        if (!optMap.has(name)) optMap.set(name, new Set())
        optMap.get(name)!.add(v.specValues[name] as string)
      })
    })
    specs = specNamesOrder.map(name => ({
      name,
      options: Array.from(optMap.get(name) ?? []),
    }))
  }


  // string[] → ProductImage[]
  const images: ProductImage[] = seller.images.map((url, idx) => ({
    id: idx,
    url,
    isMain: idx === 0,
    sortOrder: idx,
  }))

  const prices = seller.variants.map(v => v.price)
  const minPrice = seller.minPrice ?? (prices.length ? Math.min(...prices) : 0)
  const maxPrice = seller.maxPrice ?? (prices.length ? Math.max(...prices) : 0)

  return {
    id: seller.id,
    name: seller.name,
    description: seller.description,
    categoryId: seller.categoryId,
    categoryPath: seller.categoryName
      ? [{ id: seller.categoryId, name: seller.categoryName }]
      : null,
    brand: seller.brandId ? { id: seller.brandId, name: seller.brandName ?? '', logoUrl: null } : null,
    store: {
      id: seller.storeId,
      userId: 0,
      name: seller.storeName ?? '',
      status: 1,
      logoUrl: null,
      rating: null,
      productCount: 0,
      followerCount: null,
      location: null,
      responseRate: null,
      joinedYearsAgo: 0,
    },
    images,
    priceRange: { min: minPrice, max: maxPrice },
    originalPriceRange: null,
    discountRate: null,
    specs,
    variants,
    totalStock: seller.variants.reduce((sum, v) => sum + (v.stock ?? 0), 0),
    soldCount: 0,
    rating: null,
    reviewCount: null,
    isOnShelf: seller.status === 1,
    createdAt: seller.createdAt,
    attributesJson: null,
  }
}

async function loadProduct(id: number, silent = false) {
  if (!silent) loading.value = true
  productPromotions.value = []
  if (countdownTimer) { clearInterval(countdownTimer); countdownTimer = null }
  try {
    if (props.isPreview) {
      // 預覽模式：呼叫賣家 API（不過濾狀態）
      const seller = await getSellerProductDetail(id)
      product.value = mapSellerToProductDetail(seller)
      const mainImg = product.value.images?.[0]
      activeImageUrl.value = mainImg?.url || ''
      product.value.specs?.forEach(s => selectedSpecs.value[s.name] = null)

      try {
        const attrRes = await getCategoryAttributes(seller.categoryId)
        if (attrRes.success) categoryAttributes.value = attrRes.data
      } catch (e) {
        console.error('載入屬性定義失敗:', e)
      }
      // 預覽模式跳過促銷與相關商品
    } else {
      const res = await fetchProductDetail(id)
      if (res.success) {
        product.value = res.data
        const mainImg = res.data.images?.find(img => img.isMain) || res.data.images?.[0]
        activeImageUrl.value = mainImg?.url || ''
        res.data.specs?.forEach(s => selectedSpecs.value[s.name] = null)

        try {
          const attrRes = await getCategoryAttributes(res.data.categoryId)
          if (attrRes.success) categoryAttributes.value = attrRes.data
        } catch (e) {
          console.error('載入屬性定義失敗:', e)
        }

        try {
          const promoRes = await fetchProductPromotions(id)
          if (promoRes.success) {
            productPromotions.value = promoRes.data
            if (promoRes.data.length) {
              updateCountdown()
              countdownTimer = setInterval(updateCountdown, 1000)
            }
          }
        } catch (e) {
          console.error('載入活動資訊失敗:', e)
        }

        void loadRelated(id)
      }
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
  const price = getEffectivePrice(variant ? variant.price : availableMinPrice.value)
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
  const price = getEffectivePrice(variant ? variant.price : availableMinPrice.value)
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
    storeStatus: p.store?.status ?? 1,
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
    authStore.openLoginDialog()
    return
  }
  const store = safeProduct.value.store
  if (store) {
    chatStore.openChatWithUser(store.userId || 1, store.name || '賣家', store.logoUrl || '')
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
  const id = props.previewId || Number(route.params.id)
  if (id) loadProduct(id)
})

onUnmounted(() => {
  if (countdownTimer) clearInterval(countdownTimer)
})

watch(() => route.params.id, (newId) => {
  if (!props.isPreview && newId) { window.scrollTo({ top: 0, behavior: 'smooth' }); loadProduct(Number(newId)) }
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
.pd-price-row { display: flex; align-items: baseline; gap: 12px; }
.pd-price-original-row { margin-top: 4px; }
.pd-price-original { font-size: 14px; color: #94a3b8; text-decoration: line-through; }
.pd-price-main { font-size: 30px; font-weight: 700; color: #EE4D2D; line-height: 1; }
.pd-discount-tag { background: #EE4D2D; color: #fff; font-size: 12px; font-weight: 600; padding: 2px 8px; border-radius: 2px; }
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

/* 促銷活動橫幅（麵包屑下方全寬條） */
.promo-banner-strip {
  border-radius: 4px;
  padding: 10px 16px;
  margin-bottom: 12px;
}
.promo-banner--flashSale  { background: linear-gradient(90deg, #c2141e 0%, #ef4444 100%); }
.promo-banner--discount   { background: linear-gradient(90deg, #c2510c 0%, #f97316 100%); }
.promo-banner--limitedBuy { background: linear-gradient(90deg, #7f1d1d 0%, #dc2626 100%); }
.promo-banner--other      { background: linear-gradient(90deg, #374151 0%, #6b7280 100%); }
.promo-banner-content {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  color: #fff;
  font-size: 14px;
}
.promo-banner-tag {
  background: rgba(255, 255, 255, 0.22);
  border-radius: 3px;
  padding: 2px 8px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.5px;
  white-space: nowrap;
  flex-shrink: 0;
}
.promo-banner-title { font-weight: 700; }
.promo-banner-sep { color: rgba(255, 255, 255, 0.55); }
.promo-banner-desc { font-weight: 600; }
.promo-banner-countdown {
  font-variant-numeric: tabular-nums;
  font-weight: 700;
  letter-spacing: 0.5px;
}
.promo-banner-link {
  margin-left: auto;
  color: rgba(255, 255, 255, 0.9);
  text-decoration: none;
  font-size: 13px;
  white-space: nowrap;
  border: 1px solid rgba(255, 255, 255, 0.45);
  border-radius: 3px;
  padding: 3px 10px;
  transition: background 0.2s;
  flex-shrink: 0;
}
.promo-banner-link:hover { background: rgba(255, 255, 255, 0.18); }
.pd-description { font-size: 14px; color: #334155; line-height: 1.8; margin: 0; font-family: inherit; }
.pd-description :deep(img) { max-width: 100%; height: auto; display: block; margin: 8px 0; }
.pd-related-section { margin-top: 24px; }
.related-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 0 0 16px; padding-bottom: 12px; border-bottom: 2px solid #EE4D2D; display: inline-block; }
.related-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 12px; }
</style>
