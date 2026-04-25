<template>
  <div class="product-edit-page">
    <!-- 麵包屑 -->
    <el-breadcrumb separator=">">
      <el-breadcrumb-item :to="{ path: '/' }">首頁</el-breadcrumb-item>
      <el-breadcrumb-item :to="{ path: '/seller/products' }">我的商品</el-breadcrumb-item>
      <el-breadcrumb-item>{{ pageTitle }}</el-breadcrumb-item>
    </el-breadcrumb>

    <!-- 三欄布局 -->
    <div class="three-column-layout">
      <!-- 左側:填寫建議 -->
      <aside class="left-sidebar">
        <div class="sticky-wrapper">
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
        </div>
      </aside>

      <!-- 中間:主表單 -->
      <main class="main-content" v-loading="loading">
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

        <el-form ref="formRef" :model="form" :rules="rules" label-position="top" :disabled="isUnderReview">
          <!-- 審核中提示 (status=2) -->
          <el-alert
            v-if="isUnderReview"
            type="warning"
            :closable="false"
            show-icon
            title="商品審核中，暫時無法編輯"
            style="margin-bottom: 20px;"
          />

          <!-- 退回原因提示 (status=3) -->
          <el-alert
            v-if="isEditMode && productData?.status === 3 && productData?.rejectReason"
            type="error"
            :closable="false"
            show-icon
            style="margin-bottom: 20px;"
          >
            <template #title>
              <strong>審核退回原因：{{ productData.rejectReason }}</strong>
            </template>
            <template #default>
              請根據退回原因修改商品資料後重新送審
            </template>
          </el-alert>

          <!-- 已上架提示 (status=1) -->
          <el-alert
            v-if="isEditMode && productData?.status === 1"
            type="info"
            :closable="false"
            show-icon
            style="margin-bottom: 20px;"
          >
            <template #title>編輯已上架商品，儲存後立即生效</template>
          </el-alert>

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
                :file-list="form.images"
                list-type="picture-card"
                :limit="9"
                :auto-upload="false"
                accept=".jpg,.jpeg,.png,.webp"
                :on-exceed="handleImageExceed"
                :on-change="handleImageChange"
                :on-remove="handleImageRemove"
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
              <div class="form-hint">商品名稱需介於 5~60 字</div>
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
                    :placeholder="attr.allowCustom !== false ? `選擇或自行輸入${attr.name}` : `選擇${attr.name}`"
                    clearable
                    :filterable="attr.allowCustom !== false"
                    :allow-create="attr.allowCustom !== false"
                    default-first-option
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
                    :placeholder="attr.allowCustom !== false ? `選擇或多個輸入${attr.name}` : `選擇${attr.name}（最多${attr.maxSelect || 5}個）`"
                    clearable
                    :filterable="attr.allowCustom !== false"
                    :allow-create="attr.allowCustom !== false"
                    default-first-option
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
              <div class="description-editor-container">
                <!-- 隱藏的檔案輸入框 (由 Quill 圖片按鈕觸發) -->
                <input
                  ref="descImageInput"
                  type="file"
                  accept="image/*"
                  style="display: none"
                  @change="handleDescriptionImageFileChange"
                />

                <QuillEditor
                  v-if="!loading"
                  ref="quillEditorRef"
                  v-model:content="form.description"
                  content-type="html"
                  :options="editorOptions"
                  placeholder="請輸入詳細的商品描述，展示商品特色..."
                  class="description-quill"
                />
              </div>
              <div class="description-hint">
                建議至少 100 字。
              </div>
            </el-form-item>          </section>

          <!-- 銷售資訊 -->
          <section id="section-sales" class="form-section">
            <h2 class="section-title">銷售資訊</h2>

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
              <div class="form-hint">最低購買數量是指買家一次至少購買的商品數量。請注意,若庫存少於最低購買數量,買家將無法下單購買。</div>
            </el-form-item>

            <!-- 規格設定 -->
            <el-form-item>
              <template #label>
                <span>規格</span>
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
                        :on-change="(file: UploadFile) => handleOptionImageChange(specIndex, optIndex, file)"
                      >
                        <img
                          v-if="option.imagePreview"
                          :src="option.imagePreview"
                          class="option-image-preview"
                          :alt="option.name"
                        />
                        <el-icon v-else class="upload-icon"><Picture /></el-icon>
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
                  <el-table :data="variantData" border style="width: 100%">
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
          </section>
        </el-form>
      </main>

      <!-- 右側:即時預覽 -->
      <aside class="right-sidebar">
        <div class="sticky-wrapper">
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
              <div v-if="form.categoryPath" class="attr-item">
                <span class="attr-label">種類:</span>
                <span class="attr-value">{{ form.categoryPath }}</span>
              </div>
              <div v-if="form.attributes.brandId" class="attr-item">
                <span class="attr-label">品牌:</span>
                <span class="attr-value">{{ getBrandName(form.attributes.brandId) }}</span>
              </div>
              <!-- 動態屬性預覽 -->
              <div v-for="attr in previewAttributes" :key="attr.label" class="attr-item">
                <span class="attr-label">{{ attr.label }}:</span>
                <span class="attr-value">{{ attr.value }}</span>
              </div>
              <div v-if="specsEnabled && variants.length > 0" class="attr-item">
                <span class="attr-label">規格:</span>
                <span class="attr-value">{{ variants.length }} 個組合可用</span>
              </div>
            </div>

            <!-- 描述 -->
            <div class="preview-description" v-html="form.description || '商品描述...'"></div>

            <!-- 底部按鈕 -->
            <div class="preview-actions">
              <el-button class="action-btn" disabled>💬</el-button>
              <el-button class="action-btn" disabled>🛒</el-button>
              <el-button type="danger" class="buy-btn" disabled>立即購買</el-button>
            </div>
          </div>
        </div>
      </div>
    </aside>
  </div>

    <!-- 底部按鈕列 -->
    <div class="bottom-actions">
      <div class="actions-wrapper">
        <!-- 取消（永遠顯示） -->
        <el-button @click="handleCancel">取消</el-button>

        <!-- 情況 6：審核中 → 僅提示，無儲存按鈕 -->
        <template v-if="isUnderReview">
          <el-button type="info" disabled>審核中，無法編輯</el-button>
        </template>

        <!-- 情況 1 & 2：新增商品 或 草稿 -->
        <template v-else-if="isNewProduct || isDraft">
          <el-button :loading="saving" @click="handleSaveDraft">儲存草稿</el-button>
          <el-button type="primary" :loading="saving" @click="handleSaveAndSubmit">儲存並送審</el-button>
        </template>

        <!-- 情況 3：已上架 → 直接生效 -->
        <template v-else-if="isOnShelf">
          <el-button type="primary" :loading="saving" @click="handleSave">儲存修改</el-button>
        </template>

        <!-- 情況 4：賣家自己下架（曾通過審核） → 可儲存或直接上架 -->
        <template v-else-if="isSelfOffShelf">
          <el-button :loading="saving" @click="handleSave">儲存</el-button>
          <el-button type="success" :loading="saving" @click="handleReShelf">上架</el-button>
        </template>

        <!-- 情況 5：已退回 / 強制下架 → 修改後重新送審 -->
        <template v-else-if="isRejectedOrForced">
          <el-button type="primary" :loading="saving" @click="handleSaveAndSubmit">儲存並送審</el-button>
        </template>
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
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules, UploadUserFile, UploadProps, UploadFile } from 'element-plus'
import { Plus, CircleCheck, CircleClose, ArrowRight, Picture, Close, Delete } from '@element-plus/icons-vue'
import { QuillEditor } from '@vueup/vue-quill'
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import { fetchBrands } from '@/api/brand'
import { createSellerProduct, addSellerProductVariant, getSellerProductDetail, updateSellerProduct, updateProductImages, updateProductStatus, uploadDescriptionImage } from '@/api/product'
import type { Brand } from '@/types/brand'
import type { SellerProductDetail } from '@/types/product'
import { getCategoryAttributes, type CategoryAttribute } from '@/api/categoryAttribute'
import CategoryPicker from '@/components/seller/CategoryPicker.vue'

const router = useRouter()
const route = useRoute()

// 編輯模式判斷
const productId = computed(() => route.params.id ? Number(route.params.id) : null)
const isEditMode = computed(() => !!productId.value)
const pageTitle = computed(() => isEditMode.value ? '編輯商品' : '新增商品')

// 從 Query 取得來源 Tab
const fromTab = computed(() => (route.query.fromTab as string) || 'all')

const tabs = [
  { id: 'section-basic', label: '基本資訊' },
  { id: 'section-attributes', label: '屬性' },
  { id: 'section-description', label: '商品描述' },
  { id: 'section-sales', label: '銷售資訊' },
]

interface SpecOption {
  name: string
  image: File | null
  imagePreview: string | null
}

interface Spec {
  name: string
  options: SpecOption[]
}

interface Variant {
  id?: number
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
const loading = ref<boolean>(false)
const brands = ref<Brand[]>([])
const categoryAttributes = ref<CategoryAttribute[]>([])
const loadingAttributes = ref<boolean>(false)
const currentTab = ref<string>('section-basic')
const showCategoryPicker = ref<boolean>(false)
const specsEnabled = ref<boolean>(false)
const descImageInput = ref<HTMLInputElement | null>(null)
const quillEditorRef = ref<any>(null)
const currentImageIndex = ref<number>(0)
const productData = ref<SellerProductDetail | null>(null)
const originalImageCount = ref<number>(0) // 記錄載入時的圖片數量

// 編輯器配置
const editorOptions = {
  theme: 'snow',
  modules: {
    toolbar: {
      container: [
        ['bold', 'italic', 'underline', 'strike'],
        [{ header: 1 }, { header: 2 }],
        [{ list: 'ordered' }, { list: 'bullet' }],
        [{ color: [] }, { background: [] }],
        ['clean'],
        ['image'], // 保留圖片按鈕，但會被自定義攔截
      ],
      handlers: {
        image: function () {
          // 攔截點擊圖片按鈕事件，觸發隱藏的 input
          document.querySelector<HTMLInputElement>('input[ref="descImageInput"]')?.click()
          // 由於我們使用的是 ref 綁定，這裡直接用我們定義的 handleAddDescriptionImage
          handleAddDescriptionImage()
        },
      },
    },
  },
}

// 描述中的圖片數量 (計算 <img> 標籤)
const descriptionImageCount = computed(() => {
  const matches = form.description.match(/<img/g)
  return matches ? matches.length : 0
})

const batchPrice = ref<number | null>(null)
const batchStock = ref<number | null>(null)
const batchSku = ref<string>('')

const variantData = ref<Variant[]>([])

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
      options: [{ name: '', image: null, imagePreview: null }],
    },
  ],
  minPurchase: 1,
  isOnShelf: true,
})

const rules = computed<FormRules>(() => ({
  name: [
    { required: true, message: '請輸入商品名稱', trigger: 'blur' },
    { min: 5, message: '商品名稱至少需要 5 個字', trigger: 'blur' },
    { max: 60, message: '商品名稱最多 60 個字', trigger: 'blur' },
  ],
  categoryId: [{ required: true, message: '請選擇分類', trigger: 'change' }],
  minPurchase: [
    { required: true, message: '請輸入最低購買數量', trigger: 'blur' },
    { type: 'number', min: 1, message: '最低購買數量至少為 1', trigger: 'blur' },
  ],
  ...(specsEnabled.value
    ? {}
    : {
        price: [
          { required: true, message: '請輸入商品價格', trigger: 'blur' },
          { type: 'number', min: 1, message: '商品價格至少為 1', trigger: 'blur' },
        ],
        stock: [
          { required: true, message: '請輸入商品數量', trigger: 'blur' },
          { type: 'number', min: 0, message: '商品數量不能小於 0', trigger: 'blur' },
        ],
      }),
}))

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

// ── 商品狀態判斷 ────────────────────────────────────────────────
// status: 0=未上架, 1=已上架, 2=待審核, 3=審核退回
// reviewStatus: 0=待審核, 1=通過, 2=退回, 3=重新送審

const isNewProduct = computed(() => !route.params.id)

const isOnShelf = computed(() => productData.value?.status === 1)

const isUnderReview = computed(() => productData.value?.status === 2)

const isRejectedOrForced = computed(() => productData.value?.status === 3)

const hasBeenApproved = computed(() => productData.value?.reviewStatus === 1)

const isSelfOffShelf = computed(
  () => productData.value?.status === 0 && hasBeenApproved.value,
)

const isDraft = computed(
  () => isEditMode.value && productData.value?.status === 0 && !hasBeenApproved.value,
)

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

// Sync variant spec combinations → editable variantData, preserving user-entered price/stock/sku
watch(variants, (newVariants) => {
  const oldData = [...variantData.value]
  
  variantData.value = newVariants.map((nv, idx) => {
    // 1. 策略 A：完全匹配 (名稱沒變的情況，優先保留)
    const exactMatch = oldData.find(v => v.spec1 === nv.spec1 && v.spec2 === nv.spec2)
    if (exactMatch) return { ...exactMatch }

    // 2. 策略 B：智慧合併 (Smart Merge)
    // 若長度一致，通常代表使用者只是在修改某個選項的文字，我們依索引繼承資料
    if (oldData.length === newVariants.length) {
      const prev = oldData[idx]
      return {
        ...nv,
        id: prev.id,       // 關鍵：保留資料庫 ID
        price: prev.price,
        stock: prev.stock,
        sku: prev.sku
      }
    }

    // 3. 策略 C：全新組合
    return { ...nv }
  })
}, { immediate: true })

/** 預覽顯示的動態屬性 */
const previewAttributes = computed(() => {
  const result: Array<{ label: string; value: string }> = []
  
  Object.entries(form.dynamicAttributes).forEach(([attrId, val]) => {
    if (val === null || val === undefined || val === '') return
    
    const id = parseInt(attrId)
    const attrDef = categoryAttributes.value.find(a => a.id === id)
    if (!attrDef) return

    const label = attrDef.name
    const values = Array.isArray(val) ? val : [val]
    
    const displayValues = values.map(v => {
      // 若是選項 ID
      const opt = attrDef.options.find(o => o.id === v || o.value === v)
      return opt ? opt.value : String(v)
    })

    if (displayValues.length > 0) {
      result.push({ label, value: displayValues.join('、') })
    }
  })
  
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

async function loadProductData(): Promise<void> {
  if (!isEditMode.value || !productId.value) return
  
  loading.value = true
  try {
    const product = await getSellerProductDetail(productId.value)
    productData.value = product

    console.log('載入商品資料:', product)
    
    // 1. 基本資訊還原
    form.name = product.name || ''
    form.description = product.description || ''
    form.categoryId = product.categoryId || null
    form.categoryPath = product.categoryName || ''
    form.minPurchase = product.minPurchase || 1
    
    if (product.brandId) {
      form.attributes.brandId = product.brandId
    }
    
    // 無規格時的價格和庫存
    form.price = product.minPrice || 0
    form.stock = 0 
    
    // 2. 圖片還原
    if (product.images && product.images.length > 0) {
      form.images = product.images.map((url: string, idx: number) => {
        const originalUrl = url.startsWith('http')
          ? (() => { try { return new URL(url).pathname } catch { return url } })()
          : url
        return {
          uid: -(idx + 1),
          name: `image-${idx}`,
          url: url.startsWith('http') ? url : `https://localhost:7125${url}`,
          status: 'success' as const,
          _originalUrl: originalUrl,
        }
      }) as UploadUserFile[]
      originalImageCount.value = product.images.length
    } else {
      originalImageCount.value = 0
    }
    
    // 3. 規格還原與舊資料相容邏輯
    let reconstructedSpecs: Spec[] = []
    const hasSpecDef = product.specDefinitionJson && product.specDefinitionJson !== '[]' && product.specDefinitionJson !== 'null'
    const hasVariants = product.variants && product.variants.length > 0

    // A. 嘗試從 specDefinitionJson 還原
    if (hasSpecDef) {
      try {
        const specsDef = JSON.parse(product.specDefinitionJson)
        if (Array.isArray(specsDef) && specsDef.length > 0) {
          reconstructedSpecs = specsDef.map((s: any) => ({
            name: s.name || '',
            // 如果 specDefinitionJson 裡已經有 options，就直接用；否則初始化為空陣列
            options: Array.isArray(s.options) 
              ? s.options.map((opt: any) => ({
                  name: typeof opt === 'string' ? opt : (opt.name || ''),
                  image: null,
                  imagePreview: null
                }))
              : []
          }))
        }
      } catch (e) {
        console.error('解析規格定義失敗:', e)
      }
    }

    // B. 舊商品相容邏輯：如果沒有規格定義或選項為空，則從 variants 反推
    const needsInference = reconstructedSpecs.length === 0 || reconstructedSpecs.every(s => s.options.length === 0)
    
    if (needsInference && hasVariants) {
      const specOptionsMap = new Map<string, Set<string>>()
      const specNamesOrder: string[] = reconstructedSpecs.length > 0 ? reconstructedSpecs.map(s => s.name) : []

      product.variants.forEach((v: any) => {
        try {
          const specValues = typeof v.specValueJson === 'string' ? JSON.parse(v.specValueJson) : v.specValueJson
          if (specValues) {
            Object.entries(specValues).forEach(([name, value]) => {
              if (!specOptionsMap.has(name)) {
                specOptionsMap.set(name, new Set())
                if (!specNamesOrder.includes(name)) specNamesOrder.push(name)
              }
              specOptionsMap.get(name)?.add(value as string)
            })
          }
        } catch (e) {
          console.error('解析變體規格值失敗:', e)
        }
      })

      if (specNamesOrder.length > 0) {
        reconstructedSpecs = specNamesOrder.map(name => ({
          name,
          options: Array.from(specOptionsMap.get(name) || []).map(optName => ({
            name: optName,
            image: null,
            imagePreview: null
          }))
        }))
        console.log('偵測到舊資料，已自動重建規格定義結構')
      }
    }

    // 4. 賦值並同步變體表格
    if (reconstructedSpecs.length > 0) {
      specsEnabled.value = true
      form.specs = reconstructedSpecs
      
      // 確保在 nextTick 之後或直接在同一個 tick 更新 variantData
      // 由於 watch(variants) 會在 reactive 更新後執行，我們在這裡先準備好對應的資料
      if (hasVariants) {
        const specNames = reconstructedSpecs.map(s => s.name)
        variantData.value = product.variants.map((v: any) => {
          const specValues = typeof v.specValueJson === 'string' ? JSON.parse(v.specValueJson) : v.specValueJson
          return {
            id: v.id,
            spec1: specValues[specNames[0]] || '',
            spec2: specNames[1] ? (specValues[specNames[1]] || null) : null,
            price: v.price || null,
            stock: v.stock || 0,
            sku: v.skuCode || '',
          }
        })
      }
    } else {
      specsEnabled.value = false
    }
    
    // 5. 屬性還原
    if (product.categoryId) {
      loadingAttributes.value = true
      try {
        const attrRes = await getCategoryAttributes(product.categoryId)
        if (attrRes.success && attrRes.data) {
          categoryAttributes.value = attrRes.data
          
          if (product.attributesJson) {
            try {
              const savedAttrs = JSON.parse(product.attributesJson)
              if (Array.isArray(savedAttrs)) {
                savedAttrs.forEach((saved: any) => {
                  const id = saved.attributeId || saved.AttributeId
                  const value = saved.optionId || saved.OptionId || saved.customValue || saved.CustomValue
                  if (value === undefined || value === null) return

                  // 尋找定義，判斷是否多選
                  const def = categoryAttributes.value.find(c => c.id === id)
                  const isMultiple = def?.isMultiple

                  if (isMultiple) {
                    // 多選：確保是陣列並 push
                    if (!Array.isArray(form.dynamicAttributes[id])) {
                      form.dynamicAttributes[id] = []
                    }
                    ;(form.dynamicAttributes[id] as any[]).push(value)
                  } else {
                    // 單選：直接賦值
                    form.dynamicAttributes[id] = value
                  }
                })
              }
            } catch (e) {
              console.error('解析屬性 JSON 失敗:', e)
            }
          }
        }
      } catch (e) {
        console.error('載入屬性定義失敗:', e)
      } finally {
        loadingAttributes.value = false
      }
    }

    ElMessage.success('商品資料載入成功')
  } catch (error) {
    console.error('載入商品資料失敗:', error)
    ElMessage.error('載入商品資料失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadBrands()
  await loadProductData()
})

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

const handleImageRemove: UploadProps['onRemove'] = (_uploadFile, uploadFiles) => {
  form.images = uploadFiles
}

const handleImageChange: UploadProps['onChange'] = (uploadFile, uploadFiles) => {
  // === 圖片驗證（新上傳的檔案才驗證）===
  if (uploadFile.raw) {
    // 1. 格式驗證
    const validTypes = ['image/jpeg', 'image/png', 'image/webp']
    if (!validTypes.includes(uploadFile.raw.type)) {
      ElMessage.error('只支援 JPG、PNG、WEBP 格式')
      form.images = uploadFiles.filter(f => f.uid !== uploadFile.uid)
      return
    }

    // 2. 大小驗證
    if (uploadFile.raw.size > 2 * 1024 * 1024) {
      ElMessage.error('圖片大小不能超過 2MB')
      form.images = uploadFiles.filter(f => f.uid !== uploadFile.uid)
      return
    }

    // 3. 重複檔案檢查
    const isDuplicate = form.images.some(
      f => f.uid !== uploadFile.uid &&
           f.raw &&
           f.raw.name === uploadFile.raw!.name &&
           f.raw.size === uploadFile.raw!.size
    )
    if (isDuplicate) {
      ElMessage.warning('此圖片已上傳過，請勿重複上傳')
      form.images = uploadFiles.filter(f => f.uid !== uploadFile.uid)
      return
    }

    // 設定上傳狀態為成功（顯示打勾圖示）
    uploadFile.status = 'success'
  }

  // 以 uploadFiles 為準同步 form.images，避免舊圖消失
  form.images = uploadFiles
}

function handleAddDescriptionImage(): void {
  if (descriptionImageCount.value >= 12) {
    ElMessage.warning('商品描述最多只能包含 12 張圖片')
    return
  }
  descImageInput.value?.click()
}

async function handleDescriptionImageFileChange(event: Event): Promise<void> {
  const input = event.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  
  // 1. 基本檢查
  const validTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!validTypes.includes(file.type)) {
    ElMessage.error('只支援 JPG、PNG、WEBP 格式')
    input.value = ''
    return
  }
  if (file.size > 5 * 1024 * 1024) {
    ElMessage.error('圖片大小不能超過 5MB')
    input.value = ''
    return
  }

  // 2. 上傳圖片
  try {
    const res = await uploadDescriptionImage(file)
    if (res.success && (res as any).url) {
      const imgUrl = (res as any).url
      
      // 使用 Quill API 插入圖片，避免直接修改字串導致狀態不同步
      if (quillEditorRef.value) {
        const quill = quillEditorRef.value.getQuill()
        const range = quill.getSelection(true)
        quill.insertEmbed(range.index, 'image', imgUrl)
        quill.setSelection(range.index + 1)
        ElMessage.success('圖片上傳成功')
      } else {
        // Fallback: 如果抓不到實體，才用字串附加
        form.description += `<img src="${imgUrl}" style="max-width: 100%;" />`
      }
    } else {
      ElMessage.error('圖片上傳失敗')
    }
  } catch (error) {
    console.error('上傳描述圖片失敗:', error)
    console.error('上傳失敗詳細資訊:', error)
    ElMessage.error('圖片上傳發生錯誤')
  } finally {
    input.value = ''
  }
}

function handleOptionImageChange(specIndex: number, optIndex: number, file: UploadFile): void {
  const raw = file.raw
  if (!raw) return
  const validTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!validTypes.includes(raw.type)) {
    ElMessage.error('只支援 JPG、PNG、WEBP 格式')
    return
  }
  if (raw.size > 2 * 1024 * 1024) {
    ElMessage.error('圖片大小不能超過 2MB')
    return
  }
  const option = form.specs[specIndex]?.options[optIndex]
  if (!option) return
  option.image = raw
  const reader = new FileReader()
  reader.onload = (e) => {
    option.imagePreview = e.target?.result as string ?? null
  }
  reader.readAsDataURL(raw)
}

function addSpec(): void {
  if (form.specs.length < 2) {
    form.specs.push({
      name: '',
      options: [{ name: '', image: null, imagePreview: null }],
    })
  }
}

function removeSpec(index: number): void {
  form.specs.splice(index, 1)
}

function addSpecOption(specIndex: number): void {
  form.specs[specIndex].options.push({ name: '', image: null, imagePreview: null })
}

function removeSpecOption(specIndex: number, optionIndex: number): void {
  if (form.specs[specIndex].options.length > 1) {
    form.specs[specIndex].options.splice(optionIndex, 1)
  }
}

function applyBatch(): void {
  variantData.value.forEach((variant) => {
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

async function handleSubmit(publishNow: boolean, redirectAfter = true, isDraftAction = false): Promise<boolean> {
  const mode = publishNow ? 'submit' : 'draft'
  console.log('[儲存模式]', mode)

  // 1. 驗證
  if (publishNow) {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) {
      ElMessage.warning('請填寫所有必填欄位')
      return false
    }
  } else if (!form.name) {
    ElMessage.warning('請輸入商品名稱才能儲存草稿')
    return false
  }

  saving.value = true
  try {
    // 準備變體資料 (格式轉換與型別檢查)
    const processedVariants = specsEnabled.value ? variantData.value.map(v => {
      const specNames = form.specs.map(s => s.name)
      const specValueMap: Record<string, string> = {}
      if (specNames[0] && v.spec1) specValueMap[specNames[0]] = v.spec1
      if (specNames[1] && v.spec2) specValueMap[specNames[1]] = v.spec2

      return {
        id: v.id, // 保留 ID
        skuCode: v.sku || '',
        variantName: Object.values(specValueMap).join('/'),
        specValueJson: JSON.stringify(specValueMap),
        price: Number(v.price) || 0,
        stock: Number(v.stock) || 0
      }
    }) : []

    const variantsJson = JSON.stringify(processedVariants)

    // 準備屬性資料 (分流 OptionId 與 CustomValue)
    const processedAttributes: any[] = []
    Object.entries(form.dynamicAttributes).forEach(([attrId, value]) => {
      const attributeId = parseInt(attrId)
      const values = Array.isArray(value) ? value : [value]
      
      values.forEach(val => {
        if (val === null || val === undefined || val === '') return

        // 判斷方式：若能轉為純數字，代表是選中的選項 ID；否則視為自填文字
        const numericValue = Number(val)
        const isOptionId = !isNaN(numericValue) && typeof val !== 'boolean'

        if (isOptionId) {
          processedAttributes.push({
            AttributeId: attributeId,
            OptionId: numericValue,
            CustomValue: null
          })
        } else {
          processedAttributes.push({
            AttributeId: attributeId,
            OptionId: null,
            CustomValue: String(val)
          })
        }
      })
    })

    const attributesJson = JSON.stringify(processedAttributes)

    if (isEditMode.value && productId.value) {
      // ===== 編輯模式：使用 JSON =====
      const updateData = {
        name: form.name,
        description: form.description || '',
        categoryId: form.categoryId,
        brandId: form.attributes.brandId || null,
        price: Number(form.price) || 0,
        stock: Number(form.stock) || 0,
        minPurchase: Number(form.minPurchase) || 1,
        mode: mode,
        specDefinitionJson: specsEnabled.value && form.specs.length > 0
          ? JSON.stringify(form.specs.map(s => ({ name: s.name })))
          : '[]',
        variantsJson: variantsJson,
        attributesJson: attributesJson // 加入屬性 JSON
      }

      await updateSellerProduct(productId.value, updateData)
      
      // 更新圖片
      const newFiles = form.images.filter(f => f.raw)
      if (newFiles.length > 0 || form.images.length !== originalImageCount.value) {
        const formData = new FormData()
        form.images.forEach(file => {
          if (file.raw) {
            formData.append('images', file.raw)
          } else {
            const f = file as any
            const url = f._originalUrl ?? f.url ?? ''
            if (url) formData.append('existingImages', url)
          }
        })
        await updateProductImages(productId.value, formData)
      }
    } else {
      // ===== 新增模式：使用 FormData =====
      const fd = new FormData()
      fd.append('name', form.name)
      fd.append('description', form.description)
      fd.append('mode', mode)
      if (form.categoryId) fd.append('categoryId', String(form.categoryId))
      if (form.attributes.brandId) fd.append('brandId', String(form.attributes.brandId))
      fd.append('price', String(form.price || 0))
      fd.append('stock', String(form.stock || 0))
      fd.append('minPurchase', String(form.minPurchase || 1))
      
      fd.append('variantsJson', variantsJson)
      fd.append('attributesJson', attributesJson) // 加入屬性 JSON

      if (specsEnabled.value && form.specs.length > 0) {
        fd.append('specDefinitionJson', JSON.stringify(form.specs.map(s => ({ name: s.name }))))
      }

      form.images.forEach((img, idx) => {
        if (img.raw) fd.append('images', img.raw)
      })

      await createSellerProduct(fd)
    }

    ElMessage.success(publishNow ? '商品已提交審核' : '商品資料已儲存')
    if (redirectAfter) {
      const targetTab = publishNow ? 'review' : (isDraftAction ? 'draft' : fromTab.value)
      void router.push({ path: '/seller/products', query: { tab: targetTab } })
    }
    return true
  } catch (error) {
    console.error('儲存失敗:', error)
    ElMessage.error('儲存失敗，請稍後再試')
    return false
  } finally {
    saving.value = false
  }
}

function handleCancel(): void {
  void router.push({ path: '/seller/products', query: { tab: fromTab.value } })
}

async function handleSaveDraft(): Promise<void> {
  // 強制去草稿頁籤
  await handleSubmit(false, true, true)
}

async function handleSave(): Promise<void> {
  // 儲存後回到來源頁籤
  await handleSubmit(false, true, false)
}

async function handleSaveAndSubmit(): Promise<void> {
  try {
    await ElMessageBox.confirm(
      '確定要送出審核嗎？審核期間將無法編輯商品。',
      '送審確認',
      { type: 'warning' },
    )
  } catch {
    return
  }
  await handleSubmit(true)
}

async function handleReShelf(): Promise<void> {
  try {
    await ElMessageBox.confirm('確定要重新上架嗎？', '上架確認', { type: 'warning' })
  } catch {
    return
  }
  const saved = await handleSubmit(false, false)
  if (!saved) return
  try {
    await updateProductStatus(productId.value!, 1)
    ElMessage.success('商品已上架')
    void router.push('/seller/products?tab=on')
  } catch {
    ElMessage.error('上架失敗，請稍後再試')
  }
}
</script>

<style scoped>
.product-edit-page {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 16px 24px 80px;
  overflow: visible !important; /* 強制解除溢出限制，確保 sticky 生效 */
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
  align-items: flex-start; /* 確保子元素不會被拉伸，讓 sticky 有滑動空間 */
}

/* 側邊欄容器 */
.left-sidebar,
.right-sidebar {
  width: 280px;
  flex-shrink: 0;
}

/* 終極 Sticky 包裹器 */
.sticky-wrapper {
  position: sticky;
  top: 20px;
  z-index: 10;
  height: fit-content;
}

.tips-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
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
  position: sticky;
  top: 20px;
  height: fit-content;
  align-self: flex-start; /* 防止 flex 延伸 */
}

.preview-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
  overflow: hidden;
  max-height: calc(100vh - 40px); /* 限制最大高度，避免超出視窗 */
  display: flex;
  flex-direction: column;
}

.preview-header {
  padding: 16px;
  border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
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
  overflow-y: auto; /* 內容過多時可捲動 */
  flex: 1;
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
  overflow: hidden;
}

.preview-description :deep(img) {
  max-width: 100%;
  height: auto;
  display: block;
  margin: 8px 0;
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

.description-editor-container {
  width: 100%;
  border: 1px solid #d9d9d9;
  border-radius: 4px;
}

.description-quill {
  min-height: 300px;
}

.description-hint {
  font-size: 12px;
  color: #999;
  margin-top: 8px;
  width: 100%;
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

.option-image-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 4px;
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
