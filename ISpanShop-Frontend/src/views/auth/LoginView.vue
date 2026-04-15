<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../../stores/auth';
import { ElMessage } from 'element-plus';
import type { FormInstance, FormRules } from 'element-plus';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const loginFormRef = ref<FormInstance>();

const loginForm = reactive({
  email: '',
  password: ''
});

const rules = reactive<FormRules>({
  email: [
    { required: true, message: '請輸入 Email', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '請輸入密碼', trigger: 'blur' },
    { min: 6, message: '密碼長度至少 6 個字元', trigger: 'blur' }
  ]
});

const handleLogin = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid) => {
    if (valid) {
      try {
        await authStore.login(loginForm);
        ElMessage.success('登入成功');
        const redirectPath = route.query.redirect as string || '/';
        router.push(redirectPath);
      }
      catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } }
      ElMessage.error(err.response?.data?.message || '登入失敗');
      }
    }
  });
};

const quickFill = () => {
  loginForm.email = 'fuen49';
  loginForm.password = 'Fuen49.02';
};
</script>

<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <h2 class="text-center">會員登入</h2>
      </template>
      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="rules"
        label-width="0"
        @keyup.enter="handleLogin(loginFormRef)"
      >
        <el-form-item prop="email">
          <el-input v-model="loginForm.email" placeholder="Email (或帳號)" prefix-icon="User" />
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="密碼"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" class="w-full" @click="handleLogin(loginFormRef)">
            登入
          </el-button>
          <el-button type="info" class="w-full mt-2" @click="quickFill">
            快速填入帳號密碼
          </el-button>
        </el-form-item>
        <div class="text-center">
          還沒有帳號？ <router-link to="/register">立即註冊</router-link>
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
}
.login-card {
  width: 90%;
  max-width: 400px;
}
.w-full {
  width: 100%;
}
.mt-2 {
  margin-top: 10px;
}
.text-center {
  text-align: center;
}
</style>
