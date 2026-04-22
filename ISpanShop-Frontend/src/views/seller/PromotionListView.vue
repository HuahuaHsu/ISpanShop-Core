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
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          {{ isEdit ? '更新活動' : '送出審核' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { FormInstance, FormRules } from 'element-plus'
import {
  fetchSellerPromotions,
  createSellerPromotion,
  updateSellerPromotion,
  deleteSellerPromotion,
} from '@/api/promotion'

// ─── 介面定義 ─────────────────────────────────────────────────────

// TODO: 後端 GET /api/seller/promotions 回傳的活動物件目前缺少以下欄位：
// - discountValue (折扣值)
// - minimumAmount (滿額門檻，滿額折扣用)
// - limitQuantity (限量數量，限量搶購用)
// 
// 目前後端只回傳：
// {
//   id, name, description, promotionType, promotionTypeLabel,
//   startTime, endTime, status, statusText, rejectReason, createdAt, reviewedAt
// }
//
// 需要請後端補上折扣相關欄位，否則編輯時無法帶入原本的折扣值

interface SellerPromotion {
  id: number
  name: string
  description: string | null
  promotionType: number
  promotionTypeLabel: string
  discountValue?: number       // TODO: 後端需補此欄位
  minimumAmount?: number       // TODO: 後端需補此欄位 (滿額折扣用)
  limitQuantity?: number       // TODO: 後端需補此欄位 (限量搶購用)
  startTime: string
  endTime: string
  status: number
  statusText: string
  rejectReason: string | null
  createdAt: string
  reviewedAt: string | null
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

const formData = ref<PromotionFormData>({
  name: '',
  description: '',
  promotionType: 0,
  discountValue: 0,
  minimumAmount: 0,
  limitQuantity: 0,
  startTime: '',
  endTime: '',
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
  formData.value = {
    name: '',
    description: '',
    promotionType: 0,
    discountValue: 0,
    minimumAmount: 0,
    limitQuantity: 0,
    startTime: '',
    endTime: '',
  }
  dialogVisible.value = true
}

function openEditDialog(row: SellerPromotion): void {
  isEdit.value = true
  editingId.value = row.id
  
  // Debug: 印出原始資料確認欄位
  console.log('=== 編輯活動 DEBUG ===')
  console.log('編輯活動原始資料 (完整物件):', JSON.stringify(row, null, 2))
  console.log('活動名稱:', row.name)
  console.log('promotionType:', row.promotionType)
  console.log('discountValue:', row.discountValue)
  console.log('minimumAmount:', row.minimumAmount)
  console.log('limitQuantity:', row.limitQuantity)
  
  // TODO: 後端目前沒有回傳 discountValue, minimumAmount, limitQuantity 欄位
  // 需要請後端在 GET /api/seller/promotions 的回傳 DTO 中補上這些欄位
  // 目前暫時設為 0，等後端補充
  
  formData.value = {
    name: row.name,
    description: row.description || '',
    promotionType: row.promotionType,
    discountValue: row.discountValue ?? 0,      // TODO: 後端需補此欄位
    minimumAmount: row.minimumAmount ?? 0,      // TODO: 後端需補此欄位
    limitQuantity: row.limitQuantity ?? 0,      // TODO: 後端需補此欄位
    startTime: row.startTime,
    endTime: row.endTime,
  }
  
  // Debug: 印出帶入表單的值
  console.log('帶入表單的值:', formData.value)
  console.log('=== DEBUG 結束 ===')
  
  // 如果沒有折扣相關欄位，提示使用者
  if (row.discountValue === undefined && row.minimumAmount === undefined) {
    ElMessage.warning('後端未回傳折扣資訊，編輯時請重新輸入折扣值')
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
        ElMessage.success('活動更新成功')
      } else {
        const response = await createSellerPromotion(submitData)
        console.log('新增活動成功，後端回傳:', response)
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
</style>
