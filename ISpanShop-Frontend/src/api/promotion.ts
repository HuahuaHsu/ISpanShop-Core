import request from './request'
import type { ApiPromotionsResponse } from '@/types/promotion'

export async function fetchActivePromotions(): Promise<ApiPromotionsResponse> {
  const response = await request.get<ApiPromotionsResponse>('/api/promotions/active')
  return response.data
}
