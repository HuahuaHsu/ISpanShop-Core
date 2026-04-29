<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../../stores/auth';
import { ElMessage } from 'element-plus';
import { User, Lock, MagicStick, QuestionFilled } from '@element-plus/icons-vue';
import type { FormInstance, FormRules } from 'element-plus';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const loginFormRef = ref<FormInstance>();

const loginForm = reactive({
  account: '',
  password: ''
});

const rules = reactive<FormRules>({
  account: [
    { required: true, message: '請輸入帳號或 Email', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '請輸入密碼', trigger: 'blur' },
    { min: 6, message: '密碼長度至少 6 個字元', trigger: 'blur' }
  ]
});

const handleLogin = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;

  const valid = await formEl.validate().catch(() => false);
  if (!valid) return;

  try {
    await authStore.login(loginForm);
    ElMessage.success('登入成功');
    const redirectPath = (route.query.redirect as string) || '/';
    await router.push(redirectPath);
  } catch (error: any) {
    console.error('登入失敗', error);
    ElMessage.error(error.response?.data?.message ?? '登入失敗，請檢查帳號密碼');
  }
};

const quickFill = () => {
  loginForm.account = 'fuen49';
  loginForm.password = 'Fuen49.02';
};

const handleForgotPassword = () => {
  router.push('/forgot-password');
};

const handleGoogleLogin = () => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  const redirectUri = `${window.location.origin}/auth/callback`;
  
  console.log('Google Login Triggered', { clientId, redirectUri });

  if (!clientId || clientId.includes('YOUR_')) {
    ElMessage.error('Google Client ID 未設定或無效，請檢查 .env 檔案');
    return;
  }
  
  const scope = encodeURIComponent('openid email profile');
  const responseType = 'code';
  
  const googleUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=${responseType}&scope=${scope}&access_type=offline&prompt=select_account`;
  
  window.location.href = googleUrl;
};
</script>

<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <div class="card-header">
          <h2>會員登入</h2>
          <p class="subtitle">歡迎回來，請輸入您的帳號密碼</p>
        </div>
      </template>

      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="rules"
        label-width="0"
        @keyup.enter="handleLogin(loginFormRef)"
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

        <el-form-item>
          <el-button 
            type="primary" 
            class="w-full login-btn" 
            size="large"
            @click="handleLogin(loginFormRef)"
          >
            登入
          </el-button>
        </el-form-item>

        <div class="divider">
          <span>或者使用以下方式登入</span>
        </div>

        <div class="oauth-actions">
          <el-button class="oauth-btn google" size="large" @click="handleGoogleLogin">
            <img src="https://www.gstatic.com/images/branding/product/1x/gsa_512dp.png" alt="Google" width="20" />
            Google 登入
          </el-button>
          <el-button class="oauth-btn facebook" size="large">
            <img src="https://upload.wikimedia.org/wikipedia/commons/b/b8/2021_Facebook_icon.svg" alt="Facebook" width="20" />
            Facebook 登入
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
            @click="handleForgotPassword"
          >
            忘記密碼
          </el-button>
        </div>

        <div class="footer-links">
          還沒有帳號？ <span class="link" @click="authStore.openRegisterDialog()">立即註冊</span>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  padding: 20px;
}

.login-card {
  width: 100%;
  max-width: 500px;
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

.login-btn {
  font-weight: bold;
  letter-spacing: 2px;
  margin-top: 10px;
}

.divider {
  display: flex;
  align-items: center;
  margin: 25px 0;
  color: #999;
  font-size: 13px;
}

.divider::before,
.divider::after {
  content: "";
  flex: 1;
  height: 1px;
  background: #eee;
}

.divider span {
  margin: 0 15px;
}

.oauth-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.oauth-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  width: 100%;
  margin-left: 0 !important;
  border: 1px solid #ddd;
  transition: all 0.3s;
}

.oauth-btn:hover {
  background-color: #f8f9fa;
  border-color: #ccc;
}

.oauth-btn img {
  margin-right: 5px;
}

.google {
  color: #444;
}

.facebook {
  background-color: #1877f2;
  color: white;
  border: none;
}

.facebook:hover {
  background-color: #166fe5;
  color: white;
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

.link {
  color: #409eff;
  text-decoration: none;
  font-weight: 500;
  cursor: pointer;
}

.link:hover {
  text-decoration: underline;
}

:deep(.el-card__header) {
  border-bottom: 1px solid #f0f0f0;
  background-color: #fafafa;
}
</style>
