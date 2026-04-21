import axios from 'axios';
import { storage } from '../utils/storage';

// 不需要 Token 的公開路由
const PUBLIC_URLS = [
  '/api/front/auth/login',
  '/api/front/auth/register',
  '/api/front/auth/forgot-password',
  '/api/front/auth/reset-password',
  '/api/front/auth/verify-reset-token',
]

const instance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request 攔截器：自動帶入 Authorization: Bearer <token>
instance.interceptors.request.use(
  (config) => {
    const url = config.url ?? ''
    const isPublic = PUBLIC_URLS.some(path => url.includes(path))
    const token = storage.getToken();

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    } else if (!isPublic) {
      // 非公開路由才警告
      console.warn(`[API Request] 警告：發送到 ${config.url} 的請求缺少 Token`);
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response 攔截器：處理錯誤與 401 導向
instance.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response && error.response.status === 401) {
      console.error(`[API 401 Unauthorized] 觸發自動登出。請求 URL: ${error.config?.url}`);
      storage.clearAll();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default instance;
