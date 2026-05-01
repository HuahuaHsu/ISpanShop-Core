<template>
  <div class="product-list-page">
    <!-- ── 頂部 ── -->
    <div class="page-header">
      <h2 class="page-title">我的商品</h2>
      <el-button type="primary" @click="router.push('/seller/products/new')">
        <el-icon><Plus /></el-icon> 新增商品
      </el-button>
    </div>

    <!-- ── 主卡片 ── -->
    <el-card shadow="never" class="main-card">

      <!-- ── 一級 Tab ── -->
      <el-tabs v-model="activeTab" class="level1-tabs" @tab-change="onTabChange">
        <el-tab-pane v-for="tab in level1Tabs" :key="tab.key" :name="tab.key">
          <template #label>
            {{ tab.label }}<span class="tab-count">({{ tabCounts[tab.key] }})</span>
          </template>
        </el-tab-pane>
      </el-tabs>

      <!-- ── 搜尋列 ── -->
      <div class="search-section">
        <el-row :gutter="12" align="middle">
          <el-col :xs="24" :sm="8">
            <el-input
              v-model="searchKeyword"
              placeholder="搜尋 商品名稱, 商品ID"
              clearable
              @keyup.enter="handleSearch"
              @clear="handleSearch"
            >
              <template #prefix><el-icon><Search /></el-icon></template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="5">
            <el-select
              v-model="searchCategoryId"
              placeholder="全部分類"
              clearable
              style="width:100%"
              @change="handleSearch"
            >
              <el-option
                v-for="cat in categories"
                :key="cat.id"
                :label="cat.name"
                :value="cat.id"
              />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="11">
            <el-button class="search-btn" @click="handleSearch">搜尋</el-button>
            <el-button @click="handleReset">重設</el-button>
            <el-button text type="info" @click="showAdvanced = !showAdvanced">
              進階篩選
              <el-icon style="margin-left:4px">
                <component :is="showAdvanced ? ArrowUp : ArrowDown" />
              </el-icon>
            </el-button>
          </el-col>
        </el-row>

        <!-- 進階篩選（折疊） -->
        <el-collapse-transition>
          <div v-show="showAdvanced" class="advanced-filter">
            <el-row :gutter="12">
              <el-col :xs="12" :sm="5">
                <label class="adv-label">最低價</label>
                <el-input-number
                  v-model="advMinPrice"
                  :min="0"
                  placeholder="NT$"
                  controls-position="right"
                  style="width:100%"
                />
              </el-col>
              <el-col :xs="12" :sm="5">
                <label class="adv-label">最高價</label>
                <el-input-number
                  v-model="advMaxPrice"
                  :min="0"
                  placeholder="NT$"
                  controls-position="right"
                  style="width:100%"
                />
              </el-col>
            </el-row>
          </div>
        </el-collapse-transition>

        <div class="result-count">
          共 <strong class="count-num">{{ filteredProducts.length }}</strong> 件商品
        </div>
      </div>

      <!-- ── 排序列 + 切換按鈕 ── -->
      <div class="sort-bar">
        <div class="sort-left">
          <span class="sort-label">依據篩選：</span>
          <span
            v-for="(s, idx) in sortOptions"
            :key="s.field"
            class="sort-item"
            :class="{ 'sort-active': sortField === s.field }"
            @click="toggleSort(s.field as SortField)"
          >
            <span v-if="idx > 0" class="sort-divider">|</span>
            {{ s.label }}
            <el-icon :size="13">
              <component :is="getSortIcon(s.field as SortField)" />
            </el-icon>
          </span>
        </div>
        <div class="view-toggle">
          <el-tooltip content="網格模式">
            <button
              :class="['toggle-btn', { active: viewMode === 'grid' }]"
              @click="viewMode = 'grid'"
            >
              <el-icon :size="17"><Grid /></el-icon>
            </button>
          </el-tooltip>
          <el-tooltip content="列表模式">
            <button
              :class="['toggle-btn', { active: viewMode === 'list' }]"
              @click="viewMode = 'list'"
            >
              <el-icon :size="17"><List /></el-icon>
            </button>
          </el-tooltip>
        </div>
      </div>

      <!-- ── 商品區域 ── -->
      <div v-loading="loading" class="product-area">

        <!-- 網格模式 -->
        <template v-if="viewMode === 'grid'">
          <div v-if="pagedProducts.length > 0" class="product-grid">
            <div
              v-for="product in pagedProducts"
              :key="product.id"
              class="product-card"
              :class="{ 'product-card-deleted': product.isDeleted }"
            >
              <!-- 圖片 -->
              <div class="card-img-wrap">
                <el-image
                  :src="getProductImageUrl(product)"
                  fit="cover"
                  class="card-img"
                  lazy
                >
                  <template #error>
                    <div class="image-slot">
                      <img :src="defaultProductImage" alt="暫無圖片" style="width: 100%; height: 100%;" />
                    </div>
                  </template>
                </el-image>
                <div class="status-badge" :class="`badge-${product.status}`">
                  {{ product.statusText }}
                </div>
              </div>

              <!-- 商品資訊 -->
              <div class="card-body">
                <p class="card-name">{{ product.name }}</p>
                <!-- 停權隱藏標籤 -->
                <el-tag v-if="sellerStore.isBanned" type="info" size="small" style="margin-bottom:4px">前台已隱藏</el-tag>
                <!-- 退回原因橫幅 -->
                <div v-if="product.status === 'rejected' && product.rejectReason" class="reject-banner">
                  <el-icon :size="13"><WarningFilled /></el-icon>
                  <span>退回原因：{{ product.rejectReason }}</span>
                </div>
                <div class="card-price">NT$ {{ getProductPrice(product) }}</div>
                <div class="card-stock">
                  商品數量：
                  <!-- TODO: totalStock 後端尚未回傳，補上後移除 ?? '--' -->
                  <span :class="{ 'text-danger': product.totalStock === 0 }">
                    {{ product.totalStock ?? '--' }}
                  </span>
                </div>
                <div class="card-stats">
                  <!-- TODO: viewCount 後端尚未回傳，補上後移除 ?? '--' -->
                  <span title="瀏覽次數"><el-icon><View /></el-icon> {{ product.viewCount ?? '--' }}</span>
                  <!-- TODO: reviewCount 尚未由後端商品列表 API 回傳，待補上 -->
                  <span title="評論數"><el-icon><ChatDotRound /></el-icon> 0</span>
                </div>
                <div class="card-date">建立時間: {{ formatDate(product.createdAt) }}</div>
              </div>

              <!-- 操作列 -->
              <div class="card-footer">
                <template v-if="product.isDeleted">
                  <span class="deleted-footer-label">
                    <el-icon :size="12"><Delete /></el-icon>
                    已刪除
                  </span>
                </template>
                <template v-else>
                  <button
                    v-if="product.status !== 'review'"
                    class="card-action-btn edit-btn"
                    :class="{ 'resubmit-btn': product.status === 'rejected' }"
                    @click="handleEdit(product.id)"
                  >
                    <el-icon :size="13"><Edit /></el-icon>
                    {{ product.status === 'rejected' ? '重新編輯' : '編輯' }}
                  </button>
                  <el-dropdown
                    trigger="click"
                    @command="(cmd: string) => handleCardCommand(cmd, product)"
                  >
                    <button class="card-action-btn more-btn">
                      <el-icon :size="15"><MoreFilled /></el-icon>
                    </button>
                    <template #dropdown>
                      <el-dropdown-menu>
                        <!-- 即時預覽：非刪除狀態皆可預覽 -->
                        <el-dropdown-item
                          v-if="product.status !== 'deleted'"
                          command="preview"
                        >
                          即時預覽
                        </el-dropdown-item>

                        <!-- 送出審核：只有草稿才需要送審流程 -->
                        <el-dropdown-item
                          v-if="product.status === 'draft'"
                          command="submitReview"
                        >
                          送出審核
                        </el-dropdown-item>

                        <!-- 上架：只有曾審核通過後下架的商品才能直接上架 -->
                        <el-dropdown-item
                          v-if="product.status === 'off'"
                          command="shelf"
                        >
                          上架
                        </el-dropdown-item>

                        <!-- 下架：只有已上架才能下架 -->
                        <el-dropdown-item
                          v-if="product.status === 'on'"
                          command="shelf"
                        >
                          下架
                        </el-dropdown-item>

                        <!-- 低庫存提醒：已上架和未上架可設定 -->
                        <el-dropdown-item
                          v-if="product.status === 'on' || product.status === 'off'"
                          command="stock"
                        >
                          低庫存提醒
                        </el-dropdown-item>

                        <!-- 刪除：草稿、未上架、已退回可刪除 -->
                        <el-dropdown-item
                          v-if="product.status === 'draft' || product.status === 'off' || product.status === 'rejected'"
                          command="delete"
                          divided
                        >
                          <span class="text-danger">刪除</span>
                        </el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
                </template>
              </div>
            </div>
          </div>

          <el-empty
            v-else
            description="您還沒有任何商品"
            :image-size="100"
            style="padding: 40px 0"
          >
            <el-button type="primary" @click="router.push('/seller/products/new')">
              立即新增商品
            </el-button>
          </el-empty>
        </template>

        <!-- 列表模式 -->
        <template v-else>
          <el-table
            v-if="pagedProducts.length > 0"
            :data="pagedProducts"
            stripe
            style="width: 100%"
            @selection-change="(rows: SellerProduct[]) => { selectedRows = rows }"
          >
            <el-table-column type="selection" width="50" />

            <el-table-column label="商品" min-width="260">
              <template #default="{ row }">
                <div class="table-product-cell">
                  <el-image
                    :src="getProductImageUrl(row)"
                    fit="cover"
                    class="table-thumb"
                  >
                    <template #error>
                      <div class="image-slot">
                        <img :src="defaultProductImage" alt="暫無圖片" style="width: 100%; height: 100%;" />
                      </div>
                    </template>
                  </el-image>
                  <div class="table-info">
                    <div class="table-name">{{ row.name }}</div>
                    <div class="table-id">ID: {{ row.id }}</div>
                    <div v-if="row.status === 'rejected' && row.rejectReason" class="table-reject-reason">
                      <el-icon :size="12"><WarningFilled /></el-icon>
                      {{ row.rejectReason }}
                    </div>
                  </div>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="soldCount" label="已售出" width="90" align="center">
              <template #default>
                <!-- TODO: soldCount 後端尚未回傳 -->
                --
              </template>
            </el-table-column>

            <el-table-column label="商品數量" width="100" align="center">
              <template #default>
                <!-- TODO: totalStock 後端尚未回傳 -->
                --
              </template>
            </el-table-column>

            <el-table-column label="低庫存提醒" width="120" align="center">
              <template #default="{ row }">
                <el-switch
                  :model-value="row.lowStockAlert"
                  size="small"
                  active-color="#ee4d2d"
                  @change="openStockDialog(row)"
                />
              </template>
            </el-table-column>

            <el-table-column label="狀態" width="130" align="center">
              <template #default="{ row }">
                <el-tag :type="getStatusTagType(row.status)" size="small">
                  {{ row.statusText }}
                </el-tag>
                <br v-if="sellerStore.isBanned" />
                <el-tag v-if="sellerStore.isBanned" type="info" size="small" style="margin-top:4px">前台已隱藏</el-tag>
              </template>
            </el-table-column>

            <el-table-column label="操作" width="160" align="center" fixed="right">
              <template #default="{ row }">
                <template v-if="!row.isDeleted">
                  <el-button
                    v-if="row.status !== 'review'"
                    text
                    :type="row.status === 'rejected' ? 'warning' : 'primary'"
                    size="small"
                    @click="handleEdit(row.id)"
                  >
                    <el-icon><Edit /></el-icon>
                    {{ row.status === 'rejected' ? '重新編輯' : '編輯' }}
                  </el-button>
                  <el-popconfirm
                    title="確定要刪除這個商品嗎？"
                    confirm-button-text="確定"
                    cancel-button-text="取消"
                    @confirm="handleDeleteProduct(row)"
                  >
                    <template #reference>
                      <el-button text type="danger" size="small">
                        <el-icon><Delete /></el-icon>
                      </el-button>
                    </template>
                  </el-popconfirm>
                </template>
                <el-tag v-else type="danger" size="small">已刪除</el-tag>
              </template>
            </el-table-column>
          </el-table>

          <el-empty
            v-else
            description="您還沒有任何商品"
            :image-size="100"
            style="padding: 40px 0"
          >
            <el-button type="primary" @click="router.push('/seller/products/new')">
              立即新增商品
            </el-button>
          </el-empty>
        </template>
      </div>

      <!-- ── 分頁 ── -->
      <div class="pagination-wrapper" v-if="filteredProducts.length > 0">
        <el-pagination
          v-model:current-page="pagination.page"
          :page-size="pagination.pageSize"
          :total="filteredProducts.length"
          layout="total, prev, pager, next, jumper"
          background
          @current-change="(p: number) => { pagination.page = p }"
        />
      </div>
    </el-card>

    <!-- ── 低庫存提醒 Dialog ── -->
    <el-dialog
      v-model="stockDialogVisible"
      title="低庫存提醒"
      width="460px"
      :close-on-click-modal="false"
    >
      <p class="dialog-hint">
        當您的庫存數量少於您在此處設定的安全庫存，系統將會通知您
      </p>
      <div v-if="stockDialogProduct" class="dialog-product-row">
        <el-image
          :src="stockDialogProduct.mainImageUrl || defaultProductImage"
          fit="cover"
          class="dialog-img"
        />
        <span class="dialog-product-name">{{ stockDialogProduct.name }}</span>
      </div>
      <el-form label-position="top">
        <el-form-item label="安全庫存數量">
          <el-input-number
            v-model="stockForm.threshold"
            :min="0"
            :max="99999"
            controls-position="right"
            placeholder="商品數量"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="提醒狀態">
          <el-switch
            v-model="stockForm.enabled"
            active-text="開啟提醒"
            inactive-text="關閉提醒"
            active-color="#ee4d2d"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="stockDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveStockAlert">儲存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Plus, Search, Edit, Delete, Grid, List,
  ArrowDown, ArrowUp, DCaret, CaretTop, CaretBottom,
  MoreFilled, WarningFilled, View, ChatDotRound,
} from '@element-plus/icons-vue'
import { fetchSellerProducts, updateProductStatus, deleteSellerProduct, submitProductForReview } from '@/api/product'
import { fetchMainCategories } from '@/api/category'
import { useSellerStore } from '@/stores/seller'
import type { SellerProductListItem } from '@/types/product'
import type { Category } from '@/types/category'

const router = useRouter()
const route = useRoute()
const sellerStore = useSellerStore()

// ── 預設圖片 ──────────────────────────────────────────────────────
const defaultProductImage = 'https://placehold.co/200x200/f5f5f5/999?text=No+Image'

// 取得商品圖片 URL，優先使用 mainImageUrl
function getProductImageUrl(product: SellerProduct): string {
  if (product.mainImageUrl) return product.mainImageUrl
  return defaultProductImage
}

// 取得商品價格顯示文字（不含 NT$，template 自行加前綴）
function getProductPrice(product: SellerProduct): string {
  const min = product.minPrice
  const max = product.maxPrice
  if (min === null || min === undefined) return '0'
  if (max === null || max === undefined || min === max) return min.toLocaleString()
  return `${min.toLocaleString()} ~ ${max.toLocaleString()}`
}

// ── 型別定義 ──────────────────────────────────────────────────────
type ProductStatus = 'on' | 'off' | 'deleted' | 'review' | 'rejected' | 'draft'
type TabKey = 'all' | 'on' | 'off' | 'deleted' | 'review' | 'rejected' | 'draft'
type SortField = 'minPrice' | 'createdAt'
type SortDir = 'asc' | 'desc' | null

/** 將後端 status 數字 + reviewStatus 對應至 tab key 字串
 *  status=0 + reviewStatus=1(通過) → 'off'（審核通過但賣家下架）
 *  status=0 + 其他              → 'draft'（草稿，從未通過審核）
 */
function mapStatusToKey(status: number, reviewStatus: number): ProductStatus {
  switch (status) {
    case 1: return 'on'
    case 2: return 'review'
    case 3: return 'rejected'
    case 0: return reviewStatus === 1 ? 'off' : 'draft'
    default: return 'draft'
  }
}

/**
 * 擴充 SellerProductListItem：
 * - 覆寫 status 為 tab key 字串（原始數字已透過 mapStatusToKey 轉換）
 * - 加入前端本地欄位（lowStockAlert 等，尚未串接後端）
 */
interface SellerProduct extends Omit<SellerProductListItem, 'status'> {
  status: ProductStatus
  isDeleted: boolean
  lowStockAlert: boolean
  lowStockThreshold: number
  rejectReason: string | null
  reviewStatus: number
}

// ── State ─────────────────────────────────────────────────────────
const loading = ref<boolean>(false)
const allProducts = ref<SellerProduct[]>([])
const categories = ref<Category[]>([])
const selectedRows = ref<SellerProduct[]>([])

// Tabs
const activeTab = ref<TabKey>((route.query.tab as TabKey) || 'all')

// 搜尋
const searchKeyword = ref<string>('')
const searchCategoryId = ref<number | null>(null)
const showAdvanced = ref<boolean>(false)
const advMinPrice = ref<number | null>(null)
const advMaxPrice = ref<number | null>(null)

// 排序
const sortField = ref<SortField>('createdAt')
const sortDir = ref<SortDir>('desc')

// 顯示模式
const viewMode = ref<'grid' | 'list'>('grid')

// 分頁
const pagination = reactive({ page: 1, pageSize: 20 })

// Dialog — 低庫存
const stockDialogVisible = ref<boolean>(false)
const stockDialogProduct = ref<SellerProduct | null>(null)
const stockForm = reactive({ threshold: 0, enabled: false })

// ── 常數 ─────────────────────────────────────────────────────────
const level1Tabs: Array<{ key: TabKey; label: string }> = [
  { key: 'all',      label: '全部' },
  { key: 'on',       label: '架上商品' },
  { key: 'off',      label: '未上架' },
  { key: 'rejected', label: '已退回' },
  { key: 'review',   label: '審核中' },
  { key: 'draft',    label: '草稿' },
  { key: 'deleted',  label: '違規/刪除' },
]

const sortOptions: Array<{ field: string; label: string }> = [
  { field: 'minPrice',   label: '價格' },
  { field: 'createdAt',  label: '建立時間' },
  // TODO: totalStock / soldCount 後端尚未回傳，排序暫時停用
]

// ── Computed ──────────────────────────────────────────────────────

/** Step 1：依一級 Tab 篩選 */
const tabFiltered = computed<SellerProduct[]>(() => {
  let list = allProducts.value

  if (activeTab.value !== 'all') {
    list = list.filter((p) => p.status === activeTab.value)
  }

  return list
})

/** Step 2：依搜尋條件 + 進階篩選過濾 */
const filteredProducts = computed<SellerProduct[]>(() => {
  let list = tabFiltered.value

  const kw = searchKeyword.value.trim().toLowerCase()
  if (kw) {
    list = list.filter(
      (p) => p.name.toLowerCase().includes(kw) || String(p.id).includes(kw),
    )
  }

  if (searchCategoryId.value) {
    const cat = categories.value.find((c) => c.id === searchCategoryId.value)
    if (cat) {
      list = list.filter((p) => p.categoryName === cat.name)
    }
  }

  if (advMinPrice.value !== null) {
    list = list.filter((p) => (p.minPrice ?? 0) >= (advMinPrice.value ?? 0))
  }
  if (advMaxPrice.value !== null) {
    list = list.filter((p) => (p.minPrice ?? 0) <= (advMaxPrice.value ?? Infinity))
  }

  return list
})

/** Step 3：分頁切片 */
const pagedProducts = computed<SellerProduct[]>(() => {
  const start = (pagination.page - 1) * pagination.pageSize
  return filteredProducts.value.slice(start, start + pagination.pageSize)
})

/** 各 Tab 計數 */
const tabCounts = computed<Record<TabKey, number>>(() => {
  const c: Record<string, number> = { all: 0, on: 0, off: 0, deleted: 0, review: 0, rejected: 0, draft: 0 }
  allProducts.value.forEach((p) => {
    c['all'] = (c['all'] ?? 0) + 1
    const prev = c[p.status]
    c[p.status] = (prev === undefined ? 0 : prev) + 1
  })
  return c as Record<TabKey, number>
})

// ── Init ──────────────────────────────────────────────────────────
const SESSION_STATE_KEY = 'sellerProductListState'

onMounted(async () => {
  // 1. 嘗試還原狀態
  restoreListState()
  
  await Promise.all([loadCategories(), loadProducts()])
})

/** 從 sessionStorage 還原搜尋/分頁狀態 */
function restoreListState(): void {
  const saved = sessionStorage.getItem(SESSION_STATE_KEY)
  if (!saved) return

  try {
    const state = JSON.parse(saved)
    if (state.activeTab) activeTab.value = state.activeTab
    if (state.searchKeyword) searchKeyword.value = state.searchKeyword
    if (state.searchCategoryId) searchCategoryId.value = state.searchCategoryId
    if (state.advMinPrice !== undefined) advMinPrice.value = state.advMinPrice
    if (state.advMaxPrice !== undefined) advMaxPrice.value = state.advMaxPrice
    if (state.sortField) sortField.value = state.sortField
    if (state.sortDir) sortDir.value = state.sortDir
    if (state.page) pagination.page = state.page
    if (state.viewMode) viewMode.value = state.viewMode
    console.log('[State] 已還原列表狀態:', state)
  } catch (e) {
    console.error('[State] 還原列表狀態失敗:', e)
  }
}

/** 儲存狀態至 sessionStorage */
function saveListState(): void {
  const state = {
    activeTab: activeTab.value,
    searchKeyword: searchKeyword.value,
    searchCategoryId: searchCategoryId.value,
    advMinPrice: advMinPrice.value,
    advMaxPrice: advMaxPrice.value,
    sortField: sortField.value,
    sortDir: sortDir.value,
    page: pagination.page,
    viewMode: viewMode.value,
  }
  sessionStorage.setItem(SESSION_STATE_KEY, JSON.stringify(state))
}

// 監聽所有狀態變動，自動儲存
watch(
  [activeTab, searchKeyword, searchCategoryId, advMinPrice, advMaxPrice, sortField, sortDir, () => pagination.page, viewMode],
  () => {
    saveListState()
  },
  { deep: true }
)

async function loadCategories(): Promise<void> {
  try {
    const res = await fetchMainCategories()
    if (res.success) categories.value = res.data
  } catch {
    // 靜默失敗，不阻塞頁面
  }
}

async function loadProducts(): Promise<void> {
  loading.value = true
  try {
    // 後端 SortOrder 接受組合字串（date_desc / date_asc / price_asc / price_desc）
    // 而非分開的 field + direction，需在前端組好後一起送
    let sortByParam: string
    if (sortField.value === 'minPrice') {
      sortByParam = sortDir.value === 'asc' ? 'price_asc' : 'price_desc'
    } else {
      sortByParam = sortDir.value === 'asc' ? 'date_asc' : 'date_desc'
    }

    const params: any = {
      page: 1,
      pageSize: 100,
      sortBy: sortByParam,
    }
    
    const res = await fetchSellerProducts(params)

    if (res.items.length > 0) {
      console.log('第一筆商品:', res.items[0])
      console.log('isDeleted 欄位:', res.items[0]?.isDeleted)
    }

    allProducts.value = res.items.map((p): SellerProduct => {
      const { status, ...rest } = p
      const isDeleted = p.isDeleted ?? false
      return {
        ...rest,
        status: isDeleted ? 'deleted' : mapStatusToKey(status, p.reviewStatus ?? 0),
        statusText: isDeleted ? '已刪除' : p.statusText,
        isDeleted,
        lowStockAlert: false,
        lowStockThreshold: 5,
        rejectReason: p.rejectReason ?? null,
        reviewStatus: p.reviewStatus ?? 0,
      }
    })
  } catch (err) {
    console.error('[API Error] loadProducts:', err)
    ElMessage.error('載入商品失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

// 監聽排序變更，自動重新載入
watch([sortField, sortDir], () => {
  loadProducts()
})

// 監聽網址 Query 變更（如瀏覽器上一頁/下一頁）
watch(
  () => route.query.tab,
  (newTab) => {
    if (newTab && newTab !== activeTab.value) {
      activeTab.value = newTab as TabKey
    }
  }
)

// ── Tab 事件 ──────────────────────────────────────────────────────
function onTabChange(val: TabKey): void {
  pagination.page = 1
  // 更新網址 Query，但不重新跳轉頁面
  router.replace({ query: { ...route.query, tab: val } })
}

// ── 搜尋 / 重設 ───────────────────────────────────────────────────
function handleSearch(): void {
  pagination.page = 1
}

function handleReset(): void {
  sessionStorage.removeItem(SESSION_STATE_KEY) // 清除暫存
  searchKeyword.value = ''
  searchCategoryId.value = null
  advMinPrice.value = null
  advMaxPrice.value = null
  showAdvanced.value = false
  sortField.value = 'createdAt'
  sortDir.value = 'desc'
  pagination.page = 1
}

// ── 排序 ──────────────────────────────────────────────────────────
function toggleSort(field: SortField): void {
  pagination.page = 1 // 關鍵：切換排序時重置頁碼

  if (sortField.value !== field) {
    sortField.value = field
    // 預設行為：價格用 asc，建立時間用 desc
    sortDir.value = field === 'createdAt' ? 'desc' : 'asc'
  } else {
    // 同欄位切換方向
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  }
}

function getSortIcon(field: SortField): object {
  if (sortField.value !== field) return DCaret
  return sortDir.value === 'asc' ? CaretTop : CaretBottom
}

// ── 編輯導向 ────────────────────────────────────────────────────
function handleEdit(id: number): void {
  router.push({
    path: `/seller/products/${id}/edit`,
    query: { fromTab: activeTab.value }
  })
}

// ── 卡片更多選單 ──────────────────────────────────────────────────
async function handleCardCommand(cmd: string, product: SellerProduct): Promise<void> {
  switch (cmd) {
    case 'preview': {
      const previewUrl = product.status === 'on'
        ? router.resolve({ name: 'ProductDetail', params: { id: product.id } }).href
        : router.resolve({ name: 'SellerProductPreview', params: { id: product.id } }).href
      window.open(previewUrl, '_blank')
      break
    }
    case 'submitReview':
      await handleSubmitReview(product)
      break
    case 'shelf':
      await handleToggleShelf(product)
      break
    case 'stock':
      openStockDialog(product)
      break
    case 'delete':
      await handleDeleteProduct(product)
      break
  }
}

// ── 刪除商品 ──────────────────────────────────────────────────────
async function handleDeleteProduct(product: SellerProduct): Promise<void> {
  try {
    await ElMessageBox.confirm(
      '確定要刪除此商品嗎？刪除後可在「違規/刪除」中查看',
      '刪除確認',
      {
        confirmButtonText: '確定刪除',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )
    
    await deleteSellerProduct(product.id)
    ElMessage.success('商品已刪除')

    // 本地即時更新：直接標記為已刪除
    // 不重新呼叫 API，因為後端可能過濾掉已刪除商品，導致商品從所有 tab 消失
    const idx = allProducts.value.findIndex(p => p.id === product.id)
    if (idx !== -1) {
      allProducts.value.splice(idx, 1, { ...allProducts.value[idx]!, isDeleted: true, status: 'deleted', statusText: '已刪除' })
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('刪除商品失敗:', error)
      ElMessage.error('刪除失敗，請稍後再試')
    }
  }
}

// ── 上/下架切換 ───────────────────────────────────────────────────
async function handleToggleShelf(product: SellerProduct): Promise<void> {
  // 只有已上架('on') 或 未上架-已審核('off') 可以切換
  // 草稿('draft') 必須先送審，不能直接上架
  if (product.status !== 'on' && product.status !== 'off') {
    ElMessage.warning('此商品狀態無法進行上下架操作')
    return
  }

  const newStatus: ProductStatus = product.status === 'on' ? 'off' : 'on'
  const newStatusNumber = newStatus === 'on' ? 1 : 0
  const actionText = newStatus === 'on' ? '上架' : '下架'

  try {
    await ElMessageBox.confirm(
      `確定要${actionText}「${product.name}」嗎？`,
      `${actionText}確認`,
      {
        confirmButtonText: '確定',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )

    await updateProductStatus(product.id, newStatusNumber)

    const idx = allProducts.value.findIndex(p => p.id === product.id)
    if (idx !== -1) {
      allProducts.value.splice(idx, 1, { ...allProducts.value[idx]!, status: newStatus, statusText: newStatus === 'on' ? '已上架' : '未上架' })
    }

    ElMessage.success(`商品已${actionText}`)
  } catch (error) {
    if (error !== 'cancel') {
      console.error(`${actionText}失敗:`, error)
      ElMessage.error(`${actionText}失敗，請稍後再試`)
    }
  }
}

// ── 草稿送審 ──────────────────────────────────────────────────────
async function handleSubmitReview(product: SellerProduct): Promise<void> {
  try {
    await ElMessageBox.confirm(
      `確定要送出「${product.name}」審核嗎？審核期間將無法編輯商品。`,
      '送審確認',
      {
        confirmButtonText: '送出審核',
        cancelButtonText: '取消',
        type: 'info',
      }
    )

    await submitProductForReview(product.id)

    const idx = allProducts.value.findIndex(p => p.id === product.id)
    if (idx !== -1) {
      allProducts.value.splice(idx, 1, { ...allProducts.value[idx]!, status: 'review', statusText: '審核中' })
    }

    ElMessage.success('已送出審核，請等待管理員審核')
  } catch (error) {
    if (error !== 'cancel') {
      console.error('送審失敗:', error)
      ElMessage.error('送審失敗，請稍後再試')
    }
  }
}

// ── 低庫存 Dialog ─────────────────────────────────────────────────
function openStockDialog(product: SellerProduct): void {
  stockDialogProduct.value = product
  stockForm.threshold = product.lowStockThreshold
  stockForm.enabled = product.lowStockAlert
  stockDialogVisible.value = true
}

function saveStockAlert(): void {
  if (!stockDialogProduct.value) return
  // TODO: 呼叫 PUT /api/seller/products/{id}/stock-alert { threshold, enabled }
  console.log(
    '[TODO] PUT /api/seller/products/' + stockDialogProduct.value.id + '/stock-alert',
    { ...stockForm },
  )
  stockDialogProduct.value.lowStockAlert = stockForm.enabled
  stockDialogProduct.value.lowStockThreshold = stockForm.threshold
  ElMessage.success('低庫存提醒設定已儲存（TODO: 串接後端）')
  stockDialogVisible.value = false
}

// ── Helpers ───────────────────────────────────────────────────────
function formatDate(dateString: string | undefined): string {
  if (!dateString) return '—'
  const d = new Date(dateString)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const STATUS_LABEL: Record<ProductStatus, string> = {
  on: '架上', off: '未上架', deleted: '已刪除', review: '審核中', rejected: '已退回', draft: '草稿',
}
function statusLabel(status: ProductStatus): string {
  return STATUS_LABEL[status] ?? status
}

function getStatusTagType(status: ProductStatus): 'success' | 'warning' | 'danger' | 'info' {
  switch (status) {
    case 'on': return 'success'
    case 'review': return 'warning'
    case 'rejected': return 'danger'
    case 'deleted': return 'danger'
    default: return 'info'
  }
}
</script>

<style scoped>
/* ─ 佈局 ─────────────────────────────────────────────────────── */
.product-list-page {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.main-card {
  border: 1px solid #e8eaf0 !important;
  border-radius: 8px !important;
}
/* 把 el-card 的 padding 壓掉，讓 tabs 可以貼邊 */
:deep(.main-card > .el-card__body) { padding: 0; }

/* ─ 一級 Tab ──────────────────────────────────────────────────── */
:deep(.level1-tabs) { padding: 0 20px; }
:deep(.level1-tabs .el-tabs__nav-wrap::after) { background: #f1f5f9; height: 1px; }
:deep(.level1-tabs .el-tabs__active-bar)      { background: #ee4d2d; }
:deep(.level1-tabs .el-tabs__item.is-active)  { color: #ee4d2d; font-weight: 700; }
:deep(.level1-tabs .el-tabs__item:hover)      { color: #ee4d2d; }

.tab-count {
  font-size: 12px;
  color: #94a3b8;
  margin-left: 3px;
}

/* ─ 搜尋列 ─────────────────────────────────────────────────────── */
.search-section {
  padding: 16px 20px 12px;
  border-bottom: 1px solid #f1f5f9;
}
.search-btn {
  border-color: #ee4d2d !important;
  color: #ee4d2d !important;
  background: white !important;
}
.search-btn:hover { background: #fff7ed !important; }

.advanced-filter { padding-top: 12px; }
.adv-label {
  display: block;
  font-size: 12px;
  color: #94a3b8;
  margin-bottom: 4px;
}
.result-count {
  font-size: 13px;
  color: #64748b;
  margin-top: 10px;
}
.count-num { color: #ee4d2d; }

/* ─ 排序列 ──────────────────────────────────────────────────────── */
.sort-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 20px;
  border-bottom: 1px solid #f1f5f9;
}
.sort-left {
  display: flex;
  align-items: center;
  gap: 2px;
  flex-wrap: wrap;
}
.sort-label { font-size: 13px; color: #94a3b8; margin-right: 6px; }
.sort-item {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  font-size: 13px;
  color: #64748b;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 4px;
  user-select: none;
  transition: all 0.15s;
}
.sort-item:hover { color: #ee4d2d; background: #fff7ed; }
.sort-item.sort-active { color: #ee4d2d; font-weight: 600; }
.sort-divider { color: #e2e8f0; margin-right: 4px; }

.view-toggle { display: flex; gap: 4px; }
.toggle-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #e8eaf0;
  border-radius: 4px;
  background: white;
  color: #94a3b8;
  cursor: pointer;
  transition: all 0.15s;
}
.toggle-btn:hover { border-color: #ee4d2d; color: #ee4d2d; }
.toggle-btn.active { border-color: #ee4d2d; color: #ee4d2d; background: #fff7ed; }

/* ─ 商品區域 ────────────────────────────────────────────────────── */
.product-area {
  padding: 16px 20px;
  min-height: 240px;
}

/* ─ 網格模式 ─────────────────────────────────────────────────────── */
.product-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 12px;
}

/* ─ 商品卡片 ─────────────────────────────────────────────────────── */
.product-card {
  border: 1px solid #e8eaf0;
  border-radius: 8px;
  overflow: hidden;
  background: white;
  display: flex;
  flex-direction: column;
  transition: all 0.2s;
  cursor: default;
}
.product-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
  border-color: #ee4d2d;
}
.product-card-deleted {
  opacity: 0.55;
  filter: grayscale(40%);
}
.product-card-deleted:hover {
  box-shadow: none;
  transform: none;
  border-color: #e8eaf0;
}
.product-card-deleted .card-img-wrap::after {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.12);
  pointer-events: none;
}

.card-img-wrap {
  position: relative;
  width: 100%;
  padding-top: 100%; /* 1:1 */
}
.card-img {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
}

/* 狀態徽章 */
.status-badge {
  position: absolute;
  top: 6px;
  left: 6px;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
  font-weight: 600;
  pointer-events: none;
}
.badge-on       { background: #dcfce7; color: #16a34a; }
.badge-off      { background: #f1f5f9; color: #64748b; }
.badge-deleted  { background: #fee2e2; color: #dc2626; }
.badge-review   { background: #fef3c7; color: #d97706; }
.badge-rejected { background: #ffe4e6; color: #e11d48; }
.badge-draft    { background: #f1f5f9; color: #64748b; }

.card-body { padding: 8px 10px; flex: 1; }

/* 退回原因橫幅 */
.reject-banner {
  display: flex;
  align-items: flex-start;
  gap: 4px;
  background: #fff1f2;
  border: 1px solid #fecdd3;
  border-radius: 4px;
  padding: 5px 8px;
  font-size: 11px;
  color: #e11d48;
  margin-bottom: 6px;
  line-height: 1.4;
}
.reject-banner .el-icon {
  flex-shrink: 0;
  margin-top: 1px;
}

/* 重新編輯按鈕 — 橘色強調 */
.resubmit-btn { color: #ee4d2d !important; font-weight: 600; }
.resubmit-btn:hover { background: #fff7ed !important; }

.card-name {
  font-size: 13px;
  color: #334155;
  line-height: 1.4;
  margin: 0 0 6px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.card-price {
  font-size: 14px;
  font-weight: 700;
  color: #ee4d2d;
  margin-bottom: 4px;
}
.card-stock {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 4px;
}
.card-stats {
  display: flex;
  gap: 8px;
  font-size: 11px;
  color: #94a3b8;
}
.card-date {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 6px;
  padding-top: 6px;
  border-top: 1px dashed #f1f5f9;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 8px;
  border-top: 1px solid #f1f5f9;
  background: #fafafa;
}
.card-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  font-size: 12px;
  border: none;
  background: none;
  cursor: pointer;
  padding: 4px 6px;
  border-radius: 4px;
  transition: all 0.15s;
}
.edit-btn { color: #64748b; }
.edit-btn:hover { color: #ee4d2d; background: #fff7ed; }
.more-btn { color: #94a3b8; }
.more-btn:hover { color: #ee4d2d; background: #fff7ed; }
.deleted-footer-label {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #dc2626;
  padding: 4px 6px;
}

/* ─ 列表模式 ─────────────────────────────────────────────────────── */
.table-product-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}
.table-thumb {
  width: 56px;
  height: 56px;
  border-radius: 4px;
  border: 1px solid #f1f5f9;
  flex-shrink: 0;
}
.table-info { min-width: 0; }
.table-name {
  font-size: 13px;
  color: #334155;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 180px;
}
.table-id { font-size: 11px; color: #94a3b8; margin-top: 2px; }
.table-reject-reason {
  display: flex;
  align-items: center;
  gap: 3px;
  font-size: 11px;
  color: #e11d48;
  margin-top: 3px;
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* ─ 分頁 ─────────────────────────────────────────────────────────── */
.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  padding: 12px 20px 16px;
  border-top: 1px solid #f1f5f9;
}

/* ─ Dialog ───────────────────────────────────────────────────────── */
.dialog-hint {
  color: #64748b;
  font-size: 13px;
  line-height: 1.6;
  margin-bottom: 16px;
}
.dialog-product-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f8fafc;
  border-radius: 8px;
  margin-bottom: 20px;
}
.dialog-img {
  width: 48px;
  height: 48px;
  border-radius: 4px;
  flex-shrink: 0;
}
.dialog-product-name {
  font-size: 14px;
  color: #334155;
  font-weight: 500;
}

.delete-dialog-body {
  text-align: center;
  padding: 12px 0 8px;
}
.delete-dialog-body p {
  color: #334155;
  font-size: 15px;
  line-height: 1.8;
  margin-top: 12px;
}
.hint-text { font-size: 13px; color: #94a3b8; }

/* ─ Helpers ─────────────────────────────────────────────────────── */
.text-danger { color: #ef4444 !important; }

/* ─ 響應式 ────────────────────────────────────────────────────────── */
@media (max-width: 1200px) {
  .product-grid { grid-template-columns: repeat(4, 1fr); }
}
@media (max-width: 900px) {
  .product-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 600px) {
  .product-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>
