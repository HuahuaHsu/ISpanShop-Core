import axios from './axios';
import type { LoginRequest, LoginResponse, RegisterRequest } from '../types/auth';

// 登入 API
export const loginApi = (data: LoginRequest) => {
  return axios.post<LoginResponse>('/api/front/auth/login', data);
};

// 註冊 API
export const registerApi = (data: RegisterRequest) => {
  return axios.post('/api/front/auth/register', data);
};
