<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { useAuthStore } from '../../stores/auth';
import { registerApi } from '../../api/auth';
import { ElMessage } from 'element-plus';
import { User, Lock, MagicStick, QuestionFilled, Message, Iphone, Edit } from '@element-plus/icons-vue';
import type { FormInstance, FormRules } from 'element-plus';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const loginFormRef = ref<FormInstance>();
const registerFormRef = ref<FormInstance>();
const submitting = ref(false);

const loginForm = reactive({
  account: '',
  password: ''
});

const registerForm = reactive({
  account: '',
  password: '',
  confirmPassword: '',
  email: '',
  fullName: '',
  phoneNumber: ''
});

const loginRules = reactive<FormRules>({
  account: [{ required: true, message: '請輸入帳號或 Email', trigger: 'blur' }],
  password: [
    { required: true, message: '請輸入密碼', trigger: 'blur' },
    { min: 6, message: '密碼長度至少 6 個字元', trigger: 'blur' }
  ]
});

const registerRules = reactive<FormRules>({
  account: [
    { required: true, message: '請輸入帳號', trigger: 'blur' },
    { min: 6, message: '帳號至少 6 碼', trigger: 'blur' },
    {
      validator: (_rule, value, callback) => {
        if (!/^(?=.*[a-zA-Z])(?=.*\d).+$/.test(value)) {
          callback(new Error('帳號必須同時包含英文與數字'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  password: [
    { required: true, message: '請輸入密碼', trigger: 'blur' },
    { min: 8, message: '密碼至少 8 碼', trigger: 'blur' },
    {
      validator: (_rule, value, callback) => {
        if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(value)) {
          callback(new Error('密碼必須包含大寫、小寫英文與數字'))
        } else if (value === registerForm.account) {
          callback(new Error('密碼不得與帳號相同'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    { required: true, message: '請再次輸入密碼', trigger: 'blur' },
    {
      validator: (_rule, value, callback) => {
        if (value !== registerForm.password) {
          callback(new Error('兩次密碼輸入不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ],
  email: [
    { required: true, message: '請輸入 Email', trigger: 'blur' },
    { type: 'email', message: 'Email 格式不正確', trigger: 'blur' }
  ],
  fullName: [{ required: false, message: '請輸入姓名', trigger: 'blur' }]
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

const handleRegister = async () => {
  if (!registerFormRef.value) return;

  const valid = await registerFormRef.value.validate().catch(() => false);
  if (!valid) return;

  const payload = {
    account: registerForm.account,
    password: registerForm.password,
    email: registerForm.email,
    fullName: registerForm.fullName,
    phoneNumber: registerForm.phoneNumber || undefined,
  };

  try {
    submitting.value = true;
    await registerApi(payload);
    ElMessage.success('註冊成功，請登入');
    authStore.switchDialogMode('login');
    // 註冊成功後清空表單並帶入帳號到登入表單
    loginForm.account = registerForm.account;
    resetRegisterForm();
  } catch (error: any) {
    const backendMsg = error.response?.data?.message ?? error.message ?? '註冊失敗，請稍後再試';
    ElMessage.error(backendMsg);
  } finally {
    submitting.value = false;
  }
};

const resetRegisterForm = () => {
  registerForm.account = '';
  registerForm.password = '';
  registerForm.confirmPassword = '';
  registerForm.email = '';
  registerForm.fullName = '';
  registerForm.phoneNumber = '';
  if (registerFormRef.value) {
    registerFormRef.value.resetFields();
  }
};

const quickFill = () => {
  loginForm.account = 'fuen49';
  loginForm.password = 'Fuen49.02';
};

const goToForgotPassword = () => {
  authStore.closeLoginDialog();
  router.push('/forgot-password');
};

const handleGoogleLogin = () => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  const redirectUri = `${window.location.origin}/auth/callback`;

  if (!clientId || clientId.includes('YOUR_')) {
    ElMessage.error('Google Client ID 未設定或無效，請檢查 .env 檔案');
    return;
  }
  
  const scope = encodeURIComponent('openid email profile');
  const responseType = 'code';
  
  const googleUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=${responseType}&scope=${scope}&access_type=offline&prompt=select_account`;
  
  window.location.href = googleUrl;
};

// 監聽模式切換，清空表單驗證
watch(() => authStore.authDialogMode, () => {
  if (loginFormRef.value) loginFormRef.value.clearValidate();
  if (registerFormRef.value) registerFormRef.value.clearValidate();
});
</script>

<template>
  <el-dialog
    v-model="authStore.isLoginDialogOpen"
    :title="authStore.authDialogMode === 'login' ? '會員登入' : '會員註冊'"
    width="500px"
    center
    append-to-body
    destroy-on-close
    class="auth-dialog"
  >
    <!-- 登入表單 -->
    <template v-if="authStore.authDialogMode === 'login'">
      <div class="auth-header">
        <p class="subtitle">歡迎回來，請輸入您的帳號密碼</p>
      </div>

      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="loginRules"
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
            @click="goToForgotPassword"
          >
            忘記密碼
          </el-button>
        </div>

        <div class="footer-links">
          還沒有帳號？ <span class="link-btn" @click="authStore.switchDialogMode('register')">立即註冊</span>
        </div>
      </el-form>
    </template>

    <!-- 註冊表單 -->
    <template v-else>
      <div class="auth-header">
        <p class="subtitle">加入我們，享受更多專屬優惠</p>
      </div>

      <el-form
        ref="registerFormRef"
        :model="registerForm"
        :rules="registerRules"
        label-width="0"
        @keyup.enter="handleRegister"
      >
        <el-form-item prop="account">
          <el-input 
            v-model="registerForm.account" 
            placeholder="帳號 (英文與數字，至少 6 碼)" 
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>

        <el-form-item prop="password">
          <el-input
            v-model="registerForm.password"
            type="password"
            placeholder="密碼 (大小寫英文與數字，至少 8 碼)"
            :prefix-icon="Lock"
            show-password
            size="large"
          />
        </el-form-item>

        <el-form-item prop="confirmPassword">
          <el-input
            v-model="registerForm.confirmPassword"
            type="password"
            placeholder="再次輸入密碼"
            :prefix-icon="Lock"
            show-password
            size="large"
          />
        </el-form-item>

        <el-form-item prop="email">
          <el-input 
            v-model="registerForm.email" 
            placeholder="Email" 
            :prefix-icon="Message"
            size="large"
          />
        </el-form-item>

        <el-form-item prop="fullName">
          <el-input 
            v-model="registerForm.fullName" 
            placeholder="姓名 (選填)" 
            :prefix-icon="Edit"
            size="large"
          />
        </el-form-item>

        <el-form-item prop="phoneNumber">
          <el-input 
            v-model="registerForm.phoneNumber" 
            placeholder="電話 (選填，09xx-xxx-xxx)" 
            :prefix-icon="Iphone"
            size="large"
          />
        </el-form-item>

        <div class="divider">
          <span>或者使用以下方式註冊</span>
        </div>

        <div class="oauth-actions">
          <el-button class="oauth-btn google" size="large" @click="handleGoogleLogin">
            <img src="https://www.gstatic.com/images/branding/product/1x/gsa_512dp.png" alt="Google" width="20" />
            Google 註冊
          </el-button>
          <el-button class="oauth-btn facebook" size="large">
            <img src="https://upload.wikimedia.org/wikipedia/commons/b/b8/2021_Facebook_icon.svg" alt="Facebook" width="20" />
            Facebook 註冊
          </el-button>
        </div>

        <div class="login-action">
          <el-button 
            type="primary" 
            class="w-full" 
            size="large"
            :loading="submitting"
            @click="handleRegister"
          >
            立即註冊
          </el-button>
        </div>

        <div class="footer-links">
          已有帳號？ <span class="link-btn" @click="authStore.switchDialogMode('login')">返回登入</span>
        </div>
      </el-form>
    </template>
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
