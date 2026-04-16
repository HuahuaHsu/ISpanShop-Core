import axios from './axios';
import type { Order, OrderListResponse } from '@/types/order';

/**
 * 取得訂單列表
 * @param query 查詢參數
 */
export const getOrders = (query: { 
  pageNumber?: number; 
  pageSize?: number; 
  statuses?: number[] 
}): Promise<OrderListResponse> => {
  return axios.get('/api/orders', { params: query });
};

/**
 * 取得訂單詳情
 * @param id 訂單 ID
 */
export const getOrderDetail = (id: string): Promise<Order> => {
  return axios.get(`/api/orders/${id}`);
};

/**
 * 取消訂單
 * @param id 訂單 ID
 */
export const cancelOrder = (id: number): Promise<void> => {
  return axios.post(`/api/orders/${id}/cancel`);
};

/**
 * 申請退貨
 * @param id 訂單 ID
 */
export const returnOrder = (id: number): Promise<void> => {
  return axios.post(`/api/orders/${id}/return`);
};

/**
 * 產生測試訂單 (開發測試用)
 */
export const generateTestOrder = (): Promise<any> => {
  return axios.post('/api/orders/test-generate');
};
