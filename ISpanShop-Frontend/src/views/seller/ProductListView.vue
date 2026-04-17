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

      <!-- ── 二級 Tab（僅架上商品顯示）── -->
      <div v-if="activeTab === 'on'" class="level2-wrap">
        <el-tabs v-model="activeSubTab" class="level2-tabs" @tab-change="onSubTabChange">
          <el-tab-pane name="all" label="全部" />
          <el-tab-pane name="restock">
            <template #label>
              重新補貨<span class="tab-count tab-count-orange">({{ restockCount }})</span>
            </template>
          </el-tab-pane>
          <el-tab-pane name="optimize">
            <template #label>
              商品內容優化 <el-tag size="small" type="info" style="margin-left:4px">TODO</el-tag>
            </template>
          </el-tab-pane>
        </el-tabs>
      </div>

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
            >
              <!-- 圖片 -->
              <div class="card-img-wrap">
                <el-image
                  :src="product.imageUrl || 'https://via.placeholder.com/200'"
                  fit="cover"
                  class="card-img"
                  lazy
                />
                <div class="status-badge" :class="`badge-${product.status}`">
                  {{ statusLabel(product.status) }}
                </div>
              </div>

              <!-- 商品資訊 -->
              <div class="card-body">
                <p class="card-name">{{ product.name }}</p>
                <div class="card-price">NT$ {{ product.price.toLocaleString() }}</div>
                <div class="card-stock">
                  商品數量：
                  <span :class="{ 'text-danger': product.totalStock === 0 }">
                    {{ product.totalStock }}
                  </span>
                </div>
                <div class="card-stats">
                  <span title="瀏覽數">👁 {{ product.viewCount }}</span>
                  <span title="收藏數">❤️ {{ product.favoriteCount }}</span>
                  <span title="訂單數">🛒 {{ product.soldCount }}</span>
                </div>
              </div>

              <!-- 操作列 -->
              <div class="card-footer">
                <button
                  class="card-action-btn edit-btn"
                  @click="router.push(`/seller/products/${product.id}/edit`)"
                >
                  <el-icon :size="13"><Edit /></el-icon> 編輯
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
                      <el-dropdown-item command="copy">複製</el-dropdown-item>
                      <el-dropdown-item command="preview">即時預覽</el-dropdown-item>
                      <el-dropdown-item command="shelf">
                        {{ product.status === 'on' ? '下架' : '上架' }}
                      </el-dropdown-item>
                      <el-dropdown-item command="stock">低庫存提醒</el-dropdown-item>
                      <el-dropdown-item command="delete" divided>
                        <span class="text-danger">刪除</span>
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </div>
            </div>
          </div>

          <el-empty
            v-else
            description="此分類目前沒有商品"
            :image-size="100"
            style="padding: 40px 0"
          >
            <el-button type="primary" @click="router.push('/seller/products/new')">
              新增商品
            </el-button>
          </el-empty>
        </template>

        <!-- 列表模式 -->
        <el-table
          v-else
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
                  :src="row.imageUrl || 'https://via.placeholder.com/56'"
                  fit="cover"
                  class="table-thumb"
                />
                <div class="table-info">
                  <div class="table-name">{{ row.name }}</div>
                  <div class="table-id">ID: {{ row.id }}</div>
                </div>
              </div>
            </template>
          </el-table-column>

          <el-table-column prop="soldCount" label="已售出" width="90" align="center" />

          <el-table-column label="商品數量" width="100" align="center">
            <template #default="{ row }">
              <span :class="{ 'text-danger': row.totalStock === 0 }">
                {{ row.totalStock }}
              </span>
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

          <el-table-column label="狀態" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.status === 'on' ? 'success' : 'info'" size="small">
                {{ statusLabel(row.status) }}
              </el-tag>
            </template>
          </el-table-column>

          <el-table-column label="操作" width="160" align="center" fixed="right">
            <template #default="{ row }">
              <el-button
                text type="primary" size="small"
                @click="router.push(`/seller/products/${row.id}/edit`)"
              >
                <el-icon><Edit /></el-icon> 編輯
              </el-button>
              <el-popconfirm
                title="確定要刪除這個商品嗎？"
                confirm-button-text="確定"
                cancel-button-text="取消"
                @confirm="handleDelete(row.id)"
              >
                <template #reference>
                  <el-button text type="danger" size="small">
                    <el-icon><Delete /></el-icon>
                  </el-button>
                </template>
              </el-popconfirm>
            </template>
          </el-table-column>
        </el-table>
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
          :src="stockDialogProduct.imageUrl || 'https://via.placeholder.com/48'"
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

    <!-- ── 刪除確認 Dialog（卡片更多選單觸發）── -->
    <el-dialog v-model="deleteDialogVisible" title="確認刪除" width="400px">
      <div class="delete-dialog-body">
        <el-icon :size="48" color="#ef4444"><WarningFilled /></el-icon>
        <p>
          確定要刪除商品<br />
          「{{ deleteTargetProduct?.name }}」嗎？<br />
          <span class="hint-text">此操作無法復原。</span>
        </p>
      </div>
      <template #footer>
        <el-button @click="deleteDialogVisible = false">取消</el-button>
        <el-button type="danger" @click="confirmDelete">確定刪除</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  Plus, Search, Edit, Delete, Grid, List,
  ArrowDown, ArrowUp, DCaret, CaretTop, CaretBottom,
  MoreFilled, WarningFilled,
} from '@element-plus/icons-vue'
import { fetchProductList } from '@/api/product'
import { fetchMainCategories } from '@/api/category'
import type { ProductListItem } from '@/types/product'
import type { Category } from '@/types/category'

const router = useRouter()

// ── 型別定義 ──────────────────────────────────────────────────────
type ProductStatus = 'on' | 'off' | 'deleted' | 'review' | 'draft'
type TabKey = 'all' | 'on' | 'deleted' | 'review' | 'draft'
type SubTabKey = 'all' | 'restock' | 'optimize'
type SortField = 'price' | 'totalStock' | 'soldCount'
type SortDir = 'asc' | 'desc' | null

/** 擴充 ProductListItem，加入賣家專用欄位 */
interface SellerProduct extends ProductListItem {
  status: ProductStatus
  viewCount: number
  favoriteCount: number
  lowStockAlert: boolean
  lowStockThreshold: number
}

// ── State ─────────────────────────────────────────────────────────
const loading = ref<boolean>(false)
const allProducts = ref<SellerProduct[]>([])
const categories = ref<Category[]>([])
const selectedRows = ref<SellerProduct[]>([])

// Tabs
const activeTab = ref<TabKey>('all')
const activeSubTab = ref<SubTabKey>('all')

// 搜尋
const searchKeyword = ref<string>('')
const searchCategoryId = ref<number | null>(null)
const showAdvanced = ref<boolean>(false)
const advMinPrice = ref<number | null>(null)
const advMaxPrice = ref<number | null>(null)

// 排序
const sortField = ref<SortField | null>(null)
const sortDir = ref<SortDir>(null)

// 顯示模式
const viewMode = ref<'grid' | 'list'>('grid')

// 分頁
const pagination = reactive({ page: 1, pageSize: 20 })

// Dialog — 低庫存
const stockDialogVisible = ref<boolean>(false)
const stockDialogProduct = ref<SellerProduct | null>(null)
const stockForm = reactive({ threshold: 0, enabled: false })

// Dialog — 刪除
const deleteDialogVisible = ref<boolean>(false)
const deleteTargetProduct = ref<SellerProduct | null>(null)

// ── 常數 ─────────────────────────────────────────────────────────
const level1Tabs: Array<{ key: TabKey; label: string }> = [
  { key: 'all',     label: '全部' },
  { key: 'on',      label: '架上商品' },
  { key: 'deleted', label: '違規/刪除' },
  { key: 'review',  label: '審核中' },
  { key: 'draft',   label: '未上架/尚未刊登' },
]

const sortOptions: Array<{ field: string; label: string }> = [
  { field: 'price',      label: '價格' },
  { field: 'totalStock', label: '商品數量' },
  { field: 'soldCount',  label: '月銷熱賣' },
]

// ── Computed ──────────────────────────────────────────────────────

/** Step 1：依一/二級 Tab 篩選 */
const tabFiltered = computed<SellerProduct[]>(() => {
  let list = allProducts.value

  if (activeTab.value !== 'all') {
    list = list.filter((p) => p.status === activeTab.value)
  }

  // 二級 tab — 重新補貨
  if (activeTab.value === 'on' && activeSubTab.value === 'restock') {
    list = list.filter((p) => p.totalStock === 0)
  }
  // TODO: 二級 tab — 商品內容優化，後端尚未提供優化建議資料

  return list
})

/** Step 2：依搜尋條件 + 進階篩選過濾，並排序 */
const filteredProducts = computed<SellerProduct[]>(() => {
  let list = tabFiltered.value

  const kw = searchKeyword.value.trim().toLowerCase()
  if (kw) {
    list = list.filter(
      (p) => p.name.toLowerCase().includes(kw) || String(p.id).includes(kw),
    )
  }

  if (searchCategoryId.value) {
    list = list.filter((p) => p.categoryId === searchCategoryId.value)
  }

  if (advMinPrice.value !== null) {
    list = list.filter((p) => p.price >= (advMinPrice.value ?? 0))
  }
  if (advMaxPrice.value !== null) {
    list = list.filter((p) => p.price <= (advMaxPrice.value ?? Infinity))
  }

  // 排序
  if (sortField.value && sortDir.value) {
    const f = sortField.value
    const d = sortDir.value
    list = [...list].sort((a, b) => {
      const av = a[f] as number
      const bv = b[f] as number
      return d === 'asc' ? av - bv : bv - av
    })
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
  const c: Record<string, number> = { all: 0, on: 0, deleted: 0, review: 0, draft: 0 }
  allProducts.value.forEach((p) => {
    c['all'] = (c['all'] ?? 0) + 1
    const prev = c[p.status]
    c[p.status] = (prev === undefined ? 0 : prev) + 1
  })
  return c as Record<TabKey, number>
})

/** 重新補貨計數（架上商品中 totalStock === 0）*/
const restockCount = computed<number>(() =>
  allProducts.value.filter((p) => p.status === 'on' && p.totalStock === 0).length,
)

// ── Init ──────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadCategories(), loadProducts()])
})

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
    // TODO: 後端應實作 GET /api/seller/products，僅回傳當前登入賣家的商品
    //       目前暫時呼叫公開的 GET /api/products，並在前端模擬賣家專用欄位
    const res = await fetchProductList({ page: 1, pageSize: 100 })

    if (res.success) {
      console.log('[API Response] GET /api/products:', res.data)

      // 模擬賣家商品狀態分佈（串接後端後移除此段，直接使用後端回傳的 status）
      const statusPool: ProductStatus[] = ['on', 'on', 'on', 'off', 'draft', 'review']

      allProducts.value = res.data.items.map((p: ProductListItem, idx: number): SellerProduct => ({
        ...p,
        status: statusPool[idx % statusPool.length] ?? 'on',
        viewCount: Math.floor(Math.random() * 500),
        favoriteCount: Math.floor(Math.random() * 100),
        lowStockAlert: false,
        lowStockThreshold: 5,
      }))
    }
  } catch (err) {
    console.error('[API Error] loadProducts:', err)
    ElMessage.error('載入商品失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

// ── Tab 事件 ──────────────────────────────────────────────────────
function onTabChange(): void {
  activeSubTab.value = 'all'
  pagination.page = 1
}

function onSubTabChange(): void {
  pagination.page = 1
}

// ── 搜尋 / 重設 ───────────────────────────────────────────────────
function handleSearch(): void {
  pagination.page = 1
}

function handleReset(): void {
  searchKeyword.value = ''
  searchCategoryId.value = null
  advMinPrice.value = null
  advMaxPrice.value = null
  showAdvanced.value = false
  sortField.value = null
  sortDir.value = null
  pagination.page = 1
}

// ── 排序 ──────────────────────────────────────────────────────────
function toggleSort(field: SortField): void {
  if (sortField.value !== field) {
    sortField.value = field
    sortDir.value = 'asc'
  } else if (sortDir.value === 'asc') {
    sortDir.value = 'desc'
  } else {
    sortField.value = null
    sortDir.value = null
  }
}

function getSortIcon(field: SortField): object {
  if (sortField.value !== field) return DCaret
  return sortDir.value === 'asc' ? CaretTop : CaretBottom
}

// ── 卡片更多選單 ──────────────────────────────────────────────────
function handleCardCommand(cmd: string, product: SellerProduct): void {
  switch (cmd) {
    case 'copy':
      // TODO: 呼叫 POST /api/seller/products/{id}/copy 複製商品
      console.log('[TODO] POST /api/seller/products/' + product.id + '/copy')
      ElMessage.info('複製功能待後端 API 實作')
      break
    case 'preview':
      window.open(`/product/${product.id}`, '_blank')
      break
    case 'shelf':
      handleToggleShelf(product)
      break
    case 'stock':
      openStockDialog(product)
      break
    case 'delete':
      deleteTargetProduct.value = product
      deleteDialogVisible.value = true
      break
  }
}

// ── 上/下架切換 ───────────────────────────────────────────────────
function handleToggleShelf(product: SellerProduct): void {
  const newStatus: ProductStatus = product.status === 'on' ? 'off' : 'on'
  // TODO: 呼叫 PUT /api/seller/products/{id}/shelf { isOnShelf: newStatus === 'on' }
  console.log('[TODO] PUT /api/seller/products/' + product.id + '/shelf', {
    isOnShelf: newStatus === 'on',
  })
  product.status = newStatus
  ElMessage.success(newStatus === 'on' ? '商品已上架' : '商品已下架')
}

// ── 刪除 ──────────────────────────────────────────────────────────
function handleDelete(id: number): void {
  // TODO: 呼叫 DELETE /api/seller/products/{id}
  console.log('[TODO] DELETE /api/seller/products/' + id)
  allProducts.value = allProducts.value.filter((p) => p.id !== id)
  ElMessage.success('商品已刪除（TODO: 串接後端）')
}

function confirmDelete(): void {
  if (!deleteTargetProduct.value) return
  handleDelete(deleteTargetProduct.value.id)
  deleteDialogVisible.value = false
  deleteTargetProduct.value = null
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
const STATUS_LABEL: Record<ProductStatus, string> = {
  on: '架上', off: '下架', deleted: '已刪除', review: '審核中', draft: '草稿',
}
function statusLabel(status: ProductStatus): string {
  return STATUS_LABEL[status] ?? status
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

/* ─ 二級 Tab ──────────────────────────────────────────────────── */
.level2-wrap {
  background: #f8fafc;
  border-top: 1px solid #f1f5f9;
  padding: 0 20px;
}
:deep(.level2-tabs .el-tabs__nav-wrap::after) { display: none; }
:deep(.level2-tabs .el-tabs__active-bar)      { background: #ee4d2d; height: 2px; }
:deep(.level2-tabs .el-tabs__item.is-active)  { color: #ee4d2d; font-weight: 600; }
:deep(.level2-tabs .el-tabs__item:hover)      { color: #ee4d2d; }
:deep(.level2-tabs .el-tabs__header)          { margin: 0; }

.tab-count {
  font-size: 12px;
  color: #94a3b8;
  margin-left: 3px;
}
.tab-count-orange { color: #ee4d2d; }

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
.badge-on      { background: #dcfce7; color: #16a34a; }
.badge-off     { background: #f1f5f9; color: #64748b; }
.badge-deleted { background: #fee2e2; color: #dc2626; }
.badge-review  { background: #fef3c7; color: #d97706; }
.badge-draft   { background: #f1f5f9; color: #64748b; }

.card-body { padding: 8px 10px; flex: 1; }

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
