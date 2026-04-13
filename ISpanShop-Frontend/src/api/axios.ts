import axios from 'axios';
import { ElMessage } from 'element-plus';

const instance = axios.create({
  // 將 baseURL 設為 '/'，讓 Vite 的 Proxy 攔截 /api 並轉發到後端
  baseURL: import.meta.env.VITE_API_BASE_URL || '/',
  timeout: 10000,
});

// 回應攔截器：處理錯誤訊息
instance.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const message = error.response?.data?.message || '網路錯誤，請稍後再試';
    ElMessage.error(message);
    return Promise.reject(error);
  }
);

export default instance;
