import request from './request'
import type { ApiResponse, FetchProductsParams, ProductListResponse } from '@/types/product'

export async function fetchProductList(
  params: FetchProductsParams = {},
): Promise<ApiResponse<ProductListResponse>> {
  const response = await request.get<ApiResponse<ProductListResponse>>('/api/products', { params })
  return response.data
}
