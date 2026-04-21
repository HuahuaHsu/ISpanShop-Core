<template>
  <div class="store-settings-container">
    <div class="page-header">
      <h2 class="title">賣場介紹與設定</h2>
      <p class="subtitle">管理您的賣場基本資訊與經營狀態</p>
    </div>

    <el-card v-loading="loading" class="settings-card">
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-position="top"
        class="settings-form"
      >
        <el-row :gutter="40">
          <!-- 左側：基本資訊 -->
          <el-col :xs="24" :md="16">
            <el-form-item label="賣場名稱" prop="storeName">
              <el-input v-model="form.storeName" placeholder="請輸入賣場名稱" />
            </el-form-item>

            <el-form-item label="賣場介紹" prop="description">
              <el-input
                v-model="form.description"
                type="textarea"
                :rows="6"
                placeholder="介紹您的賣場特色、服務理念等..."
              />
            </el-form-item>

            <el-form-item label="營業狀態">
              <el-radio-group v-model="form.storeStatus">
                <el-radio-button :label="1">
                  <el-icon><VideoPlay /></el-icon> 營業中
                </el-radio-button>
                <el-radio-button :label="2">
                  <el-icon><CoffeeCup /></el-icon> 休假中
                </el-radio-button>
              </el-radio-group>
              <div class="status-tip">
                <el-alert
                  v-if="form.storeStatus === 2"
                  title="目前為休假狀態，買家仍可瀏覽商品但無法下單。"
                  type="warning"
                  :closable="false"
                  show-icon
                  class="mt-2"
                />
              </div>
            </el-form-item>
          </el-col>

          <!-- 右側：Logo 上傳 -->
          <el-col :xs="24" :md="8">
            <el-form-item label="賣場 Logo">
              <div class="logo-section">
                <el-upload
                  class="logo-uploader"
                  action="#"
                  :auto-upload="false"
                  :show-file-list="false"
                  :on-change="handleLogoChange"
                >
                  <img v-if="form.logoUrl" :src="getFullUrl(form.logoUrl)" class="preview-logo" />
                  <el-icon v-else class="uploader-icon"><Plus /></el-icon>
                  <div class="upload-hover">
                    <el-icon><Camera /></el-icon>
                    <span>更換 Logo</span>
                  </div>
                </el-upload>
                <div class="upload-hint">建議尺寸 200x200，不超過 2MB</div>
              </div>
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider />

        <div class="form-actions">
          <el-button type="primary" size="large" :loading="submitting" @click="handleSave(formRef)">
            儲存變更
          </el-button>
          <el-button size="large" @click="resetForm">重置</el-button>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Camera, VideoPlay, CoffeeCup } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { getStoreProfileApi, updateStoreProfileApi, uploadStoreLogoApi } from '@/api/store'
import type { StoreProfileData } from '@/types/store'

const formRef = ref<FormInstance>()
const loading = ref(false)
const submitting = ref(false)
const baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'

const form = reactive<StoreProfileData>({
  storeName: '',
  description: '',
  logoUrl: '',
  storeStatus: 1
})

const rules = reactive<FormRules>({
  storeName: [
    { required: true, message: '賣場名稱不能為空', trigger: 'blur' },
    { min: 2, max: 50, message: '長度需在 2 到 50 個字之間', trigger: 'blur' }
  ],
  description: [
    { max: 500, message: '介紹字數不能超過 500 字', trigger: 'blur' }
  ]
})

const getFullUrl = (url: string) => {
  if (!url) return ''
  return url.startsWith('http') ? url : baseUrl + url
}

const fetchStoreInfo = async () => {
  loading.value = true
  try {
    const res = await getStoreProfileApi()
    Object.assign(form, res.data)
  } catch (error: any) {
    console.error('獲取賣場資訊失敗', error)
    ElMessage.error('無法載入賣場資訊')
  } finally {
    loading.value = false
  }
}

const handleLogoChange = async (uploadFile: UploadFile) => {
  const file = uploadFile.raw
  if (!file) return

  if (file.size / 1024 / 1024 > 2) {
    ElMessage.error('圖片大小不能超過 2MB!')
    return
  }

  try {
    const res = await uploadStoreLogoApi(file as File)
    form.logoUrl = res.data.url
    ElMessage.success('Logo 已上傳')
  } catch (error) {
    ElMessage.error('圖片上傳失敗')
  }
}

const handleSave = async (formEl: FormInstance | undefined) => {
  if (!formEl) return

  await formEl.validate(async (valid) => {
    if (valid) {
      try {
        await ElMessageBox.confirm('確定要儲存賣場資訊的變更嗎？', '確認更新', {
          confirmButtonText: '儲存',
          cancelButtonText: '取消',
          type: 'info'
        })

        submitting.value = true
        await updateStoreProfileApi(form)
        ElMessage.success('賣場資訊已更新成功')
      } catch (error: any) {
        if (error !== 'cancel') {
          console.error('更新失敗', error)
          ElMessage.error(error.response?.data?.message || '更新失敗，請稍後再試')
        }
      } finally {
        submitting.value = false
      }
    }
  })
}

const resetForm = () => {
  fetchStoreInfo()
}

onMounted(() => {
  fetchStoreInfo()
})
</script>

<style scoped lang="scss">
.store-settings-container {
  max-width: 1000px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 24px;
  .title { margin: 0; font-size: 24px; font-weight: 700; color: #1a1a2e; }
  .subtitle { margin: 8px 0 0; color: #64748b; font-size: 14px; }
}

.settings-card {
  border-radius: 12px;
  border: 1px solid #e2e8f0;
}

.settings-form {
  padding: 20px 0;
}

.logo-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.logo-uploader {
  width: 180px;
  height: 180px;
  border: 2px dashed #e2e8f0;
  border-radius: 16px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f8fafc;
  transition: all 0.3s;

  &:hover {
    border-color: #ee4d2d;
    .upload-hover { opacity: 1; }
  }
}

.uploader-icon {
  font-size: 40px;
  color: #94a3b8;
}

.preview-logo {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.upload-hover {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  color: white;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 8px;
  opacity: 0;
  transition: opacity 0.3s;
  font-size: 14px;
}

.upload-hint {
  font-size: 12px;
  color: #94a3b8;
}

.status-tip {
  margin-top: 12px;
}

.mt-2 { margin-top: 8px; }

.form-actions {
  display: flex;
  gap: 16px;
  margin-top: 20px;
}
</style>
