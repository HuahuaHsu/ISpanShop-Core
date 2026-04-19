<template>
  <div class="reset-password-container">
    <el-card class="reset-password-card">
      <template #header>
        <div class="card-header">
          <h2>重設密碼</h2>
          <p class="subtitle">請輸入您的新密碼</p>
        </div>
      </template>

      <el-form
        ref="formRef"
        :model="resetForm"
        :rules="rules"
        label-position="top"
        @submit.prevent="handleReset"
      >
        <el-form-item label="新密碼" prop="newPassword">
          <el-input
            v-model="resetForm.newPassword"
            type="password"
            placeholder="請輸入新密碼 (6-20位)"
            show-password
            size="large"
          />
        </el-form-item>

        <el-form-item label="確認新密碼" prop="confirmPassword">
          <el-input
            v-model="resetForm.confirmPassword"
            type="password"
            placeholder="請再次輸入新密碼"
            show-password
            size="large"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            class="w-full submit-btn"
            size="large"
            :loading="loading"
            @click="handleReset"
          >
            確認重設
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { resetPasswordApi } from '@/api/auth'

const router = useRouter()
const route = useRoute()
const formRef = ref<FormInstance>()
const loading = ref(false)

const resetForm = reactive({
  token: '',
  newPassword: '',
  confirmPassword: ''
})

onMounted(() => {
  // 從 URL 取得參數
  const { token } = route.query
    if (!token) {
    ElMessage.error('無效的連結參數')
    router.push('/login')
    return
  }
  resetForm.token = token as string
})
const rules = reactive<FormRules>({
  newPassword: [
    { required: true, message: '請輸入新密碼', trigger: 'blur' },
    { min: 6, max: 20, message: '長度應為 6 到 20 個字元', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '請再次輸入新密碼', trigger: 'blur' },
    {
      validator: (rule: any, value: any, callback: any) => {
        if (value !== resetForm.newPassword) {
          callback(new Error('兩次輸入的新密碼不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
})

const handleReset = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        loading.value = true
        const { data } = await resetPasswordApi(resetForm)

        if (data.isSuccess) {
          ElMessage.success(data.message || '密碼重設成功，請重新登入')
          setTimeout(() => {
            router.push('/login')
          }, 1500)
        } else {
          ElMessage.error(data.message)
        }
      } catch (error: any) {
        ElMessage.error(error.response?.data?.message || '重設失敗，請稍後再試')
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
.reset-password-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  padding: 20px;
}

.reset-password-card {
  width: 100%;
  max-width: 420px;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.card-header {
  text-align: center;
  padding: 10px 0;
}

.card-header h2 {
  margin: 0;
  font-size: 24px;
  color: #333;
}

.subtitle {
  margin-top: 8px;
  font-size: 14px;
  color: #888;
}

.w-full {
  width: 100%;
}

.submit-btn {
  margin-top: 10px;
}
</style>
