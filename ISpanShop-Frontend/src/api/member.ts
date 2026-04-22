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
 * 上傳大頭貼
 */
export const uploadAvatar = (file: File) => {
  const formData = new FormData();
  formData.append('file', file);
  return axios.post<{ url: string }>('/api/front/upload/avatar', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};

/**
 * 變更密碼
 */
export const changePassword = (data: ChangePasswordRequest) => {
  return axios.put<{ message: string }>('/api/front/member/password', data);
};

/** 點數紀錄 DTO */
export interface PointHistory {
  id: number;
  changeAmount: number;
  balanceAfter: number;
  description: string;
  createdAt: string;
}

/**
 * 取得錢包餘額
 */
export const getWalletBalance = () => {
  return axios.get<any>('/api/member/wallet-balance');
};

/**
 * 取得點數變動紀錄
 */
export const getPointHistory = () => {
  return axios.get<PointHistory[]>('/api/member/point-history');
};

/**
 * 取得所有收件地址
 */
export const getAddressList = () => {
  return axios.get<AddressDto[]>('/api/member/addresses');
};

/**
 * 新增收件地址
 */
export const createAddress = (data: CreateAddressDto) => {
  return axios.post<AddressDto>('/api/member/addresses', data);
};

/**
 * 更新收件地址
 */
export const updateAddress = (id: number, data: UpdateAddressDto) => {
  return axios.put<{ message: string }>(`/api/member/addresses/${id}`, data);
};

/**
 * 刪除收件地址
 */
export const deleteAddress = (id: number) => {
  return axios.delete<{ message: string }>(`/api/member/addresses/${id}`);
};

/**
 * 設定為預設地址
 */
export const setDefaultAddress = (id: number) => {
  return axios.patch<{ message: string }>(`/api/member/addresses/${id}/default`);
};

