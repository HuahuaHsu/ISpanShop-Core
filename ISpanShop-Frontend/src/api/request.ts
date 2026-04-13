import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'

const request = axios.create({
  // Vite proxy 已設定 /api/* → https://localhost:7125，baseURL 留空走同源
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '',
  timeout: 10000,
})

// 請求攔截器：自動帶入 JWT（目前商品列表為公開 API，token 不強制）
request.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`
    }
    return config
  },
  (error: unknown) => Promise.reject(error),
)

// 回應攔截器
request.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        localStorage.removeItem('token')
        void router.push('/auth/login')
      } else {
        const msg =
          (error.response?.data as { message?: string } | undefined)?.message ??
          error.message ??
          '請求失敗'
        ElMessage.error(msg)
      }
    }
    return Promise.reject(error)
  },
)

export default request
