<template>
  <div class="search-page">
    <!-- 麵包屑 + 標題 -->
    <div class="search-header">
      <el-breadcrumb separator="/" class="breadcrumb">
        <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
        <el-breadcrumb-item>搜尋結果</el-breadcrumb-item>
      </el-breadcrumb>
      <h2 class="search-title">
        <template v-if="keyword">「{{ keyword }}」的搜尋結果</template>
        <template v-else>所有商品</template>
        <span v-if="!loading" class="result-count">（共 {{ total }} 件）</span>
      </h2>
    </div>

    <div class="search-layout">
      <!-- ── 左側篩選欄 ───────────────────────────────────── -->
      <aside class="filter-aside">

        <!-- 商品分類 -->
        <div class="filter-block">
          <div class="filter-block-title">商品分類</div>
          <div v-if="catsLoading" class="filter-loading">
            <el-skeleton :rows="5" animated />
          </div>
          <ul v-else class="filter-list">
            <li
              class="filter-item"
              :class="{ active: selectedCategoryId === null }"
              @click="selectCategory(null)"
            >全部分類</li>
            <li
              v-for="cat in categories"
              :key="cat.id"
              class="filter-item"
              :class="{ active: selectedCategoryId === cat.id }"
              @click="selectCategory(cat.id)"
            >{{ cat.name }}</li>
          </ul>
        </div>

        <!-- 價格區間 -->
        <div class="filter-block">
          <div class="filter-block-title">價格區間</div>
          <div class="price-inputs">
            <el-input
              v-model="priceMinStr"
              placeholder="最低"
              size="small"
              type="number"
              style="flex:1"
            />
            <span class="price-sep">~</span>
            <el-input
              v-model="priceMaxStr"
              placeholder="最高"
              size="small"
              type="number"
              style="flex:1"
            />
          </div>
          <el-button
            size="small"
            type="primary"
            plain
            style="width:100%;margin-top:8px"
            @click="applyPriceFilter"
          >套用</el-button>
        </div>

        <!-- 預留擴充 -->
        <!-- <div class="filter-block">條件：評分 / 出貨速度 ...</div> -->

      </aside>

      <!-- ── 右側商品區 ────────────────────────────────────── -->
      <div class="search-main">

        <!-- 排序列 -->
        <div class="sort-bar">
          <span class="sort-label">排序：</span>
          <div class="sort-btns">
            <button
              v-for="s in sortOptions"
              :key="s.value"
              class="sort-btn"
              :class="{ active: sortBy === s.value }"
              @click="setSort(s.value as SortBy)"
            >{{ s.label }}</button>
          </div>
          <div v-if="hasActiveFilters" class="clear-filters">
            <el-button link size="small" @click="clearFilters">清除篩選</el-button>
          </div>
        </div>

        <!-- 骨架屏 -->
        <div v-if="loading" class="product-grid">
          <el-skeleton
            v-for="n in 20"
            :key="n"
            animated
            class="skeleton-card"
          >
            <template #template>
              <el-skeleton-item variant="image" style="width:100%;aspect-ratio:1/1;border-radius:8px" />
              <el-skeleton-item variant="p" style="width:90%;margin-top:8px" />
              <el-skeleton-item variant="p" style="width:55%;margin-top:4px" />
            </template>
          </el-skeleton>
        </div>

        <!-- 商品網格 -->
        <div v-else-if="products.length > 0" class="product-grid">
          <ProductCard
            v-for="p in products"
            :key="p.id"
            :product="p"
          />
        </div>

        <!-- 空狀態 -->
        <el-empty
          v-else
          :description="keyword ? `找不到「${keyword}」相關商品` : '目前沒有商品'"
          :image-size="120"
          style="padding: 60px 0"
        >
          <el-button @click="clearFilters">清除所有篩選條件</el-button>
        </el-empty>

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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import ProductCard from '@/components/product/ProductCard.vue'
import { fetchProductList } from '@/api/product'
import { fetchMainCategories } from '@/api/category'
import type { ProductListItem, FetchProductsParams } from '@/types/product'
import type { Category } from '@/types/category'

type SortBy = 'latest' | 'priceAsc' | 'priceDesc' | 'soldCount'

const route = useRoute()
const router = useRouter()

// ── 排序選項 ──────────────────────────────────────────────────────
const sortOptions = [
  { value: 'latest',    label: '最新' },
  { value: 'soldCount', label: '銷量' },
  { value: 'priceAsc',  label: '價格↑' },
  { value: 'priceDesc', label: '價格↓' },
]

// ── 從 route.query 讀取當前狀態（computed = 唯一資料來源）──────────
const keyword = computed<string>(() =>
  typeof route.query['keyword'] === 'string' ? route.query['keyword'] : '',
)
const selectedCategoryId = computed<number | null>(() => {
  const v = route.query['categoryId']
  if (!v) return null
  const n = Number(v)
  return Number.isNaN(n) ? null : n
})
const sortBy = computed<SortBy>(() => {
  const v = route.query['sortBy'] as string
  return (['latest', 'priceAsc', 'priceDesc', 'soldCount'] as const).includes(v as SortBy)
    ? (v as SortBy)
    : 'latest'
})
const currentPage = computed<number>(() => {
  const v = route.query['page']
  const n = v ? Number(v) : 1
  return Number.isNaN(n) ? 1 : Math.max(1, n)
})
const routeMinPrice = computed<number | undefined>(() => {
  const v = route.query['minPrice']
  const n = v ? Number(v) : undefined
  return n !== undefined && !Number.isNaN(n) ? n : undefined
})
const routeMaxPrice = computed<number | undefined>(() => {
  const v = route.query['maxPrice']
  const n = v ? Number(v) : undefined
  return n !== undefined && !Number.isNaN(n) ? n : undefined
})

// ── 本地價格輸入（使用者打字但未套用時的暫存）───────────────────
const priceMinStr = ref<string>(routeMinPrice.value !== undefined ? String(routeMinPrice.value) : '')
const priceMaxStr = ref<string>(routeMaxPrice.value !== undefined ? String(routeMaxPrice.value) : '')

// 當 URL 的價格 query 變化時同步輸入框
watch([routeMinPrice, routeMaxPrice], ([min, max]) => {
  priceMinStr.value = min !== undefined ? String(min) : ''
  priceMaxStr.value = max !== undefined ? String(max) : ''
})

// ── API 狀態 ─────────────────────────────────────────────────────
const products = ref<ProductListItem[]>([])
const total    = ref<number>(0)
const pageSize = ref<number>(20)
const loading  = ref<boolean>(false)

// ── 分類清單 ─────────────────────────────────────────────────────
const categories  = ref<Category[]>([])
const catsLoading = ref<boolean>(false)

// ── 是否有啟用的篩選條件 ────────────────────────────────────────
const hasActiveFilters = computed<boolean>(() =>
  !!keyword.value ||
  selectedCategoryId.value !== null ||
  routeMinPrice.value !== undefined ||
  routeMaxPrice.value !== undefined ||
  sortBy.value !== 'latest',
)

// ── URL 更新工具 ────────────────────────────────────────────────
function buildQuery(
  overrides: Partial<{
    keyword: string
    categoryId: number | null
    minPrice: number | undefined
    maxPrice: number | undefined
    sortBy: SortBy
    page: number
  }> = {},
): Record<string, string> {
  const merged = {
    keyword:    overrides.keyword    !== undefined ? overrides.keyword    : keyword.value,
    categoryId: overrides.categoryId !== undefined ? overrides.categoryId : selectedCategoryId.value,
    minPrice:   overrides.minPrice   !== undefined ? overrides.minPrice   : routeMinPrice.value,
    maxPrice:   overrides.maxPrice   !== undefined ? overrides.maxPrice   : routeMaxPrice.value,
    sortBy:     overrides.sortBy     !== undefined ? overrides.sortBy     : sortBy.value,
    page:       overrides.page       !== undefined ? overrides.page       : currentPage.value,
  }
  const q: Record<string, string> = {}
  if (merged.keyword)                     q['keyword']    = merged.keyword
  if (merged.categoryId !== null)         q['categoryId'] = String(merged.categoryId)
  if (merged.minPrice !== undefined)      q['minPrice']   = String(merged.minPrice)
  if (merged.maxPrice !== undefined)      q['maxPrice']   = String(merged.maxPrice)
  if (merged.sortBy !== 'latest')         q['sortBy']     = merged.sortBy
  if (merged.page > 1)                    q['page']       = String(merged.page)
  return q
}

function pushQuery(overrides: Parameters<typeof buildQuery>[0]): void {
  void router.push({ path: '/products', query: buildQuery({ ...overrides, page: 1 }) })
}

// ── 篩選操作 ────────────────────────────────────────────────────
function selectCategory(id: number | null): void {
  pushQuery({ categoryId: id })
}

function setSort(value: SortBy): void {
  pushQuery({ sortBy: value })
}

function applyPriceFilter(): void {
  const min = priceMinStr.value ? Number(priceMinStr.value) : undefined
  const max = priceMaxStr.value ? Number(priceMaxStr.value) : undefined
  if (min !== undefined && max !== undefined && min > max) {
    ElMessage.warning('最低價不能大於最高價')
    return
  }
  pushQuery({ minPrice: min, maxPrice: max })
}

function clearFilters(): void {
  priceMinStr.value = ''
  priceMaxStr.value = ''
  void router.push({ path: '/products', query: keyword.value ? { keyword: keyword.value } : {} })
}

function onPageChange(page: number): void {
  void router.push({ path: '/products', query: buildQuery({ page }) })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// ── API 呼叫 ────────────────────────────────────────────────────
async function loadProducts(): Promise<void> {
  loading.value = true
  try {
    const params: FetchProductsParams = {
      page:     currentPage.value,
      pageSize: pageSize.value,
      sortBy:   sortBy.value,
    }
    if (keyword.value)                    params.keyword    = keyword.value
    if (selectedCategoryId.value !== null) params.categoryId = selectedCategoryId.value
    if (routeMinPrice.value !== undefined) params.minPrice   = routeMinPrice.value
    if (routeMaxPrice.value !== undefined) params.maxPrice   = routeMaxPrice.value

    const res = await fetchProductList(params)
    if (res.success) {
      products.value = res.data.items
      total.value    = res.data.total
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
  catsLoading.value = true
  try {
    const res = await fetchMainCategories()
    if (res.success) {
      categories.value = res.data.filter(c => !/^\d+$/.test(c.name) && c.name.length >= 2)
    }
  } catch {
    // 靜默失敗，不顯示分類也沒關係
  } finally {
    catsLoading.value = false
  }
}

// ── 生命週期 ─────────────────────────────────────────────────────
onMounted(() => {
  void loadProducts()
  void loadCategories()
})

// URL query 變化時重新載入
watch(
  () => route.query,
  () => void loadProducts(),
)
</script>

<style scoped>
.search-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: 24px 30px 60px;
}

/* 標題區 */
.search-header {
  margin-bottom: 20px;
}
.breadcrumb {
  margin-bottom: 10px;
}
.search-title {
  font-size: 20px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.result-count {
  font-size: 14px;
  font-weight: 400;
  color: #909399;
  margin-left: 6px;
}

/* 左右分欄 */
.search-layout {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}

/* ── 左側篩選欄 ── */
.filter-aside {
  flex: 0 0 220px;
  width: 220px;
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.07);
  position: sticky;
  top: 16px;
}
.filter-block {
  margin-bottom: 24px;
}
.filter-block:last-child {
  margin-bottom: 0;
}
.filter-block-title {
  font-size: 14px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 10px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}
.filter-loading {
  padding: 8px 0;
}
.filter-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
.filter-item {
  padding: 7px 10px;
  border-radius: 6px;
  font-size: 13px;
  color: #606266;
  cursor: pointer;
  transition: all 0.15s;
  margin-bottom: 2px;
}
.filter-item:hover {
  background: #fef2f2;
  color: #EE4D2D;
}
.filter-item.active {
  background: #fef2f2;
  color: #EE4D2D;
  font-weight: 600;
}
.price-inputs {
  display: flex;
  align-items: center;
  gap: 6px;
}
.price-sep {
  font-size: 13px;
  color: #909399;
  flex-shrink: 0;
}

/* ── 右側商品區 ── */
.search-main {
  flex: 1;
  min-width: 0;
}
.sort-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  background: white;
  border-radius: 8px;
  padding: 10px 16px;
  margin-bottom: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.07);
}
.sort-label {
  font-size: 13px;
  color: #606266;
  flex-shrink: 0;
}
.sort-btns {
  display: flex;
  gap: 6px;
}
.sort-btn {
  padding: 5px 16px;
  border: 1px solid #dcdfe6;
  background: white;
  border-radius: 4px;
  font-size: 13px;
  color: #606266;
  cursor: pointer;
  transition: all 0.15s;
}
.sort-btn:hover {
  border-color: #EE4D2D;
  color: #EE4D2D;
}
.sort-btn.active {
  background: #EE4D2D;
  border-color: #EE4D2D;
  color: white;
  font-weight: 600;
}
.clear-filters {
  margin-left: auto;
}

/* 商品網格 — 響應式 4 欄 */
.product-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}
@media (max-width: 1100px) {
  .product-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 768px) {
  .search-layout { flex-direction: column; }
  .filter-aside { width: 100%; position: static; }
  .product-grid { grid-template-columns: repeat(2, 1fr); }
}
.skeleton-card {
  background: white;
  border-radius: 8px;
  padding: 12px;
}

/* 分頁 */
.pagination-wrap {
  display: flex;
  justify-content: center;
  padding: 20px 0;
}
</style>
