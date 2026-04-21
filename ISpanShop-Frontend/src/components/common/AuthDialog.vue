<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useAuthStore } from '../../stores/auth';
import { ElMessage } from 'element-plus';
import { User, Lock, MagicStick, QuestionFilled } from '@element-plus/icons-vue';
import type { FormInstance, FormRules } from 'element-plus';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const loginFormRef = ref<FormInstance>();
const submitting = ref(false);

const loginForm = reactive({
  account: '',
  password: ''
});

const rules = reactive<FormRules>({
  account: [{ required: true, message: '請輸入帳號或 Email', trigger: 'blur' }],
  password: [
    { required: true, message: '請輸入密碼', trigger: 'blur' },
    { min: 6, message: '密碼長度至少 6 個字元', trigger: 'blur' }
  ]
});

const handleLogin = async () => {
  if (!loginFormRef.value) return;

  const valid = await loginFormRef.value.validate().catch(() => false);
  if (!valid) return;

  try {
    submitting.value = true;
    await authStore.login(loginForm);
    ElMessage.success('登入成功');
    authStore.closeLoginDialog();
    // 登入後清空表單
    loginForm.account = '';
    loginForm.password = '';
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message ?? '登入失敗');
  } finally {
    submitting.value = false;
  }
};

const quickFill = () => {
  loginForm.account = 'fuen49';
  loginForm.password = 'Fuen49.02';
};

const goToRegister = () => {
  authStore.closeLoginDialog();
  router.push('/register');
};

const goToForgotPassword = () => {
  authStore.closeLoginDialog();
  router.push('/forgot-password');
};
</script>

<template>
  <el-dialog
    v-model="authStore.isLoginDialogOpen"
    title="會員登入"
    width="400px"
    center
    append-to-body
    destroy-on-close
    class="auth-dialog"
  >
    <div class="auth-header">
      <p class="subtitle">歡迎回來，請輸入您的帳號密碼</p>
    </div>

    <el-form
      ref="loginFormRef"
      :model="loginForm"
      :rules="rules"
      label-width="0"
      @keyup.enter="handleLogin"
    >
      <el-form-item prop="account">
        <el-input 
          v-model="loginForm.account" 
          placeholder="帳號或 Email" 
          :prefix-icon="User"
          size="large"
        />
      </el-form-item>

      <el-form-item prop="password">
        <el-input
          v-model="loginForm.password"
          type="password"
          placeholder="密碼"
          :prefix-icon="Lock"
          show-password
          size="large"
        />
      </el-form-item>

      <div class="login-action">
        <el-button 
          type="primary" 
          class="w-full" 
          size="large"
          :loading="submitting"
          @click="handleLogin"
        >
          登入
        </el-button>
      </div>

      <div class="helper-actions">
        <el-button 
          type="info" 
          plain
          class="flex-1"
          :icon="MagicStick"
          @click="quickFill"
        >
          快速填入
        </el-button>
        
        <el-button 
          type="warning" 
          plain
          class="flex-1"
          :icon="QuestionFilled"
          @click="goToForgotPassword"
        >
          忘記密碼
        </el-button>
      </div>

      <div class="footer-links">
        還沒有帳號？ <span class="link-btn" @click="goToRegister">立即註冊</span>
      </div>
    </el-form>
  </el-dialog>
</template>

<style scoped>
.auth-header {
  text-align: center;
  margin-bottom: 20px;
}

.subtitle {
  font-size: 14px;
  color: #888;
}

.w-full {
  width: 100%;
}

.login-action {
  margin-top: 10px;
}

.helper-actions {
  display: flex;
  gap: 12px;
  margin: 20px 0;
}

.flex-1 {
  flex: 1;
}

.footer-links {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
  color: #666;
}

.link-btn {
  color: #409eff;
  cursor: pointer;
  font-weight: 500;
}

.link-btn:hover {
  text-decoration: underline;
}

:deep(.el-dialog__header) {
  margin-right: 0;
  padding-bottom: 10px;
  border-bottom: 1px solid #f0f0f0;
}

:deep(.el-dialog__body) {
  padding-top: 30px;
}
</style>
