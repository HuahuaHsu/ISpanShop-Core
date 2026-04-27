import axios from './axios'

/** 獲取賣家流量分析數據 */
export function getTrafficAnalyticsApi() {
  return axios.get('/api/front/seller/analytics/traffic')
}
