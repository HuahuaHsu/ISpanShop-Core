<template>
  <div class="seller-apply-container" v-loading="initialLoading">
    <template v-if="!initialLoading">
      <!-- 1. 審核中狀態 -->
      <el-card v-if="status === 'Pending'" class="status-card">
        <el-result
          icon="info"
          title="申請審核中"
          sub-title="您的賣場申請已提交，管理員正在審核中，請耐心等候。"
        >
          <template #extra>
            <div class="status-actions">
              <el-button type="primary" @click="router.push('/member/mystore')">查看賣場狀態</el-button>
              <el-button @click="router.push('/')">回首頁</el-button>
            </div>
          </template>
        </el-result>
      </el-card>

      <!-- 2. 申請表單 (未申請或已駁回) -->
      <el-card v-else-if="status === 'NotApplied' || status === 'Rejected'" class="apply-card">
        <template #header>
          <div class="card-header">
            <span>{{ status === 'Rejected' ? '重新申請成為賣家' : '申請成為賣家' }}</span>
          </div>
        </template>

        <div v-if="status === 'Rejected'" class="reject-alert">
          <el-alert
            title="先前的申請已被駁回"
            type="error"
            description="請根據管理員的建議修改資料後再次提交。"
            show-icon
            :closable="false"
          />
        </div>

        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-position="top"
          class="apply-form"
        >
          <el-form-item label="賣場名稱" prop="storeName">
            <el-input v-model="form.storeName" placeholder="請輸入您的賣場名稱 (例如: 阿明的小店)" />
          </el-form-item>

          <el-form-item label="賣場介紹" prop="description">
            <el-input
              v-model="form.description"
              type="textarea"
              :rows="4"
              placeholder="簡單描述您的賣場經營理念或商品特色..."
            />
          </el-form-item>

          <el-form-item label="賣場 Logo (可選)">
            <el-upload
              class="logo-uploader"
              action="#"
              :auto-upload="false"
              :show-file-list="false"
              :on-change="handleLogoChange"
            >
              <img v-if="form.logoUrl" :src="getFullImageUrl(form.logoUrl)" class="preview-logo" />
              <el-icon v-else class="uploader-icon"><Plus /></el-icon>
            </el-upload>

            <div class="el-upload__tip">建議尺寸 200x200，大小不超過 2MB</div>
          </el-form-item>

          <div class="form-actions">
            <el-button type="primary" :loading="submitting" @click="submitApply(formRef)">
              提交申請
            </el-button>
            <el-button @click="router.back()">取消</el-button>
          </div>
        </el-form>
      </el-card>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { applyStoreApi, getStoreStatusApi, uploadStoreLogoApi } from '@/api/store'
import { getFullImageUrl } from '@/utils/format'
import type { StoreStatus } from '@/types/store'

const router = useRouter()
const formRef = ref<FormInstance>()
const submitting = ref(false)
const initialLoading = ref(true)
const status = ref<StoreStatus | ''>('')

const form = reactive({
  storeName: '',
  description: '',
  logoUrl: ''
})

const rules = reactive<FormRules>({
  storeName: [
    { required: true, message: '請輸入賣場名稱', trigger: 'blur' },
    { min: 2, max: 50, message: '長度需在 2 到 50 個字之間', trigger: 'blur' }
  ],
  description: [
    { max: 500, message: '長度不能超過 500 個字', trigger: 'blur' }
  ]
})

const checkStatus = async () => {
  initialLoading.value = true
  try {
    const res = await getStoreStatusApi()
    status.value = res.data.status

    // 如果已經是賣家，直接去數據中心
    if (status.value === 'Approved') {
      router.replace('/seller')
    }
  } catch (error) {
    console.error('檢查狀態失敗', error)
  } finally {
    initialLoading.value = false
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
    ElMessage.success('圖片上傳成功')
  } catch (error) {
    console.error('Logo 上傳失敗', error)
    ElMessage.error('圖片上傳失敗')
  }
}

const submitApply = async (formEl: FormInstance | undefined) => {
  if (!formEl) return

  await formEl.validate(async (valid) => {
    if (valid) {
      submitting.value = true
      try {
        await applyStoreApi(form)
        ElMessage.success('申請已提交，請靜候審核')
        router.push('/member/mystore')
      } catch (error: any) {
        console.error('提交失敗', error)
        ElMessage.error(error.response?.data?.message || '提交失敗，請稍後再試')
      } finally {
        submitting.value = false
      }
    }
  })
}

onMounted(() => {
  checkStatus()
})
</script>

<style scoped lang="scss">
.seller-apply-container {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.status-card, .apply-card {
  border-radius: 8px;
}

.reject-alert {
  margin-bottom: 25px;
}

.apply-form {
  padding: 10px 0;
}

.logo-uploader {
  border: 1px dashed #d9d9d9;
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  width: 120px;
  height: 120px;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: border-color 0.3s;

  &:hover {
    border-color: #ee4d2d;
  }
}

.uploader-icon {
  font-size: 28px;
  color: #8c939d;
}

.preview-logo {
  width: 120px;
  height: 120px;
  display: block;
  object-fit: cover;
}

.form-actions {
  margin-top: 40px;
  display: flex;
  justify-content: center;
  gap: 20px;
}

.status-actions {
  display: flex;
  gap: 10px;
  justify-content: center;
}
</style>
