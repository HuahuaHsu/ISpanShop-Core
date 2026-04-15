<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { registerApi } from '../../api/auth';
import { ElMessage } from 'element-plus';
import type { FormInstance, FormRules } from 'element-plus';

const router = useRouter();
const registerFormRef = ref<FormInstance>();

const registerForm = reactive({
  account: '',
  password: '',
  confirmPassword: '',
  email: '',
  fullName: '',
  phoneNumber: ''
});

const rules = reactive<FormRules>({
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

const handleRegister = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid) => {
    if (valid) {
      try {
        await registerApi(registerForm);
        ElMessage.success('註冊成功，請重新登入');
        router.push('/login');
      } catch (error: unknown) {
        const err = error as { response?: { data?: { message?: string } } }
        ElMessage.error(err.response?.data?.message || '註冊失敗');
      }
    }
  });
};
</script>

<template>
  <div class="register-container">
    <el-card class="register-card">
      <template #header>
        <h2 class="text-center">會員註冊</h2>
      </template>
      <el-form
        ref="registerFormRef"
        :model="registerForm"
        :rules="rules"
        label-position="top"
      >
        <el-form-item label="帳號" prop="account">
          <el-input v-model="registerForm.account" placeholder="請輸入帳號" />
        </el-form-item>
        <el-form-item label="密碼" prop="password">
          <el-input v-model="registerForm.password" type="password" placeholder="至少 6 個字元" show-password />
        </el-form-item>
        <el-form-item label="確認密碼" prop="confirmPassword">
          <el-input v-model="registerForm.confirmPassword" type="password" placeholder="請再次輸入密碼" show-password />
        </el-form-item>
        <el-form-item label="Email" prop="email">
          <el-input v-model="registerForm.email" placeholder="example@ispan.com" />
        </el-form-item>
        <el-form-item label="姓名" prop="fullName">
          <el-input v-model="registerForm.fullName" placeholder="您的姓名" />
        </el-form-item>
        <el-form-item label="電話" prop="phoneNumber">
          <el-input v-model="registerForm.phoneNumber" placeholder="09xx-xxx-xxx" />
        </el-form-item>
        <el-form-item>
          <el-button type="success" class="w-full" @click="handleRegister(registerFormRef)">
            確認註冊
          </el-button>
        </el-form-item>
        <div class="text-center">
          已有帳號？ <router-link to="/login">返回登入</router-link>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 40px 0;
}
.register-card {
  width: 90%;
  max-width: 450px;
}
.w-full {
  width: 100%;
}
.text-center {
  text-align: center;
}
</style>
