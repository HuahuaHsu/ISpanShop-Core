<template>
  <div class="home">
    <!-- ── 輪播 + 右側 banner ── -->
    <section class="banner-section">
      <!-- 真實活動資料（API 有回傳時） -->
      <template v-if="promotions.length > 0">
        <div class="main-carousel">
          <el-carousel height="320px" arrow="always">
            <el-carousel-item v-for="promo in promotions" :key="promo.id">
              <div
                class="carousel-slide promo-slide"
                :style="promo.bannerImageUrl
                  ? { backgroundImage: `url(${promo.bannerImageUrl})`, backgroundSize: 'cover', backgroundPosition: 'center' }
                  : { background: 'linear-gradient(135deg, #1e293b 0%, #1e1b4b 100%)' }"
                @click="promo.linkUrl ? $router.push(promo.linkUrl) : undefined"
              >
                <div class="slide-content">
                  <div class="slide-tag">{{ promo.typeLabel }}</div>
                  <h2>{{ promo.title }}</h2>
                  <p v-if="promo.subtitle">{{ promo.subtitle }}</p>
                  <el-button type="primary" round size="large">立即搶購</el-button>
                </div>
                <div v-if="!promo.bannerImageUrl" class="slide-emoji">🎉</div>
              </div>
            </el-carousel-item>
          </el-carousel>
        </div>
        <div class="side-banners">
          <div
            v-for="promo in promotions.slice(1, 3)"
            :key="promo.id"
            class="side-banner"
            :style="promo.bannerImageUrl
              ? { backgroundImage: `url(${promo.bannerImageUrl})`, backgroundSize: 'cover', backgroundPosition: 'center' }
              : { background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)' }"
            @click="promo.linkUrl ? $router.push(promo.linkUrl) : undefined"
          >
            <div class="sb-tag">{{ promo.typeLabel }}</div>
            <h3>{{ promo.title }}</h3>
            <p v-if="promo.subtitle">{{ promo.subtitle }}</p>
            <span v-if="!promo.bannerImageUrl" class="sb-emoji">🎁</span>
          </div>
          <div
            v-for="n in Math.max(0, 2 - (promotions.length - 1))"
            :key="`placeholder-${n}`"
            class="side-banner side-banner-empty"
          />
        </div>
      </template>

      <!-- 靜態預設內容（API 空時顯示） -->
      <template v-else>
        <div class="main-carousel">
          <el-carousel height="320px" arrow="always">
            <el-carousel-item v-for="(banner, i) in staticBanners" :key="i">
              <div class="carousel-slide" :style="{ background: banner.bg }">
                <div class="slide-content">
                  <div class="slide-tag">{{ banner.tag }}</div>
                  <h2>{{ banner.title }}</h2>
                  <p>{{ banner.subtitle }}</p>
                  <el-button type="primary" round size="large">立即搶購</el-button>
                </div>
                <div class="slide-emoji">{{ banner.emoji }}</div>
              </div>
            </el-carousel-item>
          </el-carousel>
        </div>
        <div class="side-banners">
          <div class="side-banner" v-for="sb in staticSideBanners" :key="sb.title" :style="{ background: sb.bg }">
            <div class="sb-tag">{{ sb.tag }}</div>
            <h3>{{ sb.title }}</h3>
            <p>{{ sb.desc }}</p>
            <span class="sb-emoji">{{ sb.emoji }}</span>
          </div>
        </div>
      </template>
    </section>

    <!-- ── 圓形快捷服務 ── -->
    <section class="quick-icons">
      <div v-for="item in quickItems" :key="item.label" class="quick-item" @click="item.url ? $router.push(item.url) : undefined">
        <div class="quick-circle">{{ item.icon }}</div>
        <div class="quick-label">{{ item.label }}</div>
      </div>
    </section>

    <!-- ── 商品分類網格 ── -->
    <section class="category-section">
      <div class="section-title">分類</div>

      <div v-if="categoriesLoading" class="category-grid">
        <el-skeleton v-for="n in 7" :key="n" animated class="cat-skeleton">
          <template #template>
            <el-skeleton-item variant="circle" style="width: 70px; height: 70px; margin: 0 auto 10px;" />
            <el-skeleton-item variant="p" style="width: 70%; margin: 0 auto 4px;" />
          </template>
        </el-skeleton>
      </div>

      <div v-else-if="apiCategories.length > 0" class="category-grid">
        <div
          v-for="cat in apiCategories"
          :key="cat.id"
          class="category-item"
          :class="{ 'category-item--active': selectedCategoryId === cat.id }"
          @click="onCategoryClick(cat.id)"
        >
          <div class="cat-image">
            <el-image
              v-if="cat.iconUrl"
              :src="cat.iconUrl"
              fit="cover"
              style="width: 100%; height: 100%; border-radius: 50%;"
            />
            <span v-else class="cat-emoji">{{ getCategoryIcon(cat.name) }}</span>
          </div>
          <div class="cat-name">{{ cat.name }}</div>
        </div>
      </div>

      <el-empty v-else description="目前沒有分類" :image-size="80" />
    </section>

    <!-- ── 每日新發現 ── -->
    <section ref="sectionRef" class="products-section">
      <div class="discovery-header">
        <h2 class="discovery-title">每日新發現</h2>
        <el-button
          v-if="selectedCategoryId !== null"
          size="small"
          plain
          style="margin-left: 12px; vertical-align: middle;"
          @click="onClearCategory"
        >顯示全部</el-button>
      </div>

      <!-- 手機：篩選按鈕 + 抽屜（選了分類且手機才顯示） -->
      <template v-if="selectedCategoryId !== null && isMobile">
        <div class="mobile-filter-bar">
          <el-button size="small" @click="drawerOpen = true">
            <el-icon><FilterIcon /></el-icon> 篩選條件
          </el-button>
        </div>
        <el-drawer
          v-model="drawerOpen"
          direction="ltr"
          size="280px"
          title="篩選條件"
        >
          <FilterSidebar
            :category-name="selectedCategoryName"
            :sub-categories="subCategories"
            :brands="brands"
            :sub-loading="subLoading"
            :brand-loading="brandLoading"
            :selected-sub-category-id="selectedSubCategoryId"
            :selected-brand-ids="selectedBrandIds"
            :brand-keyword="isBrandSearchKeyword"
            :is-brand-expanded="isBrandListExpanded"
            @clear="onClearCategory"
            @filter-change="onFilterChange"
            @update:selected-sub-category-id="onSubCategoryChange($event)"
            @update:selected-brand-ids="selectedBrandIds = $event"
            @update:brand-keyword="isBrandSearchKeyword = $event"
            @update:is-brand-expanded="isBrandListExpanded = $event"
          />
        </el-drawer>
      </template>

      <!-- 主體：邊欄 + 商品牆 -->
      <div
        class="products-layout"
        :class="{ 'with-sidebar': selectedCategoryId !== null && !isMobile }"
      >
        <!-- 左側邊欄（桌機，選了分類才顯示） -->
        <aside v-if="selectedCategoryId !== null && !isMobile" class="filter-aside">
          <FilterSidebar
            :category-name="selectedCategoryName"
            :sub-categories="subCategories"
            :brands="brands"
            :sub-loading="subLoading"
            :brand-loading="brandLoading"
            :selected-sub-category-id="selectedSubCategoryId"
            :selected-brand-ids="selectedBrandIds"
            :brand-keyword="isBrandSearchKeyword"
            :is-brand-expanded="isBrandListExpanded"
            @clear="onClearCategory"
            @filter-change="onFilterChange"
            @update:selected-sub-category-id="onSubCategoryChange($event)"
            @update:selected-brand-ids="selectedBrandIds = $event"
            @update:brand-keyword="isBrandSearchKeyword = $event"
            @update:is-brand-expanded="isBrandListExpanded = $event"
          />
        </aside>

        <!-- 右側商品牆 -->
        <div class="products-main">
          <!-- 骨架屏 -->
          <div v-if="loading" class="product-grid">
            <el-skeleton
              v-for="n in 20"
              :key="n"
              animated
              class="skeleton-card"
            >
              <template #template>
                <el-skeleton-item variant="image" class="skeleton-image" />
                <div class="skeleton-body">
                  <el-skeleton-item variant="p" style="width: 90%" />
                  <el-skeleton-item variant="p" style="width: 70%" />
                  <el-skeleton-item variant="p" style="width: 50%" />
                </div>
              </template>
            </el-skeleton>
          </div>

          <!-- 商品牆 -->
          <div v-else-if="products.length > 0" class="product-grid">
            <ProductCard
              v-for="product in products"
              :key="product.id"
              :product="product"
            />
          </div>

          <!-- 空狀態 -->
          <el-empty v-else description="目前沒有商品" :image-size="120" />

          <!-- 分頁 -->
          <div v-if="total > 0" class="pagination-wrap">
            <el-pagination
              background
              layout="prev, pager, next, jumper, total"
              :total="total"
              :page-size="pageSize"
              :current-page="currentPage"
              :disabled="loading"
              @current-change="onPageChange"
            />
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Filter as FilterIcon } from '@element-plus/icons-vue'
import ProductCard from '@/components/product/ProductCard.vue'
import FilterSidebar from '@/components/home/FilterSidebar.vue'
import { fetchProductList } from '@/api/product'
import { fetchMainCategories, fetchChildCategories } from '@/api/category'
import { fetchActivePromotions } from '@/api/promotion'
import { fetchBrands } from '@/api/brand'
import { getCategoryIcon } from '@/constants/categoryIcon'
import type { ProductListItem, FetchProductsParams } from '@/types/product'
import type { Category, SubCategory } from '@/types/category'
import type { Promotion } from '@/types/promotion'
import type { Brand } from '@/types/brand'

// ── 路由 ─────────────────────────────────────────────────────────
const route = useRoute()

// ── 響應式斷點 ────────────────────────────────────────────────────
const isMobile = ref<boolean>(typeof window !== 'undefined' && window.innerWidth < 768)
function handleResize(): void {
  isMobile.value = window.innerWidth < 768
}

// ── 每日新發現：商品牆 ────────────────────────────────────────────
const products = ref<ProductListItem[]>([])
const loading = ref<boolean>(false)
const currentPage = ref<number>(1)
const pageSize = ref<number>(30)
const total = ref<number>(0)
const sectionRef = ref<HTMLElement | null>(null)
const keyword = ref<string>('')

// ── 主分類篩選 ───────────────────────────────────────────────────
const selectedCategoryId = ref<number | null>(null)
const apiCategories = ref<Category[]>([])
const categoriesLoading = ref<boolean>(false)

const selectedCategoryName = computed<string>(() => {
  if (selectedCategoryId.value === null) return ''
  return apiCategories.value.find(c => c.id === selectedCategoryId.value)?.name ?? ''
})

// ── 左側篩選邊欄狀態 ─────────────────────────────────────────────
const subCategories = ref<SubCategory[]>([])
const brands = ref<Brand[]>([])
const subLoading = ref<boolean>(false)
const brandLoading = ref<boolean>(false)
const selectedSubCategoryId = ref<number | null>(null)
const selectedBrandIds = ref<number[]>([])
const isBrandSearchKeyword = ref<string>('')
const isBrandListExpanded = ref<boolean>(false)
const drawerOpen = ref<boolean>(false)

// ── 活動/輪播 ────────────────────────────────────────────────────
const promotions = ref<Promotion[]>([])

// ── API 呼叫 ─────────────────────────────────────────────────────

async function loadProducts(): Promise<void> {
  loading.value = true
  try {
    const params: FetchProductsParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      sortBy: 'latest',
    }
    if (selectedCategoryId.value !== null) params.categoryId = selectedCategoryId.value
    if (selectedSubCategoryId.value !== null) params.subCategoryId = selectedSubCategoryId.value
    if (selectedBrandIds.value.length > 0) params.brandIds = selectedBrandIds.value
    if (keyword.value.trim()) params.keyword = keyword.value.trim()

    const res = await fetchProductList(params)
    if (res.success) {
      products.value = res.data.items
      total.value = res.data.total
    } else {
      ElMessage.error(res.message || '載入失敗')
    }
  } catch {
    ElMessage.error('載入失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

async function loadCategories(): Promise<void> {
  categoriesLoading.value = true
  try {
    const res = await fetchMainCategories()
    if (res.success) {
      // 過濾髒資料：純數字或名稱長度 < 2 的分類
      const dirty = res.data.filter(c => /^\d+$/.test(c.name) || c.name.length < 2)
      if (dirty.length > 0) {
        console.warn('⚠️ 偵測到髒分類資料，已過濾（請至後端清理）:', dirty.map(c => `[id:${c.id}] "${c.name}"`))
      }
      apiCategories.value = res.data.filter(c => !/^\d+$/.test(c.name) && c.name.length >= 2)
    } else {
      ElMessage.error(res.message || '分類載入失敗')
    }
  } catch {
    ElMessage.error('分類載入失敗，請稍後再試')
  } finally {
    categoriesLoading.value = false
  }
}

async function loadSubCategories(parentId: number): Promise<void> {
  subLoading.value = true
  subCategories.value = []
  try {
    const res = await fetchChildCategories(parentId)
    if (res.success) subCategories.value = res.data
  } catch {
    // 靜默失敗：沒有子分類就不顯示
  } finally {
    subLoading.value = false
  }
}

async function loadBrands(params: { categoryId: number } | { subCategoryId: number }): Promise<void> {
  brandLoading.value = true
  brands.value = []
  try {
    const res = await fetchBrands(params)
    if (res.success) {
      brands.value = res.data
      // 清理已選品牌中不再存在於新清單的 id
      const validIds = new Set(res.data.map(b => b.id))
      const cleaned = selectedBrandIds.value.filter(id => validIds.has(id))
      if (cleaned.length !== selectedBrandIds.value.length) {
        selectedBrandIds.value = cleaned
      }
    }
  } catch {
    // 靜默失敗：沒有品牌就不顯示
  } finally {
    brandLoading.value = false
  }
}

async function loadPromotions(): Promise<void> {
  try {
    const res = await fetchActivePromotions()
    if (res.success) promotions.value = res.data
  } catch {
    // 靜默失敗，fallback 到靜態內容
  }
}

// ── 篩選操作 ─────────────────────────────────────────────────────

function clearSidebarState(): void {
  selectedSubCategoryId.value = null
  selectedBrandIds.value = []
  isBrandSearchKeyword.value = ''
  isBrandListExpanded.value = false
  subCategories.value = []
  brands.value = []
}

function onCategoryClick(categoryId: number): void {
  if (selectedCategoryId.value === categoryId) {
    // 再點同一個分類 → 取消選取
    selectedCategoryId.value = null
    clearSidebarState()
  } else {
    selectedCategoryId.value = categoryId
    clearSidebarState()
    void loadSubCategories(categoryId)
    void loadBrands({ categoryId })
  }
  currentPage.value = 1
  void loadProducts()
}

function onClearCategory(): void {
  selectedCategoryId.value = null
  clearSidebarState()
  drawerOpen.value = false
  currentPage.value = 1
  void loadProducts()
}

function onFilterChange(): void {
  currentPage.value = 1
  void loadProducts()
}

async function onSubCategoryChange(subCategoryId: number | null): Promise<void> {
  selectedSubCategoryId.value = subCategoryId
  currentPage.value = 1
  const brandsParams =
    subCategoryId !== null
      ? { subCategoryId }
      : { categoryId: selectedCategoryId.value! }
  await loadBrands(brandsParams)
  void loadProducts()
}

/**
 * 從 route.query 讀取 categoryId / subCategoryId，
 * 套用篩選狀態並重新載入商品。
 * 供 onMounted 和 watch(route.query) 共用。
 */
async function applyQueryFilter(): Promise<void> {
  const rawCatId = route.query['categoryId']
  const rawSubCatId = route.query['subCategoryId']
  const rawKeyword = route.query['keyword']

  const catId = rawCatId ? Number(rawCatId) : null
  const subCatId = rawSubCatId ? Number(rawSubCatId) : null

  // 關鍵字
  keyword.value = typeof rawKeyword === 'string' ? rawKeyword : ''

  if (catId !== null && !Number.isNaN(catId)) {
    clearSidebarState()
    selectedCategoryId.value = catId

    void loadSubCategories(catId)
    const brandsParam =
      subCatId !== null && !Number.isNaN(subCatId)
        ? { subCategoryId: subCatId }
        : { categoryId: catId }
    void loadBrands(brandsParam)

    if (subCatId !== null && !Number.isNaN(subCatId)) {
      selectedSubCategoryId.value = subCatId
    }
  }

  currentPage.value = 1
  void loadProducts()
}

function onPageChange(page: number): void {
  currentPage.value = page
  void loadProducts().then(() => {
    sectionRef.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

// ── 生命週期 ─────────────────────────────────────────────────────

onMounted(() => {
  void applyQueryFilter()   // 讀 query 決定是否套分類；沒有 query 就直接載入全部
  void loadCategories()
  void loadPromotions()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})

// 若已在首頁時 query 改變（例如連點麵包屑不同分類），重新套用篩選
watch(
  () => route.query,
  () => {
    void applyQueryFilter()
  },
)

// ── 靜態預設輪播資料（API 空時顯示） ────────────────────────────

const staticBanners = [
  { tag: '🎉 會員專屬', title: '購物節送 8 折券', subtitle: '全站 $49 起免運', bg: 'linear-gradient(135deg, #1e293b 0%, #1e1b4b 100%)', emoji: '🚚' },
  { tag: '🔥 限時搶購', title: '3C 家電季', subtitle: '滿萬折千 再送好禮', bg: 'linear-gradient(135deg, #064e3b 0%, #022c22 100%)', emoji: '📱' },
  { tag: '💚 新品上架', title: '春夏新品', subtitle: '時尚穿搭一次擁有', bg: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 100%)', emoji: '👗' },
]

const staticSideBanners = [
  { tag: '商城', title: '新品喇叭上市', desc: '領券現折 $100', bg: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)', emoji: '🔊' },
  { tag: '商城', title: '幫你換新機', desc: 'AI 筆電專區', bg: 'linear-gradient(135deg, #1e293b 0%, #334155 100%)', emoji: '💻' },
]

const quickItems = [
  { icon: '🚚', label: '全站大免運' },
  { icon: '📦', label: '免運$99起' },
  { icon: '🎫', label: '領券中心', url: '/coupons' },
  { icon: '💰', label: '全額$49免運' },
  { icon: '👑', label: 'VIP 獨享 18%' },
  { icon: '🛍️', label: 'HowBuy 商城' },
  { icon: '✈️', label: '海外直送' },
  { icon: '💳', label: '銀行刷卡優惠' },
  { icon: '💻', label: 'HowBuy 3C' },
  { icon: '🔥', label: '天天超划算' },
]
</script>

<style scoped>
.home {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px 30px;
}

/* ── 輪播 ── */
.banner-section {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
}
.main-carousel {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}
.carousel-slide {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 60px;
  color: white;
  position: relative;
}
.promo-slide { cursor: pointer; }
.slide-content { z-index: 1; }
.slide-tag {
  display: inline-block;
  background: rgba(238,77,45,0.2);
  color: #EE4D2D;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 13px;
  margin-bottom: 16px;
  border: 1px solid rgba(238,77,45,0.3);
}
.carousel-slide h2 { font-size: 42px; margin: 0 0 10px; font-weight: 800; }
.carousel-slide p { font-size: 18px; opacity: 0.85; margin-bottom: 24px; }
.slide-emoji {
  font-size: 180px;
  filter: drop-shadow(0 10px 30px rgba(238,77,45,0.3));
}
.side-banners { display: flex; flex-direction: column; gap: 16px; }
.side-banner {
  flex: 1;
  border-radius: 12px;
  padding: 20px 24px;
  color: white;
  position: relative;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  cursor: pointer;
  transition: transform 0.3s;
}
.side-banner:hover { transform: translateY(-3px); }
.side-banner-empty { background: #1e293b; opacity: 0.3; cursor: default; }
.side-banner-empty:hover { transform: none; }
.sb-tag {
  display: inline-block;
  background: #EE4D2D;
  color: white;
  padding: 3px 10px;
  border-radius: 4px;
  font-size: 11px;
  margin-bottom: 8px;
}
.side-banner h3 { margin: 0 0 6px; font-size: 18px; }
.side-banner p { margin: 0; font-size: 13px; opacity: 0.85; }
.sb-emoji { position: absolute; right: 16px; bottom: 10px; font-size: 70px; opacity: 0.7; }

/* ── 快捷服務 ── */
.quick-icons {
  background: white;
  border-radius: 12px;
  padding: 24px;
  display: grid;
  grid-template-columns: repeat(10, 1fr);
  gap: 12px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.quick-item { text-align: center; cursor: pointer; transition: transform 0.2s; }
.quick-item:hover { transform: translateY(-4px); }
.quick-circle {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: linear-gradient(135deg, #FFF0ED 0%, #FCDBD5 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  margin: 0 auto 8px;
  box-shadow: 0 2px 6px rgba(238,77,45,0.15);
}
.quick-label { font-size: 12px; color: #475569; }

/* ── 商品分類 ── */
.category-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 20px;
  padding-left: 12px;
  border-left: 4px solid #EE4D2D;
}
.category-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 12px;
}
@media (max-width: 768px) { .category-grid { grid-template-columns: repeat(4, 1fr); } }
@media (max-width: 480px) { .category-grid { grid-template-columns: repeat(3, 1fr); } }

.category-item {
  text-align: center;
  padding: 16px 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  border: 2px solid transparent;
}
.category-item:hover { background: #FFF5F3; transform: translateY(-3px); }
.category-item--active { background: #FFF5F3; border-color: #EE4D2D; }
.cat-image {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 10px;
  overflow: hidden;
}
.cat-emoji { font-size: 32px; line-height: 1; }
.cat-name { font-size: 13px; color: #334155; font-weight: 500; }
.cat-skeleton { text-align: center; padding: 16px 8px; }

/* ── 每日新發現 ── */
.products-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.discovery-header {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20px;
  position: relative;
}
.discovery-title {
  display: inline-block;
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
  padding-bottom: 8px;
  border-bottom: 3px solid #EE4D2D;
}
.discovery-header::before,
.discovery-header::after {
  content: '';
  position: absolute;
  top: 50%;
  width: calc(50% - 90px);
  height: 1px;
  background: #e2e8f0;
  transform: translateY(-6px);
}
.discovery-header::before { left: 0; }
.discovery-header::after  { right: 0; }

/* 手機篩選按鈕列 */
.mobile-filter-bar {
  margin-bottom: 16px;
}

/* ── 雙欄 layout（Flexbox） ── */
.products-layout {
  /* 無邊欄：products-main 自動撐滿 */
}
.products-layout.with-sidebar {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}
.filter-aside {
  width: 240px;
  flex-shrink: 0;
  /* 黏性定位，讓邊欄跟著捲動時固定 */
  position: sticky;
  top: 16px;
}
.products-main {
  flex: 1;
  min-width: 0; /* 防止 flex 子元素撐破容器 */
}

/* 商品格線（無邊欄：6 欄；有邊欄：5 欄） */
.product-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
}
.with-sidebar .product-grid {
  grid-template-columns: repeat(5, 1fr);
}
@media (max-width: 1200px) {
  .product-grid { grid-template-columns: repeat(4, 1fr); }
  .with-sidebar .product-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 767px) {
  .product-grid { grid-template-columns: repeat(2, 1fr); }
  .with-sidebar .product-grid { grid-template-columns: repeat(2, 1fr); }
}

/* 分頁 */
.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid #f1f5f9;
}

/* 骨架屏卡片 */
.skeleton-card { border-radius: 4px; overflow: hidden; border: 1px solid #f1f5f9; }
.skeleton-image { width: 100%; aspect-ratio: 1 / 1; }
.skeleton-body { padding: 10px 12px 12px; display: flex; flex-direction: column; gap: 8px; }

/* 輪播圓點 */
:deep(.el-carousel__indicator--horizontal .el-carousel__button) {
  background: rgba(255,255,255,0.5); width: 30px; height: 4px; border-radius: 2px;
}
:deep(.el-carousel__indicator--horizontal.is-active .el-carousel__button) { background: #EE4D2D; }
:deep(.el-carousel__arrow) { background: rgba(0,0,0,0.4); }
:deep(.el-carousel__arrow:hover) { background: #EE4D2D; }
</style>
