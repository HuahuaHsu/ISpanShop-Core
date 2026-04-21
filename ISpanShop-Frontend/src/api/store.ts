import axios from './axios'
import type { StoreApplyRequest, StoreStatusResponse, SellerDashboardData, StoreProfileData } from '../types/store'
import type { ApiResponse } from '../types/api'

/**
 * 取得賣場審核狀態
 */
export const getStoreStatusApi = () => {
  return axios.get<StoreStatusResponse>('/api/front/store/status')
}

/**
 * 申請成為賣家
 */
export const applyStoreApi = (data: StoreApplyRequest) => {
  return axios.post<ApiResponse>('/api/front/store/apply', data)
}

/**
 * 上傳賣場 Logo
 */
export const uploadStoreLogoApi = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return axios.post<{ url: string }>('/api/front/store/upload-logo', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

/**
 * 取得賣家數據中心資料
 */
export const getSellerDashboardApi = () => {
  return axios.get<SellerDashboardData>('/api/front/store/dashboard')
}

/**
 * 取得賣場介紹/設定資訊
 */
export const getStoreProfileApi = () => {
  return axios.get<StoreProfileData>('/api/front/store/profile')
}

/**
 * 更新賣場介紹/設定
 */
export const updateStoreProfileApi = (data: StoreProfileData) => {
  return axios.put<ApiResponse>('/api/front/store/profile', data)
}
