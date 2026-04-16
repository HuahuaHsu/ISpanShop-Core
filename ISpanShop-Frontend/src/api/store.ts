import axios from './axios';
import type { SellerDashboardData } from '../types/store';

export const getSellerDashboardApi = () => {
  return axios.get<SellerDashboardData>('/api/front/store/dashboard');
};
