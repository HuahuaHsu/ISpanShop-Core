import axios from './axios';
import type { LoginRequest, LoginResponse, RegisterRequest, ForgotPasswordRequest, ResetPasswordRequest } from '../types/auth';
import type { ApiResponse } from '../types/api';

// 登入 API
export const loginApi = (data: LoginRequest) => {
  return axios.post<LoginResponse>('/api/front/auth/login', data);
};

// 註冊 API
export const registerApi = (data: RegisterRequest) => {
  return axios.post('/api/front/auth/register', data);
};

// 忘記密碼 API
export const forgotPasswordApi = (data: ForgotPasswordRequest) => {
  return axios.post<ApiResponse>('/api/front/auth/forgot-password', data);
};

// 重設密碼 API
export const resetPasswordApi = (data: ResetPasswordRequest) => {
  return axios.post<ApiResponse>('/api/front/auth/reset-password', data);
};
