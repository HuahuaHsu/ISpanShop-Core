<template>
  <div class="forgot-password-container">
    <el-card class="forgot-password-card">
      <template #header>
        <div class="card-header">
          <h2>忘記密碼</h2>
          <p class="subtitle">請輸入您的註冊 Email，我們將發送重設密碼連結給您</p>
        </div>
      </template>

      <div v-if="isSent" class="sent-success">
        <el-result
          icon="success"
          title="郵件已發送"
          sub-title="重設密碼連結已發送至您的信箱，請在 24 小時內完成重設。"
        >
          <template #extra>
            <el-button type="primary" @click="$router.push('/login')">返回登入</el-button>
          </template>
        </el-result>
      </div>

      <el-form
        v-else
        ref="formRef"
        :model="forgotForm"
        :rules="rules"
        label-position="top"
        @submit.prevent="handleSend"
      >
        <el-form-item label="電子郵件" prop="email">
          <el-input 
            v-model="forgotForm.email" 
            placeholder="請輸入您的 Email" 
            size="large"
          />
        </el-form-item>
        
        <el-form-item>
          <el-button 
            type="primary" 
            class="w-full submit-btn" 
            size="large" 
            :loading="loading"
            @click="handleSend"
          >
            發送驗證信
          </el-button>
        </el-form-item>

        <div class="footer-links">
          想起密碼了？ <router-link to="/login" class="link">返回登入</router-link>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { forgotPasswordApi } from '@/api/auth'

const formRef = ref<FormInstance>()
const loading = ref(false)
const isSent = ref(false)

const forgotForm = reactive({
  email: ''
})

const rules = reactive<FormRules>({
  email: [
    { required: true, message: '請輸入電子郵件', trigger: 'blur' },
    { type: 'email', message: '請輸入正確的電子郵件格式', trigger: 'blur' }
  ]
})

const handleSend = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        loading.value = true
        const { data } = await forgotPasswordApi(forgotForm)
        
        if (data.isSuccess) {
          isSent.value = true
          ElMessage.success(data.message)
        } else {
          ElMessage.error(data.message)
        }
      } catch (error: any) {
        ElMessage.error(error.response?.data?.message || '發送失敗，請稍後再試')
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
.forgot-password-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  padding: 20px;
}

.forgot-password-card {
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

.sent-success {
  padding: 20px 0;
}

.footer-links {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
}

.link {
  color: #409eff;
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}
</style>
