import axios from './axios';
import type { OrderListItem, OrderDetail } from '../types/order';

export const getMyOrdersApi = () => {
  return axios.get<OrderListItem[]>('/api/front/orders');
};

export const getOrderDetailApi = (id: number) => {
  return axios.get<OrderDetail>(`/api/front/orders/${id}`);
};

// ── 訂單操作 ──
export const cancelOrderApi = (id: number) => {
  return axios.post(`/api/front/orders/${id}/cancel`);
};

export const confirmReceiptApi = (id: number) => {
  return axios.post(`/api/front/orders/${id}/confirm-receipt`);
};

export const requestRefundApi = (id: number, data: any) => {
  return axios.post(`/api/front/orders/${id}/return`, data);
};

export const uploadImagesApi = (formData: FormData) => {
  return axios.post('/api/upload', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
};
