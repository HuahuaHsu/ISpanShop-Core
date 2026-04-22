<template>
  <div class="password-container">
    <div class="password-card">
      <div class="card-header">
        <h2>修改密碼</h2>
        <p>為了確保帳號安全，建議您定期更改密碼，並避免使用與其他網站相同的密碼。</p>
      </div>

      <el-form
        ref="passwordFormRef"
        :model="passwordForm"
        :rules="rules"
        label-width="100px"
        label-position="top"
        class="password-form"
      >
        <el-form-item label="目前密碼" prop="oldPassword">
          <el-input
            v-model="passwordForm.oldPassword"
            type="password"
            show-password
            placeholder="請輸入目前使用的密碼"
          />
        </el-form-item>

        <el-form-item label="新密碼" prop="newPassword">
          <el-input
            v-model="passwordForm.newPassword"
            type="password"
            show-password
            placeholder="請輸入 6-20 位包含英數字的新密碼"
          />
        </el-form-item>

        <el-form-item label="確認新密碼" prop="confirmPassword">
          <el-input
            v-model="passwordForm.confirmPassword"
            type="password"
            show-password
            placeholder="請再次輸入新密碼"
          />
        </el-form-item>

        <el-form-item class="form-actions">
          <el-button 
            type="primary" 
            :loading="submitting" 
            @click="handleSubmit"
            class="submit-btn"
          >
            確認修改
          </el-button>
          <el-button @click="$router.push('/member')" :disabled="submitting">
            取消
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { changePassword } from '@/api/member'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const passwordFormRef = ref<FormInstance>()
const submitting = ref(false)

const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 表單驗證規則
const rules = reactive<FormRules>({
  oldPassword: [
    { required: true, message: '請輸入目前密碼', trigger: 'blur' }
  ],
  newPassword: [
    { required: true, message: '請輸入新密碼', trigger: 'blur' },
    { min: 6, max: 20, message: '長度應為 6 到 20 個字元', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '請再次輸入新密碼', trigger: 'blur' },
    {
      validator: (rule: any, value: any, callback: any) => {
        if (value !== passwordForm.newPassword) {
          callback(new Error('兩次輸入的新密碼不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
})

const handleSubmit = async () => {
  if (!passwordFormRef.value) return

  await passwordFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        submitting.value = true
        
        const response = await changePassword({
          oldPassword: passwordForm.oldPassword,
          newPassword: passwordForm.newPassword,
          confirmPassword: passwordForm.confirmPassword
        })

        ElMessage.success(response.data.message || '密碼修改成功，請重新登入')
        
        // 成功後清除 Token 並強制登出
        authStore.logout()
        
        // 延遲導向登入頁面
        setTimeout(() => {
          router.push('/login')
        }, 1500)

      } catch (error: any) {
        console.error('修改密碼失敗:', error)
        const errorMsg = error.response?.data?.message || '修改失敗，請檢查目前密碼是否正確'
        ElMessage.error(errorMsg)
      } finally {
        submitting.value = false
      }
    }
  })
}
</script>

<style scoped>
.password-container {
  display: flex;
  justify-content: center;
  padding: 20px 0;
}

.password-card {
  width: 100%;
  max-width: 500px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
  padding: 40px;
}

.card-header {
  margin-bottom: 30px;
  text-align: center;
}

.card-header h2 {
  font-size: 24px;
  color: #333;
  margin-bottom: 10px;
}

.card-header p {
  font-size: 14px;
  color: #999;
  line-height: 1.5;
}

.password-form {
  margin-top: 20px;
}

.form-actions {
  margin-top: 40px;
  display: flex;
  justify-content: center;
}

.submit-btn {
  padding: 12px 30px;
  font-size: 16px;
}

:deep(.el-form-item__label) {
  font-weight: 500;
  padding-bottom: 8px;
}

:deep(.el-input__wrapper) {
  padding: 8px 12px;
}

@media (max-width: 480px) {
  .password-card {
    padding: 20px;
    box-shadow: none;
  }
}
</style>
