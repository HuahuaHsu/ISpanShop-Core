<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules, UploadProps } from 'element-plus'
import { User, Message, Iphone, Calendar } from '@element-plus/icons-vue'
import { getMemberProfile, updateMemberProfile } from '../../api/member'

const authStore = useAuthStore()
const profileFormRef = ref<FormInstance>()
const isSaving = ref(false)
const isLoading = ref(true)

// ── 表單資料 (初始化時直接從 authStore 帶入資料) ─────────────
const profileForm = reactive({
  id: authStore.memberInfo?.memberId || 0,
  account: authStore.memberInfo?.account || '',
  memberName: authStore.memberInfo?.memberName || '',
  email: authStore.memberInfo?.email || '',
  phone: '',
  gender: null as number | null,
  birthday: '',
  avatarUrl: ''
})

// ── 表單驗證規則 ──────────────────────────────────
const rules = reactive<FormRules>({
  email: [
    { required: true, message: '請輸入 Email', trigger: 'blur' },
    { type: 'email', message: '請輸入正確的 Email 格式', trigger: 'blur' }
  ],
  memberName: [
    { max: 50, message: '姓名長度不可超過 50 個字', trigger: 'blur' }
  ],
  phone: [
    { pattern: /^09\d{8}$/, message: '請輸入正確的手機號碼 (09xxxxxxxx)', trigger: 'blur' }
  ]
})

// ── 初始化資料 ────────────────────────────────────
const fetchProfile = async () => {
  const memberId = Number(profileForm.id)
  if (!memberId) return

  try {
    isLoading.value = true
    const response = await getMemberProfile(memberId)
    const data = response.data as any
    
    // 檢查回傳的是否為 HTML (如果是 HTML 代表 Proxy 或路徑有問題)
    if (typeof data === 'string' && data.includes('<!DOCTYPE html>')) {
      console.error('API 錯誤: 回傳了 HTML 內容，可能是路徑錯誤或代理失效')
      ElMessage.error('伺服器設定錯誤 (API 404)')
      return
    }

    console.log('載入資料:', data)
    
    // 補足其餘細節欄位
    profileForm.memberName = data.fullName ?? data.FullName ?? profileForm.memberName
    profileForm.email = data.email ?? data.Email ?? profileForm.email
    profileForm.phone = data.phoneNumber ?? data.PhoneNumber ?? profileForm.phone
    profileForm.gender = data.gender ?? data.Gender ?? null
    
    const rawBirthday = data.birthday ?? data.Birthday ?? data.DateOfBirth ?? ''
    profileForm.birthday = rawBirthday ? String(rawBirthday).split('T')[0] : ''
    profileForm.avatarUrl = data.avatarUrl ?? data.AvatarUrl ?? ''
  } catch (error: any) {
    console.error('取得資料發生錯誤:', error)
    ElMessage.error(error.response?.data?.message || '無法連線至伺服器')
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchProfile()
})

// ── 儲存按鈕 ──────────────────────────────────────
const handleSave = async (formEl: FormInstance | undefined) => {
  if (!formEl) return
  
  const valid = await formEl.validate().catch(() => false)
  if (!valid) return

  isSaving.value = true

  try {
    // 嚴格對應 MemberDto 的結構，避免多餘欄位造成 400
    const submitData = {
      Id: Number(profileForm.id),
      Account: profileForm.account,
      Email: profileForm.email,
      FullName: profileForm.memberName,
      PhoneNumber: profileForm.phone,
      Gender: profileForm.gender,
      Birthday: profileForm.birthday || null,
      IsBlacklisted: false,
      IsSeller: false
    }

    console.log('提交資料:', submitData)
    await updateMemberProfile(submitData.Id, submitData as any)
    ElMessage.success('個人資料已成功更新')
    
    // 同步更新 Pinia 中的資訊
    authStore.memberInfo.email = profileForm.email
    authStore.memberInfo.memberName = profileForm.memberName
  } catch (error: any) {
    console.error('更新失敗錯誤:', error.response?.data)
    const errorMsg = error.response?.data?.message || '更新失敗'
    ElMessage.error(errorMsg)
  } finally {
    isSaving.value = false
  }
}

// ── 頭像處理 ──────────────────────────────────────
const beforeAvatarUpload: UploadProps['beforeUpload'] = (rawFile) => {
  const isImage = ['image/jpeg', 'image/png', 'image/jpg'].includes(rawFile.type)
  const isLt1M = rawFile.size / 1024 / 1024 < 1

  if (!isImage) {
    ElMessage.error('頭像只能是 JPG 或 PNG 格式!')
    return false
  }
  if (!isLt1M) {
    ElMessage.error('頭像檔案大小不能超過 1MB!')
    return false
  }
  
  profileForm.avatarUrl = URL.createObjectURL(rawFile)
  return false 
}
</script>

<template>
  <div class="profile-container" v-loading="isLoading">
    <div class="profile-header">
      <h2>我的檔案</h2>
      <p>管理你的檔案以保護你的帳戶</p>
    </div>

    <el-row :gutter="40" class="profile-body">
      <el-col :xs="24" :md="16">
        <el-form
          ref="profileFormRef"
          :model="profileForm"
          :rules="rules"
          label-width="100px"
          label-position="right"
        >
          <el-form-item label="帳號">
            <span class="read-only-text">{{ profileForm.account }}</span>
            <span class="hint-text">帳號無法更改</span>
          </el-form-item>

          <el-form-item label="姓名" prop="memberName">
            <el-input v-model="profileForm.memberName" placeholder="請輸入姓名" :prefix-icon="User" />
          </el-form-item>

          <el-form-item label="Email" prop="email">
            <el-input v-model="profileForm.email" placeholder="請輸入 Email" :prefix-icon="Message" />
          </el-form-item>

          <el-form-item label="手機號碼" prop="phone">
            <el-input v-model="profileForm.phone" placeholder="請輸入手機號碼" :prefix-icon="Iphone" />
          </el-form-item>

          <el-form-item label="性別">
            <el-radio-group v-model="profileForm.gender">
              <el-radio :value="1">男</el-radio>
              <el-radio :value="2">女</el-radio>
              <el-radio :value="0">不願透露</el-radio>
            </el-radio-group>
          </el-form-item>

          <el-form-item label="生日">
            <el-date-picker
              v-model="profileForm.birthday"
              type="date"
              placeholder="選擇你的生日"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              :prefix-icon="Calendar"
              class="full-width"
            />
          </el-form-item>

          <el-form-item>
            <el-button type="primary" class="save-btn" :loading="isSaving" @click="handleSave(profileFormRef)">
              儲存
            </el-button>
          </el-form-item>
        </el-form>
      </el-col>

      <el-col :xs="24" :md="8" class="avatar-col">
        <div class="avatar-upload-section">
          <div class="avatar-preview-wrap">
            <el-avatar v-if="profileForm.avatarUrl" :size="120" :src="profileForm.avatarUrl" />
            <el-avatar v-else :size="120" :icon="User" />
          </div>
          <el-upload
            class="avatar-uploader"
            action=""
            :auto-upload="false"
            :show-file-list="false"
            :on-change="(file: any) => beforeAvatarUpload(file.raw)"
          >
            <el-button size="default">選擇圖片</el-button>
          </el-upload>
          <div class="upload-hint">
            <p>檔案大小：最大 1MB</p>
            <p>檔案延伸：.JPEG, .PNG</p>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.profile-container { padding: 10px; }
.profile-header { border-bottom: 1px solid #efefef; padding-bottom: 20px; margin-bottom: 30px; }
.profile-header h2 { font-size: 18px; color: #333; margin-bottom: 4px; }
.profile-header p { font-size: 14px; color: #666; }
.read-only-text { font-weight: 600; color: #333; }
.hint-text { font-size: 12px; color: #999; margin-left: 10px; }
.full-width { width: 100% !important; }
.save-btn { background-color: #EE4D2D; border-color: #EE4D2D; padding: 12px 30px; }
.save-btn:hover { background-color: #BE3E24; border-color: #BE3E24; }
.avatar-col { border-left: 1px solid #efefef; display: flex; justify-content: center; align-items: flex-start; padding-top: 20px; }
@media (max-width: 992px) { .avatar-col { border-left: none; border-top: 1px solid #efefef; padding-top: 40px; margin-top: 40px; } }
.avatar-upload-section { text-align: center; }
.avatar-preview-wrap { margin-bottom: 20px; display: flex; justify-content: center; }
.avatar-uploader { margin-bottom: 15px; }
.upload-hint { font-size: 13px; color: #999; line-height: 1.6; }
</style>
