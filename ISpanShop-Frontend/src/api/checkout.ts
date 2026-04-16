import axios from './axios';
import type { CheckoutRequest, CheckoutResponse } from '../types/checkout';

/**
 * 建立訂單（結帳）
 */
export const createOrderApi = (data: CheckoutRequest) => {
  return axios.post<CheckoutResponse>('/api/checkout', data);
};
