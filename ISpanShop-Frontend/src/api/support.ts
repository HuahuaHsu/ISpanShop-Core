import axios from './axios';
import type { SupportTicket } from '@/types/support';

/**
 * 取得我的工單列表
 */
export const getMyTickets = (): Promise<SupportTicket[]> => {
  return axios.get('/api/support-tickets/my');
};

/**
 * 取得工單詳情
 * @param id 工單 ID
 */
export const getTicketDetail = (id: number): Promise<SupportTicket> => {
  return axios.get(`/api/support-tickets/${id}`);
};

/**
 * 提交新工單
 * @param data 工單資料
 */
export const createTicket = (data: Partial<SupportTicket>): Promise<any> => {
  return axios.post('/api/support-tickets', data);
};
