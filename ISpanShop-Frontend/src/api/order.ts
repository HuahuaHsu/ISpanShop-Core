import axios from './axios';
import type { Order, OrderListResponse } from '@/types/order';

/**
 * 取得訂單列表
 * @param userId 使用者 ID
 * @param pageNumber 頁碼
 * @param pageSize 每頁筆數
 */
export const getOrders = (userId: number, pageNumber: number = 1, pageSize: number = 10): Promise<OrderListResponse> => {
  return axios.get('/api/orders', { params: { userId, pageNumber, pageSize } });
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
