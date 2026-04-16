import axios from './axios';
import type { OrderListItem, OrderDetail } from '../types/order';

export const getMyOrdersApi = () => {
  return axios.get<OrderListItem[]>('/api/front/orders');
};

export const getOrderDetailApi = (id: number) => {
  return axios.get<OrderDetail>(`/api/front/orders/${id}`);
};
