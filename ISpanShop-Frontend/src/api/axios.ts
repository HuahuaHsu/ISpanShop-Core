import axios from 'axios';
import { storage } from '../utils/storage';

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
    const token = storage.getToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
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
      // 收到 401 → 清除 token → 導向 /login
      storage.clearAll();
      window.location.href = '/login'; // 強制導向登入頁
    }
    return Promise.reject(error);
  }
);

export default instance;
