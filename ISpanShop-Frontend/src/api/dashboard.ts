import axios from './axios';
import type { DashboardKpi, ApexChartData, TopProductSales } from '@/types/dashboard';

export const getDashboardKpis = (storeId?: number, period: string = 'month'): Promise<DashboardKpi> => {
  return axios.get('/api/dashboard/kpis', { params: { storeId, period } });
};

export const getCategoryCompositionChart = (storeId?: number, period: string = 'month', type: string = 'Bar'): Promise<ApexChartData> => {
  return axios.get('/api/dashboard/category-composition', { params: { storeId, period, type } });
};

export const getMonthlyTrend = (storeId?: number, year?: number): Promise<ApexChartData> => {
  return axios.get('/api/dashboard/monthly-trend', { params: { storeId, year } });
};

export const getTopCategories = (storeId?: number, period: string = 'month', orderBy: string = 'revenue'): Promise<TopProductSales[]> => {
  return axios.get('/api/dashboard/top-categories', { params: { storeId, period, orderBy } });
};

export const getCategoryContribution = (storeId?: number, period: string = 'month'): Promise<ApexChartData> => {
  return axios.get('/api/dashboard/category-contribution', { params: { storeId, period } });
};
