import request from './request'
import type { ApiCategoriesResponse } from '@/types/category'

export async function fetchMainCategories(): Promise<ApiCategoriesResponse> {
  const response = await request.get<ApiCategoriesResponse>('/api/categories')
  return response.data
}
