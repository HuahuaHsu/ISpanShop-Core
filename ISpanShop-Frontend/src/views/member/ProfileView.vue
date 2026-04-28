<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { User, Message, Iphone, Calendar, SuccessFilled } from '@element-plus/icons-vue'
import { getMemberProfile, updateMemberProfile, uploadAvatar, type UpdateMemberProfileDto, type MemberDto } from '../../api/member'
import { unbindOAuth } from '../../api/auth'
import { storage } from '../../utils/storage'
import { getFullImageUrl } from '../../utils/format'

const authStore = useAuthStore()
const profileFormRef = ref<FormInstance>()
const isSaving = ref(false)
const isLoading = ref(true)

// ── 第三方綁定處理 ──────────────────────────────────
const handleUnbind = async () => {
  if (!authStore.memberInfo.hasPassword) {
    ElMessage.warning('請先設定登入密碼後再解除綁定')
    return
  }

  try {
    const res = await unbindOAuth()
    if (res.data.success) {
      ElMessage.success('已解除綁定')
      authStore.memberInfo.provider = null
      storage.setUser(authStore.memberInfo)
    }
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '解除綁定失敗')
  }
}

const handleBindGoogle = () => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
  const redirectUri = encodeURIComponent(`${window.location.origin}/auth/callback`)
  const scope = encodeURIComponent('openid email profile')
  const googleUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=code&scope=${scope}&prompt=select_account`
  window.location.href = googleUrl
}

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
  memberName: [
    { max: 50, message: '姓名長度不可超過 50 個字', trigger: 'blur' }
  ],
  phone: [
    { pattern: /^09\d{8}$/, message: '請輸入正確的手機號碼 (09xxxxxxxx)', trigger: 'blur' }
  ]
})

// ── 初始化資料 ────────────────────────────────────
const fetchProfile = async () => {
  try {
    isLoading.value = true
    const response = await getMemberProfile()
    const data = response.data as MemberDto

    // 補足其餘細節欄位 (相容大小寫)
    profileForm.id = data.id // 同步最新的 ID
    profileForm.memberName = data.fullName ?? data.fullName ?? profileForm.memberName
    profileForm.email = data.email ?? data.email ?? profileForm.email
    profileForm.phone = data.phoneNumber ?? data.phoneNumber ?? profileForm.phone
    profileForm.gender = data.gender ?? data.gender ?? null

    const rawBirthday = data.birthday ?? ''
    profileForm.birthday = rawBirthday ? (String(rawBirthday).split('T')[0] || '') : ''
    profileForm.avatarUrl = (data.avatarUrl ?? data.avatarUrl ?? '') as string
  } catch (error: unknown) {
  const err = error as { response?: { status?: number; data?: { message?: string } } }
  const status = err.response?.status
    if (status === 404) {
      ElMessage.error('找不到會員資料，會員 ID 可能不存在')
    } else {
      ElMessage.error('無法從伺服器載入資料')
    }
  } finally {
    isLoading.value = false
  }
}

// ── Email 遮罩邏輯 (前兩碼 + 固定星號 + 最後一碼) ──
const maskEmail = (email: string) => {
  if (!email) return ''
  const [user, domain] = email.split('@')
  if (!domain) return email
  
  if (user.length <= 2) return `${user}***@${domain}`
  
  const prefix = user.substring(0, 2)
  const suffix = user.slice(-1)
  return `${prefix}***${suffix}@${domain}`
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
    // 使用專用 DTO 結構，僅發送必要的欄位
    const submitData: UpdateMemberProfileDto = {
      id: Number(profileForm.id),
      account: profileForm.account,
      email: profileForm.email,
      fullName: profileForm.memberName,
      phoneNumber: profileForm.phone,
      gender: profileForm.gender,
      birthday: profileForm.birthday || null,
      avatarUrl: profileForm.avatarUrl
    }

    console.log('送出資料:', submitData) // 除錯用

    await updateMemberProfile(submitData)
    ElMessage.success('個人資料已成功更新')

    // 同步更新 Pinia 與 LocalStorage
    authStore.memberInfo.email = profileForm.email
    authStore.memberInfo.memberName = profileForm.memberName
    authStore.memberInfo.avatarUrl = profileForm.avatarUrl
    storage.setUser(authStore.memberInfo)
  } catch (error: unknown) {
    console.error('更新失敗:', error)
    const err = error as { response?: { data?: { message?: string } } }
    console.error('錯誤詳情:', err.response?.data) // 印出後端錯誤訊息
    const msg = err.response?.data?.message || '更新失敗，請稍後再試'
    ElMessage.error(msg)
  } finally {
    isSaving.value = false
  }
}

// ── 頭像處理 (預覽) ──────────────────────────────────
// ── 上傳狀態 ──────────────────────────────────────
const isUploading = ref(false)

// ── 頭像上傳（完整流程）──────────────────────────────
const handleAvatarUpload = async (rawFile: File) => {
  const isImage = ['image/jpeg', 'image/png', 'image/jpg'].includes(rawFile.type)
  const isLt1M = rawFile.size / 1024 / 1024 < 1

  if (!isImage) { ElMessage.error('頭像只能是 JPG 或 PNG 格式!'); return }
  if (!isLt1M) { ElMessage.error('頭像檔案大小不能超過 1MB!'); return }

  // 1. 先用 Blob URL 做即時預覽（讓使用者感覺流暢）
  profileForm.avatarUrl = URL.createObjectURL(rawFile)

  try {
    isUploading.value = true

    // 2. 真正上傳到伺服器
    const res = await uploadAvatar(rawFile)

    // 3. 用伺服器回傳的真實路徑取代 Blob URL
    profileForm.avatarUrl = res.data.url

    ElMessage.success('頭像上傳成功，請記得點擊儲存')
  } catch {
  ElMessage.error('頭像上傳失敗，請稍後再試')
    profileForm.avatarUrl = '' // 上傳失敗就清掉預覽
  } finally {
    isUploading.value = false
  }
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
          </el-form-item>

          <el-form-item label="姓名" prop="memberName">
            <el-input v-model="profileForm.memberName" placeholder="請輸入姓名" :prefix-icon="User" />
          </el-form-item>

          <el-form-item label="Email">
            <span class="read-only-text">{{ maskEmail(profileForm.email) }}</span>
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

          <el-divider />
          
          <el-form-item label="第三方帳號">
            <div v-if="authStore.memberInfo.provider" class="oauth-connected-wrap">
              <span class="connected-email">{{ maskEmail(authStore.memberInfo.email || '') }}</span>
              
              <el-button type="info" plain class="connected-btn-gray" disabled>
                已連結 Google 帳號
              </el-button>
              
              <el-tooltip
                v-if="!authStore.memberInfo.hasPassword"
                content="請先設定密碼後才能解綁第三方帳號"
                placement="top"
              >
                <el-button type="danger" link class="unbind-link" disabled>解除綁定</el-button>
              </el-tooltip>
              <el-button v-else type="danger" link class="unbind-link" @click="handleUnbind">解除綁定</el-button>
            </div>
            
            <div v-else>
              <el-button type="default" class="google-bind-btn" @click="handleBindGoogle">
                <img src="https://www.gstatic.com/images/branding/product/1x/gsa_512dp.png" width="18" class="mr-2" />
                連結 Google 帳號
              </el-button>
            </div>
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
          <div class="avatar-preview-wrap" v-loading="isUploading">
            <el-avatar v-if="profileForm.avatarUrl" :size="120" :src="getFullImageUrl(profileForm.avatarUrl)" />
            <el-avatar v-else :size="120" :icon="User" />
          </div>
          <el-upload
            class="avatar-uploader"
            action=""
            :auto-upload="false"
            :show-file-list="false"
            :on-change="(file: any) => handleAvatarUpload(file.raw)"
          >
            <el-button size="default">選擇圖片</el-button>
          </el-upload>
          <div class="upload-hint">
            <p>檔案大小：最大 1MB</p>
            <p>檔案格式：.JPEG, .PNG</p>
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

/* 第三方帳號樣式 */
.oauth-connected-wrap { display: flex; align-items: center; gap: 15px; }
.connected-email { color: #333; font-weight: 500; font-size: 14px; }
.connected-btn-gray { cursor: not-allowed !important; background-color: #f4f4f5 !important; border-color: #e9e9eb !important; color: #909399 !important; }
.google-bind-btn { border: 1px solid #dcdfe6; color: #606266; font-weight: 500; }
.google-bind-btn:hover { background-color: #f5f7fa; border-color: #c0c4cc; }
.unbind-link { font-size: 13px; text-decoration: underline; }
.mr-2 { margin-right: 8px; }
.ml-1 { margin-left: 4px; }

.avatar-col { border-left: 1px solid #efefef; display: flex; justify-content: center; align-items: flex-start; padding-top: 20px; }
@media (max-width: 992px) { .avatar-col { border-left: none; border-top: 1px solid #efefef; padding-top: 40px; margin-top: 40px; } }
.avatar-upload-section { text-align: center; }
.avatar-preview-wrap { margin-bottom: 20px; display: flex; justify-content: center; }
.avatar-uploader { margin-bottom: 15px; }
.upload-hint { font-size: 13px; color: #999; line-height: 1.6; }
</style>
