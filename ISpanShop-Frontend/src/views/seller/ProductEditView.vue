<template>
  <div class="product-edit-page">
    <div class="page-header">
      <div class="page-header-left">
        <el-button text @click="router.push('/seller/products')">
          <el-icon><ArrowLeft /></el-icon> 返回商品列表
        </el-button>
        <h1 class="page-title">{{ isEdit ? '編輯商品' : '新增商品' }}</h1>
      </div>
    </div>

    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
      class="product-form"
    >
      <el-row :gutter="24">
        <!-- 左欄 -->
        <el-col :xs="24" :lg="16">

          <!-- 基本資訊 -->
          <el-card class="form-section" shadow="never">
            <template #header><span class="section-title">📝 基本資訊</span></template>

            <el-form-item label="商品名稱" prop="name">
              <el-input v-model="form.name" placeholder="請輸入商品名稱（最多 100 字）" maxlength="100" show-word-limit />
            </el-form-item>

            <el-form-item label="商品描述" prop="description">
              <el-input
                v-model="form.description"
                type="textarea"
                :rows="6"
                placeholder="詳細描述商品特色、規格、材質等資訊..."
                maxlength="2000"
                show-word-limit
              />
              <!-- TODO: 可替換為富文字編輯器（如 Quill、TipTap） -->
            </el-form-item>

            <el-row :gutter="16">
              <el-col :xs="24" :sm="12">
                <el-form-item label="商品分類" prop="categoryId">
                  <el-select
                    v-model="form.categoryId"
                    placeholder="選擇分類"
                    filterable
                    style="width: 100%"
                    @change="handleCategoryChange"
                  >
                    <el-option
                      v-for="cat in categories"
                      :key="cat.id"
                      :label="cat.name"
                      :value="cat.id"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12">
                <el-form-item label="品牌" prop="brandId">
                  <el-select
                    v-model="form.brandId"
                    placeholder="選擇品牌（可選）"
                    filterable
                    clearable
                    style="width: 100%"
                  >
                    <el-option
                      v-for="brand in brands"
                      :key="brand.id"
                      :label="brand.name"
                      :value="brand.id"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
          </el-card>

          <!-- 商品圖片 -->
          <el-card class="form-section" shadow="never">
            <template #header>
              <div class="section-header">
                <span class="section-title">🖼️ 商品圖片</span>
                <span class="section-hint">最多 5 張，第一張為主圖</span>
              </div>
            </template>
            <el-upload
              v-model:file-list="form.imageFiles"
              list-type="picture-card"
              :limit="5"
              :auto-upload="false"
              accept="image/*"
              @exceed="handleImageExceed"
            >
              <el-icon><Plus /></el-icon>
              <template #tip>
                <div class="upload-tip">支援 JPG / PNG / WebP，每張不超過 5MB</div>
              </template>
            </el-upload>
            <!-- TODO: 實作圖片上傳至後端或 CDN，POST /api/seller/upload/image -->
          </el-card>

          <!-- 價格與庫存 -->
          <el-card class="form-section" shadow="never">
            <template #header><span class="section-title">💰 價格與庫存</span></template>

            <el-row :gutter="16">
              <el-col :xs="24" :sm="8">
                <el-form-item label="售價（NT$）" prop="price">
                  <el-input-number
                    v-model="form.price"
                    :min="1"
                    :max="9999999"
                    controls-position="right"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="8">
                <el-form-item label="原價（NT$）">
                  <el-input-number
                    v-model="form.originalPrice"
                    :min="0"
                    :max="9999999"
                    controls-position="right"
                    style="width: 100%"
                    placeholder="可選"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="8">
                <el-form-item label="庫存數量" prop="stock">
                  <el-input-number
                    v-model="form.stock"
                    :min="0"
                    :max="99999"
                    controls-position="right"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
            </el-row>
          </el-card>

          <!-- 規格設定（TODO 進階功能） -->
          <el-card class="form-section" shadow="never">
            <template #header>
              <div class="section-header">
                <span class="section-title">🎛️ 規格設定</span>
                <el-tag type="warning" size="small">進階功能開發中</el-tag>
              </div>
            </template>
            <el-empty
              description="多規格（顏色 / 尺寸）設定功能開發中"
              :image-size="60"
            >
              <!-- TODO: 實作動態規格軸（SpecDefinitionJson）+ SKU 組合（ProductVariant） -->
            </el-empty>
          </el-card>

        </el-col>

        <!-- 右欄 -->
        <el-col :xs="24" :lg="8">

          <!-- 上架狀態 -->
          <el-card class="form-section" shadow="never">
            <template #header><span class="section-title">⚙️ 商品狀態</span></template>
            <el-form-item label="立即上架">
              <el-switch
                v-model="form.isOnShelf"
                active-text="上架中"
                inactive-text="已下架"
                active-color="#ee4d2d"
              />
            </el-form-item>
          </el-card>

          <!-- 底部操作 -->
          <el-card class="form-section action-card" shadow="never">
            <el-button
              type="default"
              style="width: 100%; margin-bottom: 12px"
              :loading="saving"
              @click="handleSubmit(false)"
            >
              💾 儲存草稿
            </el-button>
            <el-button
              type="primary"
              style="width: 100%; margin-bottom: 12px"
              :loading="saving"
              @click="handleSubmit(true)"
            >
              🚀 儲存並上架
            </el-button>
            <el-button
              style="width: 100%"
              @click="router.push('/seller/products')"
            >
              取消
            </el-button>
          </el-card>

        </el-col>
      </el-row>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules, UploadUserFile } from 'element-plus'
import { ArrowLeft, Plus } from '@element-plus/icons-vue'
import { fetchMainCategories } from '@/api/category'
import { fetchBrands } from '@/api/brand'
import type { Category } from '@/types/category'
import type { Brand } from '@/types/brand'

const route = useRoute()
const router = useRouter()

const isEdit = computed<boolean>(() => !!route.params['id'])
const productId = computed<number | null>(() =>
  route.params['id'] ? Number(route.params['id']) : null
)

// ── 表單狀態 ──
const formRef = ref<FormInstance>()
const saving = ref<boolean>(false)
const categories = ref<Category[]>([])
const brands = ref<Brand[]>([])

interface ProductForm {
  name: string
  description: string
  categoryId: number | null
  brandId: number | null
  price: number
  originalPrice: number | null
  stock: number
  isOnShelf: boolean
  imageFiles: UploadUserFile[]
}

const form = reactive<ProductForm>({
  name: '',
  description: '',
  categoryId: null,
  brandId: null,
  price: 1,
  originalPrice: null,
  stock: 0,
  isOnShelf: true,
  imageFiles: [],
})

const rules: FormRules = {
  name: [
    { required: true, message: '請輸入商品名稱', trigger: 'blur' },
    { max: 100, message: '商品名稱不得超過 100 字', trigger: 'blur' },
  ],
  categoryId: [
    { required: true, message: '請選擇商品分類', trigger: 'change' },
  ],
  price: [
    { required: true, message: '請輸入售價', trigger: 'blur' },
    { type: 'number', min: 1, message: '售價至少為 1', trigger: 'blur' },
  ],
  stock: [
    { required: true, message: '請輸入庫存數量', trigger: 'blur' },
  ],
}

// ── 初始化 ──
onMounted(async () => {
  await Promise.all([loadCategories(), loadBrands()])
  if (isEdit.value && productId.value) {
    await loadProduct(productId.value)
  }
})

async function loadCategories(): Promise<void> {
  try {
    const res = await fetchMainCategories()
    if (res.success) categories.value = res.data
  } catch {
    // 靜默失敗
  }
}

async function loadBrands(): Promise<void> {
  try {
    const res = await fetchBrands()
    if (res.success) brands.value = res.data
  } catch {
    // 靜默失敗
  }
}

async function loadProduct(id: number): Promise<void> {
  // TODO: 呼叫 GET /api/seller/products/{id} 取得現有商品資料
  console.log('[TODO] GET /api/seller/products/' + id)
  ElMessage.info('載入商品資料功能待後端 API 實作')
}

function handleCategoryChange(): void {
  // 重置品牌（分類變更後品牌可能需要重新篩選）
  form.brandId = null
  // TODO: 依分類載入對應品牌 fetchBrands({ categoryId: form.categoryId })
}

function handleImageExceed(): void {
  ElMessage.warning('最多只能上傳 5 張圖片')
}

// ── 提交 ──
async function handleSubmit(publishNow: boolean): Promise<void> {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  form.isOnShelf = publishNow

  saving.value = true
  try {
    const payload = {
      name: form.name,
      description: form.description,
      categoryId: form.categoryId,
      brandId: form.brandId,
      price: form.price,
      originalPrice: form.originalPrice,
      stock: form.stock,
      isOnShelf: form.isOnShelf,
      // imageFiles 需先上傳取得 URL，再附上
    }

    if (isEdit.value && productId.value) {
      // TODO: 呼叫 PUT /api/seller/products/{id}
      console.log('[TODO] PUT /api/seller/products/' + productId.value, payload)
      ElMessage.success('商品更新成功（TODO: 串接後端）')
    } else {
      // TODO: 呼叫 POST /api/seller/products
      console.log('[TODO] POST /api/seller/products', payload)
      ElMessage.success('商品新增成功（TODO: 串接後端）')
    }

    void router.push('/seller/products')
  } catch {
    ElMessage.error('儲存失敗，請稍後再試')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.product-edit-page {
  max-width: 1200px;
  margin: 0 auto;
}
.page-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
}
.page-header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.form-section {
  margin-bottom: 20px;
  border: 1px solid #e8eaf0 !important;
  border-radius: 12px !important;
}
.section-title {
  font-size: 15px;
  font-weight: 700;
  color: #1e293b;
}
.section-header {
  display: flex;
  align-items: center;
  gap: 10px;
}
.section-hint {
  color: #94a3b8;
  font-size: 13px;
}
.upload-tip {
  color: #94a3b8;
  font-size: 12px;
  margin-top: 8px;
  text-align: center;
}

.action-card :deep(.el-card__body) { padding: 16px; }
</style>
