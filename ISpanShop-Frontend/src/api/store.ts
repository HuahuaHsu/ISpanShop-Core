import axios from './axios'
import request from './request'
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
export const getSellerDashboardApi = (params: { days?: number } = {}) => {
  return axios.get<SellerDashboardData>('/api/front/store/dashboard', { params })
}
/**
 * 取得賣場訂單列表
 */
export const getSellerOrdersApi = (params: { status?: number, page?: number, pageSize?: number, keyword?: string } = {}) => {
  return axios.get<{ items: SellerOrder[], totalCount: number }>('/api/front/store/orders', {
    params
  })
}

/**
 * 取得賣家視角的訂單詳情
 */
export const getSellerOrderDetailApi = (orderId: string | number) => {
  return axios.get<SellerOrderDetail>(`/api/front/store/orders/${orderId}`)
}

/**
 * 更新賣家訂單狀態 (如：安排出貨)
 */
export const updateSellerOrderStatusApi = (orderId: number, status: number) => {
  return axios.put<ApiResponse>(`/api/front/store/orders/${orderId}/status`, { status })
}

/**
 * 取得賣場退貨申請列表
 */
export const getSellerReturnsApi = (params: { isProcessed?: boolean, page?: number, pageSize?: number } = {}) => {
  return axios.get<{ items: SellerReturnItem[], totalCount: number }>('/api/front/store/returns', { params })
}

/**
 * 取得退貨詳情
 */
export const getSellerReturnDetailApi = (orderId: string | number) => {
  return axios.get<SellerReturnDetail>(`/api/front/store/returns/${orderId}`)
}

/**
 * 審核退貨申請
 */
export const reviewReturnApi = (orderId: number, data: { isApproved: boolean, remark: string }) => {
  return axios.post<ApiResponse>(`/api/front/store/returns/${orderId}/review`, data)
}

/**
 * 賣家回覆評價
 */
export const replyToReviewApi = (data: { orderId: number, replyText: string }) => {
  return axios.post<ApiResponse>('/api/front/store/reviews/reply', data)
}

/**
 * 取得賣場公開資訊（名稱、評分、商品數、粉絲數、加入時間等）
...
 * GET /api/stores/:id
 */
export const getStoreInfo = (storeId: number) =>
  request.get('/api/stores/' + storeId)

/**
 * 取得賣場商品列表（分頁）
 * GET /api/stores/:id/products
 */
export const getStoreProducts = (
  storeId: number,
  params: { page?: number; pageSize?: number } = {},
) => request.get('/api/stores/' + storeId + '/products', { params })

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

/**
 * 取得待出貨訂單數量
 */
export const getPendingOrdersCountApi = () => {
  return axios.get<{ count: number }>('/api/front/store/pending-orders')
}
