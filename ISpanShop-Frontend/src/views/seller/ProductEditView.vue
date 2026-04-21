<template>
  <div class="product-edit-page">
    <!-- 麵包屑 -->
    <el-breadcrumb separator=">">
      <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
      <el-breadcrumb-item :to="{ path: '/seller/products' }">我的商品</el-breadcrumb-item>
      <el-breadcrumb-item>新增商品</el-breadcrumb-item>
    </el-breadcrumb>

    <!-- 三欄布局 -->
    <div class="three-column-layout">
      <!-- 左側:填寫建議 -->
      <aside class="left-sidebar">
        <div class="tips-card">
          <div class="tips-header">填寫建議</div>
          <div class="tips-list">
            <div class="tip-item" :class="{ completed: isImageComplete }">
              <el-icon class="tip-icon">
                <CircleCheck v-if="isImageComplete" />
                <CircleClose v-else />
              </el-icon>
              <span>新增至少 3 張圖片</span>
            </div>
            <div class="tip-item" :class="{ completed: isNameComplete }">
              <el-icon class="tip-icon">
                <CircleCheck v-if="isNameComplete" />
                <CircleClose v-else />
              </el-icon>
              <span>商品名稱的字數需介於 5~100</span>
            </div>
            <div class="tip-item" :class="{ completed: isDescriptionComplete }">
              <el-icon class="tip-icon">
                <CircleCheck v-if="isDescriptionComplete" />
                <CircleClose v-else />
              </el-icon>
              <span>商品描述需填入至少 100 個文字或是 1 張圖片</span>
            </div>
            <div class="tip-item" :class="{ completed: isBrandComplete }">
              <el-icon class="tip-icon">
                <CircleCheck v-if="isBrandComplete" />
                <CircleClose v-else />
              </el-icon>
              <span>新增品牌資訊</span>
            </div>
          </div>
        </div>
      </aside>

      <!-- 中間:主表單 -->
      <main class="main-content">
        <!-- Tab 導航 -->
        <div class="tab-nav">
          <div
            v-for="tab in tabs"
            :key="tab.id"
            class="tab-item"
            :class="{ active: currentTab === tab.id }"
            @click="scrollToSection(tab.id)"
          >
            {{ tab.label }}
          </div>
        </div>

        <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
          <!-- 基本資訊 -->
          <section id="section-basic" class="form-section">
            <h2 class="section-title">基本資訊</h2>

            <!-- 商品圖片 -->
            <el-form-item prop="images">
              <template #label>
                <span class="required-label">* 商品圖片</span>
                <span class="image-count">({{ form.images.length }}/9)</span>
              </template>
              <el-upload
                v-model:file-list="form.images"
                list-type="picture-card"
                :limit="9"
                :auto-upload="false"
                accept=".jpg,.jpeg,.png,.webp"
                :on-exceed="handleImageExceed"
                :on-change="handleImageChange"
                draggable
              >
                <el-icon><Plus /></el-icon>
              </el-upload>
              <div class="form-hint">檔案大小：最大 2MB，格式：JPG, PNG, WEBP，第一張為主圖</div>
            </el-form-item>

            <!-- 商品名稱 -->
            <el-form-item prop="name">
              <template #label>
                <span class="required-label">* 商品名稱</span>
                <span class="char-count">{{ form.name.length }}/60</span>
              </template>
              <el-input
                v-model="form.name"
                placeholder="品牌名稱 + 商品類型 + 重要功能(材質/顏色/尺寸/規格)"
                maxlength="60"
              />
            </el-form-item>

            <!-- 類別選擇 -->
            <el-form-item prop="categoryId">
              <template #label>
                <span class="required-label">* 類別</span>
              </template>
              <el-input
                :model-value="form.categoryPath"
                placeholder="點擊選擇分類"
                readonly
                @click="showCategoryPicker = true"
              >
                <template #suffix>
                  <el-icon><ArrowRight /></el-icon>
                </template>
              </el-input>
            </el-form-item>
          </section>

          <!-- 屬性 -->
          <section id="section-attributes" class="form-section" v-loading="loadingAttributes">
            <div class="section-header-with-progress">
              <h2 class="section-title">屬性</h2>
              <div class="completion-hint">
                完成度：{{ attributesCompletionCount }}/{{ categoryAttributes.length || 0 }} 
                填寫更多的屬性資料來加強您商品的曝光機會
              </div>
            </div>

            <!-- 動態屬性載入 -->
            <div v-if="!form.categoryId" class="empty-hint">
              <el-empty description="請先選擇商品分類" :image-size="80" />
            </div>

            <div v-else-if="loadingAttributes" class="empty-hint">
              <!-- Loading 中，顯示骨架屏或留空 -->
            </div>

            <div v-else-if="categoryAttributes.length === 0" class="empty-hint">
              <el-alert
                title="該分類暫無屬性設定"
                type="info"
                :closable="false"
                show-icon
              />
            </div>

            <el-row v-else :gutter="16">
              <el-col
                v-for="attr in categoryAttributes"
                :key="attr.id"
                :span="12"
              >
                <el-form-item :prop="`dynamicAttributes.${attr.id}`">
                  <template #label>
                    <span v-if="attr.isRequired" class="required-label">
                      * {{ attr.name }}
                    </span>
                    <span v-else>{{ attr.name }}</span>
                    <span
                      v-if="attr.isMultiple && attr.maxSelect"
                      class="attr-count"
                    >
                      {{ getAttributeCount(attr.id) }}/{{ attr.maxSelect }}
                    </span>
                  </template>

                  <!-- 單選 -->
                  <el-select
                    v-if="!attr.isMultiple"
                    v-model="form.dynamicAttributes[attr.id]"
                    :placeholder="`選擇${attr.name}`"
                    clearable
                    filterable
                    allow-create
                    :reserve-keyword="false"
                    style="width: 100%"
                  >
                    <el-option
                      v-for="opt in attr.options"
                      :key="opt.id"
                      :label="opt.value"
                      :value="opt.value"
                    />
                  </el-select>

                  <!-- 多選 -->
                  <el-select
                    v-else
                    v-model="form.dynamicAttributes[attr.id]"
                    multiple
                    :multiple-limit="attr.maxSelect || 5"
                    :placeholder="`選擇${attr.name}（最多${attr.maxSelect || 5}個）`"
                    clearable
                    filterable
                    allow-create
                    :reserve-keyword="false"
                    style="width: 100%"
                  >
                    <el-option
                      v-for="opt in attr.options"
                      :key="opt.id"
                      :label="opt.value"
                      :value="opt.value"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
          </section>

          <!-- 商品描述 -->
          <section id="section-description" class="form-section">
            <h2 class="section-title">商品描述</h2>
            <el-form-item prop="description">
              <template #label>
                <span class="required-label">* 商品描述</span>
              </template>
              <div class="description-toolbar">
                <el-button size="small" @click="handleAddDescriptionImage">
                  <el-icon><Picture /></el-icon>
                  新增圖片 ({{ descriptionImageCount }}/12)
                </el-button>
                <span class="char-count">{{ form.description.length }}/3000</span>
              </div>
              <el-input
                v-model="form.description"
                type="textarea"
                :rows="10"
                placeholder="請輸入商品描述或點選以新增圖片"
                maxlength="3000"
              />
              <div class="form-hint">
                建議至少 100 字。TODO: 之後可換成 Tiptap 或 Quill 富文字編輯器
              </div>
            </el-form-item>
          </section>

          <!-- 銷售資訊 -->
          <section id="section-sales" class="form-section">
            <h2 class="section-title">銷售資訊</h2>

            <!-- 規格設定 -->
            <el-form-item>
              <template #label>
                <span class="required-label">* 規格</span>
              </template>

              <!-- 開啟規格按鈕 -->
              <div v-if="!specsEnabled" class="enable-specs-btn">
                <el-button type="primary" plain @click="specsEnabled = true">
                  <el-icon><Plus /></el-icon>
                  開啟商品規格
                </el-button>
              </div>

              <!-- 規格設定區塊 -->
              <div v-else class="specs-config">
                <!-- 規格1 -->
                <div
                  v-for="(spec, specIndex) in form.specs"
                  :key="specIndex"
                  class="spec-group"
                >
                  <div class="spec-header">
                    <el-input
                      v-model="spec.name"
                      :placeholder="`輸入規格名稱，如：${specIndex === 0 ? '顏色' : '尺寸'}`"
                      :maxlength="14"
                      style="width: 200px"
                    />
                    <span class="char-count">{{ spec.name.length }}/14</span>
                    <el-button
                      v-if="form.specs.length > 1"
                      text
                      type="danger"
                      @click="removeSpec(specIndex)"
                    >
                      <el-icon><Close /></el-icon>
                    </el-button>
                  </div>

                  <!-- 規格選項 -->
                  <div class="spec-options">
                    <div
                      v-for="(option, optIndex) in spec.options"
                      :key="optIndex"
                      class="spec-option-row"
                    >
                      <span class="option-label">選項 {{ optIndex + 1 }}</span>
                      <el-upload
                        v-if="specIndex === 0"
                        class="option-image-upload"
                        :show-file-list="false"
                        :auto-upload="false"
                        accept="image/*"
                      >
                        <el-icon class="upload-icon"><Picture /></el-icon>
                      </el-upload>
                      <el-input
                        v-model="option.name"
                        placeholder="輸入選項，例如：紅色"
                        :maxlength="20"
                        style="width: 240px"
                      />
                      <span class="char-count">{{ option.name.length }}/20</span>
                      <el-button
                        text
                        type="primary"
                        @click="addSpecOption(specIndex)"
                      >
                        <el-icon><Plus /></el-icon>
                      </el-button>
                      <el-button
                        v-if="spec.options.length > 1"
                        text
                        type="danger"
                        @click="removeSpecOption(specIndex, optIndex)"
                      >
                        <el-icon><Delete /></el-icon>
                      </el-button>
                    </div>
                  </div>
                </div>

                <!-- 新增規格2 -->
                <el-button
                  v-if="form.specs.length < 2"
                  type="primary"
                  plain
                  @click="addSpec"
                  style="margin-top: 12px"
                >
                  <el-icon><Plus /></el-icon>
                  新增規格 2
                </el-button>

                <!-- 規格表 -->
                <div v-if="variants.length > 0" class="variants-table">
                  <div class="table-header">規格明細</div>

                  <!-- 批量設定 -->
                  <div class="batch-edit">
                    <el-input-number
                      v-model="batchPrice"
                      placeholder="價格"
                      :min="0"
                      :controls="false"
                      style="width: 120px"
                    />
                    <el-input-number
                      v-model="batchStock"
                      placeholder="商品數量"
                      :min="0"
                      :controls="false"
                      style="width: 120px"
                    />
                    <el-input
                      v-model="batchSku"
                      placeholder="商品選項貨號"
                      style="width: 160px"
                    />
                    <el-button type="primary" @click="applyBatch">全部套用</el-button>
                  </div>

                  <!-- 表格 -->
                  <el-table :data="variants" border style="width: 100%">
                    <el-table-column
                      v-if="form.specs.length > 0"
                      :label="form.specs[0]?.name || '規格一'"
                      width="120"
                    >
                      <template #default="{ row }">
                        {{ row.spec1 }}
                      </template>
                    </el-table-column>
                    <el-table-column
                      v-if="form.specs.length > 1"
                      :label="form.specs[1]?.name || '規格二'"
                      width="120"
                    >
                      <template #default="{ row }">
                        {{ row.spec2 }}
                      </template>
                    </el-table-column>
                    <el-table-column label="* 價格" width="140">
                      <template #default="{ row }">
                        <el-input-number
                          v-model="row.price"
                          placeholder="NT$"
                          :min="0"
                          :controls="false"
                          style="width: 100%"
                        />
                      </template>
                    </el-table-column>
                    <el-table-column label="* 商品數量" width="120">
                      <template #default="{ row }">
                        <el-input-number
                          v-model="row.stock"
                          :min="0"
                          :controls="false"
                          style="width: 100%"
                        />
                      </template>
                    </el-table-column>
                    <el-table-column label="商品選項貨號" min-width="160">
                      <template #default="{ row }">
                        <el-input v-model="row.sku" placeholder="選填" />
                      </template>
                    </el-table-column>
                  </el-table>
                </div>
              </div>
            </el-form-item>

            <!-- 無規格時的價格/數量 -->
            <el-row v-if="!specsEnabled" :gutter="16">
              <el-col :span="12">
                <el-form-item prop="price">
                  <template #label>
                    <span class="required-label">* 價格</span>
                  </template>
                  <el-input-number
                    v-model="form.price"
                    placeholder="NT$"
                    :min="1"
                    :max="9999999"
                    controls-position="right"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item prop="stock">
                  <template #label>
                    <span class="required-label">* 商品數量</span>
                  </template>
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

            <!-- 最低購買數量 -->
            <el-form-item prop="minPurchase">
              <template #label>
                <span class="required-label">* 最低購買數量</span>
              </template>
              <el-input-number
                v-model="form.minPurchase"
                :min="1"
                :max="999"
                controls-position="right"
                style="width: 200px"
              />
              <div class="form-hint">最低購買數量是指買家一次至少購買的商品數量</div>
            </el-form-item>
          </section>
        </el-form>
      </main>

      <!-- 右側:即時預覽 -->
      <aside class="right-sidebar">
        <div class="preview-card">
          <div class="preview-header">
            <div class="preview-title">預覽</div>
            <div class="preview-subtitle">商品詳情</div>
          </div>
          <div class="preview-content">
            <!-- 主圖輪播 -->
            <div class="preview-image">
              <el-carousel
                v-if="form.images.length > 0"
                height="250px"
                :autoplay="false"
                :arrow="form.images.length > 1 ? 'hover' : 'never'"
                indicator-position="outside"
                @change="handleCarouselChange"
              >
                <el-carousel-item v-for="(img, index) in form.images" :key="index">
                  <img
                    :src="getImageUrl(img)"
                    :alt="`商品圖片 ${index + 1}`"
                    style="width: 100%; height: 100%; object-fit: contain; background: #f5f5f5"
                  />
                </el-carousel-item>
              </el-carousel>
              <div v-else class="preview-placeholder">
                <el-icon :size="48"><Picture /></el-icon>
              </div>
            </div>
            <div v-if="form.images.length > 0" class="image-counter">
              {{ currentImageIndex + 1 }}/{{ form.images.length }}
            </div>

            <!-- 商品名稱 -->
            <div class="preview-name">
              {{ form.name || '商品名稱' }}
            </div>

            <!-- 價格 -->
            <div class="preview-price">
              <span class="price-symbol">NT$</span>
              <span class="price-value">{{ formatPrice(form.price) }}</span>
            </div>

            <!-- 屬性 -->
            <div class="preview-attrs">
              <div v-if="form.attributes.brandId" class="attr-item">
                <span class="attr-label">品牌:</span>
                <span class="attr-value">{{ getBrandName(form.attributes.brandId) }}</span>
              </div>
              <div v-if="form.categoryPath" class="attr-item">
                <span class="attr-label">種類:</span>
                <span class="attr-value">{{ form.categoryPath }}</span>
              </div>
              <div v-if="form.attributes.productType" class="attr-item">
                <span class="attr-label">商品種類:</span>
                <span class="attr-value">{{ form.attributes.productType }}</span>
              </div>
              <div v-if="form.attributes.materials.length > 0" class="attr-item">
                <span class="attr-label">材質:</span>
                <span class="attr-value">{{ form.attributes.materials.join('、') }}</span>
              </div>
              <div v-if="specsEnabled && variants.length > 0" class="attr-item">
                <span class="attr-label">規格:</span>
                <span class="attr-value">{{ variants.length }} 個規格可用</span>
              </div>

              <!-- 動態屬性預覽 -->
              <template v-if="filledAttributes.length > 0">
                <div v-for="item in filledAttributes" :key="item.name" class="attr-item">
                  <span class="attr-label">{{ item.name }}:</span>
                  <span class="attr-value">{{ item.displayValue }}</span>
                </div>
              </template>
            </div>

            <!-- 描述 -->
            <div class="preview-description">
              {{ form.description ? form.description.slice(0, 50) + (form.description.length > 50 ? '...' : '') : '商品描述...' }}
            </div>

            <!-- 底部按鈕 -->
            <div class="preview-actions">
              <el-button class="action-btn" disabled>💬</el-button>
              <el-button class="action-btn" disabled>🛒</el-button>
              <el-button type="danger" class="buy-btn" disabled>立即購買</el-button>
            </div>
          </div>
        </div>
      </aside>
    </div>

    <!-- 底部按鈕列 -->
    <div class="bottom-actions">
      <div class="actions-wrapper">
        <el-button @click="handleCancel">取消</el-button>
        <el-button :loading="saving" @click="handleSubmit(false)">
          儲存並下架
        </el-button>
        <el-button type="primary" :loading="saving" @click="handleSubmit(true)">
          儲存並上架
        </el-button>
      </div>
    </div>

    <!-- 分類選擇器彈窗 -->
    <CategoryPicker
      v-model="showCategoryPicker"
      @confirm="handleCategorySelect"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules, UploadUserFile, UploadProps } from 'element-plus'
import { Plus, CircleCheck, CircleClose, ArrowRight, Picture, Close, Delete } from '@element-plus/icons-vue'
import { fetchBrands } from '@/api/brand'
import type { Brand } from '@/types/brand'
import { getCategoryAttributes, type CategoryAttribute } from '@/api/categoryAttribute'
import CategoryPicker from '@/components/seller/CategoryPicker.vue'

const router = useRouter()

const tabs = [
  { id: 'section-basic', label: '基本資訊' },
  { id: 'section-attributes', label: '屬性' },
  { id: 'section-description', label: '商品描述' },
  { id: 'section-sales', label: '銷售資訊' },
]

interface SpecOption {
  name: string
  image: File | null
}

interface Spec {
  name: string
  options: SpecOption[]
}

interface Variant {
  spec1: string
  spec2: string | null
  price: number | null
  stock: number
  sku: string
}

interface ProductAttributes {
  brandId: number | null
  productType: string
  materials: string[]
  origin: string
  collarType: string
  occasion: string
  patterns: string[]
  season: string
  styles: string[]
  topLength: string
}

interface ProductForm {
  name: string
  categoryId: number | null
  categoryPath: string
  attributes: ProductAttributes
  dynamicAttributes: Record<number, number | string | number[] | string[]>  // 支援 ID、自填字串、或陣列
  description: string
  images: UploadUserFile[]
  price: number
  stock: number
  specs: Spec[]
  minPurchase: number
  isOnShelf: boolean
}

const formRef = ref<FormInstance>()
const saving = ref<boolean>(false)
const brands = ref<Brand[]>([])
const categoryAttributes = ref<CategoryAttribute[]>([])
const loadingAttributes = ref<boolean>(false)
const currentTab = ref<string>('section-basic')
const showCategoryPicker = ref<boolean>(false)
const specsEnabled = ref<boolean>(false)
const descriptionImageCount = ref<number>(0)
const currentImageIndex = ref<number>(0)

const batchPrice = ref<number | null>(null)
const batchStock = ref<number | null>(null)
const batchSku = ref<string>('')

const form = reactive<ProductForm>({
  name: '',
  categoryId: null,
  categoryPath: '',
  attributes: {
    brandId: null,
    productType: '',
    materials: [],
    origin: '',
    collarType: '',
    occasion: '',
    patterns: [],
    season: '',
    styles: [],
    topLength: '',
  },
  dynamicAttributes: {},
  description: '',
  images: [],
  price: 0,
  stock: 0,
  specs: [
    {
      name: '',
      options: [{ name: '', image: null }],
    },
  ],
  minPurchase: 1,
  isOnShelf: true,
})

const rules: FormRules = {
  name: [
    { required: true, message: '請輸入商品名稱', trigger: 'blur' },
    { min: 5, max: 100, message: '商品名稱需介於 5~100 字', trigger: 'blur' },
  ],
  categoryId: [{ required: true, message: '請選擇分類', trigger: 'change' }],
  price: [
    { required: true, message: '請輸入售價', trigger: 'blur' },
    { type: 'number', min: 1, message: '售價至少為 1', trigger: 'blur' },
  ],
  stock: [{ required: true, message: '請輸入庫存', trigger: 'blur' }],
}

const attributesCompletionCount = computed(() => {
  if (categoryAttributes.value.length === 0) {
    // 降級為靜態計數（舊版）
    let count = 0
    if (form.attributes.brandId) count++
    if (form.attributes.productType) count++
    if (form.attributes.materials.length > 0) count++
    if (form.attributes.origin) count++
    if (form.attributes.collarType) count++
    if (form.attributes.occasion) count++
    if (form.attributes.patterns.length > 0) count++
    if (form.attributes.season) count++
    if (form.attributes.styles.length > 0) count++
    if (form.attributes.topLength) count++
    return count
  }
  
  // 動態屬性計數
  return Object.keys(form.dynamicAttributes).filter((key) => {
    const value = form.dynamicAttributes[parseInt(key)]
    if (Array.isArray(value)) return value.length > 0
    return value !== null && value !== undefined
  }).length
})

function getAttributeCount(attrId: number): number {
  const value = form.dynamicAttributes[attrId]
  return Array.isArray(value) ? value.length : 0
}

const isImageComplete = computed(() => form.images.length >= 3)
const isNameComplete = computed(() => form.name.length >= 5 && form.name.length <= 100)
const isDescriptionComplete = computed(() => form.description.length >= 100 || descriptionImageCount.value >= 1)
const isBrandComplete = computed(() => form.attributes.brandId !== null)

const filledAttributes = computed(() => {
  return categoryAttributes.value
    .filter(attr => {
      const val = form.dynamicAttributes[attr.id]
      if (Array.isArray(val)) return val.length > 0
      return val !== undefined && val !== null && val !== ''
    })
    .map(attr => {
      const val = form.dynamicAttributes[attr.id]
      let displayValue = ''
      if (Array.isArray(val)) {
        displayValue = val
          .map(v => {
            // 如果是字串，直接顯示（允許自填的情況）
            if (typeof v === 'string') return v
            // 否則從選項中找對應的值
            const opt = attr.options.find(o => o.id === v || o.value === v)
            return opt ? opt.value : String(v)
          })
          .join('、')
      } else {
        // 如果是字串，直接顯示（允許自填的情況）
        if (typeof val === 'string') {
          displayValue = val
        } else {
          const opt = attr.options.find(o => o.id === val || o.value === val)
          displayValue = opt ? opt.value : String(val)
        }
      }
      return { name: attr.name, displayValue }
    })
})

function getImageUrl(img: UploadUserFile | string): string {
  if (typeof img === 'string') return img
  if (img.url) return img.url
  if (img.raw) return URL.createObjectURL(img.raw)
  return ''
}

function handleCarouselChange(index: number): void {
  currentImageIndex.value = index
}

const variants = computed<Variant[]>(() => {
  if (!specsEnabled.value || form.specs.length === 0) return []
  
  const validSpecs = form.specs.filter(
    (spec) => spec.name && spec.options.some((opt) => opt.name)
  )
  
  if (validSpecs.length === 0) return []
  
  if (validSpecs.length === 1) {
    return validSpecs[0].options
      .filter((opt) => opt.name)
      .map((opt) => ({
        spec1: opt.name,
        spec2: null,
        price: null,
        stock: 0,
        sku: '',
      }))
  }
  
  const result: Variant[] = []
  const spec1Options = validSpecs[0].options.filter((opt) => opt.name)
  const spec2Options = validSpecs[1].options.filter((opt) => opt.name)
  
  for (const opt1 of spec1Options) {
    for (const opt2 of spec2Options) {
      result.push({
        spec1: opt1.name,
        spec2: opt2.name,
        price: null,
        stock: 0,
        sku: '',
      })
    }
  }
  
  return result
})

async function loadBrands(): Promise<void> {
  try {
    const res = await fetchBrands()
    if (res.success) brands.value = res.data
  } catch (error) {
    console.error('載入品牌失敗', error)
  }
}

loadBrands()

watch(
  () => form.categoryId,
  async (newCategoryId, oldCategoryId) => {
    // 清空舊的屬性值
    if (oldCategoryId !== newCategoryId) {
      form.dynamicAttributes = {}
    }

    if (!newCategoryId) {
      categoryAttributes.value = []
      return
    }

    // 顯示載入狀態
    loadingAttributes.value = true

    try {
      const res = await getCategoryAttributes(newCategoryId)
      
      if (res.success && res.data) {
        categoryAttributes.value = res.data

        // 如果有屬性，顯示成功訊息
        if (res.data.length > 0) {
          ElMessage.success(`已載入 ${res.data.length} 個屬性欄位`)
        }
      } else {
        categoryAttributes.value = []
      }
    } catch (error) {
      console.error('❌ 載入分類屬性失敗:', error)
      categoryAttributes.value = []
      ElMessage.error('載入分類屬性失敗，請稍後再試')
    } finally {
      loadingAttributes.value = false
    }
  }
)

function scrollToSection(sectionId: string): void {
  currentTab.value = sectionId
  const element = document.getElementById(sectionId)
  if (element) {
    element.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

function handleCategorySelect(categoryId: number, categoryPath: string): void {
  form.categoryId = categoryId
  form.categoryPath = categoryPath
}

function handleImageExceed(): void {
  ElMessage.warning('最多只能上傳 9 張圖片')
}

const handleImageChange: UploadProps['onChange'] = (uploadFile, uploadFiles) => {
  // === 圖片驗證 ===
  if (uploadFile.raw) {
    // 1. 格式驗證
    const validTypes = ['image/jpeg', 'image/png', 'image/webp']
    if (!validTypes.includes(uploadFile.raw.type)) {
      ElMessage.error('只支援 JPG、PNG、WEBP 格式')
      const idx = form.images.findIndex(f => f.uid === uploadFile.uid)
      if (idx !== -1) form.images.splice(idx, 1)
      return
    }
    
    // 2. 大小驗證
    if (uploadFile.raw.size > 2 * 1024 * 1024) {
      ElMessage.error('圖片大小不能超過 2MB')
      const idx = form.images.findIndex(f => f.uid === uploadFile.uid)
      if (idx !== -1) form.images.splice(idx, 1)
      return
    }
    
    // 3. 重複檔案檢查
    const isDuplicate = form.images.some(
      f => f.uid !== uploadFile.uid && 
           f.raw && 
           f.raw.name === uploadFile.raw.name && 
           f.raw.size === uploadFile.raw.size
    )
    if (isDuplicate) {
      ElMessage.warning('此圖片已上傳過，請勿重複上傳')
      const idx = form.images.findIndex(f => f.uid === uploadFile.uid)
      if (idx !== -1) form.images.splice(idx, 1)
      return
    }
  }
}

function handleAddDescriptionImage(): void {
  ElMessage.info('TODO: 實作描述圖片上傳功能')
}

function addSpec(): void {
  if (form.specs.length < 2) {
    form.specs.push({
      name: '',
      options: [{ name: '', image: null }],
    })
  }
}

function removeSpec(index: number): void {
  form.specs.splice(index, 1)
}

function addSpecOption(specIndex: number): void {
  form.specs[specIndex].options.push({ name: '', image: null })
}

function removeSpecOption(specIndex: number, optionIndex: number): void {
  if (form.specs[specIndex].options.length > 1) {
    form.specs[specIndex].options.splice(optionIndex, 1)
  }
}

function applyBatch(): void {
  variants.value.forEach((variant) => {
    if (batchPrice.value !== null) variant.price = batchPrice.value
    if (batchStock.value !== null) variant.stock = batchStock.value
    if (batchSku.value) variant.sku = batchSku.value
  })
  ElMessage.success('批量套用成功')
}

function formatPrice(price: number): string {
  return price.toLocaleString('zh-TW')
}

function getBrandName(brandId: number): string {
  const brand = brands.value.find((b) => b.id === brandId)
  return brand?.name || ''
}

async function handleSubmit(publishNow: boolean): Promise<void> {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  form.isOnShelf = publishNow

  saving.value = true
  try {
    const payload = {
      name: form.name,
      categoryId: form.categoryId,
      attributes: form.attributes,
      description: form.description,
      price: specsEnabled.value ? null : form.price,
      stock: specsEnabled.value ? null : form.stock,
      minPurchase: form.minPurchase,
      isOnShelf: form.isOnShelf,
      images: form.images,
      specs: specsEnabled.value ? form.specs : null,
      variants: specsEnabled.value ? variants.value : null,
    }

    // TODO: 實際呼叫後端 API
    // await createProduct(payload)
    ElMessage.success(`商品${publishNow ? '上架' : '下架'}成功（待串接後端）`)
    
    void router.push('/seller/products')
  } catch (error) {
    ElMessage.error('儲存失敗，請稍後再試')
  } finally {
    saving.value = false
  }
}

function handleCancel(): void {
  void router.push('/seller/products')
}
</script>

<style scoped>
.product-edit-page {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 16px 24px 80px;
}

:deep(.el-breadcrumb) {
  margin-bottom: 20px;
  font-size: 14px;
}

.three-column-layout {
  display: flex;
  gap: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

/* 左側:填寫建議 */
.left-sidebar {
  width: 280px;
  flex-shrink: 0;
}

.tips-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
  position: sticky;
  top: 20px;
}

.tips-header {
  font-size: 16px;
  font-weight: 700;
  color: #333;
  margin-bottom: 16px;
  padding-left: 12px;
  border-left: 4px solid #ee4d2d;
}

.tips-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.tip-item {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  font-size: 13px;
  color: #666;
  transition: color 0.2s;
}

.tip-item.completed {
  color: #52c41a;
}

.tip-icon {
  font-size: 18px;
  flex-shrink: 0;
  margin-top: 2px;
}

/* 中間:主表單 */
.main-content {
  flex: 1;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.tab-nav {
  display: flex;
  border-bottom: 2px solid #f0f0f0;
  background: #fff;
  position: sticky;
  top: 0;
  z-index: 10;
}

.tab-item {
  flex: 1;
  padding: 16px 24px;
  text-align: center;
  font-size: 15px;
  font-weight: 500;
  color: #666;
  cursor: pointer;
  border-bottom: 3px solid transparent;
  transition: all 0.3s;
}

.tab-item:hover {
  color: #ee4d2d;
}

.tab-item.active {
  color: #ee4d2d;
  border-bottom-color: #ee4d2d;
}

.form-section {
  padding: 32px 40px;
  border-bottom: 1px solid #f0f0f0;
}

.form-section:last-child {
  border-bottom: none;
}

.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #333;
  margin: 0 0 24px 0;
}

.required-label {
  color: #333;
  font-weight: 500;
}

.required-label::before {
  content: '* ';
  color: #ee4d2d;
}

.image-count,
.char-count {
  float: right;
  font-size: 12px;
  color: #999;
}

.form-hint {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

/* 右側:即時預覽 */
.right-sidebar {
  width: 280px;
  flex-shrink: 0;
}

.preview-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
  position: sticky;
  top: 20px;
  overflow: hidden;
}

.preview-header {
  padding: 16px;
  border-bottom: 1px solid #f0f0f0;
}

.preview-title {
  font-size: 15px;
  font-weight: 700;
  color: #333;
}

.preview-subtitle {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

.preview-content {
  padding: 16px;
}

.preview-image {
  width: 100%;
  aspect-ratio: 1;
  background: #f5f5f5;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 4px;
}

.preview-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.preview-image :deep(.el-carousel__container) {
  height: 250px;
}

.preview-image :deep(.el-carousel__indicators) {
  display: none;
}

.image-counter {
  text-align: center;
  font-size: 12px;
  color: #999;
  margin-bottom: 12px;
}

.preview-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ccc;
}

.preview-name {
  font-size: 14px;
  font-weight: 500;
  color: #333;
  margin-bottom: 8px;
  line-height: 1.4;
  min-height: 40px;
}

.preview-price {
  color: #ee4d2d;
  margin-bottom: 12px;
}

.price-symbol {
  font-size: 14px;
}

.price-value {
  font-size: 20px;
  font-weight: 700;
  margin-left: 4px;
}

.preview-attrs {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 12px;
}

.attr-item {
  font-size: 12px;
  color: #666;
}

.attr-label {
  color: #999;
  margin-right: 8px;
}

.attr-value {
  color: #333;
}

.preview-description {
  font-size: 12px;
  color: #666;
  line-height: 1.6;
  margin-bottom: 16px;
  min-height: 40px;
}

.preview-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 40px;
  padding: 8px;
}

.buy-btn {
  flex: 1;
  background: #ee4d2d;
  color: #fff;
  border: none;
}

/* 底部按鈕列 */
.bottom-actions {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: #fff;
  border-top: 1px solid #e0e0e0;
  padding: 16px 24px;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  z-index: 100;
}

.actions-wrapper {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.actions-wrapper .el-button {
  min-width: 120px;
}

/* 屬性區塊 */
.section-header-with-progress {
  margin-bottom: 24px;
}

.completion-hint {
  font-size: 13px;
  color: #999;
  margin-top: 8px;
}

.add-link {
  color: #ee4d2d;
  text-decoration: none;
}

.add-link:hover {
  text-decoration: underline;
}

.empty-hint {
  padding: 40px 20px;
  text-align: center;
}

.attr-count {
  margin-left: 8px;
  font-size: 12px;
  color: #999;
}

/* 商品描述區塊 */
.description-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

/* 規格設定 */
.enable-specs-btn {
  padding: 20px;
  text-align: center;
  background: #f7f8fa;
  border: 1px dashed #d9d9d9;
  border-radius: 4px;
}

.specs-config {
  margin-top: 16px;
}

.spec-group {
  margin-bottom: 24px;
  padding: 20px;
  background: #f7f8fa;
  border-radius: 8px;
}

.spec-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.spec-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.spec-option-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.option-label {
  font-size: 13px;
  color: #666;
  width: 50px;
}

.option-image-upload {
  width: 40px;
  height: 40px;
}

.option-image-upload :deep(.el-upload) {
  width: 40px;
  height: 40px;
  border: 1px dashed #d9d9d9;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: border-color 0.3s;
}

.option-image-upload :deep(.el-upload:hover) {
  border-color: #ee4d2d;
}

.upload-icon {
  font-size: 18px;
  color: #999;
}

.variants-table {
  margin-top: 24px;
  padding: 20px;
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}

.table-header {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  margin-bottom: 16px;
}

.batch-edit {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  padding: 16px;
  background: #fff5f0;
  border-radius: 4px;
}

.batch-edit .el-input-number {
  flex-shrink: 0;
}

:deep(.el-table) {
  font-size: 13px;
}

:deep(.el-table th) {
  background: #f7f8fa;
  font-weight: 600;
}

:deep(.el-table .el-input-number) {
  width: 100%;
}

:deep(.el-table .el-input-number .el-input__inner) {
  text-align: left;
}
</style>
