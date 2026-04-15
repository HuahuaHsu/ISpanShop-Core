import request from './request'
import type { ApiCategoriesResponse, ApiSubCategoriesResponse } from '@/types/category'

export async function fetchMainCategories(): Promise<ApiCategoriesResponse> {
  const response = await request.get<ApiCategoriesResponse>('/api/categories')
  return response.data
}

export async function fetchChildCategories(parentId: number): Promise<ApiSubCategoriesResponse> {
  const response = await request.get<ApiSubCategoriesResponse>(
    `/api/categories/${parentId}/children`,
  )
  return response.data
}
