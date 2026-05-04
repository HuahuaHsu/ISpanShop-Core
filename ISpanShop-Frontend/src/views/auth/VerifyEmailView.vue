<template>
  <div class="verify-email-container">
    <el-card class="verify-email-card">
      <el-result
        :icon="resultIcon"
        :title="resultTitle"
        :sub-title="message"
      >
        <template #extra>
          <el-button v-if="isSuccess" type="primary" @click="router.push('/login')">
            前往登入
          </el-button>
          <el-button v-else @click="router.push('/register')">
            返回註冊
          </el-button>
        </template>
      </el-result>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { verifyEmailApi } from '@/api/auth'

const route = useRoute()
const router = useRouter()
const loading = ref(true)
const isSuccess = ref(false)
const message = ref('正在驗證您的 Email，請稍候。')

const resultIcon = computed(() => {
  if (loading.value) return 'info'
  return isSuccess.value ? 'success' : 'error'
})

const resultTitle = computed(() => {
  if (loading.value) return 'Email 驗證中'
  return isSuccess.value ? 'Email 驗證成功' : 'Email 驗證失敗'
})

onMounted(async () => {
  const code = route.query.code
  if (!code || typeof code !== 'string') {
    loading.value = false
    message.value = '驗證連結缺少必要參數。'
    return
  }

  try {
    const { data } = await verifyEmailApi(code)
    isSuccess.value = data.isSuccess
    message.value = data.message || 'Email 驗證成功，請登入。'
  } catch (error: any) {
    isSuccess.value = false
    message.value = error.response?.data?.message || '驗證連結無效或帳號已啟用。'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.verify-email-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 80vh;
  padding: 20px;
}

.verify-email-card {
  width: 100%;
  max-width: 460px;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}
</style>
