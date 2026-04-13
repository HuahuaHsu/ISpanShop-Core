import request from './request'
import type { ApiBrandsResponse } from '@/types/brand'

export interface FetchBrandsParams {
  categoryId?: number
  subCategoryId?: number
  keyword?: string
}

export async function fetchBrands(params: FetchBrandsParams = {}): Promise<ApiBrandsResponse> {
  const response = await request.get<ApiBrandsResponse>('/api/brands', { params })
  return response.data
}
