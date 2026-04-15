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

    <!-- ── 正常內容：isReady 是最後一道防線 ── -->
    <template v-else-if="isReady">
      <div class="pd-container">

        <!-- 麵包屑 -->
        <el-breadcrumb separator="/" class="pd-breadcrumb">
          <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
          <!-- 主分類（可點，跳回首頁並套用主分類篩選） -->
          <el-breadcrumb-item
            v-if="mainCategoryItem"
            :to="{ path: '/', query: { categoryId: String(mainCategoryItem.id) } }"
          >{{ mainCategoryItem.name }}</el-breadcrumb-item>
          <!-- 子分類（可點，跳回首頁並套用主+子分類篩選） -->
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
          <!-- 商品名稱（純文字，不可點） -->
          <el-breadcrumb-item class="pd-breadcrumb-product">{{ safeProduct.name }}</el-breadcrumb-item>
        </el-breadcrumb>

        <!-- 主區塊：左圖 + 右資訊 -->
        <div class="pd-main-section">

          <!-- 左側圖片區 -->
          <div class="pd-gallery">
            <div class="pd-main-image-wrap">
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
            </div>

            <!-- 縮圖列（超過 1 張才顯示） -->
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

            <!-- (a) 商品名稱 -->
            <h1 class="pd-name">{{ safeProduct.name }}</h1>

            <!-- (b) 評分列 -->
            <div class="pd-rating-row">
              <template v-if="safeProduct.rating !== null">
                <el-rate :model-value="safeProduct.rating" disabled show-score />
                <span class="pd-review-count">{{ safeProduct.reviewCount }} 評價</span>
              </template>
              <template v-else>
                <span class="pd-no-rating">暫無評價</span>
                <span class="pd-review-count">{{ safeProduct.soldCount.toLocaleString() }} 已售</span>
              </template>
            </div>

            <!-- (c) 價格區塊 -->
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
              <!-- 原價刪除線 -->
              <div
                v-if="safeProduct.originalPriceRange"
                class="pd-original-price"
              >
                原價：
                <span class="pd-strikethrough">
                  ${{ formatPrice(safeProduct.originalPriceRange.min) }}
                  <template v-if="safeProduct.originalPriceRange.max !== safeProduct.originalPriceRange.min">
                    &nbsp;-&nbsp;${{ formatPrice(safeProduct.originalPriceRange.max) }}
                  </template>
                </span>
              </div>
            </div>

            <!-- (d) 規格選擇器（有 specs 且有 variants 才顯示） -->
            <div
              v-if="safeProduct.specs.length > 0 && safeProduct.variants.length > 0"
              class="pd-spec-selector"
            >
              <div
                v-for="spec in safeProduct.specs"
                :key="spec.name"
                class="pd-spec-row"
              >
                <span class="pd-spec-label">{{ spec.name }}</span>
                <div class="pd-spec-options">
                  <button
                    v-for="option in spec.options"
                    :key="option"
                    class="pd-spec-btn"
                    :class="{
                      selected: selectedSpecs[spec.name] === option,
                      unavailable: getOptionStatus(spec.name, option) !== 'available',
                    }"
                    :disabled="getOptionStatus(spec.name, option) !== 'available'"
                    @click="selectSpec(spec.name, option)"
                  >
                    <el-icon v-if="selectedSpecs[spec.name] === option" class="spec-check"><Check /></el-icon>
                    {{ option }}
                  </button>
                </div>
              </div>
            </div>

            <!-- (e) 數量選擇器 -->
            <div class="pd-quantity-row">
              <span class="pd-quantity-label">數量</span>
              <el-input-number
                v-model="quantity"
                :min="1"
                :max="currentStock"
                :disabled="!allSpecsSelected"
                controls-position="right"
                size="default"
              />
              <span class="pd-stock-hint" :class="{ 'pd-stock-hint--active': !!selectedVariant }">
                <template v-if="selectedVariant">
                  庫存 {{ selectedVariant.stock }} 件
                </template>
                <template v-else>
                  共 {{ safeProduct.totalStock }} 件
                </template>
              </span>
            </div>

            <!-- (f) 按鈕 -->
            <div class="pd-action-buttons">
              <el-tooltip
                :content="needsSpecSelection ? '請選擇規格' : ''"
                :disabled="!needsSpecSelection"
                placement="top"
              >
                <span class="btn-wrapper">
                  <el-button
                    class="btn-cart"
                    :disabled="!canAddToCart"
                    @click="handleAddToCart"
                  >加入購物車</el-button>
                </span>
              </el-tooltip>
              <el-tooltip
                :content="needsSpecSelection ? '請選擇規格' : ''"
                :disabled="!needsSpecSelection"
                placement="top"
              >
                <span class="btn-wrapper">
                  <el-button
                    type="primary"
                    class="btn-buy"
                    :disabled="!canAddToCart"
                    @click="handleBuyNow"
                  >直接購買</el-button>
                </span>
              </el-tooltip>
            </div>

          </div><!-- /pd-info -->
        </div><!-- /pd-main-section -->

        <!-- 商品規格卡 -->
        <el-card class="pd-detail-card" shadow="never">
          <template #header><span class="card-title">商品規格</span></template>
          <div class="pd-spec-table">
            <div class="pd-spec-item">
              <span class="spec-key">分類</span>
              <span class="spec-val">
                <template v-if="safeProduct.categoryPath.length > 0">
                  <span
                    v-for="(cat, idx) in safeProduct.categoryPath"
                    :key="cat.id"
                  >{{ cat.name }}<span v-if="idx < safeProduct.categoryPath.length - 1"> &gt; </span></span>
                </template>
                <template v-else>—</template>
              </span>
            </div>
            <div class="pd-spec-item">
              <span class="spec-key">品牌</span>
              <span class="spec-val">{{ safeProduct.brandName }}</span>
            </div>
            <div class="pd-spec-item">
              <span class="spec-key">店家</span>
              <span class="spec-val">{{ safeProduct.storeName }}</span>
            </div>
            <div class="pd-spec-item">
              <span class="spec-key">庫存</span>
              <span class="spec-val">{{ safeProduct.totalStock }}</span>
            </div>
            <div class="pd-spec-item">
              <span class="spec-key">上架日</span>
              <span class="spec-val">{{ safeProduct.createdAt }}</span>
            </div>
          </div>
        </el-card>

        <!-- 賣家資訊卡（store 為 null 時整卡隱藏） -->
        <el-card v-if="safeProduct.store" class="pd-detail-card pd-store-card" shadow="never">
          <template #header><span class="card-title">賣家資訊</span></template>
          <div class="pd-store-inner">
            <div class="store-avatar-wrap">
              <el-avatar
                v-if="safeProduct.store.logoUrl"
                :src="safeProduct.store.logoUrl"
                :size="72"
              />
              <el-avatar v-else :size="72" class="store-avatar-fallback">
                {{ (safeProduct.store.name ?? '?').charAt(0) }}
              </el-avatar>
            </div>
            <div class="store-meta">
              <div class="store-name">{{ safeProduct.store.name ?? '—' }}</div>
              <div class="store-stats">
                <span>評分 {{ safeProduct.store.rating !== null ? safeProduct.store.rating.toFixed(1) : '—' }}</span>
                <el-divider direction="vertical" />
                <span>商品 {{ safeProduct.store.productCount ?? 0 }} 件</span>
                <el-divider direction="vertical" />
                <span>粉絲 {{ safeProduct.store.followerCount ?? 0 }}</span>
                <el-divider direction="vertical" />
                <span>{{ (safeProduct.store.joinedYearsAgo ?? 0) === 0 ? '新加入' : `加入 ${safeProduct.store.joinedYearsAgo} 年` }}</span>
              </div>
              <div v-if="safeProduct.store.location" class="store-location">
                {{ safeProduct.store.location }}
              </div>
            </div>
            <div class="store-action">
              <el-button @click="handleViewStore">查看賣場</el-button>
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
            <el-skeleton v-for="n in 6" :key="n" animated>
              <template #template>
                <el-skeleton-item variant="image" style="width: 100%; aspect-ratio: 1/1; border-radius: 4px" />
                <el-skeleton :rows="2" style="margin-top: 8px" />
              </template>
            </el-skeleton>
          </div>
          <div v-else class="related-grid">
            <ProductCard
              v-for="p in relatedProducts"
              :key="p.id"
              :product="p"
            />
          </div>
        </div>

      </div><!-- /pd-container -->
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Picture, Check } from '@element-plus/icons-vue'
import ProductCard from '@/components/product/ProductCard.vue'
import { fetchProductDetail, fetchRelatedProducts } from '@/api/product'
import { useCartStore } from '@/stores/cart'
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

// ─── 路由 ───────────────────────────────────────────────────────
const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()

// ─── 狀態 ───────────────────────────────────────────────────────
const product = ref<ProductDetail | null>(null)
const loading = ref(false)
const loadError = ref<string | null>(null)
const relatedProducts = ref<ProductListItem[]>([])
const relatedLoading = ref(false)

const activeImageUrl = ref<string>('')
const quantity = ref(1)

/** Record<規格名, 選中值|null> */
const selectedSpecs = ref<Record<string, string | null>>({})

const fallbackImage =
  'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII='

// ─── 全域防呆：isReady 是進入正常渲染的最後一道門 ────────────────

/** product 非 null 且載入結束才算 ready */
const isReady = computed<boolean>(() => !loading.value && product.value !== null)

/**
 * safeProduct：把所有 nullable 欄位收攏到一個地方補 fallback，
 * template 只讀 safeProduct，不再散落讀 product.value。
 * 只在 isReady = true 時才會被 template 存取，故 as non-null 安全。
 */
const safeProduct = computed(() => {
  const p = product.value!
  return {
    id:                p.id,
    name:              p.name               ?? '（無名稱）',
    description:       p.description        ?? '賣家尚未提供商品描述',
    categoryPath:      (p.categoryPath       ?? []) as CategoryPathItem[],
    brand:             p.brand,
    brandName:         p.brand?.name        ?? '—',
    store:             p.store              as StoreInfo | null,
    storeName:         p.store?.name        ?? '—',
    images:            (p.images            ?? []) as ProductImage[],
    priceRange:        (p.priceRange        ?? { min: 0, max: 0 }) as PriceRange,
    originalPriceRange: p.originalPriceRange ?? null,
    discountRate:      p.discountRate        ?? null,
    specs:             (p.specs             ?? []) as ProductSpec[],
    variants:          (p.variants          ?? []) as ProductVariant[],
    totalStock:        p.totalStock          ?? 0,
    soldCount:         p.soldCount           ?? 0,
    rating:            p.rating,
    reviewCount:       p.reviewCount         ?? 0,
    createdAt:         p.createdAt ? p.createdAt.substring(0, 10) : '—',
  }
})

// ─── 麵包屑分類節點（noUncheckedIndexedAccess 安全取用） ──────────

const mainCategoryItem = computed(() => {
  if (!isReady.value) return null
  return safeProduct.value.categoryPath[0] ?? null
})

const subCategoryItem = computed(() => {
  if (!isReady.value) return null
  return safeProduct.value.categoryPath[1] ?? null
})

// ─── 計算屬性 ────────────────────────────────────────────────────

/** 所有規格軸都已選齊（無規格商品視為「已選齊」） */
const allSpecsSelected = computed<boolean>(() => {
  if (!isReady.value) return false
  const specs = safeProduct.value.specs
  if (specs.length === 0) return true
  return specs.every((s) => selectedSpecs.value[s.name] !== null)
})

/** 當前選中的 variant（所有軸選齊後才有值） */
const selectedVariant = computed<ProductVariant | null>(() => {
  if (!isReady.value || !allSpecsSelected.value) return null
  const { specs, variants } = safeProduct.value
  if (specs.length === 0) return null
  return (
    variants.find((v) =>
      specs.every((s) => v.specValues[s.name] === selectedSpecs.value[s.name]),
    ) ?? null
  )
})

/** tooltip 需要顯示「請選擇規格」的情況 */
const needsSpecSelection = computed<boolean>(() => {
  if (!isReady.value) return false
  return safeProduct.value.specs.length > 0 && !allSpecsSelected.value
})

/** 按鈕是否可按：無規格商品直接可按；有規格商品需選齊且庫存 > 0 */
const canAddToCart = computed<boolean>(() => {
  if (!isReady.value) return false
  const { specs, totalStock } = safeProduct.value
  if (specs.length === 0) return totalStock > 0
  if (!allSpecsSelected.value) return false
  return (selectedVariant.value?.stock ?? 0) > 0
})

/** 數量選擇上限 */
const currentStock = computed<number>(() => {
  if (!isReady.value) return 1
  const { specs, totalStock } = safeProduct.value
  if (specs.length === 0) return totalStock || 1
  return selectedVariant.value?.stock ?? 1
})

// ─── 規格選擇器邏輯 ──────────────────────────────────────────────

type OptionStatus = 'available' | 'soldOut' | 'unavailable'

/**
 * 判斷一個規格選項的狀態。
 *
 * 先用 filter 找出「符合 (specName, optionValue) 且其他已選軸都相符」
 * 的所有 variants（不管庫存），再判斷：
 *   - 無符合 variants → 'unavailable'（組合不存在）
 *   - 有符合 variants 且至少一個 stock > 0 → 'available'
 *   - 有符合 variants 但全部 stock = 0 → 'soldOut'（缺貨）
 *
 * 用 filter 而非 find 是關鍵：find 只看第一筆，若第一筆剛好缺貨
 * 但其他筆有庫存，會錯誤地回傳缺貨。
 */
function getOptionStatus(specName: string, optionValue: string): OptionStatus {
  if (!isReady.value) return 'unavailable'

  const matching = safeProduct.value.variants.filter((v) => {
    if (v.specValues[specName] !== optionValue) return false
    for (const [name, value] of Object.entries(selectedSpecs.value)) {
      if (name === specName) continue   // 跳過自己這軸
      if (value === null) continue      // 未選的軸不篩選
      if (v.specValues[name] !== value) return false
    }
    return true
  })

  if (matching.length === 0) return 'unavailable'
  if (matching.some((v) => v.stock > 0)) return 'available'
  return 'soldOut'
}

function selectSpec(specName: string, optionValue: string): void {
  // 非 available（包含 soldOut 和 unavailable）一律擋住
  if (getOptionStatus(specName, optionValue) !== 'available') return

  if (selectedSpecs.value[specName] === optionValue) {
    // 再次點擊 → 取消選擇
    selectedSpecs.value[specName] = null
  } else {
    selectedSpecs.value[specName] = optionValue
    // 切換後清掉已失效的其他軸選擇
    for (const s of safeProduct.value.specs) {
      if (s.name === specName) continue
      const cur = selectedSpecs.value[s.name]
      if (cur != null && getOptionStatus(s.name, cur) !== 'available') {
        selectedSpecs.value[s.name] = null
      }
    }
  }

  quantity.value = 1

  if (selectedVariant.value?.imageUrl) {
    activeImageUrl.value = selectedVariant.value.imageUrl
  }
}

// ─── 初始化規格（只在 product watch 後才執行） ───────────────────

watch(
  () => product.value,
  (p) => {
    if (!p) return
    const init: Record<string, string | null> = {}
    for (const s of (p.specs ?? [])) {
      init[s.name] = null
    }
    selectedSpecs.value = init
  },
  { immediate: false },
)

// ─── 資料載入 ────────────────────────────────────────────────────

async function loadProduct(id: number): Promise<void> {
  loading.value = true
  loadError.value = null
  product.value = null
  quantity.value = 1
  relatedProducts.value = []

  try {
    const res = await fetchProductDetail(id)
    if (!res.success) {
      loadError.value = res.message || '商品不存在或已下架'
      product.value = null
      return
    }
    // 設定主圖（在 product 賦值前計算，避免 watch 觸發時資料還沒好）
    const imgs = res.data.images ?? []
    const mainImg = imgs.find((img) => img.isMain) ?? imgs[0]
    activeImageUrl.value = mainImg?.url ?? ''

    product.value = res.data  // ← 這裡觸發 watch，initSpecs 自動執行

    void loadRelated(id)
  } catch (err: unknown) {
    console.error('[ProductDetail] loadProduct failed:', err)
    product.value = null
    const axiosErr = err as { response?: { status?: number } }
    if (axiosErr?.response?.status === 404) {
      loadError.value = '商品不存在或已下架'
    } else {
      loadError.value = '載入商品失敗，請稍後再試'
    }
  } finally {
    loading.value = false
  }
}

async function loadRelated(id: number): Promise<void> {
  relatedLoading.value = true
  try {
    const res = await fetchRelatedProducts(id, 12)
    relatedProducts.value = Array.isArray(res.data) ? res.data : []
  } catch (err: unknown) {
    console.error('[ProductDetail] loadRelated failed:', err)
    relatedProducts.value = []
  } finally {
    relatedLoading.value = false
  }
}

// ─── 生命週期 ────────────────────────────────────────────────────

onMounted(() => {
  const id = Number(route.params['id'])
  if (id) void loadProduct(id)
})

watch(
  () => route.params['id'],
  (newId) => {
    if (newId) {
      window.scrollTo({ top: 0, behavior: 'smooth' })
      void loadProduct(Number(newId))
    }
  },
)

// ─── 按鈕行為 ────────────────────────────────────────────────────

function handleAddToCart(): void {
  const p = safeProduct.value
  const variant = selectedVariant.value
  const image = activeImageUrl.value || p.images[0]?.url || ''
  const price = variant ? variant.price : p.priceRange.min
  const variantId = variant?.id ?? null
  const specLabel = variant
    ? Object.entries(variant.specValues).map(([k, v]) => `${k}: ${v}`).join('、')
    : ''

  cartStore.addItem({
    productId: p.id,
    variantId,
    name: p.name,
    image,
    price,
    quantity: quantity.value,
    specLabel,
  })
  ElMessage.success('已加入購物車')
}

function handleBuyNow(): void {
  ElMessage.info('即將前往結帳')
}

function handleViewStore(): void {
  console.log('查看賣場：', product.value?.store)
}

// ─── 工具函式 ────────────────────────────────────────────────────

function formatPrice(price: number): string {
  return price.toLocaleString('zh-TW')
}
</script>

<style scoped>
/* 整頁容器 */
.pd-page {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 16px 0 48px;
}

.pd-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 16px;
}

/* 麵包屑 */
.pd-breadcrumb {
  margin-bottom: 16px;
  font-size: 12px;
}

/* 麵包屑可點連結（el-breadcrumb-item 加了 :to 才會有 <a>） */
.pd-breadcrumb :deep(.el-breadcrumb__inner a) {
  color: #475569;
  font-weight: normal;
  text-decoration: none;
  transition: color 0.15s;
}
.pd-breadcrumb :deep(.el-breadcrumb__inner a:hover) {
  color: #EE4D2D;
  text-decoration: underline;
}

/* 商品名稱（最後一節，純文字不可點） */
.pd-breadcrumb-product :deep(.el-breadcrumb__inner) {
  color: #94a3b8 !important;
  cursor: default;
  font-weight: normal;
}

/* ── 主區塊：左右兩欄 ── */
.pd-main-section {
  display: flex;
  gap: 32px;
  background: #fff;
  border-radius: 4px;
  padding: 24px;
  margin-bottom: 16px;
}

/* 左側圖片 */
.pd-gallery {
  flex: 0 0 45%;
  min-width: 0;
}

.pd-main-image-wrap {
  width: 100%;
  aspect-ratio: 1/1;
  background: #f8fafc;
  border-radius: 4px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pd-main-image {
  width: 100%;
  height: 100%;
}

.image-error-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  color: #cbd5e1;
}

.pd-thumbnails {
  margin-top: 12px;
}

.thumb-scroll {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 4px;
  scrollbar-width: thin;
}

.pd-thumb {
  flex: 0 0 68px;
  width: 68px;
  height: 68px;
  border: 2px solid #e2e8f0;
  border-radius: 4px;
  overflow: hidden;
  cursor: pointer;
  transition: border-color 0.2s;
}

.pd-thumb:hover,
.pd-thumb.active {
  border-color: #EE4D2D;
}

.thumb-img {
  width: 100%;
  height: 100%;
}

/* 右側資訊 */
.pd-info {
  flex: 1;
  min-width: 0;
}

.pd-name {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  line-height: 1.4;
  margin: 0 0 12px;
}

/* 評分列 */
.pd-rating-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid #f1f5f9;
}

.pd-review-count {
  font-size: 13px;
  color: #64748b;
}

.pd-no-rating {
  font-size: 13px;
  color: #94a3b8;
}

/* 價格區塊 */
.pd-price-block {
  background: #fffbf8;
  border-radius: 4px;
  padding: 16px;
  margin-bottom: 20px;
}

.pd-price-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.pd-price-main {
  font-size: 30px;
  font-weight: 700;
  color: #EE4D2D;
  line-height: 1;
}

.pd-discount-tag {
  background: #EE4D2D;
  color: #fff;
  font-size: 12px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 2px;
}

.pd-original-price {
  margin-top: 8px;
  font-size: 13px;
  color: #94a3b8;
}

.pd-strikethrough {
  text-decoration: line-through;
}

/* 規格選擇器 */
.pd-spec-selector {
  margin-bottom: 20px;
}

.pd-spec-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 14px;
}

.pd-spec-label {
  flex: 0 0 52px;
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  padding-top: 6px;
}

.pd-spec-options {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.pd-spec-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 5px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 4px;
  background: #fff;
  font-size: 13px;
  color: #334155;
  cursor: pointer;
  transition: border-color 0.2s, color 0.2s, background 0.2s;
  position: relative;
  white-space: nowrap;
}

.pd-spec-btn:hover:not(:disabled):not(.unavailable) {
  border-color: #EE4D2D;
  color: #EE4D2D;
}

.pd-spec-btn.selected {
  border-color: #EE4D2D;
  color: #EE4D2D;
  background: #fff5f3;
}

.pd-spec-btn.unavailable,
.pd-spec-btn:disabled {
  border-color: #e2e8f0;
  color: #cbd5e1;
  background: #f8fafc;
  cursor: not-allowed;
  pointer-events: none;
}

.spec-check {
  font-size: 12px;
  color: #EE4D2D;
}

/* 數量選擇 */
.pd-quantity-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.pd-quantity-label {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
}

.pd-stock-hint {
  font-size: 13px;
  color: #94a3b8;
}

.pd-stock-hint--active {
  color: #334155;
  font-weight: 500;
}

/* 按鈕 */
.pd-action-buttons {
  display: flex;
  gap: 12px;
}

.btn-wrapper {
  display: inline-block;
}

.btn-cart {
  color: #EE4D2D !important;
  border-color: #EE4D2D !important;
  background: #fff !important;
  min-width: 160px;
  height: 44px;
  font-size: 15px;
  font-weight: 600;
}

.btn-cart:hover:not(:disabled) {
  background: #fff5f3 !important;
}

.btn-buy {
  background: #EE4D2D !important;
  border-color: #EE4D2D !important;
  min-width: 160px;
  height: 44px;
  font-size: 15px;
  font-weight: 600;
}

.btn-buy:hover:not(:disabled) {
  background: #d94424 !important;
  border-color: #d94424 !important;
}

/* ── 卡片通用 ── */
.pd-detail-card {
  margin-bottom: 16px;
  border-radius: 4px;
}

.card-title {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
}

/* 商品規格表格 */
.pd-spec-table {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px 32px;
}

.pd-spec-item {
  display: flex;
  gap: 12px;
}

.spec-key {
  flex: 0 0 60px;
  font-size: 13px;
  color: #94a3b8;
}

.spec-val {
  font-size: 13px;
  color: #334155;
}

/* 賣家資訊 */
.pd-store-inner {
  display: flex;
  align-items: center;
  gap: 20px;
}

.store-avatar-fallback {
  background: #EE4D2D;
  color: #fff;
  font-size: 28px;
  font-weight: 700;
}

.store-meta {
  flex: 1;
}

.store-name {
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 8px;
}

.store-stats {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  color: #64748b;
  flex-wrap: wrap;
}

.store-location {
  margin-top: 6px;
  font-size: 13px;
  color: #94a3b8;
}

/* 商品描述 */
.pd-description {
  white-space: pre-wrap;
  font-size: 14px;
  color: #334155;
  line-height: 1.8;
  margin: 0;
  font-family: inherit;
}

/* 相關商品 */
.pd-related-section {
  margin-top: 24px;
}

.related-title {
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 16px;
  padding-bottom: 12px;
  border-bottom: 2px solid #EE4D2D;
  display: inline-block;
}

.related-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
}

.related-skeleton-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
}

.related-empty {
  text-align: center;
  color: #94a3b8;
  padding: 32px 0;
}

/* ── 響應式 ── */
@media (max-width: 768px) {
  .pd-main-section {
    flex-direction: column;
    gap: 20px;
  }

  .pd-gallery {
    flex: none;
    width: 100%;
  }

  .pd-spec-table {
    grid-template-columns: 1fr;
  }

  .pd-store-inner {
    flex-wrap: wrap;
  }

  .related-grid,
  .related-skeleton-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .pd-action-buttons {
    flex-direction: column;
  }

  .btn-cart,
  .btn-buy {
    width: 100%;
  }
}

@media (min-width: 769px) and (max-width: 1024px) {
  .related-grid,
  .related-skeleton-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}
</style>
