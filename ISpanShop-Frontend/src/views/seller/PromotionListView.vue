<template>
  <div class="promotion-list-page">
    <!-- 頁面標題列 -->
    <div class="page-header">
      <h1 class="page-title">我的行銷活動</h1>
      <el-button type="primary" @click="openCreateDialog">
        <el-icon style="margin-right: 6px;"><Plus /></el-icon>新增活動
      </el-button>
    </div>

    <!-- 統計卡片列 -->
    <el-row :gutter="16" class="stats-row">
      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card stat-pending" @click="filterByStatus('pending')">
          <div class="stat-label">待審核</div>
          <div class="stat-value">{{ stats.pending }}</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card stat-active" @click="filterByStatus('active')">
          <div class="stat-label">進行中</div>
          <div class="stat-value">{{ stats.active }}</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card stat-upcoming" @click="filterByStatus('upcoming')">
          <div class="stat-label">即將開始</div>
          <div class="stat-value">{{ stats.upcoming }}</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card stat-ended" @click="filterByStatus('ended')">
          <div class="stat-label">已結束/已拒絕</div>
          <div class="stat-value">{{ stats.ended }}</div>
        </div>
      </el-col>
    </el-row>

    <!-- Tab 列 -->
    <el-tabs v-model="activeTab" @tab-click="handleTabChange" class="promo-tabs">
      <el-tab-pane label="全部" name="all">
        <template #label>
          全部 <span class="tab-count">({{ allPromotions.length }})</span>
        </template>
      </el-tab-pane>
      <el-tab-pane label="待審核" name="pending">
        <template #label>
          待審核 <span class="tab-count">({{ stats.pending }})</span>
        </template>
      </el-tab-pane>
      <el-tab-pane label="進行中" name="active">
        <template #label>
          進行中 <span class="tab-count">({{ stats.active }})</span>
        </template>
      </el-tab-pane>
      <el-tab-pane label="即將開始" name="upcoming">
        <template #label>
          即將開始 <span class="tab-count">({{ stats.upcoming }})</span>
        </template>
      </el-tab-pane>
      <el-tab-pane label="已拒絕" name="rejected">
        <template #label>
          已拒絕 <span class="tab-count">({{ stats.rejected }})</span>
        </template>
      </el-tab-pane>
      <el-tab-pane label="已結束" name="ended">
        <template #label>
          已結束 <span class="tab-count">({{ stats.endedOnly }})</span>
        </template>
      </el-tab-pane>
    </el-tabs>

    <!-- 活動列表 -->
    <el-table
      v-loading="loading"
      :data="paginatedPromotions"
      stripe
      style="width: 100%"
      class="promo-table"
    >
      <el-table-column prop="name" label="活動名稱" min-width="180" />
      <el-table-column label="類型" width="120">
        <template #default="{ row }">
          <el-tag :type="getTypeTagColor(row.promotionType)" size="small">{{ row.promotionTypeLabel }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="折扣資訊" min-width="150">
        <template #default="{ row }">
          <div v-if="row.promotionType === 1">
            折扣：{{ row.discountValue ?? '--' }}% off
          </div>
          <div v-else-if="row.promotionType === 2">
            <div>滿 {{ row.minimumAmount ?? '--' }} 折 {{ row.discountValue ?? '--' }} 元</div>
          </div>
          <div v-else-if="row.promotionType === 3">
            折扣：{{ row.discountValue ?? '--' }} 元
          </div>
          <div v-else>--</div>
        </template>
      </el-table-column>
      <el-table-column label="時間區間" min-width="280">
        <template #default="{ row }">
          {{ formatDateRange(row.startTime, row.endTime) }}
        </template>
      </el-table-column>
      <el-table-column label="狀態" width="100">
        <template #default="{ row }">
          <el-tag :type="getStatusTagColor(row.status)" size="small">{{ row.statusText }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="審核備註" min-width="150">
        <template #default="{ row }">
          <span v-if="row.rejectReason" class="reject-reason">{{ row.rejectReason }}</span>
          <span v-else class="text-muted">—</span>
        </template>
      </el-table-column>
      <el-table-column label="商品數" width="80" align="center">
        <template #default="{ row }">
          {{ row.productCount ?? '--' }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="canEdit(row.statusText)"
            link
            type="primary"
            size="small"
            @click="openEditDialog(row)"
          >編輯</el-button>
          <el-button
            v-if="canDelete(row.statusText)"
            link
            type="danger"
            size="small"
            @click="handleDelete(row.id)"
          >刪除</el-button>
          <span v-if="!canEdit(row.statusText) && !canDelete(row.statusText)" class="text-muted">—</span>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分頁 -->
    <div class="pagination-wrap">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
      />
    </div>

    <!-- 新增/編輯活動彈窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '編輯活動' : '新增活動'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
      >
        <el-form-item label="活動名稱" prop="name">
          <el-input v-model="formData.name" placeholder="請輸入活動名稱" maxlength="50" show-word-limit />
        </el-form-item>
        <el-form-item label="活動描述" prop="description">
          <el-input
            v-model="formData.description"
            type="textarea"
            :rows="3"
            placeholder="請輸入活動描述（選填）"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="活動類型" prop="promotionType">
          <el-select v-model="formData.promotionType" placeholder="請選擇活動類型" style="width: 100%;">
            <el-option label="限時特賣" :value="1" />
            <el-option label="滿額折扣" :value="2" />
            <el-option label="限量搶購" :value="3" />
            <el-option label="新品優惠" :value="4" />
          </el-select>
        </el-form-item>
        
        <!-- 滿額折扣：顯示滿額門檻 + 折扣金額 -->
        <template v-if="formData.promotionType === 2">
          <el-form-item label="滿額門檻" prop="minimumAmount">
            <el-input-number
              v-model="formData.minimumAmount"
              :min="0"
              :precision="0"
              :controls="true"
              :step="100"
              placeholder="例如：1000 代表消費滿 1000 元"
              style="width: 100%;"
            />
            <span class="form-hint">消費滿多少元才能使用此折扣</span>
          </el-form-item>
          <el-form-item label="折扣金額" prop="discountValue">
            <el-input-number
              v-model="formData.discountValue"
              :min="0"
              :max="999999"
              :precision="0"
              :controls="true"
              :step="10"
              placeholder="例如：100 代表折扣 100 元"
              style="width: 100%;"
            />
            <span class="form-hint">例如：100 代表滿額折 100 元</span>
          </el-form-item>
        </template>
        
        <!-- 限時特賣：顯示折扣比例 -->
        <template v-else-if="formData.promotionType === 1">
          <el-form-item label="折扣(%off)" prop="discountValue">
            <el-input-number
              v-model="formData.discountValue"
              :min="1"
              :max="99"
              :precision="0"
              :controls="true"
              :step="5"
              placeholder="例如：20 代表打 8 折"
              style="width: 100%;"
            />
            <span class="form-hint">例如：20 代表打 8 折（20% off）</span>
          </el-form-item>
        </template>
        
        <!-- 限量搶購：顯示限量數量 + 折扣金額 -->
        <template v-else-if="formData.promotionType === 3">
          <el-form-item label="限量數量" prop="limitQuantity">
            <el-input-number
              v-model="formData.limitQuantity"
              :min="1"
              :precision="0"
              :controls="true"
              :step="10"
              placeholder="例如：100 代表限量 100 件"
              style="width: 100%;"
            />
            <span class="form-hint">此活動限量多少件商品</span>
          </el-form-item>
          <el-form-item label="折扣金額" prop="discountValue">
            <el-input-number
              v-model="formData.discountValue"
              :min="0"
              :max="999999"
              :precision="0"
              :controls="true"
              :step="10"
              placeholder="例如：100 代表每件折 100 元"
              style="width: 100%;"
            />
            <span class="form-hint">例如：100 代表每件折 100 元</span>
          </el-form-item>
        </template>
        
        <!-- 新品優惠或其他類型：只顯示折扣金額 -->
        <template v-else-if="formData.promotionType === 4 || formData.promotionType > 0">
          <el-form-item label="折扣金額" prop="discountValue">
            <el-input-number
              v-model="formData.discountValue"
              :min="0"
              :max="999999"
              :precision="0"
              :controls="true"
              :step="10"
              placeholder="請輸入折扣金額"
              style="width: 100%;"
            />
            <span class="form-hint">折扣金額（元）</span>
          </el-form-item>
        </template>
        <el-form-item label="開始時間" prop="startTime">
          <el-date-picker
            v-model="formData.startTime"
            type="datetime"
            placeholder="選擇開始時間"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss"
            style="width: 100%;"
          />
        </el-form-item>
        <el-form-item label="結束時間" prop="endTime">
          <el-date-picker
            v-model="formData.endTime"
            type="datetime"
            placeholder="選擇結束時間"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss"
            style="width: 100%;"
          />
        </el-form-item>

        <!-- 活動商品 -->
        <el-divider content-position="left" style="margin: 16px 0 12px;">活動商品</el-divider>
        <el-form-item label-width="0" style="margin-bottom: 0;">
          <div style="width: 100%;">
            <el-button size="small" type="primary" plain @click="openProductSelector">
              <el-icon><Plus /></el-icon>&nbsp;新增商品
            </el-button>
            <!-- 已選商品卡片 -->
            <div v-if="selectedProducts.length > 0" class="selected-products">
              <div v-for="p in selectedProducts" :key="p.productId" class="product-card">
                <img :src="p.imageUrl ?? ''" class="product-thumb" />
                <div class="product-info">
                  <div class="product-name">{{ p.productName }}</div>
                  <div class="product-price">NT$ {{ p.minPrice ?? p.originalPrice }}</div>
                </div>
                <el-button link type="danger" size="small" @click="removeProduct(p.productId)">移除</el-button>
              </div>
            </div>
            <div v-else class="no-products-hint">尚未選擇商品（選填）</div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          {{ isEdit ? '更新活動' : '送出審核' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 商品選擇器 Dialog -->
    <el-dialog
      v-model="selectorVisible"
      title="選擇活動商品"
      width="720px"
      :close-on-click-modal="false"
      append-to-body
    >
      <div class="selector-toolbar">
        <el-input
          v-model="selectorKeyword"
          placeholder="搜尋商品名稱"
          clearable
          style="width: 240px;"
          @change="onSelectorSearch"
          @clear="onSelectorSearch"
        />
      </div>
      <el-table
        ref="selectorTableRef"
        v-loading="selectorLoading"
        :data="selectorProducts"
        @selection-change="handleSelectorSelectionChange"
        max-height="400"
        style="width: 100%; margin-top: 12px;"
        row-key="id"
      >
        <el-table-column type="selection" width="50" />
        <el-table-column label="商品" min-width="280">
          <template #default="{ row }">
            <div class="selector-product-row">
              <img :src="row.imageUrl ?? ''" class="selector-thumb" />
              <span>{{ row.name }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="售價" width="120">
          <template #default="{ row }">
            NT$ {{ row.minPrice ?? '--' }}
          </template>
        </el-table-column>
        <el-table-column label="庫存" width="80" align="center">
          <template #default="{ row }">
            {{ row.totalStock ?? '--' }}
          </template>
        </el-table-column>
      </el-table>
      <div class="selector-pagination">
        <el-pagination
          v-model:current-page="selectorPage"
          :total="selectorTotal"
          :page-size="selectorPageSize"
          layout="prev, pager, next, total"
          @current-change="loadSelectorProducts"
        />
      </div>
      <template #footer>
        <el-button @click="selectorVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmProductSelection">
          確認（已選 {{ pendingSelection.length }} 件）
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, ElTable } from 'element-plus'
import {
  fetchSellerPromotions,
  createSellerPromotion,
  updateSellerPromotion,
  deleteSellerPromotion,
  fetchPromotionProducts,
  bindPromotionProducts,
  unbindPromotionProduct,
  fetchAvailableProductsForPromotion,
  fetchSellerProductsSimple,
} from '@/api/promotion'

// ─── 介面定義 ─────────────────────────────────────────────────────

interface SellerPromotion {
  id: number
  name: string
  description: string | null
  promotionType: number
  promotionTypeLabel: string
  discountValue?: number
  minimumAmount?: number
  limitQuantity?: number
  productCount?: number
  startTime: string
  endTime: string
  status: number
  statusText: string
  rejectReason: string | null
  createdAt: string
  reviewedAt: string | null
}

interface AvailableProduct {
  id: number
  name: string
  imageUrl: string | null
  minPrice: number | null
  maxPrice: number | null
  totalStock: number | null
  status: number
}

interface PromotionProduct {
  productId: number
  productName: string
  imageUrl: string | null
  minPrice: number | null
  originalPrice: number
  discountPrice: number | null
}

interface PromotionFormData {
  name: string
  description: string
  promotionType: number
  discountValue: number
  minimumAmount: number  // 滿額門檻
  limitQuantity: number  // 限量數量
  startTime: string
  endTime: string
}

// ─── 狀態管理 ─────────────────────────────────────────────────────

const loading = ref(false)
const allPromotions = ref<SellerPromotion[]>([])  // 存放所有活動資料
const currentPage = ref(1)
const pageSize = ref(20)
const activeTab = ref('all')

// 用 computed 篩選活動
const filteredPromotions = computed(() => {
  if (activeTab.value === 'all') return allPromotions.value
  
  const statusMap: Record<string, string> = {
    pending: '待審核',
    active: '進行中',
    upcoming: '即將開始',
    rejected: '已拒絕',
    ended: '已結束',
  }
  const targetStatus = statusMap[activeTab.value]
  if (!targetStatus) return allPromotions.value
  
  return allPromotions.value.filter(p => p.statusText === targetStatus)
})

// 前端分頁：從篩選後的資料中取出當前頁的資料
const paginatedPromotions = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredPromotions.value.slice(start, end)
})

// 總筆數 = 篩選後的資料筆數
const total = computed(() => filteredPromotions.value.length)

// 統計各狀態的筆數
const stats = computed(() => ({
  pending: allPromotions.value.filter(p => p.statusText === '待審核').length,
  active: allPromotions.value.filter(p => p.statusText === '進行中').length,
  upcoming: allPromotions.value.filter(p => p.statusText === '即將開始').length,
  rejected: allPromotions.value.filter(p => p.statusText === '已拒絕').length,
  ended: allPromotions.value.filter(p => p.statusText === '已結束' || p.statusText === '已拒絕').length,
  endedOnly: allPromotions.value.filter(p => p.statusText === '已結束').length,
}))

// ─── 彈窗表單 ─────────────────────────────────────────────────────

const dialogVisible = ref(false)
const isEdit = ref(false)
const editingId = ref<number | null>(null)
const submitting = ref(false)
const formRef = ref<FormInstance>()

// 輔助函式：格式化日期為 YYYY-MM-DDTHH:mm:ss
const getFormattedDate = (date: Date) => {
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const hh = String(date.getHours()).padStart(2, '0');
  const min = String(date.getMinutes()).padStart(2, '0');
  const ss = String(date.getSeconds()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}T${hh}:${min}:${ss}`;
};

const getDefaultDates = () => {
  const now = new Date();
  const nextMonth = new Date();
  nextMonth.setMonth(now.getMonth() + 1);
  return {
    start: getFormattedDate(now),
    end: getFormattedDate(nextMonth)
  };
};

const defaultDates = getDefaultDates();

// ─── 商品選擇器狀態 ───────────────────────────────────────────────

const selectedProducts = ref<PromotionProduct[]>([])
const originalBoundIds = ref<number[]>([])
const selectorVisible = ref(false)
const selectorLoading = ref(false)
const selectorKeyword = ref('')
const selectorProducts = ref<AvailableProduct[]>([])
const selectorPage = ref(1)
const selectorPageSize = 10
const selectorTotal = ref(0)
const pendingSelection = ref<AvailableProduct[]>([])
const selectorTableRef = ref<InstanceType<typeof ElTable>>()

const formData = ref<PromotionFormData>({
  name: '',
  description: '',
  promotionType: 0,
  discountValue: 0,
  minimumAmount: 0,
  limitQuantity: 0,
  startTime: defaultDates.start,
  endTime: defaultDates.end,
})

const formRules: FormRules = {
  name: [{ required: true, message: '請輸入活動名稱', trigger: 'blur' }],
  promotionType: [{ required: true, message: '請選擇活動類型', trigger: 'change' }],
  discountValue: [
    { required: true, message: '請輸入折扣值', trigger: 'blur' },
    { 
      validator: (_rule, value, callback) => {
        if (formData.value.promotionType === 1 && (value < 1 || value > 99)) {
          callback(new Error('折扣比例需在 1-99 之間'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  minimumAmount: [
    { 
      validator: (_rule, value, callback) => {
        if (formData.value.promotionType === 2 && (!value || value <= 0)) {
          callback(new Error('請輸入滿額門檻'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  limitQuantity: [
    { 
      validator: (_rule, value, callback) => {
        if (formData.value.promotionType === 3 && (!value || value <= 0)) {
          callback(new Error('請輸入限量數量'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  startTime: [{ required: true, message: '請選擇開始時間', trigger: 'change' }],
  endTime: [{ required: true, message: '請選擇結束時間', trigger: 'change' }],
}

const discountLabel = computed(() => {
  if (formData.value.promotionType === 1) return '折扣比例(%)'
  if (formData.value.promotionType === 2) return '折扣金額'
  if (formData.value.promotionType === 3) return '折扣金額'
  return '折扣值'
})

const discountPlaceholder = computed(() => {
  if (formData.value.promotionType === 1) return '請輸入折扣比例（0-100）'
  if (formData.value.promotionType === 2) return '請輸入折扣金額（例如：100）'
  if (formData.value.promotionType === 3) return '請輸入折扣金額'
  return '請輸入折扣值'
})

const discountHint = computed(() => {
  if (formData.value.promotionType === 1) return '例如：20 代表打 8 折（20% off）'
  if (formData.value.promotionType === 2) return '例如：100 代表滿額折 100 元'
  if (formData.value.promotionType === 3) return '例如：100 代表每件折 100 元'
  return ''
})

// ─── API 呼叫 ─────────────────────────────────────────────────────

async function loadPromotions(): Promise<void> {
  loading.value = true
  try {
    // 一次載入所有活動，前端做篩選和分頁
    const params = {
      page: 1,
      pageSize: 100,  // 取較大筆數，確保能取得所有活動
    }
    const res = await fetchSellerPromotions(params)
    
    // API 回傳格式：axios response.data = { success, data: { items, totalCount, ... } }
    if (res.success) {
      allPromotions.value = res.data.items || []
    } else {
      console.error('API 回傳 success=false:', res)
      allPromotions.value = []
    }
  } catch (error: any) {
    console.error('載入活動列表失敗:', error)
    console.error('錯誤詳情:', error.response?.data)
    if (error.response?.status === 401) {
      ElMessage.error('請先登入')
    } else if (error.response?.status === 404) {
      console.warn('API 端點不存在，可能後端尚未實作')
      ElMessage.warning('功能開發中，請稍後再試')
    } else {
      ElMessage.error('載入失敗，請稍後再試')
    }
    allPromotions.value = []
  } finally {
    loading.value = false
  }
}

// ─── 彈窗操作 ─────────────────────────────────────────────────────

function openCreateDialog(): void {
  isEdit.value = false
  editingId.value = null
  const dates = getDefaultDates()
  formData.value = {
    name: '',
    description: '',
    promotionType: 0,
    discountValue: 0,
    minimumAmount: 0,
    limitQuantity: 0,
    startTime: dates.start,
    endTime: dates.end,
  }
  selectedProducts.value = []
  originalBoundIds.value = []
  dialogVisible.value = true
}

async function openEditDialog(row: SellerPromotion): Promise<void> {
  isEdit.value = true
  editingId.value = row.id

  console.log('editing item:', JSON.stringify(row))

  formData.value = {
    name: row.name,
    description: row.description || '',
    promotionType: row.promotionType,
    discountValue: row.discountValue ?? 0,
    minimumAmount: row.minimumAmount ?? 0,
    limitQuantity: row.limitQuantity ?? 0,
    startTime: row.startTime,
    endTime: row.endTime,
  }

  // 載入已綁定商品
  selectedProducts.value = []
  originalBoundIds.value = []
  try {
    const res = await fetchPromotionProducts(row.id)
    if (res.success && Array.isArray(res.data)) {
      selectedProducts.value = res.data as PromotionProduct[]
      originalBoundIds.value = (res.data as PromotionProduct[]).map(p => p.productId)
    }
  } catch {
    console.error('載入已綁定商品失敗')
  }

  dialogVisible.value = true
}

async function handleSubmit(): Promise<void> {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    
    submitting.value = true
    try {
      // 準備送出的資料
      const submitData = {
        name: formData.value.name,
        description: formData.value.description,
        promotionType: formData.value.promotionType,
        discountValue: formData.value.discountValue,
        minimumAmount: formData.value.minimumAmount,
        limitQuantity: formData.value.limitQuantity,
        startTime: formData.value.startTime,
        endTime: formData.value.endTime,
      }
      
      // 記錄要送出的資料，方便 debug
      console.log('準備送出活動資料:', submitData)
      
      // TODO: 如果後端 DTO 沒有 minimumAmount 和 limitQuantity 欄位
      // 暫時在 description 裡補充說明，例如：
      // if (formData.value.promotionType === 2 && formData.value.minimumAmount > 0) {
      //   submitData.description = `消費滿${formData.value.minimumAmount}折${formData.value.discountValue}\n${formData.value.description}`
      // }
      
      if (isEdit.value && editingId.value !== null) {
        await updateSellerPromotion(editingId.value, submitData)

        // 計算商品增減差異
        const currentIds = new Set(selectedProducts.value.map(p => p.productId))
        const originalIds = new Set(originalBoundIds.value)
        const toAdd = [...currentIds].filter(id => !originalIds.has(id))
        const toRemove = [...originalIds].filter(id => !currentIds.has(id))
        if (toAdd.length > 0) {
          await bindPromotionProducts(editingId.value, toAdd)
        }
        for (const pid of toRemove) {
          await unbindPromotionProduct(editingId.value, pid)
        }

        ElMessage.success('活動更新成功')
      } else {
        const response = await createSellerPromotion(submitData)
        console.log('新增活動成功，後端回傳:', response)

        // 綁定商品到新建立的活動
        if (response?.success && selectedProducts.value.length > 0) {
          const newId = response.data?.id as number
          const ids = selectedProducts.value.map(p => p.productId)
          try {
            await bindPromotionProducts(newId, ids)
          } catch {
            ElMessage.warning('活動已建立，但商品綁定失敗，請稍後在編輯頁補綁')
          }
        }

        ElMessage.success('活動已送出審核')
      }
      dialogVisible.value = false
      currentPage.value = 1
      await loadPromotions()
    } catch (error: any) {
      console.error('提交活動失敗:', error)
      console.error('錯誤回應:', error.response?.data)
      console.error('Request payload:', formData.value)
      
      if (error.response?.status === 400) {
        const errMsg = error.response?.data?.message || error.response?.data?.errors || '資料格式錯誤'
        ElMessage.error(`提交失敗：${JSON.stringify(errMsg)}`)
        console.error('400 錯誤詳情:', error.response?.data)
      } else if (error.response?.status === 401) {
        ElMessage.error('請先登入')
      } else if (error.response?.status === 404) {
        ElMessage.warning('功能開發中，請稍後再試')
      } else {
        ElMessage.error(error.response?.data?.message || '提交失敗，請稍後再試')
      }
    } finally {
      submitting.value = false
    }
  })
}

// ─── 商品選擇器 ───────────────────────────────────────────────────

function openProductSelector(): void {
  selectorKeyword.value = ''
  selectorPage.value = 1
  pendingSelection.value = []
  selectorVisible.value = true
  void loadSelectorProducts()
}

async function loadSelectorProducts(): Promise<void> {
  selectorLoading.value = true
  try {
    if (isEdit.value && editingId.value) {
      // 編輯模式：後端已排除已綁定商品
      const res = await fetchAvailableProductsForPromotion(editingId.value, {
        keyword: selectorKeyword.value || undefined,
        page: selectorPage.value,
        pageSize: selectorPageSize,
      })
      if (res.success) {
        selectorProducts.value = res.data.items as AvailableProduct[]
        selectorTotal.value = res.data.totalCount as number
      }
    } else {
      // 新增模式：用賣家商品列表，前端排除已選
      const res = await fetchSellerProductsSimple({
        keyword: selectorKeyword.value || undefined,
        page: selectorPage.value,
        pageSize: selectorPageSize,
      })
      const selectedIds = new Set(selectedProducts.value.map(p => p.productId))
      selectorProducts.value = (res.items ?? [])
        .filter(p => !selectedIds.has(p.id))
        .map(p => ({
          id: p.id,
          name: p.name,
          imageUrl: p.mainImageUrl,
          minPrice: p.minPrice,
          maxPrice: p.maxPrice,
          totalStock: p.totalStock,
          status: p.status,
        }))
      selectorTotal.value = res.totalCount ?? 0
    }
  } catch {
    ElMessage.error('載入商品列表失敗')
  } finally {
    selectorLoading.value = false
  }
}

function onSelectorSearch(): void {
  selectorPage.value = 1
  void loadSelectorProducts()
}

function handleSelectorSelectionChange(rows: AvailableProduct[]): void {
  pendingSelection.value = rows
}

function confirmProductSelection(): void {
  const selectedIds = new Set(selectedProducts.value.map(p => p.productId))
  for (const p of pendingSelection.value) {
    if (!selectedIds.has(p.id)) {
      selectedProducts.value.push({
        productId: p.id,
        productName: p.name,
        imageUrl: p.imageUrl,
        minPrice: p.minPrice,
        originalPrice: p.minPrice ?? 0,
        discountPrice: null,
      })
    }
  }
  pendingSelection.value = []
  selectorVisible.value = false
}

function removeProduct(productId: number): void {
  selectedProducts.value = selectedProducts.value.filter(p => p.productId !== productId)
}

async function handleDelete(id: number): Promise<void> {
  try {
    await ElMessageBox.confirm('確定要刪除此活動嗎？', '提示', {
      confirmButtonText: '確定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    
    await deleteSellerPromotion(id)
    ElMessage.success('刪除成功')
    await loadPromotions()
  } catch (error: any) {
    if (error === 'cancel') return
    console.error('刪除活動失敗:', error)
    ElMessage.error('刪除失敗，請稍後再試')
  }
}

// ─── Tab 和篩選 ───────────────────────────────────────────────────

function handleTabChange(): void {
  currentPage.value = 1  // 切換 tab 時重置到第一頁
  // 不需要重新載入資料，computed 會自動更新
}

function filterByStatus(status: string): void {
  activeTab.value = status
  currentPage.value = 1  // 切換狀態時重置到第一頁
  // 不需要重新載入資料，computed 會自動更新
}

// ─── 工具函式 ─────────────────────────────────────────────────────

function formatDateRange(start: string, end: string): string {
  const formatDate = (dateStr: string) => {
    if (!dateStr) return ''
    return dateStr.replace('T', ' ').substring(0, 16)
  }
  return `${formatDate(start)} ~ ${formatDate(end)}`
}

function getTypeTagColor(promotionType: number): string {
  const colors: Record<number, string> = {
    1: 'danger',      // 限時特賣
    2: 'warning',     // 滿額折扣
    3: 'success',     // 限量搶購
    4: 'primary',     // 新品優惠
  }
  return colors[promotionType] || 'info'
}

function getStatusTagColor(status: number): string {
  const colors: Record<number, string> = {
    0: 'warning',   // 待審核
    1: 'success',   // 進行中
    2: 'info',      // 即將開始
    3: 'danger',    // 已拒絕
    4: 'info',      // 已結束
  }
  return colors[status] || 'info'
}

function getStatusLabel(status: number): string {
  const labels: Record<number, string> = {
    0: '待審核',
    1: '進行中',
    2: '即將開始',
    3: '已拒絕',
    4: '已結束',
  }
  return labels[status] || `狀態${status}`
}

function canEdit(statusText: string): boolean {
  return statusText === '待審核' || statusText === '已拒絕'
}

function canDelete(statusText: string): boolean {
  return statusText === '待審核'
}

// ─── 生命週期 ─────────────────────────────────────────────────────

onMounted(() => {
  void loadPromotions()
})
</script>

<style scoped>
.promotion-list-page {
  padding: 24px;
}

/* 頁面標題 */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}
.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

/* 統計卡片 */
.stats-row {
  margin-bottom: 24px;
}
.stat-card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  cursor: pointer;
  transition: all 0.3s;
  text-align: center;
}
.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
}
.stat-label {
  font-size: 14px;
  color: #64748b;
  margin-bottom: 8px;
}
.stat-value {
  font-size: 32px;
  font-weight: 700;
}
.stat-pending .stat-value { color: #EE4D2D; }
.stat-active .stat-value { color: #22c55e; }
.stat-upcoming .stat-value { color: #3b82f6; }
.stat-ended .stat-value { color: #94a3b8; }

/* Tab */
.promo-tabs {
  background: white;
  padding: 16px 16px 0;
  border-radius: 8px;
  margin-bottom: 16px;
}
.tab-count {
  color: #94a3b8;
  font-size: 12px;
  margin-left: 4px;
}

/* 表格 */
.promo-table {
  background: white;
  border-radius: 8px;
  overflow: hidden;
}
.reject-reason {
  color: #ef4444;
  font-size: 13px;
}
.text-muted {
  color: #94a3b8;
}

/* 分頁 */
.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 24px;
  padding: 20px;
  background: white;
  border-radius: 8px;
}

/* 表單 */
.form-hint {
  display: block;
  font-size: 12px;
  color: #94a3b8;
  margin-top: 4px;
}

/* 已選商品卡片列表 */
.selected-products {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 10px;
  width: 100%;
  max-height: 220px;
  overflow-y: auto;
  padding-right: 4px;
}
.product-card {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
}
.product-thumb {
  width: 44px;
  height: 44px;
  object-fit: cover;
  border-radius: 4px;
  flex-shrink: 0;
  background: #e2e8f0;
}
.product-info {
  flex: 1;
  min-width: 0;
}
.product-name {
  font-size: 13px;
  color: #334155;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.product-price {
  font-size: 12px;
  color: #64748b;
  margin-top: 2px;
}
.no-products-hint {
  margin-top: 8px;
  font-size: 13px;
  color: #94a3b8;
}

/* 商品選擇器 Dialog */
.selector-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
}
.selector-product-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.selector-thumb {
  width: 36px;
  height: 36px;
  object-fit: cover;
  border-radius: 4px;
  background: #e2e8f0;
  flex-shrink: 0;
}
.selector-pagination {
  display: flex;
  justify-content: center;
  margin-top: 12px;
}
</style>
