import axios from './axios';
import type { LoginRequest, LoginResponse, RegisterRequest, ForgotPasswordRequest, ResetPasswordRequest, OAuthCallbackDto, OAuthMergeRequest, OAuthResult } from '../types/auth';
import type { ApiResponse } from '../types/api';

// 登入 API
export const loginApi = (data: LoginRequest) => {
  return axios.post<LoginResponse>('/api/front/auth/login', data);
};

// 註冊 API
export const registerApi = (data: RegisterRequest) => {
  return axios.post('/api/front/auth/register', data);
};

// Email 驗證 API
export const verifyEmailApi = (code: string) => {
  return axios.get<{ isSuccess: boolean; message: string }>('/api/front/auth/verify-email', { params: { code } });
};

// 忘記密碼 API
export const forgotPasswordApi = (data: ForgotPasswordRequest) => {
  return axios.post<ApiResponse>('/api/front/auth/forgot-password', data);
};

// 重設密碼 API
export const resetPasswordApi = (data: ResetPasswordRequest) => {
  return axios.post<ApiResponse>('/api/front/auth/reset-password', data);
};

// Google 登入 API
export const googleLogin = (code: string, redirectUri: string) => {
  return axios.post<OAuthResult>('/api/front/auth/oauth/google', { code, redirectUri });
};

// 綁定 Google 帳號 API
export const bindOAuthApi = (code: string, redirectUri: string) => {
  return axios.post<{ success: boolean; message: string }>('/api/front/auth/oauth/bind-callback', { code, redirectUri });
};

// 合併 OAuth 帳號 API
export const mergeOAuthAccount = (data: OAuthMergeRequest) => {
  return axios.post<LoginResponse>('/api/front/auth/oauth/merge', data);
};

// 解除 OAuth 綁定 API
export const unbindOAuth = () => {
  return axios.post<{ success: boolean; message: string }>('/api/front/auth/oauth/unbind');
};
