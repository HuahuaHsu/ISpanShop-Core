import axios from './axios';
import type { ChangePasswordRequest } from '@/types/auth';

/** 會員資料 DTO (用於讀取) */
export interface MemberDto {
  id: number;
  account: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  gender: number | null;
  birthday: string | null;
  pointBalance?: number;
  levelName?: string;
  avatarUrl?: string;
}

/** 更新會員資料 DTO (用於寫入) - 對應 C# UpdateMemberProfileDto */
export interface UpdateMemberProfileDto {
  id: number;
  account: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  gender: number | null;  // byte? (0-255)
  birthday: string | null;  // DateOnly? -> 送 "YYYY-MM-DD" 字串
  avatarUrl?: string;
}

/**
 * 取得會員個人資料
 */
export const getMemberProfile = (id: number) => {
  return axios.get<MemberDto>(`/api/front/profile/${id}`);
};

/**
 * 更新會員個人資料
 */
export const updateMemberProfile = (id: number, data: UpdateMemberProfileDto) => {
  return axios.put<{ message: string }>(`/api/front/profile/${id}`, data);
};

/**
 * 上傳大頭照
 */
export const uploadAvatar = (file: File) => {
  const formData = new FormData();
  formData.append('file', file);
  return axios.post<{ url: string }>('/api/front/profile/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};

/**
 * 變更密碼
 */
export const changePassword = (data: ChangePasswordRequest) => {
  return axios.put<{ isSuccess: boolean; message: string }>('/api/front/member/password', data);
};
